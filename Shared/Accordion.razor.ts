export function toggleDetails(elem: HTMLElement, expanded: boolean) {

    if (expanded) {
        elem.style.maxHeight = elem.scrollHeight + "px";
    } else {
        elem.style.maxHeight = "0";
    }
}