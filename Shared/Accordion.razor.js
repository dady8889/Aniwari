export function toggleDetails(elem, expanded) {
    if (expanded) {
        elem.style.maxHeight = elem.scrollHeight + "px";
        if (elem.children.length >= 1) {
            elem.children[0].scrollTop = elem.scrollHeight;
        }
    }
    else {
        elem.style.maxHeight = "0";
    }
}
//# sourceMappingURL=Accordion.razor.js.map