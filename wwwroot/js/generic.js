function getDocumentScrollTop() {
    return document.documentElement.getElementsByTagName("main")[0].scrollTop;
}
function setDocumentScrollTop(scroll, smooth) {
    if (smooth) {
        document.documentElement.getElementsByTagName("main")[0].scrollTo({ behavior: "smooth", top: scroll });
    }
    else {
        document.documentElement.getElementsByTagName("main")[0].scrollTop = scroll;
    }
}
function getElementWidth(elem) {
    return elem.clientWidth;
}
function setMaxElementWidth(elem, width) {
    elem.style.maxWidth = `${width}px`;
}
function getElementInnerText(elem) {
    return elem.innerText;
}
//# sourceMappingURL=generic.js.map