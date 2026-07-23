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

export function registerOutsideClickHandler(containerEl, dotNetRef) {
    function onPointerDown(event) {
        if (containerEl.contains(event.target) || event.target.closest("[data-calls-toggle]"))
            return;

        dotNetRef.invokeMethodAsync("OnOutsideClick");
    }

    document.addEventListener("pointerdown", onPointerDown, true);

    return {
        dispose() {
            document.removeEventListener("pointerdown", onPointerDown, true);
        }
    };
}

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

export async function join(token, roomId, localVideoEl, remoteVideoContainerEl) {
    const callClient = new CallClient();
    const tokenCredential = new AzureCommunicationTokenCredential(token);
    const callAgent = await callClient.createCallAgent(tokenCredential);

    let localVideoStream = null;
    let localRenderer = null;

    try {
        const deviceManager = await callClient.getDeviceManager();
        await deviceManager.askDevicePermission({ video: true, audio: true });
        const cameras = await deviceManager.getCameras();

        if (cameras.length > 0)
            localVideoStream = new LocalVideoStream(cameras[0]);
    } catch {
        // No camera / permission denied -- proceed audio-only.
        localVideoStream = null;
    }

    const callOptions = localVideoStream
        ? { videoOptions: { localVideoStreams: [localVideoStream] } }
        : {};

    const call = callAgent.join({ roomId }, callOptions);

    if (localVideoStream) {
        localRenderer = new VideoStreamRenderer(localVideoStream);
        const view = await localRenderer.createView();
        localVideoEl.appendChild(view.target);
    }

    const remoteRenderers = new Map();

    async function renderRemoteStream(participant, stream) {
        const participantKey = participant.identifier.communicationUserId ?? participant.identifier.rawId;
        const key = `${participantKey}-${stream.id}`;

        if (remoteRenderers.has(key))
            return;

        const renderer = new VideoStreamRenderer(stream);
        remoteRenderers.set(key, renderer);
        const view = await renderer.createView();
        remoteVideoContainerEl.appendChild(view.target);

        stream.on("isAvailableChanged", () => {
            if (stream.isAvailable)
                return;

            renderer.dispose();
            remoteRenderers.delete(key);
        });
    }

    function watchRemoteParticipant(participant) {
        for (const stream of participant.videoStreams) {
            if (stream.isAvailable)
                renderRemoteStream(participant, stream);
        }

        participant.on("videoStreamsUpdated", (e) => {
            for (const stream of e.added)
                renderRemoteStream(participant, stream);
        });
    }

    call.remoteParticipants.forEach(watchRemoteParticipant);
    call.on("remoteParticipantsUpdated", (e) => {
        for (const participant of e.added)
            watchRemoteParticipant(participant);
    });

    return {
        async hangUp() {
            await call.hangUp();
        },
        dispose() {
            if (localRenderer)
                localRenderer.dispose();

            for (const renderer of remoteRenderers.values())
                renderer.dispose();

            remoteRenderers.clear();
        }
    };
}
