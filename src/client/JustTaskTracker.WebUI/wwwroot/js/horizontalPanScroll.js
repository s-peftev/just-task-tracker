export function attach(scrollEl) {
    if (!scrollEl)
        return { dispose: () => { } };

    let isPanning = false;
    let pointerId = null;
    let startX = 0;
    let startScrollLeft = 0;
    let moved = false;

    const onPointerDown = (event) => {
        if (isPanning || event.button !== 0)
            return;

        isPanning = true;
        moved = false;
        pointerId = event.pointerId;
        startX = event.clientX;
        startScrollLeft = scrollEl.scrollLeft;

        scrollEl.setPointerCapture(pointerId);
        scrollEl.classList.add("is-panning");
    };

    const onPointerMove = (event) => {
        if (!isPanning || event.pointerId !== pointerId)
            return;

        const delta = event.clientX - startX;
        if (Math.abs(delta) > 2)
            moved = true;

        scrollEl.scrollLeft = startScrollLeft - delta;
    };

    const endPan = (event) => {
        if (!isPanning || (event && event.pointerId !== pointerId))
            return;

        isPanning = false;
        scrollEl.classList.remove("is-panning");

        if (pointerId !== null && scrollEl.hasPointerCapture(pointerId))
            scrollEl.releasePointerCapture(pointerId);

        pointerId = null;
    };

    // Prevent click-through after a drag gesture.
    const onClickCapture = (event) => {
        if (!moved)
            return;

        event.preventDefault();
        event.stopPropagation();
        moved = false;
    };

    scrollEl.addEventListener("pointerdown", onPointerDown);
    scrollEl.addEventListener("pointermove", onPointerMove);
    scrollEl.addEventListener("pointerup", endPan);
    scrollEl.addEventListener("pointercancel", endPan);
    scrollEl.addEventListener("click", onClickCapture, true);

    return {
        dispose() {
            endPan();
            scrollEl.removeEventListener("pointerdown", onPointerDown);
            scrollEl.removeEventListener("pointermove", onPointerMove);
            scrollEl.removeEventListener("pointerup", endPan);
            scrollEl.removeEventListener("pointercancel", endPan);
            scrollEl.removeEventListener("click", onClickCapture, true);
            scrollEl.classList.remove("is-panning");
        }
    };
}
