function getDocumentScrollTop(): number {
    return document.documentElement.getElementsByTagName("main")[0].scrollTop;
}

function setDocumentScrollTop(scroll: number, smooth: boolean) {

    if (smooth) {
        document.documentElement.getElementsByTagName("main")[0].scrollTo({ behavior: "smooth", top: scroll });
    } else {
        document.documentElement.getElementsByTagName("main")[0].scrollTop = scroll;
    }
}