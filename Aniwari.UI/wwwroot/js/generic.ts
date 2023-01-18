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

function hexToRGB(hex: string): number[] {

    var aRgbHex = hex.substring(1).match(/.{1,2}/g);
    var aRgb = [
        parseInt(aRgbHex[0], 16),
        parseInt(aRgbHex[1], 16),
        parseInt(aRgbHex[2], 16)
    ];
    return aRgb;
}

function rgbToHsl(r: number, g: number, b: number): number[] {
    r /= 255;
    g /= 255;
    b /= 255;
    const l = Math.max(r, g, b);
    const s = l - Math.min(r, g, b);
    const h = s
        ? l === r
            ? (g - b) / s
            : l === g
                ? 2 + (b - r) / s
                : 4 + (r - g) / s
        : 0;
    return [
        60 * h < 0 ? 60 * h + 360 : 60 * h,
        100 * (s ? (l <= 0.5 ? s / (2 * l - s) : s / (2 - (2 * l - s))) : 0),
        (100 * (2 * l - s)) / 2,
    ];
};

function setThemeColors(color: string) {
    const css = document.documentElement.style;
    const rgb = hexToRGB(color);
    const hsl = rgbToHsl(rgb[0], rgb[1], rgb[2]);

    css.setProperty("--aniwari-primary-r", `${rgb[0]}`);
    css.setProperty("--aniwari-primary-g", `${rgb[1]}`);
    css.setProperty("--aniwari-primary-b", `${rgb[2]}`);

    css.setProperty("--aniwari-primary-h", `${hsl[0]}`)
    css.setProperty("--aniwari-primary-s", `${hsl[1]}%`);
    css.setProperty("--aniwari-primary-l", `${hsl[2]}%`);
}
