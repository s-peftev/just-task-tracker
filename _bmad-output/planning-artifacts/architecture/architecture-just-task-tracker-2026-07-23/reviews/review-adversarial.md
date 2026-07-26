---
name: 'Adversarial Review — JustTaskTracker Video Calls (ACS) Architecture Spine'
type: review
target: '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-23/ARCHITECTURE-SPINE.md'
reviewer-role: adversarial ("two units one level down")
date: '2026-07-23'
---

# Adversarial Review — Video Calls Architecture Spine

## Method

For each AD, I constructed a pair of implementers (developer or coding agent A and B) who each
read the AD literally, comply with it fully, and build a piece the spine assigns to them
(`CreateCallCommand`/`JoinCallCommand`/webhook handlers/screen-share commands/etc.) without
coordinating on anything the spine itself doesn't pin down. Where two compliant readings produce
incompatible runtime behavior, or where a rule's own worked example (the sequence diagrams)
contradicts the rule's stated ownership, that's a hole.

## Overall verdict

**Not yet safe to parallelize.** The spine is strong on transport/layering/authorization
(AD-1 through AD-7 are unambiguous and well-bound), but the call-lifecycle and session-state
invariants (AD-8, AD-9, AD-12) each leave at least one load-bearing mechanism unspecified. Two of
the gaps (presenter-clear-on-leave, Event-Grid-to-CallSession correlation) are severe enough that
independently-compliant implementations will not just diverge in style but will produce a
**broken feature** (permanently locked screen-share, or a webhook that cannot route events to the
right session at all once a board has >1 concurrent call). None of these are fixed by "just
follow the sequence diagrams" — the diagrams themselves pick one interpretation silently, which is
worse than not picking one, because it hides the ambiguity from a reviewer skimming AD text only.

---

## Finding 1 (Critical) — AD-9 never defines who clears `CurrentPresenterUserId` when the presenter leaves without stopping share

**AD-9 text:** "`CallSession.CurrentPresenterUserId` is set/cleared via `RequestScreenShareCommand`/`StopScreenShareCommand`... checked and written before the client invokes the ACS SDK's local start-screen-share."

**The gap:** AD-9 names exactly two commands as the write path. AD-12 assigns *all* writes to
`CallParticipant`/`CallSession` state that result from a departure (join/leave/close) exclusively
to the Event-Grid-driven `RecordParticipantLeftCommand`/`CloseCallCommand`. Neither AD says what
happens to `CurrentPresenterUserId` when the presenter's participant record gets a `LeftAtUtc` via
the Event Grid path (crash, network drop, or just closing the browser tab without hitting "stop
share").

**Two compliant-but-incompatible builds:**
- **Developer A** (owns `RequestScreenShareCommand`/`StopScreenShareCommand`, reading AD-9
  narrowly): treats `CurrentPresenterUserId` as *exclusively* those two commands' concern, per the
  literal text ("set/cleared via" names only these two). Does nothing about it elsewhere.
- **Developer B** (owns `RecordParticipantLeftCommand`, reading AD-12's "exclusive lifecycle
  authority" broadly): adds "if the departing participant is the current presenter, clear
  `CurrentPresenterUserId`" inside the webhook-driven handler, because leaving *is* part of the
  lifecycle AD-12 assigns to them.

Both are individually defensible readings of their own AD. If the team ends up with A's
implementation only (very plausible — it's the more literal reading of AD-9, and the dev owning
`RequestScreenShareCommand` has no reason to go touch the Internal/webhook commands), the result
is: presenter disconnects ungracefully → `CurrentPresenterUserId` stays pinned to a user no longer
in the call → `RequestScreenShareCommand`'s "checked and written before ACS start" guard (implicitly:
only proceed if presenter is null/self) permanently blocks every other participant from ever
screen-sharing again for the life of that session. This is a full feature deadlock, not a
cosmetic bug, and it's fully spec-compliant on both sides.

**Fix direction:** AD-9 needs an explicit third trigger: "`CurrentPresenterUserId` MUST also be
cleared by `RecordParticipantLeftCommand` (AD-12) when the departing participant is the current
presenter." Name the owning command.

---

## Finding 2 (Critical) — AD-8 + AD-12 never specify how an Event Grid event is correlated back to a specific `CallSession` when a board has multiple concurrent Rooms

**AD-8:** each `CallSession` gets its own fresh ACS Room. **AD-12:** Event Grid
`CallParticipantAdded`/`Removed`/`CallEnded` events are the *exclusive* source of truth for
join/leave/close, delivered to one shared `POST /calls/acs-events` endpoint for the whole app.

**The gap:** the spine never states which field in the Event Grid payload the webhook uses to look
up the target `CallSession` row (i.e., to know *which* of a board's several concurrently-open Rooms
a given `CallParticipantAdded` belongs to), nor does it guarantee `CallSession.AcsRoomId` is even
the identifier ACS's Calling-SDK-driven Event Grid events surface (Rooms-scoped group calls are
commonly correlated by `serverCallId`/call id, not necessarily the Room resource id used to create
the room). The underlying research doc this spine cites (`technical-webrtc-group-calls-...md`)
never resolves this either — it only says "Event Grid delivers call-state events," never how they
key back to an app-level session. AD-8's whole value proposition (many concurrent independent
rooms per board) depends on this correlation existing and being reliable; right now it's assumed,
not specified.

**Two compliant-but-incompatible builds:**
- **Developer A** (owns `CreateCallCommand`/`AcsCallProvisioningService`, AD-6/AD-8): persists
  whatever identifier `Rooms.CreateRoomAsync` returns as `CallSession.AcsRoomId`, assuming that's
  what will show up in the event payload.
- **Developer B** (owns `CallsWebhookController`/`RecordParticipantJoinedCommand`, AD-11/AD-12):
  deserializes the Event Grid payload and matches on whatever field is actually present at
  runtime (e.g. a `serverCallId`/`groupId` the Calling SDK generates client-side at join time, which
  may have no stored counterpart on `CallSession` at all).

If those two identifiers are not the same value — a real possibility given Rooms + Calling SDK is
a two-object model (Room resource vs. the call/thread that happens inside it) — the webhook simply
cannot resolve an incoming event to a `CallSession` once there is more than one active session on
a board (with exactly one active session it might accidentally work by "closes the only Active
row for that board," which would mask the bug until the multi-session case AD-8 explicitly exists
to support is actually exercised).

**Fix direction:** Add a rule (or extend AD-12) that names the exact Event Grid payload field used
for correlation, confirms it's populated for Rooms-scoped Calling SDK sessions (not just Call
Automation), and states what `CallSession` column stores the matching value (may not be
`AcsRoomId` as currently modeled — may need a second correlation column, e.g. `AcsServerCallId`,
populated the first time a participant-added event arrives, or captured at join time from the
client).

---

## Finding 3 (High) — No defined ordering/compensation for ACS Room creation vs. `CallSession` persistence in `CreateCallCommand`

**The gap:** AD-8 says a Room is "created at session-creation time," and the sequence diagram shows
`API->>ACS: create Room` *then* `API->>API: persist CallSession`. But this is only the diagram's
chosen order — no AD states it as a rule, and no AD defines the compensating action for either
failure mode: ACS Room create succeeds but the DB `SaveChangesAsync` fails (orphaned billable ACS
Room, never referenced by any row, never cleaned up — AD-7/AD-11 say nothing about Room garbage
collection), or DB persist succeeds first and the ACS call then fails (a `CallSession` row with no
`AcsRoomId`, `Status` presumably `Active`, that nothing can ever actually join).

**Two compliant-but-incompatible builds:**
- **Developer A** builds exactly what the diagram shows (Room-first), and on DB failure does
  nothing special — leaves the orphaned Room, since no AD requires cleanup.
- **Developer B**, reading AD-3 ("mirrors the existing... convention") and seeing existing
  `Application.Boards` commands are simple "persist, `SaveChangesAsync`, done" patterns, persists
  the `CallSession` row first (with a placeholder/null `AcsRoomId`) inside the same
  `IUnitOfWork` transaction convention, then calls ACS and updates the row — a legitimate reading
  of "matches `UpdateBoardCommand.cs`" (AD-3), which has no external side-effect to sequence
  against.

Neither ordering is wrong by the letter of any AD, but they produce different failure signatures
(dangling ACS resource vs. a `CallSession` that's `Active` in the DB, visible in
`ListActiveCallSessionsForBoardQuery`, but has no working Room) — and if different devs build
`CreateCallCommand` at different times (spine's own stated risk: built by different
developers/agents at different times), whichever one lands first quietly sets the failure-mode
contract for the whole feature with no review signal that a decision was even made.

**Fix direction:** AD-8 (or a new AD) should state the required order explicitly and the
compensating action: e.g. "create the ACS Room first; if `SaveChangesAsync` fails afterward, the
handler must delete the just-created Room before returning failure" (or the reverse, with an
explicit reconciliation sweep for rows with null `AcsRoomId`). Either is fine — silence is not.

---

## Finding 4 (Medium) — No AD defines who may force-end a `CallSession` early, or whether `CreatedByUserId` carries any special authority

**The gap:** AD-12 is written as a strong, exclusive-authority rule ("the only source that writes...
closes a `CallSession`... the client's 'leave' button... does not itself call an API command"). Taken
literally, this forecloses *any* explicit "end call now" action — the only way a session closes is
the last participant's ACS departure propagating through Event Grid. But `CallSession` already
carries `CreatedByUserId` in the schema (Structural Seed), which strongly implies some intended
creator/admin authority that is never actually granted anywhere. There's no `EndCallCommand`, and
no statement of whether one is permitted, and if permitted, how it's reconciled with AD-12's
"exclusive" wording (does it forcibly remove all ACS participants and let Event Grid observe the
resulting `CallEnded`, i.e. go through the same authoritative path — or does it write `Status`
directly, contradicting AD-12?).

**Two compliant-but-incompatible builds:**
- **Developer A**, reading AD-12 literally, builds no early-termination path at all — closing only
  ever happens via last-participant-leaves.
- **Developer B**, asked by product/UX for a "creator can end the call for everyone" affordance
  (a reasonable ask the PRD/UX layer may well specify, board-owner-style), implements
  `EndCallCommand` that sets `Status = Closed`/`EndedAtUtc = now` directly from the API layer —
  fully violating AD-12's "exclusive" rule, but with no AD telling them that rule extends to this
  case, since AD-12's text and binds line only mention join/leave races and crash/drop
  self-healing, never early creator-initiated termination.

Both are plausible outcomes of the current text; one silently breaks AD-12's single-writer
invariant the moment product asks for the obviously-expected "end call" button.

**Fix direction:** Add a rule: is there a creator/admin-initiated close, and if so does it (a) call
ACS to remove all participants and let Event Grid's `CallEnded` be the actual writer (compliant,
slower/eventually-consistent), or (b) get a narrow, explicitly-named exception to AD-12's exclusivity
for this one command. Either answer is acceptable; the current spine gives neither.

---

## Finding 5 (Medium) — `CallSessionAllowedParticipant` mutability and creator self-inclusion are unspecified

**The gap:** AD-8 says the allow-list is "chosen at creation." It never says whether it's mutable
afterward (add/remove a participant mid-session), and never says whether the creator is implicitly
in their own allow-list or must be included in the `AllowedUserIds` payload like everyone else.

**Two compliant-but-incompatible builds:**
- **Developer A** (client/`CreateCallCommand` payload contract) assumes the caller's own UI always
  includes the creator in `AllowedUserIds`, so the handler inserts exactly what's given.
- **Developer B** (handler-side, reading AD-4's "the caller is in that session's
  `CallSessionAllowedParticipant` list" as the *sole* gate) writes the handler to auto-insert
  `CreatedByUserId` into the allow-list server-side regardless of payload, because otherwise the
  creator could get locked out of rejoining their own restricted call after a drop — a defensible
  safety reading nothing in AD-4/AD-8 rules out or requires.

If A's client omits the creator (reasonable — "why would I need to allow myself") and B's handler
doesn't auto-include, the creator is silently locked out of their own restricted session on
reconnect. Low-frequency but user-visible, and entirely a product of two teams both being "in
spec." Separately: whether the allow-list can be edited after creation (e.g. an admin adding a
latecomer to a Restricted session) is simply undefined — one dev might build an
`UpdateAllowedParticipantsCommand` believing it's obviously in scope of "membership" (AD-4's bind
list), another might treat the list as immutable per AD-8's "chosen at creation" wording.

**Fix direction:** State explicitly whether the creator is auto-included, and whether the allow-list
is mutable post-creation (and if so, name the command and who may call it).

---

## Other minor observations (not full findings)

- AD-10's rule text says the cross-page alert is for "call *started*" only; the diagram and the
  capability map both also route "closed" through the same `Clients.User(...)` path. Harmless if
  intentional, but the rule prose and the diagram disagree on scope — worth tightening wording so
  two notifier implementers don't diverge on whether close-alerts are user-targeted or group-only.
- Dual token-issuance entry points (`CreateCallCommand` returns a Room+token directly to the
  creator per the sequence diagram, while `JoinCallCommand` presumably does the equivalent for
  everyone else) are never unified under one explicitly-named "issue my access token" step — both
  happen to call into `IAcsCallProvisioningService` per AD-6, which likely prevents real drift, but
  the spine never states that the creator's own join *is* effectively a first call to the same
  join path, which is worth making explicit to avoid a duplicated/divergent token-scope
  implementation between the two commands.
