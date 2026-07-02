export function download(fileName, contentType, content) {
    const blob = new Blob([content], { type: contentType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");

    anchor.href = url;
    anchor.download = fileName;
    anchor.style.display = "none";
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
}

export async function downloadFromUrl(url, fileName) {
    try {
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Download failed (${response.status})`);
        }

        const contentType = response.headers.get("content-type") || "application/zip";
        const content = await response.arrayBuffer();
        download(fileName, contentType, content);
    } catch {
        const anchor = document.createElement("a");

        anchor.href = url;
        anchor.download = fileName;
        anchor.target = "_blank";
        anchor.rel = "noopener noreferrer";
        anchor.style.display = "none";
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
    }
}
