---
stepsCompleted: [1, 2, 3, 4, 5, 6]
inputDocuments: []
workflowType: 'research'
lastStep: 1
research_type: 'technical'
research_topic: 'WebRTC group video calls in JustTaskTracker: SignalR vs Azure Communication Services architecture'
research_goals: 'Determine the best architecture for board-scoped group WebRTC video calls with single-presenter screen sharing in a .NET Aspire (Blazor WASM + ASP.NET Core API) app, comparing self-hosted SignalR signaling (mesh/SFU), Azure Communication Services Calling SDK, and hybrid approaches — weighing cost, complexity, Blazor WASM integration, scaling limits, and compatibility with a future roadmap of scheduled meetings and calendar views.'
user_name: 'Stan'
date: '2026-07-23'
web_research_enabled: true
source_verification: true
---

# Research Report: technical

**Date:** 2026-07-23
**Author:** Stan
**Research Type:** technical

---

## Research Overview

This research compares three architectural paths for adding board-scoped group WebRTC video calls with single-presenter screen sharing to JustTaskTracker (.NET Aspire: Blazor WASM + ASP.NET Core API + SQL Server): self-hosted signaling via SignalR (mesh or SFU), Azure Communication Services (ACS) Calling SDK, and a SignalR/ACS hybrid. Given the project's existing Azure commitment and a deferred (but architecturally-anticipated) roadmap of scheduled meetings and calendar views, the research evaluated technology stack fit, integration patterns, architecture, and implementation practicality. **Recommendation: Azure Communication Services**, with the existing SignalR hub retained for app-level presence/state relay only. Full rationale, trade-off comparison, and implementation roadmap are in the [Research Synthesis](#research-synthesis) section below.

---

<!-- Content will be appended sequentially through research workflow steps -->

## Technical Research Scope Confirmation

**Research Topic:** WebRTC group video calls in JustTaskTracker: SignalR vs Azure Communication Services architecture

**Research Goals:** Determine the best architecture for board-scoped group WebRTC video calls with single-presenter screen sharing in a .NET Aspire (Blazor WASM + ASP.NET Core API) app, comparing self-hosted SignalR signaling (mesh/SFU), Azure Communication Services Calling SDK, and hybrid approaches — weighing cost, complexity, Blazor WASM integration, scaling limits, and compatibility with a future roadmap of scheduled meetings and calendar views.

**Technical Research Scope:**

- Architecture Analysis - design patterns, frameworks, system architecture
- Implementation Approaches - development methodologies, coding patterns
- Technology Stack - languages, frameworks, tools, platforms
- Integration Patterns - APIs, protocols, interoperability
- Performance Considerations - scalability, optimization, patterns

**Research Methodology:**

- Current web data with rigorous source verification
- Multi-source validation for critical technical claims
- Confidence level framework for uncertain information
- Comprehensive technical coverage with architecture-specific insights

**Scope Confirmed:** 2026-07-23

---

## Technology Stack Analysis

### Signaling Approaches in .NET (Self-Hosted SignalR)

SignalR is a proven signaling transport for WebRTC in .NET: it exchanges SDP offers/answers and ICE candidates between clients over hubs, and SignalR **groups** map naturally onto "one board = one call room" — clients `JoinGroup(boardId)` and the hub relays offers/answers/ICE candidates via `Clients.OthersInGroup`.

_Maturity: proven pattern, not a packaged product._ Reference implementations exist (`Shhzdmrz/SignalRCoreWebRTC`, `aykay76/blazorcam`) but they are samples/starting points, not production SDKs — the app team owns the hub logic, reconnection handling, and room lifecycle.

_Scaling constraint:_ a single SignalR server holds group membership in memory. Scaling signaling beyond one instance requires a Redis (or Azure SignalR Service) backplane to keep group membership synchronized across nodes.

_Source: [How WebRTC, SignalR, and .NET Work Together](https://medium.com/@aayushadhikari601/how-webrtc-signalr-and-net-work-together-to-enable-real-time-video-calls-0182514a4e30), [MDN: Signaling and video calling](https://developer.mozilla.org/en-US/docs/Web/API/WebRTC_API/Signaling_and_video_calling), [SignalRCoreWebRTC](https://github.com/Shhzdmrz/SignalRCoreWebRTC)_

### Azure Communication Services (ACS) Calling SDK

ACS Calling is a fully managed calling service (signaling + media routing + screen share) exposed via .NET, JavaScript, Java, and Objective-C SDKs — no signaling server or media server to build or operate.

_Pricing:_ **$0.004 per participant per minute**, billed to millisecond precision, and the rate is identical whether the participant is on audio, video, or **screen sharing** — no separate screen-share surcharge. No data egress charge.

_Rooms API (GA):_ ACS **Rooms** are server-managed call containers with a `validFrom`/`validUntil` time window and explicit participant membership — the call only accepts connections inside that window. This gives "who can join" and "when" control out of the box, which is directly the shape of the deferred roadmap items (scheduled meetings, calendar-visible sessions) — Rooms effectively **is** a scheduling primitive, not something to bolt on later.

_Source: [ACS Pricing scenarios](https://learn.microsoft.com/en-us/azure/communication-services/concepts/pricing), [Azure Communication Services pricing](https://azure.microsoft.com/en-us/pricing/details/communication-services/), [Rooms API for structured meetings](https://learn.microsoft.com/en-us/azure/communication-services/concepts/rooms/room-concept), [Virtual Rooms GA announcement](https://techcommunity.microsoft.com/blog/azurecommunicationservicesblog/azure-communication-services-virtual-rooms-is-now-generally-available/3845412)_

### Blazor WebAssembly Integration Path

Blazor has **no first-party native WebRTC or ACS support** — every option routes browser media APIs through JS interop:

- `ParrhesiaJoe/BlazorRtc` — sample pattern: code-behind JS file + SignalR signaling, direct DOM `<video>` element control (avoids marshalling media objects through Blazor interop, which is the main pain point).
- `SpawnDev.BlazorJS.PeerJS` — strongly-typed Blazor wrapper around PeerJS (mesh-oriented, not a fit for our SFU/managed direction).
- `Soenneker.Telnyx.Blazor.WebRtc` — shows the general shape of a *vendor-SDK* Blazor wrapper (typed wrappers + event bridging over a JS calling SDK); no equivalent first-party or community Blazor wrapper was found specifically for the **ACS Calling JS SDK**, so integrating ACS from Blazor WASM means writing a thin custom JS-interop wrapper of similar shape ourselves.

_Practical implication:_ neither path is "native Blazor" — both self-hosted WebRTC and ACS require a JS interop layer. The self-hosted route needs interop code we write and maintain for raw `RTCPeerConnection`; the ACS route needs a comparable (likely smaller) interop layer around Microsoft's JS Calling SDK, which already handles the actual peer connection/media complexity for us.

_Source: [BlazorRtc](https://github.com/ParrhesiaJoe/BlazorRtc), [SpawnDev.BlazorJS.PeerJS](https://github.com/LostBeard/SpawnDev.BlazorJS.PeerJS), [Soenneker.Telnyx.Blazor.WebRtc](https://github.com/soenneker/soenneker.telnyx.blazor.webrtc)_

### WebRTC Topology & Scaling (Mesh vs SFU vs ACS-Managed)

- **Mesh (full P2P):** recommended only for 2-4 participants (extendable to ~6-10 with degraded 100-300ms latency); bandwidth/CPU per client scales as N-1 up/down streams. Not viable for an open "any board member can join" room without a hard participant cap.
- **Self-hosted SFU:** a single SFU node comfortably handles under 100 participants; 100-1K needs simulcast plus receive-only viewers; 1K-10K needs cascading SFUs across regions. This is real infrastructure to stand up and operate (e.g., mediasoup/Janus/LiveKit) — servers, TURN/STUN, monitoring, scaling policy.
- **ACS-managed media:** Microsoft operates the equivalent of the SFU tier — group call media routing and scaling are handled inside the service; the team only calls the SDK.

_Source: [Mesh vs SFU vs MCU](https://antmedia.io/webrtc-network-topology/), [WebRTC Architecture Explained: P2P vs SFU vs MCU vs XDN](https://www.red5.net/blog/webrtc-architecture-p2p-sfu-mcu-xdn/), [WebRTC Architecture for Production](https://www.forasoft.com/learn/webrtc-architecture-production-systems)_

### Cloud Platform Fit (Azure Commitment)

Given the stated preference to build on Azure, ACS sits on the same subscription/billing plane as the rest of the Aspire deployment, and its Rooms API already anticipates the deferred roadmap (scheduled meetings, time-boxed calendar sessions) that a hand-rolled SignalR + mesh/SFU stack would otherwise have to design and build from scratch.

---

## Integration Patterns Analysis

### Identity & Token Issuance

ACS requires a **trusted backend** to mint short-lived user access tokens — the ACS connection string/key must never reach the Blazor client. The API service calls the **Azure Communication Identity SDK** to create/reuse an ACS identity and issue a token (validity configurable **60–1440 minutes**, default 1440).

_Custom ID mapping:_ rather than maintaining a separate `AppUserId → AcsIdentityId` table, ACS's **Custom ID** capability lets the API pass the app's own user ID (e.g. the JustTaskTracker user GUID) as `customId` when creating the identity, and ACS deterministically maps it to the same ACS identity on every subsequent call — no custom mapping storage needed.

_Secrets handling:_ the ACS connection string belongs in Key Vault (or Aspire's secret configuration), read only by the API; the Blazor client only ever receives the short-lived token, scoped to one call session.

_Source: [Create and manage access tokens](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/identity/access-tokens), [Authenticate to ACS](https://learn.microsoft.com/en-us/azure/communication-services/concepts/authentication), [Identities with Custom ID](https://techcommunity.microsoft.com/blog/azurecommunicationservicesblog/identities-with-custom-id-a-simpler-smarter-way-to-manage-communication-users/4435083), [Credentials best practices](https://learn.microsoft.com/en-us/azure/communication-services/concepts/credentials-best-practices)_

### Board ↔ Room Mapping

The natural integration shape: **one Board = one ACS Room**, created (or reused) by the API when the "start video" action fires. The API remains the **authorization gate** — it checks board membership before issuing a token or adding the caller as a Room participant, so ACS never has to know about the app's board/permission model. Room `validFrom`/`validUntil` is left open-ended for the MVP (join-anytime) but is exactly the field that will carry the deferred scheduled-meeting dates later, with no new ACS-side concept required.

### Event-Driven Integration (Call Lifecycle → App UI)

ACS publishes call/participant lifecycle events (call started/ended, participant joined/left, recording, etc.) through **Azure Event Grid**, delivered via webhook (or natively to Azure Functions/Logic Apps). The API can subscribe an Event Grid webhook endpoint to react to these events — e.g. mark the board's "call in progress" indicator live for all board viewers.

_Practical pattern for this app:_ keep **SignalR for app-level, low-latency UI state** (who's in the call, board presence, "call started" banners) fed by the Event Grid webhook, while **ACS owns the actual media plane**. This is a clean separation, not a competing choice with the "SignalR vs ACS" question from the earlier section — SignalR remains useful as the app's general real-time transport even when ACS handles the call media itself.

_Security note:_ Call Automation-style mid-call webhooks support signature validation for securing the callback endpoint — the same discipline should apply to the Event Grid webhook subscription.

_Source: [ACS as an Event Grid source](https://learn.microsoft.com/en-us/azure/event-grid/event-schema-communication-services), [Subscribe to events](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/events/subscribe-to-events), [Securing webhook endpoint](https://learn.microsoft.com/en-us/azure/communication-services/how-tos/call-automation/secure-webhook-endpoint)_

### Client-Server Call Flow (Summary)

1. User clicks "Start video" on a Board → Blazor client calls API.
2. API verifies board membership → creates/reuses ACS Room for that board → issues (or reuses, via Custom ID) an ACS access token for the calling user, scoped to that Room.
3. Blazor client passes the token to the ACS Calling JS SDK (via the JS-interop wrapper) → joins the Room, video/audio/screen-share flow entirely through ACS's managed media path.
4. Event Grid delivers call-state events back to the API → API relays relevant state to all board viewers over the existing SignalR hub (presence/banner only, not media).

_Source: [Client and server architecture (trusted auth sample)](https://github.com/Azure-Samples/communication-services-authentication-hero-csharp/blob/main/docs/design-guides/architecture-overview.md)_

---

## Architectural Patterns and Design

### System Architecture Fit

No new microservice is warranted for the MVP: video calling is a **bounded module inside the existing ASP.NET Core API** (a "Calls" feature area alongside Boards/Tasks), calling out to ACS for media and Event Grid for state — not a separately deployed service. ACS itself already absorbs the part of the system that would traditionally justify a dedicated media/service split (the SFU tier), so splitting further would add deployment/ops overhead without a corresponding benefit at this scale.

### Data Architecture

A minimal new aggregate is enough — no event sourcing or CQRS needed for what is fundamentally a status + timestamps record:

- **CallSession**: `BoardId`, `AcsRoomId`, `Status` (e.g. NotStarted/Active/Ended), `StartedAt`, `EndedAt`.
- **CallParticipant**: `CallSessionId`, `UserId`, `JoinedAt`, `LeftAt` — join/leave audit, also usable later for "who attended" history.

Deliberately deferred, not modeled now: `ScheduledStart`/`ScheduledEnd` fields on `CallSession` and any calendar-read model — but the shape above extends cleanly to carry them later (mirroring ACS Room's own `validFrom`/`validUntil`), so today's schema isn't a dead end for the roadmap items.

### Deployment & Operations Architecture

**Not a gap in this project's context:** .NET Aspire has no first-party hosting integration for Azure Communication Services (open community feature request, not shipped as of this research) — but this doesn't matter here. In JustTaskTracker, **Aspire is a local-dev composer only** and is not part of the prod/CI-CD deployment path. The project already follows this exact pattern for **Azure SignalR Service**: it is not modeled as an Aspire resource either — the backend simply connects to it via a connection string from configuration. ACS integration follows the same established convention: connection string in configuration/Key Vault, consumed directly by the API, no Aspire resource needed or expected.

**Region/data residency:** an ACS resource is provisioned at the **geography** level (not a specific datacenter) — Communication Services internally chooses the datacenter, though data may transit other geographies. Practical guidance: pick the geography closest to the primary user base to minimize latency, and confirm it satisfies any data-residency requirement the team has (none stated so far for this project).

### Security Architecture

Recap and reinforcement of the integration-pattern findings: token issuance is server-side only, board-membership authorization happens in the API before a Room/token is granted, and the ACS connection string lives in Key Vault/Aspire secrets — never in client-reachable config. No additional security architecture (mTLS, custom gateway, etc.) is warranted beyond what the existing API already does for other endpoints, since ACS terminates the actual media security (DTLS-SRTP) itself.

### Scalability Considerations

Because ACS owns the media-routing tier, this app's own scaling surface is limited to what it already has to scale for other features: the ASP.NET Core API/SQL Server persisting `CallSession`/`CallParticipant` rows, and SignalR fanout for presence/banners. Neither grows meaningfully with call size — the expensive part (participant media fan-out) is Microsoft's problem, not this app's.

_Source: [Data residency and user privacy for ACS](https://learn.microsoft.com/en-us/azure/communication-services/concepts/privacy), [Aspire Azure integrations overview](https://aspire.dev/integrations/cloud/azure/overview/), [Add Azure Communication Services (Aspire feature request)](https://github.com/dotnet/aspire/issues/2802)_

---

## Implementation Approaches and Technology Adoption

### Technology Adoption Strategy

Adopt directly, no abstraction layer: since the provider decision (ACS) is already made and the app isn't hedging across multiple calling vendors, building a provider-agnostic abstraction now would be speculative complexity. Roll out incrementally by scope already agreed (group call + single-presenter screen share only); the deferred scheduling/calendar items slot into the same `CallSession`/Room model later rather than requiring a re-platform.

### Development Workflow and Tooling

**UI Library caveat:** Microsoft ships `@azure/communication-react` — a **React** component library (composites + building blocks on FluentUI) that would normally accelerate calling UI development. It does **not** apply directly here since the client is **Blazor WebAssembly, not React** — mounting a React island inside Blazor purely to reuse the composite UI would add more complexity than it saves. The practical path is to interop directly against the lower-level `@azure/communication-calling` JS SDK and build the call UI (tiles, mute/camera/screen-share controls) as native Blazor components, consistent with the JS-interop pattern already surveyed for Blazor+WebRTC in the technology stack section.

### Testing and Quality Assurance

- **Browser support check:** the Calling SDK exposes `getEnvironmentInfo()` / `isSupportedBrowser` — call this before offering the "join call" button so unsupported browsers get a clear message rather than a silent failure.
- **Pre-call diagnostics:** SDK v1.9.1-beta.1+ includes pre-call diagnostics (mic/camera/network checks) — worth wiring into the "start/join call" flow for the board.
- **HTTPS requirement:** the Web Calling SDK requires HTTPS (or `localhost`/`file:` for local dev) — already satisfied, since the existing webui already runs on `https://localhost:7108` per the project's README.
- **No official local emulator:** unlike SQL/Storage, there's no ACS emulator — development and testing happen against a real (low-cost, pay-as-you-go) ACS resource, e.g. a dedicated dev-tier resource.

### Deployment and Operations Practices

No new deployment infrastructure: the ACS connection string is configured per environment (dev/staging/prod) exactly like the existing Azure SignalR Service connection string — a config/secret value, not an Aspire-managed resource, consistent with how this project already treats external Azure services.

### Team Organization and Skills

The skillset needed is **JS interop and calling-SDK integration**, not WebRTC/media engineering — ACS deliberately absorbs the signaling/media/SFU complexity that would otherwise require dedicated real-time media expertise (relevant given team size/seniority on this project).

### Cost Optimization and Resource Management

Cost is usage-based, not infrastructure-based: **$0.004 per participant-minute**, identical for audio/video/screen-share, no data egress charge — cost scales with actual call activity rather than servers to provision or idle capacity to pay for. Since free-tier/trial allowances change over time, confirm current figures on the official pricing page before budgeting. Recommend tracking spend via Azure Cost Management once usage begins, tagged to the ACS resource.

### Risk Assessment and Mitigation

- **Vendor lock-in to Azure:** acceptable and already the stated direction for this project; not a new risk introduced by this decision.
- **Browser compatibility:** mitigated by the SDK's built-in `isSupportedBrowser` check surfaced in the UI before join.
- **No first-party Aspire integration:** confirmed as a non-issue — Aspire is local-dev-only in this project and already excludes Azure SignalR Service the same way.

---

## Technical Research Recommendations

### Implementation Roadmap

1. API: ACS Identity token endpoint (Custom ID = app UserId) + board-membership authorization check.
2. Data model: `CallSession` (BoardId, AcsRoomId, Status, StartedAt, EndedAt) + `CallParticipant` (join/leave audit).
3. "Start video" action on a Board: create/reuse ACS Room, issue token, return to client.
4. Blazor: JS-interop wrapper around `@azure/communication-calling` for join/leave/mute/camera/single-presenter screen share.
5. Event Grid webhook → API → existing SignalR hub, to relay call-state (active/ended, who's in) to board viewers.
6. Manual cross-browser validation using `isSupportedBrowser` + pre-call diagnostics.

### Technology Stack Recommendations

- **Azure Communication Services Calling SDK** (JS, via interop) — media, group calls, screen share.
- **Azure Communication Identity SDK** (.NET) — server-side token issuance with Custom ID mapping.
- **Azure Event Grid** — call lifecycle events → webhook.
- **Existing SignalR hub** — retained, scoped to app-level presence/state only, not media.
- **Existing SQL Server** — `CallSession`/`CallParticipant` persistence via the current data access approach.

### Skill Development Requirements

- Blazor WASM JS-interop patterns (already used elsewhere per the technology stack findings — not a new skill for the team).
- ACS Calling/Identity SDK basics (token lifecycle, Room API) — narrow, well-documented surface area; no WebRTC internals required.

### Success Metrics and KPIs

- Call join success rate (join attempts vs. successful media connection).
- Time-to-join latency (click "start video" → connected).
- Screen-share reliability (successful start/stop, no more than one concurrent presenter enforced correctly).
- Actual ACS spend vs. projected participant-minutes, tracked monthly once live.

---

## Research Synthesis

### Executive Summary

JustTaskTracker needs board-scoped group video calls with single-presenter screen sharing, with an explicit constraint to build on Azure and an explicit (deferred) roadmap toward scheduled meetings with a calendar view. Three architectures were evaluated: self-hosted signaling over SignalR (mesh or SFU), Azure Communication Services (ACS) Calling SDK, and a SignalR/ACS hybrid.

**Recommendation: Azure Communication Services**, with the app's existing SignalR hub retained purely as the app-level presence/state transport (fed by ACS's Event Grid call events), not as the call's signaling path.

The deciding factors: ACS removes the need to build and operate signaling *and* media infrastructure (no SFU to run, no mesh participant ceiling to work around); its pricing is usage-based ($0.004/participant-minute, uniform across audio/video/screen-share, no egress fee) rather than infrastructure to provision; and its **Rooms API** (time-boxed, participant-controlled call containers) already models exactly the deferred scheduled-meeting requirement, so today's build doesn't have to be re-architected when that roadmap item is picked up.

### Trade-off Comparison

| Dimension | Self-hosted SignalR (mesh) | Self-hosted SignalR + SFU | Azure Communication Services |
|---|---|---|---|
| Group call scaling | Practical ceiling ~6-10 participants | Scales well, but you operate the SFU (mediasoup/Janus/LiveKit) | Managed by Microsoft, no infra to run |
| Screen sharing | Must implement manually over WebRTC | Must implement manually over WebRTC | Built into the Calling SDK |
| Blazor WASM integration | Custom JS interop over raw `RTCPeerConnection` | Custom JS interop over raw `RTCPeerConnection` | Custom JS interop over `@azure/communication-calling` (comparable interop effort, less peer-connection complexity to wrap) |
| Cost model | Infra to provision/operate (compute for signaling + SFU) | Infra to provision/operate (compute for signaling + SFU) | Pay-as-you-go per participant-minute, no idle cost |
| Ops burden | Signaling server + Redis backplane for scale | + media server fleet, TURN/STUN, monitoring | None beyond normal app deployment |
| Roadmap fit (scheduling/calendar) | Build time-window/participant control from scratch | Build time-window/participant control from scratch | Rooms API's `validFrom`/`validUntil` already models this |
| Azure alignment | Runs on Azure compute, but not an Azure-native calling product | Runs on Azure compute, but not an Azure-native calling product | First-party Azure service, same subscription/billing plane |

### Key Risks and Mitigations

- **Vendor lock-in to Azure** — accepted; consistent with the project's stated direction, not a new risk this decision introduces.
- **Browser compatibility** — mitigated via the Calling SDK's `isSupportedBrowser` check before offering "join call".
- **No first-party .NET Aspire hosting integration for ACS** — a non-issue: Aspire is local-dev-only in this project and Azure SignalR Service already follows the same connection-string pattern.
- **No local ACS emulator** — development/testing happens against a real low-cost dev-tier ACS resource; budget for this in the dev workflow.

### Implementation Roadmap (recap)

1. API: ACS Identity token endpoint (Custom ID = app `UserId`) + board-membership authorization check.
2. Data model: `CallSession` (BoardId, AcsRoomId, Status, StartedAt, EndedAt) + `CallParticipant` (join/leave audit) — schema left open to carry `ScheduledStart`/`ScheduledEnd` later.
3. "Start video" action on a Board creates/reuses an ACS Room and issues a token to the caller.
4. Blazor JS-interop wrapper around `@azure/communication-calling` for join/leave/mute/camera/single-presenter screen share.
5. Event Grid webhook relays call-state to the existing SignalR hub for board-wide presence/banners.
6. Cross-browser validation via `isSupportedBrowser` + pre-call diagnostics.

### Next Steps

This research is sufficient technical grounding to move into formal requirements/architecture work:

- Capture the MVP scope and this technology decision in a **Product Brief / PRD**, so it's a documented, traceable input.
- Hand off to the **Architect** for detailed system design (API contracts, sequence diagrams, Blazor component structure) building directly on this research's recommendations.

### Source Documentation

- [How WebRTC, SignalR, and .NET Work Together](https://medium.com/@aayushadhikari601/how-webrtc-signalr-and-net-work-together-to-enable-real-time-video-calls-0182514a4e30)
- [MDN: Signaling and video calling](https://developer.mozilla.org/en-US/docs/Web/API/WebRTC_API/Signaling_and_video_calling)
- [SignalRCoreWebRTC (GitHub)](https://github.com/Shhzdmrz/SignalRCoreWebRTC)
- [ACS Pricing scenarios](https://learn.microsoft.com/en-us/azure/communication-services/concepts/pricing)
- [Azure Communication Services pricing](https://azure.microsoft.com/en-us/pricing/details/communication-services/)
- [Rooms API for structured meetings](https://learn.microsoft.com/en-us/azure/communication-services/concepts/rooms/room-concept)
- [Virtual Rooms GA announcement](https://techcommunity.microsoft.com/blog/azurecommunicationservicesblog/azure-communication-services-virtual-rooms-is-now-generally-available/3845412)
- [BlazorRtc (GitHub)](https://github.com/ParrhesiaJoe/BlazorRtc)
- [SpawnDev.BlazorJS.PeerJS (GitHub)](https://github.com/LostBeard/SpawnDev.BlazorJS.PeerJS)
- [Soenneker.Telnyx.Blazor.WebRtc (GitHub)](https://github.com/soenneker/soenneker.telnyx.blazor.webrtc)
- [Mesh vs SFU vs MCU](https://antmedia.io/webrtc-network-topology/)
- [WebRTC Architecture Explained: P2P vs SFU vs MCU vs XDN](https://www.red5.net/blog/webrtc-architecture-p2p-sfu-mcu-xdn/)
- [WebRTC Architecture for Production](https://www.forasoft.com/learn/webrtc-architecture-production-systems)
- [Create and manage access tokens](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/identity/access-tokens)
- [Authenticate to ACS](https://learn.microsoft.com/en-us/azure/communication-services/concepts/authentication)
- [Identities with Custom ID](https://techcommunity.microsoft.com/blog/azurecommunicationservicesblog/identities-with-custom-id-a-simpler-smarter-way-to-manage-communication-users/4435083)
- [Credentials best practices](https://learn.microsoft.com/en-us/azure/communication-services/concepts/credentials-best-practices)
- [ACS as an Event Grid source](https://learn.microsoft.com/en-us/azure/event-grid/event-schema-communication-services)
- [Subscribe to events](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/events/subscribe-to-events)
- [Securing webhook endpoint](https://learn.microsoft.com/en-us/azure/communication-services/how-tos/call-automation/secure-webhook-endpoint)
- [Client and server architecture (trusted auth sample)](https://github.com/Azure-Samples/communication-services-authentication-hero-csharp/blob/main/docs/design-guides/architecture-overview.md)
- [Data residency and user privacy for ACS](https://learn.microsoft.com/en-us/azure/communication-services/concepts/privacy)
- [Aspire Azure integrations overview](https://aspire.dev/integrations/cloud/azure/overview/)
- [Add Azure Communication Services (Aspire feature request)](https://github.com/dotnet/aspire/issues/2802)
- [UI Library overview](https://learn.microsoft.com/en-us/azure/communication-services/concepts/ui-library/ui-library-overview)
- [@azure/communication-react (npm)](https://www.npmjs.com/package/@azure/communication-react)
- [Azure Communication Service Calling SDK check for supported browser](https://learn.microsoft.com/en-us/answers/questions/1379857/azure-communication-service-calling-sdk-check-for)
- [Pre-call diagnostics](https://learn.microsoft.com/en-us/azure/communication-services/concepts/voice-video-calling/pre-call-diagnostics)

---

**Technical Research Completion Date:** 2026-07-23
**Source Verification:** All claims cited to sources above; ACS pricing/free-tier figures should be re-confirmed on the official pricing page before budgeting, as allowances can change.
