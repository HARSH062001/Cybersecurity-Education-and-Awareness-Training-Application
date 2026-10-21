(() => {
    const sectionKeys = [
        "purpose", "outcomes", "terms", "concepts",
        "workplace", "behaviours", "review"
    ];

    const signedIn = document.body.dataset.authenticated === "true";
    const workspace = document.querySelector("[data-module-learning]");
    const moduleId = Number(workspace?.dataset.moduleLearning || 0);
    const workspaceKeys = workspace
        ? [...workspace.querySelectorAll("[data-progress-section]")]
            .map(element => element.dataset.progressSection)
            .filter(Boolean)
        : sectionKeys;

    const moduleNames = new Map(
        [...document.querySelectorAll("[data-dashboard-modules] [data-module-id]")]
            .map(element => [
                Number(element.dataset.moduleId),
                element.dataset.moduleTitle
            ])
    );

    const progressByModule = new Map();

    function showMessage(message, kind = "warning") {
        const container = workspace || document.querySelector("#main-content");
        if (!container) return;

        let notice = container.querySelector("[data-progress-notice]");
        if (!notice) {
            notice = document.createElement("p");
            notice.dataset.progressNotice = "";
            notice.setAttribute("role", "status");
            container.prepend(notice);
        }

        notice.className = `alert alert-${kind}`;
        notice.textContent = message;
        notice.hidden = !message;
    }

    function percentageOf(completed, total = sectionKeys.length) {
        return total ? Math.round((completed.size / total) * 100) : 0;
    }

    function updateSummaryCards(id, completed) {
        const percentage = percentageOf(completed);

        document.querySelectorAll(`[data-module-progress="${id}"]`)
            .forEach(card => {
                card.style.setProperty("--module-progress", percentage);

                card.querySelectorAll("[data-progress-value]")
                    .forEach(element => {
                        element.textContent = `${percentage}%`;
                    });

                card.querySelectorAll("[data-progress-sections]")
                    .forEach(element => {
                        element.textContent =
                            `${completed.size} of ${sectionKeys.length} sections complete`;
                    });

                card.querySelectorAll("[data-module-bar]")
                    .forEach(element => {
                        element.style.width = `${percentage}%`;
                    });

                card.querySelectorAll("[data-module-status]")
                    .forEach(element => {
                        element.textContent = percentage === 100
                            ? "Complete"
                            : percentage > 0 ? "In progress" : "Not started";
                    });

                card.querySelectorAll('[role="progressbar"]')
                    .forEach(element => {
                        element.setAttribute("aria-valuenow", String(percentage));
                    });

                const action = card.querySelector("[data-progress-action]");
                if (action) {
                    action.textContent = percentage === 0
                        ? `Start Module ${id}`
                        : percentage === 100
                            ? `Review Module ${id}`
                            : `Continue Module ${id}`;
                }
            });
    }

    function updateOverallProgress() {
        const completedSections = [...progressByModule.values()]
            .reduce((total, completed) => total + completed.size, 0);

        const completedModules = [...progressByModule.values()]
            .filter(completed => completed.size === sectionKeys.length)
            .length;

        const percentage = Math.round(
            (completedSections / (8 * sectionKeys.length)) * 100
        );

        document.querySelectorAll("[data-overall-progress]")
            .forEach(card => {
                card.querySelectorAll("[data-overall-percent]")
                    .forEach(element => {
                        element.textContent = `${percentage}%`;
                    });

                card.querySelectorAll("[data-overall-detail]")
                    .forEach(element => {
                        element.textContent =
                            `${completedModules} of 8 modules completed`;
                    });

                card.querySelectorAll("[data-overall-bar]")
                    .forEach(element => {
                        element.style.width = `${percentage}%`;
                    });

                card.querySelectorAll('[role="progressbar"]')
                    .forEach(element => {
                        element.setAttribute(
                            "aria-valuenow",
                            String(percentage)
                        );
                    });
            });
    }

    function updateDashboardFocus() {
        const focusCards = document.querySelectorAll("[data-dashboard-focus]");
        if (!focusCards.length) return;

        const availableIds = moduleNames.size
            ? [...moduleNames.keys()].sort((a, b) => a - b)
            : [1, 2, 3, 4, 5, 6, 7, 8];

        const firstUnfinished = availableIds.find(id =>
            (progressByModule.get(id)?.size || 0) < sectionKeys.length
        );

        const allComplete = firstUnfinished === undefined;
        const currentId = firstUnfinished ?? availableIds.at(-1) ?? 1;
        const currentProgress = progressByModule.get(currentId) ?? new Set();

        focusCards.forEach(card => {
            card.dataset.moduleProgress = String(currentId);
        });

        updateSummaryCards(currentId, currentProgress);

        const label = document.querySelector("[data-current-module-label]");
        if (label) label.textContent = `Module ${currentId} progress`;

        const progressBar = document.querySelector(
            "[data-dashboard-focus] [role='progressbar']"
        );
        progressBar?.setAttribute(
            "aria-label",
            `Module ${currentId} progress`
        );

        const title = document.querySelector("[data-current-module-title]");
        if (title) {
            title.textContent = allComplete
                ? "All eight modules completed"
                : moduleNames.get(currentId) || `Module ${currentId}`;
        }

        const action = document.querySelector(
            "[data-dashboard-focus] [data-progress-action]"
        );
        if (action) {
            action.href = allComplete
                ? "/Modules"
                : `/Module?id=${currentId}`;

            if (allComplete) action.textContent = "Review all modules";
        }
    }

    function updateModulePage(completed) {
        if (!workspace) return;

        const percentage = percentageOf(completed, workspaceKeys.length);

        workspace.querySelector("[data-progress-ring]")
            ?.style.setProperty("--progress", percentage);

        workspace.querySelectorAll("[data-progress-percent]")
            .forEach(element => {
                element.textContent = `${percentage}%`;
            });

        workspace.querySelectorAll("[data-progress-count]")
            .forEach(element => {
                element.textContent =
                    `${completed.size} of ${workspaceKeys.length} sections`;
            });

        workspaceKeys.forEach(key => {
            const isComplete = completed.has(key);
            const section = workspace.querySelector(
                `[data-progress-section="${key}"]`
            );
            const link = workspace.querySelector(
                `[data-section-link="${key}"]`
            );

            section?.classList.toggle("is-complete", isComplete);
            link?.classList.toggle("is-complete", isComplete);

            const status = section?.querySelector("[data-section-status]");
            const label = section?.querySelector("[data-button-label]");
            const button = section?.querySelector("[data-complete-section]");

            if (status) {
                status.textContent = isComplete
                    ? "Completed"
                    : "Not completed";
            }

            if (label) {
                label.textContent = isComplete
                    ? "Completed — undo"
                    : "Mark section complete";
            }

            button?.setAttribute("aria-pressed", String(isComplete));
        });

        const banner = workspace.querySelector("[data-completion-banner]");
        if (banner) banner.hidden = percentage !== 100;
    }

    function parseProgress(data, allowedKeys) {
        if (!Array.isArray(data?.completedSections)) {
            throw new Error("The progress response was invalid.");
        }

        return new Set(
            data.completedSections.filter(key => allowedKeys.includes(key))
        );
    }

    async function getProgress(id, allowedKeys = sectionKeys) {
        const response = await fetch(`/api/progress/modules/${id}`, {
            credentials: "same-origin",
            cache: "no-store"
        });

        if (!response.ok) {
            throw new Error(`Progress could not be loaded (${response.status}).`);
        }

        return parseProgress(await response.json(), allowedKeys);
    }

    if (!signedIn) {
        if (workspace) {
            workspace.querySelectorAll("[data-complete-section]")
                .forEach(button => {
                    button.disabled = true;
                });

            showMessage(
                "Sign in to save and view your own learning progress.",
                "info"
            );
        }

        // The page's initial HTML shows 0%. Never read another visitor's
        // browser-stored progress.
        return;
    }

    if (workspace) {
        const buttons = [
            ...workspace.querySelectorAll("[data-complete-section]")
        ];
        let completed = new Set();

        buttons.forEach(button => {
            button.disabled = true;
        });

        showMessage("Loading your progress…", "info");

        async function loadModule() {
            try {
                completed = await getProgress(moduleId, workspaceKeys);
                updateModulePage(completed);
                updateSummaryCards(moduleId, completed);
                buttons.forEach(button => {
                    button.disabled = false;
                });
                showMessage("");
            } catch {
                showMessage(
                    "Your progress could not be loaded. Try refreshing the page. Completion controls are unavailable until it loads."
                );
            }
        }

        buttons.forEach(button => {
            button.addEventListener("click", async () => {
                const key = button.dataset.completeSection;
                if (!workspaceKeys.includes(key)) return;

                const token = document.querySelector(
                    'meta[name="csrf-token"]'
                )?.content;

                if (!token) {
                    showMessage(
                        "Your security token is missing. Refresh the page and try again."
                    );
                    return;
                }

                button.disabled = true;

                try {
                    const response = await fetch(
                        `/api/progress/modules/${moduleId}`,
                        {
                            method: "POST",
                            credentials: "same-origin",
                            cache: "no-store",
                            headers: {
                                "Content-Type": "application/json",
                                "X-CSRF-TOKEN": token
                            },
                            body: JSON.stringify({
                                sectionKey: key,
                                completed: !completed.has(key)
                            })
                        }
                    );

                    if (!response.ok) {
                        throw new Error(
                            `Progress could not be saved (${response.status}).`
                        );
                    }

                    // Change the display only after the server confirms it.
                    completed = parseProgress(
                        await response.json(),
                        workspaceKeys
                    );

                    updateModulePage(completed);
                    updateSummaryCards(moduleId, completed);
                    showMessage("");

                    window.dispatchEvent(
                        new CustomEvent("moduleprogresschanged", {
                            detail: { moduleId }
                        })
                    );
                } catch {
                    showMessage(
                        "Your change was not saved. Check your connection, then try again."
                    );
                } finally {
                    button.disabled = false;
                }
            });
        });

        void loadModule();
        return;
    }

    const ids = new Set(
        [...document.querySelectorAll("[data-module-progress]")]
            .map(card => Number(card.dataset.moduleProgress))
            .filter(id => id >= 1 && id <= 8)
    );

    if (document.querySelector("[data-overall-progress]")) {
        for (let id = 1; id <= 8; id++) ids.add(id);
    }

    if (!ids.size) return;

    showMessage("Loading your progress…", "info");

    void Promise.all(
        [...ids].map(async id => {
            const completed = await getProgress(id);
            return { id, completed };
        })
    )
        .then(results => {
            results.forEach(({ id, completed }) => {
                progressByModule.set(id, completed);
                updateSummaryCards(id, completed);
            });

            updateOverallProgress();
            updateDashboardFocus();
            showMessage("");
        })
        .catch(() => {
            showMessage(
                "Your progress could not be loaded. The percentages below may not reflect your saved progress; try refreshing the page."
            );
        });
})();