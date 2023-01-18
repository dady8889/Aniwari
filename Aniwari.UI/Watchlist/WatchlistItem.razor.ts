document.addEventListener("keydown", onKeyDown);

function onKeyDown(e: any) {
    if (e.target.localName == "span") {
        if (e.target.contentEditable) {
            if (e.keyCode === 13) { // Enter key
                e.preventDefault();
            }
        }
    }
}