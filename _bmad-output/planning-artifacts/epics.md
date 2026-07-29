---
stepsCompleted: [1, 2, 3, 4]
inputDocuments:
  - '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-23/ARCHITECTURE-SPINE.md'
  - '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-23/.memlog.md'
  - '_bmad-output/planning-artifacts/research/technical-webrtc-group-calls-signalr-vs-acs-research-2026-07-23.md'
  - '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-29/ARCHITECTURE-SPINE.md'
  - '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-29/.memlog.md'
---

# JustTaskTracker - Epic Breakdown

## Overview

This document provides the epic and story breakdown for the **Video Calls (ACS)** feature in JustTaskTracker. No PRD or UX design contract exists for this feature — both were deliberately skipped; the finalized Architecture Spine (`AD-1`..`AD-15`) is the requirements source, since it already encodes the confirmed product decisions (from the BA research handoff + the user's own requirements revision) as enforceable invariants. Every FR/NFR below cites the `AD-n` it derives from for traceability back to the architecture.

**Extended 2026-07-29** with **Scheduled Video Calls** (one-time future scheduling; recurrence was considered and explicitly dropped) — a child feature built on top of the Video Calls (ACS) feature above. Same precedent: no PRD/UX contract, the child Architecture Spine (`architecture-just-task-tracker-2026-07-29`, local `AD-1`..`AD-7`, inheriting `AD-4/8/9/10/12/14/15` from the parent spine above) is the requirements source. FR14 onward and NFR7 onward below derive from it.

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

FR14: When creating a call, the creator can choose to schedule it for a future date and time (year/month/day/hour/minute) instead of starting it immediately, using the same Title/Topic/Visibility/linked-tasks fields as an immediate call. `[2026-07-29 AD-1, inherited AD-8/AD-14]`
FR15: A board member cannot join a scheduled call until at most one minute before its scheduled start time. `[2026-07-29 AD-4]`
FR16: All eligible recipients (the same Open/Restricted+allow-list eligibility rules as FR8) receive a notification the moment a call is scheduled. `[2026-07-29 AD-6, inherited AD-10]`
FR17: All eligible recipients receive a reminder notification one minute before a scheduled call's start time. `[2026-07-29 AD-2, AD-6]`
FR18: All eligible recipients receive the existing "call started" notification (FR8) at the moment a scheduled call actually activates — whether activation is triggered by the clock or by the first participant joining early. `[2026-07-29 AD-4, AD-6]`
FR19: The call's creator, or a board member with the Owner or Admin role, can change a scheduled call's planned start date and time after creation; doing so sends a new notification to all eligible recipients and resets the one-minute reminder. `[2026-07-29 AD-4, inherited AD-15]`
FR20: A board member can discover scheduled calls on a board the same point-in-time way FR13 already covers for active ones — no live push required. `[2026-07-29 AD-5]`
FR21: The call's creator, or a board Owner/Admin, can cancel a scheduled call before it ever activates; it closes immediately without needing anyone to have joined it. `[2026-07-29 AD-3]`
FR22: A scheduled call that activates but that nobody joins within five minutes of its actual start closes itself automatically, without needing anyone to act. `[2026-07-29 AD-7]`

### NonFunctional Requirements

NFR1: All call audio/video/screen-share media and signaling flows exclusively through Azure Communication Services — no self-hosted WebRTC signaling path is introduced alongside it. `[AD-1]`
NFR2: SignalR carries only call-state notifications; it must never carry SDP, ICE candidates, or media. `[AD-2]`
NFR3: The ACS Event Grid webhook is the sole authoritative writer of participant join/leave and session-closure state; its handlers must be idempotent and tolerant of at-least-once, possibly out-of-order event delivery. `[AD-12]`
NFR4: The Event Grid webhook endpoint is unauthenticated at the ASP.NET auth-policy level but must validate Event Grid's subscription-validation handshake and delivery signature. `[AD-11]`
NFR5: ACS connection-string configuration is environment-level config (Key Vault/appsettings), never an Aspire-hosted resource — consistent with how Azure SignalR Service is already wired in this project. `[AD-7]`
NFR6: User-to-ACS-identity mapping uses a self-owned mapping table (`AcsUserIdentityMapping`), not ACS's preview-only Custom ID feature, to avoid depending on a non-stable SDK/API surface. `[AD-6]`

NFR7: Scheduled-call activation and reminder notifications carry no acting user and cannot depend on an authenticated request/hub context, since they are triggered by a clock, not a person. `[2026-07-29 AD-2]`
NFR8: The Scheduled→Active transition, the Scheduled-cancel close, and the no-show auto-close are each a single atomic conditional database write guarded by the session's current status (and, for no-show, its participant count) — never a read-then-write — so two triggers racing on the same session can't corrupt its state. `[2026-07-29 AD-3, AD-4, AD-7, inherited AD-9]`
NFR9: Scheduled-call polling reuses the existing Hangfire infrastructure already running in the API host, at the same one-minute cadence as the existing board-export jobs — no new scheduling technology is introduced. `[2026-07-29 Stack]`

### Additional Requirements (from Architecture)

- New Calls feature module mirrors the existing layered/CQRS structure exactly: `Domain/Application/Infrastructure/Persistence/API`, mirrored client-side, one file per command/query (record + handler + validator co-located). `[AD-3]`
- `Application.Calls` may depend on `IBoardRepository`/`BoardRolePermissions`; `Application.Boards` must never depend on `*.Calls`. `[AD-5]`
- New entities: `CallSession`, `CallParticipant`, `CallSessionAllowedParticipant`, `CallSessionLinkedTask`, `AcsUserIdentityMapping`. `[Structural Seed]`
- ACS Room is created before the `CallSession` DB row is persisted; on DB-persist failure, the orphaned Room is deleted (best-effort compensation). `[AD-14]`
- Pinned stack: `@azure/communication-calling` 1.43.1, `Azure.Communication.Identity` 1.3.1, `Azure.Communication.Rooms` 1.2.0, `Azure.Messaging.EventGrid` 5.0.0. `[Stack]`
- No infrastructure-as-code / starter template applies — this is a brownfield feature slice added to the existing solution, not a new project scaffold.
- **2026-07-29:** `CallSession` gains `ScheduledStartUtc`/`StartingSoonNotifiedAtUtc` (nullable `DateTime`); `StartedAtUtc` becomes nullable (null while `Scheduled`, and permanently null for a cancelled-before-start or no-show-closed session). `CallStatus` gains `Scheduled = 3`. `CallSessionHistoryDto.StartedAtUtc` (server + client) becomes nullable to match. `[2026-07-29 AD-1]`
- **2026-07-29:** New `ICallRepository` methods, all atomic guarded `ExecuteUpdate`s mirroring the existing `TryAcquirePresenterAsync` pattern, never load-then-`SaveChanges`: `TryActivateScheduledCallAsync`, `TryCloseScheduledCallAsync`, `TryCloseNoShowCallAsync`, plus board-agnostic `GetDueScheduledSessionsAsync`/`GetDueNoShowSessionsAsync` for the poller. `[2026-07-29 AD-3, AD-4, AD-7, inherited AD-9]`
- **2026-07-29:** New Hangfire recurring job (`ScheduledCallPollerJob`, `"* * * * *"`, `[DisableConcurrentExecution]`) mirroring the existing `BoardExportSchedulerJob` convention exactly. `[2026-07-29 AD-2, AD-4, AD-7]`
- **2026-07-29:** `RecordParticipantJoinedCommand` (the existing AD-12 Event-Grid webhook handler) is extended to call the same guarded activation write before recording a join, when the event lands for a session still `Scheduled` inside the join window. `[2026-07-29 AD-4]`
- **2026-07-29:** `EndCallCommand`'s existing `Active` branch also tries the no-show guarded close first, falling through to the unchanged ACS-remove-participants path only if real participants exist — fixes a related latent gap (ending a truly-empty `Active` call closed nothing today) as a side effect of the same mechanism. `[2026-07-29 AD-7]`
- **2026-07-29:** New shared eligibility helper (`CallAlertEligibility`) for alert recipients computed from persisted state (`CallSessionAllowedParticipant` + board membership) rather than a request payload — used by the reminder, activation, and reschedule alerts; `CreateCallCommand`'s existing from-request computation is unchanged. `[2026-07-29 AD-6]`
- **2026-07-29:** `ListActiveCallSessionsForBoardQuery` is renamed `ListActiveOrScheduledCallSessionsForBoardQuery` and now also returns `Scheduled` sessions. `[2026-07-29 AD-5]`

### UX Design Requirements

No UX design contract exists for this feature (no `bmad-ux` run was performed). Per the Architecture Spine's Structural Seed, the call UI is a set of new Blazor components embedded into the existing `Pages/Boards/BoardPage.razor` (session list, create/join/screen-share controls) plus a small addition to `Layout/MainLayout.razor` (the app-wide "call started" alert). No separate design tokens, component library, or accessibility audit work is in scope beyond following the existing UI's established look and components.

**2026-07-29:** same precedent, no separate UX contract for Scheduled Video Calls. The date/time picker, allow-list, and linked-task fields are additions to the existing create-call UI, not a new surface; the scheduled/starting-soon/rescheduled alerts reuse the existing app-wide alert component `MainLayout.razor` already renders for `CallStarted`.

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
| FR14, FR20, FR21 | Epic 4 |
| FR15, FR22 | Epic 4 |
| FR16, FR17, FR18, FR19 | Epic 4 |
| NFR7–NFR9 | cross-cutting, enforced across Epic 4 |
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

### Epic 4: Schedule a Board Video Call for the Future
*(Added 2026-07-29, extends the Video Calls (ACS) feature above — not part of the original three epics.)* Board members can schedule a video call for a future date/time instead of starting it immediately — with the same Title/Topic/Visibility/linked-tasks options as an immediate call — discover it on the board the same way they discover active calls, get notified when it's created and again one minute before it starts, join it starting one minute before its scheduled time, have it activate and notify automatically (from either the clock or an early joiner), have it close itself if nobody shows up within 5 minutes, and be reschedulable or cancellable by its creator or a board Owner/Admin before it ever starts. This is the complete, standalone core of the capability for the same reason Epic 1 bundled discovery and closability with creation: a scheduled call nobody could find, cancel, or that could get stuck forever would not be usable end-to-end.
**FRs covered:** FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR21, FR22
**NFRs covered:** NFR7, NFR8, NFR9

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

## Epic 4: Schedule a Board Video Call for the Future

*(Added 2026-07-29, extends the Video Calls (ACS) feature above.)* Board members can schedule a video call for a future date/time instead of starting it immediately — with the same Title/Topic/Visibility/linked-tasks options as an immediate call — discover it on the board the same way they discover active calls, get notified when it's created and again one minute before it starts, join it starting one minute before its scheduled time, have it activate and notify automatically (from either the clock or an early joiner), have it close itself if nobody shows up within 5 minutes, and be reschedulable or cancellable by its creator or a board Owner/Admin before it ever starts.

> **Sequencing note:** mirroring Epic 1's own corrected lesson (a call nobody can find or ever close isn't usable end-to-end), Story 4.1 bundles discovery, join-gating, *and* the clock-driven activation path together with scheduling itself — without activation, a scheduled call's `Status` would never correctly flip to `Active` even after real participants joined, leaving the board's own "active calls" badge and live-state UI silently wrong. Reschedule (4.3) and the one-minute reminder (4.4) are refinements layered on an already-complete core, not prerequisites for one. Recurring calls were explicitly considered and dropped for this iteration (see the architecture spine's Deferred section) — every story below is one-time scheduling only.

### Story 4.1: Schedule, discover, join, and cancel a video call for the future

As a board member,
I want to schedule a video call for a future date and time instead of starting it immediately, have other members discover it and be able to join it once it's about to start, and be able to cancel it before it ever starts,
So that I can plan calls in advance without either leaving them undiscoverable or stuck forever if my plans change.

**Acceptance Criteria:**

**Given** I am creating a call
**When** I provide a future `ScheduledStartUtc` alongside the existing Title/Topic/Visibility/linked-tasks fields
**Then** the session is persisted with `Status = Scheduled` and `StartedAtUtc = null`, and its Azure Communication Services Room is still created immediately, exactly as for an immediate call (`AD-14` unchanged)

**Given** I am creating a call
**When** I omit `ScheduledStartUtc` entirely
**Then** the call starts immediately exactly as it does today — `Status = Active`, `StartedAtUtc` set to now — with no change in behavior

**Given** a board has one or more `Scheduled` or `Active` sessions
**When** any board member loads or refreshes the board page
**Then** they see both kinds together in one list, visually distinguished by status, as a point-in-time read — no live push required

**Given** an authorized `Scheduled` session I want to join
**When** I try to join it more than one minute before its `ScheduledStartUtc`
**Then** I am denied with a clear "not started yet" error and receive no token

**Given** the same session
**When** I try to join it at or after `ScheduledStartUtc - 1 minute`
**Then** I receive a valid Azure Communication Services join token exactly as I would for an `Active` call, whether or not its `Status` has flipped to `Active` yet

**Given** the Hangfire-based poller ticks once a minute
**When** a `Scheduled` session's `ScheduledStartUtc` has passed
**Then** it flips to `Status = Active` with `StartedAtUtc` set to that moment, and the existing "call started" alert fires to eligible recipients exactly as it does for an immediate call today

**Given** I am the creator of a `Scheduled` session, or I hold the Owner or Admin role on its board
**When** I cancel it before it has ever activated
**Then** it closes immediately (`Status = Closed`, `EndedAtUtc = now`) and its Azure Communication Services Room is deleted, even though nobody ever joined it

**Given** a `Scheduled` session that a concurrent poller tick or an early joiner has already activated
**When** my cancel request for it lands right afterward
**Then** it safely falls back to ending it the normal way an `Active` call is ended, instead of deleting a Room someone is now connected to

**Given** a `Closed` session that was cancelled before it ever activated
**When** its history is later viewed
**Then** it appears with no `StartedAtUtc` and no participants, rather than causing an error

### Story 4.2: Reliable activation from an early joiner, and self-closing a call nobody ever joins

As a board member,
I want a scheduled call to activate the instant someone joins early, and to close itself automatically if nobody ever shows up,
So that joining early always works correctly and a forgotten scheduled call doesn't sit open forever.

**Acceptance Criteria:**

**Given** a `Scheduled` session within its one-minute join window
**When** a participant actually joins through Azure Communication Services before the next poller tick
**Then** the session activates (`Status = Active`, `StartedAtUtc` set) at that same moment, not just on the next tick

**Given** that same participant then leaves without anyone else having joined
**When** their departure is processed
**Then** the session closes normally through the existing participant-departure pipeline, exactly as it would for an immediate call

**Given** a `Scheduled` session that activates, by either trigger, and nobody ever joins it
**When** 5 minutes pass since it activated
**Then** it closes itself automatically (`Status = Closed`, `EndedAtUtc` set), sends no notification, and appears in call history with zero participants

**Given** an `Active` session with zero participants ever recorded
**When** its creator or a board Owner/Admin manually chooses to end it before the 5-minute timeout fires
**Then** it closes immediately through the same mechanism, rather than the existing manual-end action silently doing nothing as it would today

**Given** an immediate (non-scheduled) call that nobody ever joins
**When** time passes
**Then** it is unaffected by the 5-minute no-show timeout — that stays scoped to calls that were originally scheduled

### Story 4.3: Get notified when a scheduled call is created or its time changes

As a board member,
I want to be alerted when a call is scheduled on a board I have access to, and again if its time changes,
So that I know about it and don't plan around a stale time.

**Acceptance Criteria:**

**Given** I am an eligible board member (the same Open/Restricted+allow-list rules as today's call-started alert)
**When** someone schedules a call
**Then** I receive a real-time "call scheduled" alert naming the board, the call, and its planned start time, regardless of which page I'm on — except the creator, who isn't told about their own action

**Given** a Restricted scheduled call, and I am neither on its allow-list nor an Owner/Admin of that board
**When** it is scheduled
**Then** I do **not** receive an alert for it

**Given** I am the creator of a `Scheduled` session, or I hold the Owner or Admin role on its board
**When** I change its planned start date/time to a new value more than one minute in the future
**Then** the change is saved, its title/topic/visibility/allow-list/linked tasks are left untouched, and every eligible recipient receives a "call rescheduled" alert naming the new time

**Given** I try to reschedule to a time less than one minute in the future, or in the past
**When** I submit the change
**Then** it is rejected with a clear validation error and nothing is saved

**Given** I try to reschedule a session that has already activated or already closed
**When** I submit the change
**Then** it is rejected rather than silently doing nothing

**Given** a session's start time is changed
**When** the change is saved
**Then** any previously-scheduled "starting soon" reminder for the old time is cancelled, so it can't fire late for a time that's no longer correct

### Story 4.4: Get reminded one minute before a scheduled call starts

As a board member,
I want a reminder shortly before a scheduled call I have access to actually begins,
So that I don't miss it even if I didn't note the exact time.

**Acceptance Criteria:**

**Given** an eligible board member for a `Scheduled` session (the same Open/Restricted+allow-list rules)
**When** the session's `ScheduledStartUtc` is one minute away
**Then** they receive a real-time "starting soon" alert, regardless of which page they're on

**Given** that reminder has already been sent for a session
**When** the next poller tick runs
**Then** it is not sent again for that same occurrence

**Given** a session was rescheduled (Story 4.3) after its reminder had already been sent for the old time
**When** its new time comes due
**Then** a fresh reminder is sent for the new time — the earlier send does not suppress it

**Given** a session's `ScheduledStartUtc` is less than one minute away at the moment it is created or rescheduled
**When** the very next poller tick runs
**Then** the reminder still fires correctly despite the narrow window, and does not fire a second time once the session activates moments later
