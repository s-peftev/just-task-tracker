export function isOverflowing(element, maxHeight) {
    if (!element)
        return false;

    return element.scrollHeight > maxHeight;
}
