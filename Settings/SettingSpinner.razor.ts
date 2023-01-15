export function addKeyPressEventListener(input: HTMLInputElement) {
    input.addEventListener('keypress', (event: KeyboardEvent) => {
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

export function addPasteEventListener(input: HTMLInputElement) {
    input.addEventListener('paste', (event: ClipboardEvent) => {
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

export function setInputElementValue(input: HTMLInputElement, value: string) {
    input.value = value;
}