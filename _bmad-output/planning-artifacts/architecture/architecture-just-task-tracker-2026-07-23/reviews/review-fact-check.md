---
name: 'ARCHITECTURE-SPINE Fact-Check Review — JustTaskTracker Video Calls (ACS)'
type: review
purpose: independent skeptical verification of committed technical decisions against web/reality
target: '_bmad-output/planning-artifacts/architecture/architecture-just-task-tracker-2026-07-23/ARCHITECTURE-SPINE.md'
reviewed: '2026-07-23'
---

# Fact-Check Review — ARCHITECTURE-SPINE.md (JustTaskTracker Video Calls / ACS)

Overall verdict: **PASS WITH CORRECTIONS**. Four of five claims are solidly confirmed by primary sources (Microsoft Learn, NuGet, npm, ASP.NET Core docs). One claim — the ACS "Custom ID" identity-mapping mechanism used in the create/join sequence diagram — is real, but it directly contradicts the pinned `Azure.Communication.Identity` version in the Stack table, which is a genuine internal inconsistency in the spine, not just an unverifiable detail.

---

## 1. Stack table versions

**Verdict: CONFIRMED** (as of 2026-07-23), with one important caveat feeding into Finding 5.

| Package | Spine version | Verified latest stable | Source |
| --- | --- | --- | --- |
| `@azure/communication-calling` | 1.43.1 | 1.43.1 (npm `dist-tags.latest`) | npm registry JSON (`registry.npmjs.org/@azure/communication-calling/latest`), npmjs.com package page |
| `Azure.Communication.Identity` | 1.3.1 | 1.3.1 stable (last updated 3/22/2024); 1.4.0-beta.1 prerelease exists (6/10/2025) | nuget.org/packages/Azure.Communication.Identity |
| `Azure.Communication.Rooms` | 1.2.0 | 1.2.0 (last updated 3/18/2025) | nuget.org/packages/Azure.Communication.Rooms |
| `Azure.Messaging.EventGrid` | 5.0.0 | 5.0.0 (published 6/26/2025, latest since) | nuget.org/packages/Azure.Messaging.EventGrid |

All four version numbers are accurate for "current stable" as of today. However, `Azure.Communication.Identity` 1.3.1 is the version pinned, and it does **not** include the Custom ID feature the sequence diagram relies on — see Finding 5. Consider annotating this row or cross-referencing AD-6/the sequence diagram from the Stack table so the version pin and the design decision don't silently drift apart.

---

## 2. AD-12 — ACS Event Grid emits CallParticipantAdded/Removed/CallEnded for plain Calling-SDK Rooms calls

**Verdict: CONFIRMED.** This is the strongest-evidenced claim in the review.

Source: [Azure Communication Services - events - Azure Event Grid | Microsoft Learn](https://learn.microsoft.com/en-us/azure/event-grid/communication-services-voice-video-events) (via GitHub source, `articles/event-grid/communication-services-voice-video-events.md`, last content update 2025-01-22, page refreshed 2026-05-15).

The page's own example payloads for `Microsoft.Communication.CallStarted`, `CallEnded`, `CallParticipantAdded`, and `CallParticipantRemoved` all include:
```json
"room": { "id": "{roomId}" },
"isTwoParty": false,
"isRoomsCall": true
```
i.e. Microsoft's own reference examples are explicitly Rooms-based group calls, and the `startedBy`/`user` identifiers are plain `communicationUser` identities — no `callConnectionId`/Call Automation fields appear anywhere in these four events (Call Automation has its own distinct event vocabulary: `CallConnected`, `AddParticipantSucceeded`, etc., not used here). This confirms the events are emitted for plain Calling-SDK group calls over Rooms, not only Call Automation-controlled calls — directly supporting AD-12's load-bearing assumption.

One relevant limitation from the same page's "Limitations" section, not currently called out in the spine: *"Aside from `IncomingCall`, Calling events are only available for Azure Communication Services VoIP users. PSTN, bots, echo bot, and Teams users events are excluded. No calling events are available for ... Teams meeting interop call."* Not a problem for this project (pure ACS VoIP users, no PSTN/Teams interop), but worth a one-line note in AD-12 for future-proofing.

---

## 3. AD-10 — Blazor WASM `AddScoped` behaves as one app-lifetime instance

**Verdict: CONFIRMED.**

Blazor WebAssembly creates exactly one root DI container per browser tab/app instance, with no per-request scope (unlike server-side ASP.NET Core). Scoped and Singleton registrations are therefore functionally identical in WASM — one instance for the lifetime of that browser tab, shared across every component that injects it, and torn down only when the tab/app is closed. Sources: Microsoft Learn / community write-ups incl. Blazor School, Blazor University, Thinktecture, and corroborating dotnet/aspnetcore GitHub issue discussion confirming this is documented, intentional behavior, not a knowledge-cutoff assumption.

Implication for the design: a single `AddScoped` `BoardActionsHub`/SignalR connection service in the Blazor client is indeed one persistent connection per browser tab for the app's session — the "no new SignalR connection needed" justification holds. (Caveat, not a correctness issue: if the user opens multiple browser tabs, each tab gets its *own* scope/instance and its own hub connection — `Clients.User(...)` correctly fans out to all of them per Finding 4, so this doesn't break the design, just worth knowing it's "per tab" not "per user session" instance.)

---

## 4. `IUserIdProvider` + `Clients.User(userId)` as the standard mechanism to target a user across all connections

**Verdict: CONFIRMED.**

Source: [SignalR authentication and authorization | Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz) (moniker range covers aspnetcore-6.0 through 11.0; page updated 2026-06-26/2026-07-22). Direct quotes:
- *"Authentication allows the hub to call methods on all connections associated with a user... Multiple connections can be associated with a single user."*
- *"IUserIdProvider is used by `User(String)` to invoke connections associated with a user."*
- Official worked examples register a custom `IUserIdProvider` (`NameUserIdProvider`/`EmailBasedUserIdProvider`) via `AddSingleton<IUserIdProvider, T>()` and derive the ID from a claim — exactly the pattern AD-10 describes (claim-derived ID, here `AzureAdObjectId` instead of name/email).
- The docs' own warning ("the value you choose must be unique among all users in your system") is satisfied since `AzureAdObjectId` is already the app's stable per-user unique claim.

This is the documented, intended mechanism — not a workaround. `Clients.User(userId)` fans out to every active connection tied to that user ID and is a safe no-op if the user has none, matching AD-10's "no extra bookkeeping needed" claim.

---

## 5. ACS Rooms/Identity "Custom ID" deterministic mapping

**Verdict: CONFIRMED as a real feature, but NEEDS CORRECTION against the Stack table — this is the one substantive issue found.**

The feature exists and works as described: creating an ACS identity with a given `customId` deterministically returns the same ACS communication-user identity on every subsequent call with that same `customId` — confirmed by Microsoft's own quickstart sample ("Access Tokens with Custom Id with Azure Communication Services"), whose expected output explicitly demonstrates "User ID (second call)" matching the first, with the comment *"Validation successful: Both identities have the same ID as expected."* This is exactly the mechanism AD's create/join sequence diagram invokes ("issue token (Custom ID = UserId)").

**The correction:** this feature is **preview-only**, requiring:
- `Azure.Communication.Identity` SDK version **1.4.0-beta.1** (a prerelease, published 2025-06-10) — not the stable 1.3.1 pinned in the spine's Stack table (published 2024-03-22, over a year *before* Custom ID existed).
- REST API version `2025-03-02-preview`.

As of this review (2026-07-23), there is still no stable/GA release of `Azure.Communication.Identity` beyond 1.3.1 — Custom ID has been sitting in beta for over a year with no promotion to GA visible on NuGet.

**Why this matters (load-bearing):** the Stack table and the sequence diagram currently contradict each other. As written, the architecture cannot be implemented on the pinned stable SDK — either:
1. Bump `Azure.Communication.Identity` to `1.4.0-beta.1` and accept taking a prerelease/preview dependency (and the preview REST API version) into a real feature — a decision that deserves its own explicit AD given the project's general preference for stable, pinned versions elsewhere in this same table, or
2. Drop Custom ID and build the "map app UserId → ACS identity" table the project would otherwise have needed anyway (store `AcsUserId` per app user, created once via a plain, non-custom `CommunicationIdentityClient.CreateUserAsync()`), which works fine on the stable 1.3.1 SDK and was in fact the standard pre-Custom-ID pattern.

Recommend the architecture explicitly pick one of these two paths rather than leaving Custom ID referenced only in a sequence-diagram annotation while the Stack table pins a version that doesn't support it.

---

## Sources consulted

- npm: https://registry.npmjs.org/@azure/communication-calling/latest , https://www.npmjs.com/package/@azure/communication-calling
- NuGet: https://www.nuget.org/packages/Azure.Communication.Identity , https://www.nuget.org/packages/Azure.Communication.Identity/1.4.0-beta.1 , https://www.nuget.org/packages/Azure.Communication.Rooms , https://www.nuget.org/packages/Azure.Messaging.EventGrid
- Microsoft Learn / Event Grid: https://learn.microsoft.com/en-us/azure/event-grid/communication-services-voice-video-events (and GitHub source at MicrosoftDocs/azure-docs)
- Microsoft Learn / SignalR: https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz
- Microsoft Learn / ACS identity model: https://github.com/MicrosoftDocs/azure-docs/blob/main/articles/communication-services/concepts/identity-model.md
- Microsoft sample: https://learn.microsoft.com/en-us/samples/azure-samples/communication-services-dotnet-quickstarts/access-tokens-with-custom-id-with-azure-communication-services/
- Community/background on Blazor WASM DI scoping: Blazor School, Blazor University, Thinktecture AG write-ups; dotnet/aspnetcore GitHub issue discussion corroborating single-root-scope behavior.
