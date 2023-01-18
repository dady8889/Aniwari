export function animateActiveNav(nav: HTMLElement) {
    const elems = document.getElementsByClassName("active");

    if (elems.length < 1)
        return;

    const activeButton = elems[0].parentElement;
    const allButtons = document.getElementsByClassName("nav-item");
    let activeButtonIndex = 0;

    for (let i = 0; i < allButtons.length; i++) {
        if (activeButton == allButtons[i]) {
            break;
        }

        activeButtonIndex++;
    }

    const shift = 2 * activeButton.clientWidth * activeButtonIndex;

    nav.style.paddingLeft = `${nav.children[0].clientWidth / 2}px`;

    nav.style.paddingRight = `${shift}px`;
}