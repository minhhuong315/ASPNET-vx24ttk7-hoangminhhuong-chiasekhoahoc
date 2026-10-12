(() => {
    "use strict";

    const body =
        document.body;

    const filterToggle =
        document.querySelector(
            "[data-courses-filter-toggle]");

    const filterPanel =
        document.getElementById(
            "coursesFilterPanel");

    const filterClose =
        document.querySelector(
            "[data-courses-filter-close]");

    const filterOverlay =
        document.querySelector(
            "[data-courses-filter-overlay]");

    const filterMedia =
        window.matchMedia(
            "(max-width: 1100px)");

    let lastFilterFocus = null;


    function closeFilter(restoreFocus = true) {
        if (!filterPanel ||
            !filterToggle ||
            !filterOverlay) {
            return;
        }

        filterPanel.classList.remove(
            "is-open");

        filterToggle.setAttribute(
            "aria-expanded",
            "false");

        filterOverlay.hidden = true;

        body.classList.remove(
            "courses-filter-open");

        if (restoreFocus &&
            lastFilterFocus instanceof HTMLElement) {
            lastFilterFocus.focus();
        }
    }


    function openFilter() {
        if (!filterPanel ||
            !filterToggle ||
            !filterOverlay) {
            return;
        }

        lastFilterFocus =
            document.activeElement;

        filterPanel.classList.add(
            "is-open");

        filterToggle.setAttribute(
            "aria-expanded",
            "true");

        filterOverlay.hidden = false;

        body.classList.add(
            "courses-filter-open");

        window.requestAnimationFrame(
            () => filterClose?.focus());
    }


    filterToggle?.addEventListener(
        "click",
        () => {
            if (filterPanel?.classList.contains(
                    "is-open")) {
                closeFilter();
            }
            else {
                openFilter();
            }
        });


    filterClose?.addEventListener(
        "click",
        () => closeFilter());


    filterOverlay?.addEventListener(
        "click",
        () => closeFilter());


    function syncFilterState() {
        if (!filterMedia.matches) {
            closeFilter(false);
        }
    }


    if (typeof filterMedia.addEventListener ===
        "function") {
        filterMedia.addEventListener(
            "change",
            syncFilterState);
    }
    else {
        filterMedia.addListener(
            syncFilterState);
    }


    document
        .querySelectorAll(
            "[data-course-thumbnail]")
        .forEach(image => {
            image.addEventListener(
                "error",
                () => {
                    const fallback =
                        image.dataset.fallback;

                    const placeholder =
                        image.dataset.placeholder;

                    if (!image.dataset.fallbackTried &&
                        fallback) {
                        image.dataset.fallbackTried =
                            "true";

                        image.src =
                            fallback;

                        return;
                    }

                    if (!image.dataset.placeholderTried &&
                        placeholder) {
                        image.dataset.placeholderTried =
                            "true";

                        image.src =
                            placeholder;
                    }
                });
        });


    document.addEventListener(
        "keydown",
        event => {
            if (event.key === "Escape" &&
                filterPanel?.classList.contains(
                    "is-open")) {
                closeFilter();
            }
        });
})();
