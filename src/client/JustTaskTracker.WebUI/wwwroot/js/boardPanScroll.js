const panningClass = "board-page__content--panning";

function canPan(content, target) {
    if (!(target instanceof Element))
        return false;

    if (target.closest(".board-column"))
        return false;

    if (target.closest("button, a, input, textarea, select, [contenteditable='true']"))
        return false;

    return content.contains(target);
}

export function attach(content, scrollEl) {
    if (!content || !scrollEl)
        return { dispose: () => { } };

    let isPanning = false;
    let pointerId = null;
    let startX = 0;
    let startScrollLeft = 0;

    const onPointerDown = (event) => {
        if (isPanning || event.button !== 0)
            return;

        if (!canPan(content, event.target))
            return;

        isPanning = true;
        pointerId = event.pointerId;
        startX = event.clientX;
        startScrollLeft = scrollEl.scrollLeft;

        content.setPointerCapture(pointerId);
        content.classList.add(panningClass);
    };

    const onPointerMove = (event) => {
        if (!isPanning || event.pointerId !== pointerId)
            return;

        scrollEl.scrollLeft = startScrollLeft - (event.clientX - startX);
    };

    const endPan = (event) => {
        if (!isPanning || (event && event.pointerId !== pointerId))
            return;

        isPanning = false;
        content.classList.remove(panningClass);

        if (pointerId !== null && content.hasPointerCapture(pointerId))
            content.releasePointerCapture(pointerId);

        pointerId = null;
    };

    content.addEventListener("pointerdown", onPointerDown);
    content.addEventListener("pointermove", onPointerMove);
    content.addEventListener("pointerup", endPan);
    content.addEventListener("pointercancel", endPan);

    return {
        dispose() {
            endPan();
            content.removeEventListener("pointerdown", onPointerDown);
            content.removeEventListener("pointermove", onPointerMove);
            content.removeEventListener("pointerup", endPan);
            content.removeEventListener("pointercancel", endPan);
            content.classList.remove(panningClass);
        }
    };
}
