document.addEventListener("input", onInput);
function onInput(e) {
    if (e.target.localName == "span") {
        if (e.target.contentEditable) {
            e.target.value = e.target.innerText;
        }
    }
}
document.addEventListener("keydown", onKeyDown);
function onKeyDown(e) {
    if (e.target.localName == "span") {
        if (e.target.contentEditable) {
            if (e.keyCode === 13) { // Enter key
                e.preventDefault();
            }
        }
    }
}
//# sourceMappingURL=WatchlistItem.razor.js.map