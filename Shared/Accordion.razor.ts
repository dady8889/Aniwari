export function toggleDetails(elem: HTMLElement, expanded: boolean) {

    if (expanded) {
        elem.style.maxHeight = elem.scrollHeight + "px";

        if (elem.children.length >= 1) {
            elem.children[0].scrollTop = elem.scrollHeight;
        }
    } else {
        elem.style.maxHeight = "0";
    }
}