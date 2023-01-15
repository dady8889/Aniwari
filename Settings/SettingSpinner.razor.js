export function addKeyPressEventListener(input) {
    input.addEventListener('keypress', (event) => {
        const key = event.key;
        const isNumber = !isNaN(parseInt(key));
        const isDecimal = key === '.' || key === ',';
        const isMinus = key === '-';
        const isBackspace = key === 'Backspace';
        const isDelete = key === 'Delete';
        const isTab = key === 'Tab';
        if ((isDecimal && (input.value.includes('.') || input.value.includes(','))) || (isMinus && input.value.includes('-')) || !(isNumber || isDecimal || isMinus || isBackspace || isDelete || isTab)) {
            event.preventDefault();
        }
    });
}
export function addPasteEventListener(input) {
    input.addEventListener('paste', (event) => {
        event.preventDefault();
        const paste = event.clipboardData.getData('text');
        if (!/^-?\d*[.,]?\d*$/.test(paste)) {
            return;
        }
        if ((paste.includes(".") || paste.includes(",")) && (input.value.includes(".") || input.value.includes(","))) {
            return;
        }
        if (paste.includes("-") && input.value.includes("-")) {
            return;
        }
        input.value = paste;
    });
}
export function setInputElementValue(input, value) {
    input.value = value;
}
//# sourceMappingURL=SettingSpinner.razor.js.map