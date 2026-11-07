// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
    const header = document.querySelector(".signal-header");
    const toggle = header?.querySelector(".signal-menu__toggle");
    const navigation = header?.querySelector("#main-navigation");
    if (!header || !toggle || !navigation) return;

    const compactScreen = window.matchMedia("(max-width: 1199px)");
    const setOpen = (open) => {
        navigation.classList.toggle("is-open", open);
        toggle.setAttribute("aria-expanded", String(open));
        toggle.setAttribute("aria-label", open ? "Close navigation menu" : "Open navigation menu");
    };

    // With JavaScript disabled, links remain visible at every screen size.
    header.classList.add("signal-header--enhanced");
    toggle.addEventListener("click", () => {
        setOpen(toggle.getAttribute("aria-expanded") !== "true");
    });
    header.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && compactScreen.matches &&
            toggle.getAttribute("aria-expanded") === "true") {
            setOpen(false);
            toggle.focus();
        }
    });
    document.addEventListener("click", (event) => {
        if (compactScreen.matches && !header.contains(event.target)) setOpen(false);
    });
    compactScreen.addEventListener("change", () => {
        if (compactScreen.matches && navigation.contains(document.activeElement)) toggle.focus();
        setOpen(false);
    });
})();
