export function scrollToDay(day: string) {
    const d = day.toLowerCase();
    document.getElementsByClassName(d)[0].scrollIntoView({ behavior: "smooth", block: "center" });
}