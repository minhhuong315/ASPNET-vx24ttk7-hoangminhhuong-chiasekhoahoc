(() => {
    "use strict";

    const STORAGE_KEY =
        "edulearn.workspace.sidebar.collapsed";

    const desktopMedia =
        window.matchMedia(
            "(min-width: 1081px)"
        );

    const workspace =
        document.querySelector(
            ".role-workspace"
        );

    const toggleButton =
        document.querySelector(
            "[data-dashboard-sidebar-toggle]"
        );

    if (!workspace || !toggleButton) {
        return;
    }

    const backdrop =
        document.createElement(
            "div"
        );

    backdrop.className =
        "dashboard-sidebar-backdrop";

    document.body.appendChild(
        backdrop
    );


    function readCollapsed() {
        try {
            return localStorage.getItem(
                STORAGE_KEY
            ) === "true";
        }
        catch {
            return false;
        }
    }


    function saveCollapsed(value) {
        try {
            localStorage.setItem(
                STORAGE_KEY,
                value.toString()
            );
        }
        catch {
        }
    }


    function setDesktopCollapsed(collapsed) {
        workspace.classList.toggle(
            "role-workspace--sidebar-collapsed",
            collapsed
        );

        document.body.classList.toggle(
            "workspace-sidebar-collapsed",
            collapsed
        );

        workspace.classList.remove(
            "role-workspace--mobile-sidebar-open"
        );

        backdrop.classList.remove(
            "is-visible"
        );

        document.body.classList.remove(
            "dashboard-mobile-sidebar-open"
        );

        toggleButton.classList.toggle(
            "is-collapsed",
            collapsed
        );

        toggleButton.setAttribute(
            "aria-expanded",
            collapsed
                ? "false"
                : "true"
        );

        const label =
            collapsed
                ? "Mở rộng menu tác vụ"
                : "Thu gọn menu tác vụ";

        toggleButton.setAttribute(
            "aria-label",
            label
        );

        toggleButton.setAttribute(
            "title",
            label
        );
    }


    function setMobileOpen(open) {
        workspace.classList.remove(
            "role-workspace--sidebar-collapsed"
        );

        document.body.classList.remove(
            "workspace-sidebar-collapsed"
        );

        workspace.classList.toggle(
            "role-workspace--mobile-sidebar-open",
            open
        );

        backdrop.classList.toggle(
            "is-visible",
            open
        );

        document.body.classList.toggle(
            "dashboard-mobile-sidebar-open",
            open
        );

        toggleButton.classList.remove(
            "is-collapsed"
        );

        toggleButton.setAttribute(
            "aria-expanded",
            open
                ? "true"
                : "false"
        );

        toggleButton.setAttribute(
            "aria-label",
            open
                ? "Đóng menu tác vụ"
                : "Mở menu tác vụ"
        );
    }


    function applyMode() {
        if (desktopMedia.matches) {
            setDesktopCollapsed(
                readCollapsed()
            );
        }
        else {
            setMobileOpen(
                false
            );
        }
    }


    toggleButton.addEventListener(
        "click",
        () => {
            if (desktopMedia.matches) {
                const collapsed =
                    !workspace.classList.contains(
                        "role-workspace--sidebar-collapsed"
                    );

                setDesktopCollapsed(
                    collapsed
                );

                saveCollapsed(
                    collapsed
                );
            }
            else {
                const open =
                    !workspace.classList.contains(
                        "role-workspace--mobile-sidebar-open"
                    );

                setMobileOpen(
                    open
                );
            }
        }
    );


    backdrop.addEventListener(
        "click",
        () => {
            if (!desktopMedia.matches) {
                setMobileOpen(
                    false
                );
            }
        }
    );


    workspace.addEventListener(
        "click",
        event => {
            if (desktopMedia.matches) {
                return;
            }

            if (
                event.target.closest(
                    ".role-nav-link"
                )
            ) {
                setMobileOpen(
                    false
                );
            }
        }
    );


    document.addEventListener(
        "keydown",
        event => {
            if (
                event.key === "Escape" &&
                !desktopMedia.matches
            ) {
                setMobileOpen(
                    false
                );
            }
        }
    );


    if (
        typeof desktopMedia.addEventListener ===
        "function"
    ) {
        desktopMedia.addEventListener(
            "change",
            applyMode
        );
    }
    else {
        desktopMedia.addListener(
            applyMode
        );
    }


    applyMode();
})();
