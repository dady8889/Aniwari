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

function getElementWidth(elem: HTMLElement): number {
    return elem.clientWidth;
}

function setMaxElementWidth(elem: HTMLElement, width: number) {
    elem.style.maxWidth = `${width}px`;
}

function getElementInnerText(elem: HTMLElement): string {
    return elem.innerText;
}