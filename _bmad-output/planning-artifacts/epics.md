---
stepsCompleted: [1, 2, 3, 4]
inputDocuments:
  - '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-23/ARCHITECTURE-SPINE.md'
  - '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-23/.memlog.md'
  - '_bmad-output/planning-artifacts/research/technical-webrtc-group-calls-signalr-vs-acs-research-2026-07-23.md'
---

# JustTaskTracker - Epic Breakdown

## Overview

This document provides the epic and story breakdown for the **Video Calls (ACS)** feature in JustTaskTracker. No PRD or UX design contract exists for this feature — both were deliberately skipped; the finalized Architecture Spine (`AD-1`..`AD-15`) is the requirements source, since it already encodes the confirmed product decisions (from the BA research handoff + the user's own requirements revision) as enforceable invariants. Every FR/NFR below cites the `AD-n` it derives from for traceability back to the architecture.

## Requirements Inventory

### Functional Requirements

FR1: A board member can create a video chat session on a board immediately (not scheduled), specifying a Title and a Topic. `[AD-8]`
FR2: The creator chooses the session's Visibility: **Open** (any board member may join) or **Restricted** (only an explicit, creator-chosen list of board members may join; the creator is always implicitly included). `[AD-8, AD-4]`
FR3: The creator may optionally link zero or more existing board tasks (`BoardTask`) to the session as discussion-topic references. `[AD-13]`
FR4: A board may have multiple video chat sessions active at the same time. `[AD-8]`
FR5: An authorized board member (per FR2's Visibility rule) can join an active session, which connects them via the Azure Communication Services Calling SDK. `[AD-4, AD-8]`
FR6: At most one participant may share their screen at a time in a session; a share request is rejected if someone else is already presenting. `[AD-9]`
FR7: The screen-share presenter lock is released automatically if the presenter leaves or disconnects (not only on an explicit "stop sharing"). `[AD-9, AD-12]`
FR8: When a session starts, every eligible board member (all members if Open, only the creator's chosen allow-list if Restricted) receives a real-time "call started" alert regardless of which page of the app they are currently viewing. `[AD-10]`
FR9: On the board page, members see the active session's live state — who's currently in the call, how long it has been running, and when it ends — updating in real time without a page refresh. `[AD-2, AD-10]`
FR10: A session closes automatically (soft-close, not deleted) when its last active participant leaves; the board retains closed sessions as call history. `[AD-8, AD-12]`
FR11: The session's creator, or a board member with the Owner or Admin role, can force-end an active session before it would otherwise close on its own. `[AD-15]`
FR12: Before a user can create or join a session, the client checks browser support (`isSupportedBrowser`) and runs pre-call diagnostics, surfacing a clear message rather than a silent failure if unsupported. `[Consistency Conventions — Client join UX]`
FR13: A board member can retrieve the current list of active call sessions on a board as a point-in-time read (independent of any real-time push), so another member can discover and join a call simply by loading/refreshing the board page. `[AD-8]`

### NonFunctional Requirements

NFR1: All call audio/video/screen-share media and signaling flows exclusively through Azure Communication Services — no self-hosted WebRTC signaling path is introduced alongside it. `[AD-1]`
NFR2: SignalR carries only call-state notifications; it must never carry SDP, ICE candidates, or media. `[AD-2]`
NFR3: The ACS Event Grid webhook is the sole authoritative writer of participant join/leave and session-closure state; its handlers must be idempotent and tolerant of at-least-once, possibly out-of-order event delivery. `[AD-12]`
NFR4: The Event Grid webhook endpoint is unauthenticated at the ASP.NET auth-policy level but must validate Event Grid's subscription-validation handshake and delivery signature. `[AD-11]`
NFR5: ACS connection-string configuration is environment-level config (Key Vault/appsettings), never an Aspire-hosted resource — consistent with how Azure SignalR Service is already wired in this project. `[AD-7]`
NFR6: User-to-ACS-identity mapping uses a self-owned mapping table (`AcsUserIdentityMapping`), not ACS's preview-only Custom ID feature, to avoid depending on a non-stable SDK/API surface. `[AD-6]`

### Additional Requirements (from Architecture)

- New Calls feature module mirrors the existing layered/CQRS structure exactly: `Domain/Application/Infrastructure/Persistence/API`, mirrored client-side, one file per command/query (record + handler + validator co-located). `[AD-3]`
- `Application.Calls` may depend on `IBoardRepository`/`BoardRolePermissions`; `Application.Boards` must never depend on `*.Calls`. `[AD-5]`
- New entities: `CallSession`, `CallParticipant`, `CallSessionAllowedParticipant`, `CallSessionLinkedTask`, `AcsUserIdentityMapping`. `[Structural Seed]`
- ACS Room is created before the `CallSession` DB row is persisted; on DB-persist failure, the orphaned Room is deleted (best-effort compensation). `[AD-14]`
- Pinned stack: `@azure/communication-calling` 1.43.1, `Azure.Communication.Identity` 1.3.1, `Azure.Communication.Rooms` 1.2.0, `Azure.Messaging.EventGrid` 5.0.0. `[Stack]`
- No infrastructure-as-code / starter template applies — this is a brownfield feature slice added to the existing solution, not a new project scaffold.

### UX Design Requirements

No UX design contract exists for this feature (no `bmad-ux` run was performed). Per the Architecture Spine's Structural Seed, the call UI is a set of new Blazor components embedded into the existing `Pages/Boards/BoardPage.razor` (session list, create/join/screen-share controls) plus a small addition to `Layout/MainLayout.razor` (the app-wide "call started" alert). No separate design tokens, component library, or accessibility audit work is in scope beyond following the existing UI's established look and components.

### FR Coverage Map

| Requirement | Epic |
| --- | --- |
| FR1, FR2, FR3, FR4 | Epic 1 |
| FR5 | Epic 1 |
| FR6, FR7 | Epic 2 |
| FR8, FR9 | Epic 3 |
| FR10, FR11 | Epic 1 |
| FR12 | Epic 1 |
| FR13 | Epic 1 |
| NFR1–NFR6 | cross-cutting, enforced across Epics 1–3 |

## Epic List

### Epic 1: Start and Join Board Video Calls
Board members can start a video chat session on a board right now — open to everyone on the board or restricted to a chosen few, optionally tied to specific tasks as the discussion topic — discover and join it, have the system reliably track who's actually in it, and see it close itself and land in call history when everyone's left (or be force-ended early by its creator/a board admin). This is the complete, standalone core of the feature: without discoverability (finding an active call at all) and a way to actually close one, "starting a call" wouldn't be usable end-to-end. Story order within this epic was corrected mid-build for exactly that reason: discovery (list active calls) moved into Story 1.1 itself, and reliable participant-tracking/auto-close (originally last) was promoted to Story 1.2, right after create/join — Restricted visibility and task-linking are refinements on an already-working core, not prerequisites for one.
**FRs covered:** FR1, FR2, FR3, FR4, FR5, FR10, FR11, FR12, FR13
**NFRs covered:** NFR1, NFR3, NFR4, NFR5, NFR6

### Epic 2: Share Your Screen During a Call
Any participant already in a call (per Epic 1) can share their screen, with the system guaranteeing only one presenter at a time and automatically freeing the slot if the presenter leaves or disconnects — no stuck locks, no silent double-presenting.
**FRs covered:** FR6, FR7

### Epic 3: Stay Aware of Calls Across the App
Board members find out a call has started even when they're not looking at that board — every board member if the call is Open, only the creator's chosen list if it's Restricted — and, on the board page itself, see the call's live state (who's currently in it, how long it's been running, when it ends) update in real time, without refreshing.
**FRs covered:** FR8, FR9
**NFRs covered:** NFR2

## Epic 1: Start and Join Board Video Calls

Board members can start a video chat session on a board right now — open to everyone on the board or restricted to a chosen few, optionally tied to specific tasks as the discussion topic — discover and join it, have the system reliably track who's actually in it, and see it close itself and land in call history when everyone's left (or be force-ended early by its creator/a board admin).

> **Sequencing note (corrected mid-build):** the original draft put call discovery in Story 1.5 (history) and participant-tracking/auto-close last, in Story 1.4 — meaning Story 1.1 alone shipped a call with no way for other members to find it and no way to ever close it. Both gaps are fixed here: discovery moves into **Story 1.1** itself; participant-tracking/auto-close moves up to **Story 1.2**, immediately after create/join. Story 1.1 also ships a **temporary, interim** creator-only "end call" direct status write (cheap to build, unblocks manual testing without leaking ACS Rooms) — Story 1.2 supersedes it with the real Event-Grid-driven mechanism, and Story 1.6 converts the manual path from a direct write into a proper trigger through that same pipeline while extending it to Owner/Admin.

### Story 1.1: Create, discover, and join an open video call on a board

As a board member,
I want to start a video call on a board, have other members be able to find it, join it, and be able to end it when we're done,
So that I can talk to other board members in real time without leaving the app, and without a test call lingering forever with no way to close it.

**Acceptance Criteria:**

**Given** I am a member of a board
**When** I choose to start a call and give it a Title (required, up to 50 characters) and an optional Topic (up to 200 characters)
**Then** a new call session is created for that board, an Azure Communication Services Room is created for it (`AcsRoomId` stored on the session), and I receive the created session's details
**And** if creating the ACS Room succeeds but persisting the session fails, the just-created Room is deleted and I see an error, not a half-created call
**And** creating a call does not by itself grant a join token — I (like any other authorized member) call the join action separately to get one, keeping "provision a call" and "get a token to enter it" as two distinct, clearly-owned actions

**Given** I am creating a call
**When** I leave the Title blank, or enter a Title longer than 50 characters, or a Topic longer than 200 characters
**Then** the call is not created and I see a validation error naming the field and the limit

**Given** an active, open call session exists on a board I'm a member of
**When** I choose to join it
**Then** I receive a valid ACS access token (via my own `AcsUserIdentityMapping` entry, created on first use) and connect to the call's audio/video through the Azure Communication Services Calling SDK
**And** a non-member of the board cannot create or join a call on it, even with a direct API call

**Given** a board has one or more active call sessions
**When** any board member loads or refreshes the board page
**Then** they see the current list of active sessions (title, topic, who created it) as a point-in-time read — no SignalR/live push required for this to work (FR13), which is how anyone other than the creator can actually find and join a call in this story

**Given** I am the creator of an active call
**When** I choose to end it
**Then** the session is marked `Closed` and Azure Communication Services is asked to end the Room — **this is a temporary, interim mechanism**: a direct status write triggered by the creator's explicit action, not yet the Event-Grid-driven pipeline (that lands in Story 1.2) and not yet available to Owner/Admin (that lands in Story 1.6)
**And** a board member who is not the call's creator cannot end it in this story (broader authority arrives in Story 1.6)

**Given** I am about to create or join a call
**When** the app checks my browser
**Then** if my browser is unsupported, I see a clear message instead of a silent failure, and pre-call diagnostics (mic/camera/network) run before I'm connected

### Story 1.2: Reliably track participants and auto-close a call when everyone leaves

As a board member,
I want a call to know who's actually still in it and close itself once everyone has left,
So that calls don't linger open forever or lose track of who's really there, without anyone needing to remember to click "end."

**Acceptance Criteria:**

**Given** I join a call
**When** Azure Communication Services reports my join via its Event Grid event
**Then** the system records me as a participant with a join time, correlating the event to the right call session by its `AcsRoomId` — even if the board has other concurrent calls

**Given** I am the last active participant in a call
**When** I leave (or my connection drops/crashes)
**Then** Azure Communication Services' departure event closes the call session automatically — no explicit "leave" API call from my client is required for this to work

**Given** Azure Communication Services redelivers the same participant event, or delivers events out of order
**When** the system processes it
**Then** the recorded state is unaffected by the duplicate or reordering (idempotent, timestamped from the event itself)

**Given** Story 1.1's interim direct-write "end call" action
**When** this story's Event-Grid pipeline goes live
**Then** it becomes the sole authority for `Closed`/`EndedAtUtc` going forward, per `AD-12` — reconciling the interim mechanism is Story 1.6's job, not this one

### Story 1.3: Restrict a call to specific board members

As a call creator,
I want to limit who can join my call to a specific set of board members,
So that sensitive discussions aren't open to the whole board.

**Acceptance Criteria:**

**Given** I am creating a call
**When** I choose "Restricted" and pick specific board members
**Then** only those members (plus myself, automatically) can join the call — everyone else on the board is denied

**Given** a Restricted call already exists
**When** a board member who is not on the allow-list, and holds neither the Owner nor Admin role on that board, tries to join
**Then** they are denied

**Given** a Restricted call already exists and I am not on its allow-list
**When** I hold the Owner or Admin role on that board
**Then** I can join anyway — Owner/Admin can always join any call on their own board, restricted or not

### Story 1.4: Link board tasks as a call's discussion topic

As a call creator,
I want to optionally attach existing board tasks to the call I'm starting,
So that participants know what will be discussed.

**Acceptance Criteria:**

**Given** I am creating a call on a board that has tasks
**When** I optionally select one or more existing tasks
**Then** those tasks are linked to the call session and visible to anyone viewing the call as its discussion topics
**And** creating a call without selecting any tasks still works exactly as before (linking is optional)

### Story 1.5: View a board's call history

As a board member,
I want to see past video calls on a board,
So that I know what discussions happened even if I missed them.

**Acceptance Criteria:**

**Given** a board has one or more closed call sessions
**When** I view the board
**Then** I see a history list of past sessions with their title, topic, linked tasks (if any), when they started/ended, and **who was present** (each participant and their individual join/leave times, not just an aggregate count)

Note: seeing currently **active** sessions is Story 1.1's job (FR13), not this story's — this story covers closed/historical sessions only.

### Story 1.6: Force-end a call early, for its creator or a board Owner/Admin

As a call's creator, or a board Owner or Admin,
I want to end an active call before everyone has left on their own,
So that I can wrap up a session that's run its course.

**Acceptance Criteria:**

**Given** I am the creator of an active call, or hold the Owner or Admin role on its board
**When** I choose to end the call
**Then** all participants are disconnected from Azure Communication Services and the call closes through the same participant-departure pipeline as a normal end (not a direct status write) — this story converts Story 1.1's interim creator-only direct write into a proper trigger through Story 1.2's Event-Grid pipeline, and extends who may invoke it to Owner/Admin

**Given** I am a board member who did not create the call and hold neither the Owner nor Admin role (e.g. ScrumMaster or User)
**When** I try to end someone else's active call
**Then** I am denied

## Epic 2: Share Your Screen During a Call

Any participant already in a call (per Epic 1) can share their screen, with the system guaranteeing only one presenter at a time and automatically freeing the slot if the presenter leaves or disconnects — no stuck locks, no silent double-presenting.

### Story 2.1: Share your screen as the sole presenter

As a call participant,
I want to share my screen with everyone else in the call,
So that I can show something instead of just describing it.

**Acceptance Criteria:**

**Given** I am in an active call and no one else is currently sharing their screen
**When** I choose to share my screen
**Then** I become the call's presenter (`CurrentPresenterUserId` set to me), and my screen share starts through the Azure Communication Services Calling SDK

**Given** I am in an active call and another participant is already sharing their screen
**When** I try to start sharing mine
**Then** my request is rejected — I'm told someone else is presenting, and my screen-share does not start

**Given** two participants request to start screen-share within the same moment
**When** both requests are processed
**Then** only one of them wins the presenter slot; the other's request is rejected, never both

**Given** I am the current presenter
**When** I choose to stop sharing
**Then** the presenter slot is freed and any other participant can now request it

### Story 2.2: Presenter lock releases automatically if I leave or disconnect

As a call participant,
I want the screen-share slot to free up if the current presenter's connection drops or they leave without stopping their share first,
So that the call doesn't get stuck with no one able to present.

**Acceptance Criteria:**

**Given** I am the current presenter in an active call
**When** I leave the call, or my connection drops/crashes, without explicitly stopping my screen share first
**Then** the participant-departure event that records my leaving also clears `CurrentPresenterUserId`, and any remaining participant can immediately request to present

**Given** I am a participant who is not the current presenter, and the presenter has just disconnected
**When** I check whether I can share my screen
**Then** the presenter slot shows as free, with no delay beyond normal participant-departure processing

## Epic 3: Stay Aware of Calls Across the App

Board members find out a call has started even when they're not looking at that board — every board member if the call is Open, only the creator's chosen list (plus Owner/Admin) if it's Restricted — and, on the board page itself, see the call's live state (who's currently in it, how long it's been running, when it ends) update in real time, without refreshing.

### Story 3.1: Get notified anywhere in the app when a call starts

As a board member,
I want to be alerted the moment a video call starts on a board I have access to, no matter what page I'm currently on,
So that I don't miss a call just because I wasn't looking at that board.

**Acceptance Criteria:**

**Given** I am logged in and connected, viewing any page of the app (not necessarily the board in question)
**When** an Open call starts on a board I'm a member of
**Then** I receive a real-time "call started" alert naming the board and the call

**Given** a Restricted call starts on a board I'm a member of, and I am neither on its allow-list nor an Owner/Admin of that board
**When** the call starts
**Then** I do **not** receive an alert for it

**Given** a Restricted call starts and I am on its allow-list, or I hold the Owner/Admin role on that board
**When** the call starts
**Then** I receive the alert regardless of which page I'm on

**Given** an eligible recipient is not currently connected to the app at all
**When** a call starts
**Then** no error occurs — they simply don't receive a live alert (they'll still see the active call listed when they next open/refresh the board, per Story 1.1's FR13 point-in-time list)

### Story 3.2: See a call's live state while viewing its board

As a board member viewing a board with an active call,
I want to see who's currently in the call, how long it's been running, and when it ends, updating live,
So that I know what's happening without needing to join or refresh the page.

**Acceptance Criteria:**

**Given** I am viewing a board with an active call
**When** a participant joins or leaves, the presenter changes, or the call ends
**Then** the board page reflects the change in real time, without a manual refresh

**Given** I am viewing a board with an active call
**When** time passes
**Then** the displayed call duration keeps counting up live

**Given** I navigate away from the board page
**When** the call's state continues to change
**Then** I stop receiving those live updates (they're board-scoped, not app-wide — app-wide awareness is covered by Story 3.1)
