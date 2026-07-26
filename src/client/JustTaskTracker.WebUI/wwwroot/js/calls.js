// ACS Calling Web SDK has no npm/bundler pipeline in this project (no package.json exists),
// and its own dist-esm bundle has unresolved bare imports ("@azure/logger", "@azure/communication-common"),
// so it can't be imported directly via a plain <script type="module">. esm.sh serves a pre-resolved,
// self-contained ESM build (no import map / bundler needed) -- this is the load-bearing reason for the CDN URL below.
import {
    CallClient,
    Features,
    LocalVideoStream,
    VideoStreamRenderer
} from "https://esm.sh/@azure/communication-calling@1.43.1";
import { AzureCommunicationTokenCredential } from "https://esm.sh/@azure/communication-common@2.3.1";

export async function checkEnvironment() {
    try {
        const callClient = new CallClient();
        const envInfo = await callClient.feature(Features.DebugInfo).getEnvironmentInfo();

        // isSupportedEnvironment also gates on isSupportedBrowserVersion/isSupportedPlatform, which
        // excludes browsers ACS only supports in preview (e.g. Firefox, as of this SDK version) even
        // though the browser itself works. Gate on isSupportedBrowser alone -- a deliberate, accepted
        // trade-off: Firefox is allowed through on ACS's public-preview support, not GA.
        if (!envInfo.isSupportedBrowser) {
            return {
                isSupported: false,
                reason: `Unsupported browser (${envInfo.environment.browser} ${envInfo.environment.browserVersion} on ${envInfo.environment.platform}).`
            };
        }

        return { isSupported: true, reason: null };
    } catch (error) {
        console.error("calls.js checkEnvironment failed:", error);
        return { isSupported: false, reason: `Could not determine browser compatibility: ${error?.message ?? error}` };
    }
}

// Module-level (not local to join()) on purpose: join() calls back into Blazor
// (OnTileAdded) *while it's still running*, and Blazor's OnAfterRenderAsync calls
// registerTileElement back here in response -- if these lived only in a handle object
// returned at the end of join(), that registration would race against join() itself
// still executing, and C# wouldn't have the handle yet to call it on. One call at a
// time per CallsInteropService instance (Scoped), so module-level state is safe.
let call = null;
let localVideoStream = null;
let localRenderer = null;
let micOn = true;
let cameraOn = false;
const remoteRenderers = new Map();
const tileElements = new Map();
const pendingViews = new Map();
// Tracks which DOM node is *currently appended* for a tile, separate from pendingViews (which
// tracks a view waiting for its tile element to exist). Needed because disposing a renderer
// (camera turned off, or re-rendering after it's turned back on) does not itself remove the
// <video>/<canvas> node it created -- without this, repeated on/off toggles would leave a growing
// pile of stale, disconnected media elements behind in the tile.
const activeViewElements = new Map();

// view.target ships with no intrinsic size or aspect-ratio handling -- style it directly
// rather than relying on CSS to reach into an element Blazor's scoped-CSS isolation doesn't
// know about (it's appended via plain DOM APIs, not rendered by Blazor, so scoped selectors
// don't reliably reach it). object-fit: cover goes on the actual <video>/<canvas> (which may
// be view.target itself, or nested inside it depending on the SDK's internal DOM shape) so
// the feed fills the tile without stretching/distorting.
function styleView(viewTarget) {
    viewTarget.style.position = "absolute";
    viewTarget.style.inset = "0";
    viewTarget.style.width = "100%";
    viewTarget.style.height = "100%";

    const mediaElements = viewTarget.matches?.("video, canvas")
        ? [viewTarget, ...viewTarget.querySelectorAll("video, canvas")]
        : [...viewTarget.querySelectorAll("video, canvas")];

    for (const media of mediaElements) {
        media.style.width = "100%";
        media.style.height = "100%";
        media.style.objectFit = "cover";
    }
}

function attachView(tileId, viewTarget) {
    styleView(viewTarget);
    detachView(tileId);

    const el = tileElements.get(tileId);

    if (el) {
        el.appendChild(viewTarget);
        activeViewElements.set(tileId, viewTarget);
    } else {
        pendingViews.set(tileId, viewTarget);
    }
}

function detachView(tileId) {
    const existing = activeViewElements.get(tileId);

    if (existing?.parentElement)
        existing.parentElement.removeChild(existing);

    activeViewElements.delete(tileId);
    pendingViews.delete(tileId);
}

function participantKeyOf(participant) {
    return participant.identifier.communicationUserId ?? participant.identifier.rawId;
}

async function renderRemoteStream(tileId, stream) {
    if (remoteRenderers.has(tileId))
        return;

    try {
        const renderer = new VideoStreamRenderer(stream);
        remoteRenderers.set(tileId, renderer);
        const view = await renderer.createView();
        attachView(tileId, view.target);
    } catch (error) {
        console.error(`calls.js failed to render remote video for tile "${tileId}":`, error);
        remoteRenderers.delete(tileId);
    }
}

function disposeRemoteRenderer(tileId) {
    const renderer = remoteRenderers.get(tileId);

    if (!renderer)
        return;

    renderer.dispose();
    remoteRenderers.delete(tileId);
    detachView(tileId);
}

// Zoom-like grid: Blazor owns one <div> tile per participant (local + each remote) and reports
// its element back here via registerTileElement once rendered. This module owns ACS state and
// only tells Blazor (via dotNetRef) when a tile should exist/stop existing -- rendering the actual
// <video> into a tile is always driven from here, since only this module knows when a stream
// becomes available, independent of Blazor's render cycle.
export async function join(token, roomId, dotNetRef) {
    const callClient = new CallClient();
    const tokenCredential = new AzureCommunicationTokenCredential(token);
    const callAgent = await callClient.createCallAgent(tokenCredential);

    try {
        const deviceManager = await callClient.getDeviceManager();
        await deviceManager.askDevicePermission({ video: true, audio: true });
        const cameras = await deviceManager.getCameras();

        if (cameras.length > 0)
            localVideoStream = new LocalVideoStream(cameras[0]);
    } catch (error) {
        // No camera / permission denied -- proceed audio-only, but don't hide *why*.
        console.error("calls.js camera setup failed, proceeding audio-only:", error);
        localVideoStream = null;
    }

    const callOptions = localVideoStream
        ? { videoOptions: { localVideoStreams: [localVideoStream] } }
        : {};

    call = callAgent.join({ roomId }, callOptions);

    await dotNetRef.invokeMethodAsync("OnTileAdded", "local", true, !!localVideoStream);

    if (localVideoStream) {
        cameraOn = true;
        await renderLocalVideo();
    }

    // Handles both "a new video stream showed up" (participant.videoStreams changed) and "an
    // existing stream's camera was turned back on/off" (same stream object, isAvailable flips) --
    // ACS reuses the same stream object across a camera toggle rather than removing/re-adding it,
    // so re-rendering can't be driven only from videoStreamsUpdated.
    async function syncParticipantVideo(participant, tileId) {
        const availableStream = participant.videoStreams.find((s) => s.isAvailable);

        if (availableStream)
            await renderRemoteStream(tileId, availableStream);
        else
            disposeRemoteRenderer(tileId);

        await dotNetRef.invokeMethodAsync("OnTileVideoStateChanged", tileId, !!availableStream);
    }

    async function watchParticipant(participant) {
        const tileId = participantKeyOf(participant);

        await dotNetRef.invokeMethodAsync("OnTileAdded", tileId, false, participant.videoStreams.some((s) => s.isAvailable));

        for (const stream of participant.videoStreams)
            stream.on("isAvailableChanged", () => syncParticipantVideo(participant, tileId));

        await syncParticipantVideo(participant, tileId);

        participant.on("videoStreamsUpdated", (e) => {
            for (const stream of e.added)
                stream.on("isAvailableChanged", () => syncParticipantVideo(participant, tileId));

            syncParticipantVideo(participant, tileId);
        });
    }

    async function removeParticipant(participant) {
        const tileId = participantKeyOf(participant);
        disposeRemoteRenderer(tileId);
        tileElements.delete(tileId);
        pendingViews.delete(tileId);
        await dotNetRef.invokeMethodAsync("OnTileRemoved", tileId);
    }

    call.remoteParticipants.forEach(watchParticipant);
    call.on("remoteParticipantsUpdated", (e) => {
        for (const participant of e.added)
            watchParticipant(participant);

        for (const participant of e.removed)
            removeParticipant(participant);
    });
}

async function renderLocalVideo() {
    try {
        localRenderer = new VideoStreamRenderer(localVideoStream);
        const view = await localRenderer.createView();
        attachView("local", view.target);
    } catch (error) {
        console.error("calls.js failed to render local video:", error);
    }
}

export function registerTileElement(tileId, element) {
    tileElements.set(tileId, element);
    const pending = pendingViews.get(tileId);

    if (pending) {
        element.appendChild(pending);
        activeViewElements.set(tileId, pending);
        pendingViews.delete(tileId);
    }
}

export function unregisterTileElement(tileId) {
    tileElements.delete(tileId);
}

export async function toggleMic() {
    if (!call)
        return micOn;

    if (micOn)
        await call.mute();
    else
        await call.unmute();

    micOn = !micOn;
    return micOn;
}

export async function toggleCamera() {
    if (!call || !localVideoStream)
        return cameraOn;

    if (cameraOn) {
        await call.stopVideo(localVideoStream);

        if (localRenderer) {
            localRenderer.dispose();
            localRenderer = null;
        }

        detachView("local");
    } else {
        await call.startVideo(localVideoStream);
        await renderLocalVideo();
    }

    cameraOn = !cameraOn;
    return cameraOn;
}

export async function hangUp() {
    if (call)
        await call.hangUp();
}

export function disposeCall() {
    if (localRenderer)
        localRenderer.dispose();

    for (const renderer of remoteRenderers.values())
        renderer.dispose();

    remoteRenderers.clear();
    tileElements.clear();
    pendingViews.clear();
    activeViewElements.clear();
    call = null;
    localVideoStream = null;
    localRenderer = null;
    micOn = true;
    cameraOn = false;
}
