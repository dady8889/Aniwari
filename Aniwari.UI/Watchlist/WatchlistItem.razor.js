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