export function scrollToDay(day) {
    const d = day.toLowerCase();
    const dayElement = document.getElementsByClassName(d);
    if (dayElement.length == 0)
        return;
    dayElement[0].scrollIntoView({ behavior: "smooth", block: "center" });
}
//# sourceMappingURL=Schedule.razor.js.map