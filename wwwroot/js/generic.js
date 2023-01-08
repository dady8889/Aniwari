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
//# sourceMappingURL=generic.js.map