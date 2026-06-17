const enterSubmitHandlers = new WeakMap();

export function autoResize(element) {
    if (!element)
        return;

    element.style.height = "auto";

    const minHeight = Number.parseFloat(getComputedStyle(element).minHeight) || 0;
    const nextHeight = Math.max(element.scrollHeight, minHeight);

    element.style.height = `${nextHeight}px`;
}

export function attachEnterSubmitBehavior(element) {
    if (!element || enterSubmitHandlers.has(element))
        return;

    const handler = (event) => {
        if (event.key === "Enter" && !event.shiftKey)
            event.preventDefault();
    };

    element.addEventListener("keydown", handler);
    enterSubmitHandlers.set(element, handler);
}

export function detachEnterSubmitBehavior(element) {
    if (!element)
        return;

    const handler = enterSubmitHandlers.get(element);

    if (!handler)
        return;

    element.removeEventListener("keydown", handler);
    enterSubmitHandlers.delete(element);
}
