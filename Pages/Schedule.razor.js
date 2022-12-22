export function scrollToDay(day) {
    const d = day.toLowerCase();
    document.getElementsByClassName(d)[0].scrollIntoView({ behavior: "smooth", block: "center" });
}
//# sourceMappingURL=Schedule.razor.js.map