export function autoResize(element) {
    if (!element)
        return;

    element.style.height = "auto";

    const minHeight = Number.parseFloat(getComputedStyle(element).minHeight) || 0;
    const nextHeight = Math.max(element.scrollHeight, minHeight);

    element.style.height = `${nextHeight}px`;
}
