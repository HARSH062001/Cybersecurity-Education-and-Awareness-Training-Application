(() => {
    const sectionKeys = [
        "purpose", "outcomes", "terms", "concepts",
        "workplace", "behaviours", "review"
    ];

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

    const storageKey = id => `cybertraining.module.${id}.sections.v1`;

    const readCompleted = (id, allowedKeys = sectionKeys) => {
        try {
            const saved = JSON.parse(localStorage.getItem(storageKey(id)) || "[]");
            return new Set(
                Array.isArray(saved)
                    ? saved.filter(key => allowedKeys.includes(key))
                    : []
            );
        } catch {
            return new Set();
        }
    };

    const saveCompleted = (id, completed) => {
        try {
            localStorage.setItem(storageKey(id), JSON.stringify([...completed]));
        } catch {
            // Progress still works on this page if browser storage is unavailable.
        }

        window.dispatchEvent(
            new CustomEvent("moduleprogresschanged", { detail: { moduleId: id } })
        );
    };

    const percentageOf = (completed, total = sectionKeys.length) =>
        total ? Math.round((completed.size / total) * 100) : 0;

    const updateSummaryCards = (id, completed) => {
        const percentage = percentageOf(completed);

        document.querySelectorAll(`[data-module-progress="${id}"]`).forEach(card => {
            card.style.setProperty("--module-progress", percentage);

            card.querySelectorAll("[data-progress-value]").forEach(element => {
                element.textContent = `${percentage}%`;
            });

            card.querySelectorAll("[data-progress-sections]").forEach(element => {
                element.textContent =
                    `${completed.size} of ${sectionKeys.length} sections complete`;
            });

            card.querySelectorAll("[data-module-bar]").forEach(element => {
                element.style.width = `${percentage}%`;
            });

            card.querySelectorAll("[data-module-status]").forEach(element => {
                element.textContent = percentage === 100
                    ? "Complete"
                    : percentage > 0 ? "In progress" : "Not started";
            });

            card.querySelectorAll('[role="progressbar"]').forEach(element => {
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
    };

    const updateOverallProgress = () => {
        if (!document.querySelector("[data-overall-progress]")) return;

        const completedSections = [...progressByModule.values()]
            .reduce((total, completed) => total + completed.size, 0);

        const completedModules = [...progressByModule.values()]
            .filter(completed => completed.size === sectionKeys.length).length;

        const percentage = Math.round(
            (completedSections / (8 * sectionKeys.length)) * 100
        );

        document.querySelectorAll("[data-overall-progress]").forEach(card => {
            card.querySelectorAll("[data-overall-percent]").forEach(element => {
                element.textContent = `${percentage}%`;
            });

            card.querySelectorAll("[data-overall-detail]").forEach(element => {
                element.textContent =
                    `${completedModules} of 8 modules completed`;
            });

            card.querySelectorAll("[data-overall-bar]").forEach(element => {
                element.style.width = `${percentage}%`;
            });

            card.querySelectorAll('[role="progressbar"]').forEach(element => {
                element.setAttribute("aria-valuenow", String(percentage));
            });
        });
    };

    const updateDashboardFocus = () => {
        const focusCards = document.querySelectorAll("[data-dashboard-focus]");
        if (!focusCards.length) return;

        // Move forward only when every section of the current module is complete.
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
        if (progressBar) {
            progressBar.setAttribute(
                "aria-label",
                `Module ${currentId} progress`
            );
        }

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
    };

    const updateModulePage = completed => {
        if (!workspace) return;

        const percentage = percentageOf(completed, workspaceKeys.length);

        workspace.querySelector("[data-progress-ring]")
            ?.style.setProperty("--progress", percentage);

        workspace.querySelectorAll("[data-progress-percent]").forEach(element => {
            element.textContent = `${percentage}%`;
        });

        workspace.querySelectorAll("[data-progress-count]").forEach(element => {
            element.textContent =
                `${completed.size} of ${workspaceKeys.length} sections`;
        });

        workspaceKeys.forEach(key => {
            const isComplete = completed.has(key);
            const learningSection = workspace.querySelector(
                `[data-progress-section="${key}"]`
            );
            const navigationLink = workspace.querySelector(
                `[data-section-link="${key}"]`
            );

            learningSection?.classList.toggle("is-complete", isComplete);
            navigationLink?.classList.toggle("is-complete", isComplete);

            const status = learningSection?.querySelector("[data-section-status]");
            const label = learningSection?.querySelector("[data-button-label]");
            const button = learningSection?.querySelector("[data-complete-section]");

            if (status) status.textContent =
                isComplete ? "Completed" : "Not completed";

            if (label) label.textContent =
                isComplete ? "Completed — undo" : "Mark section complete";

            button?.setAttribute("aria-pressed", String(isComplete));
        });

        const banner = workspace.querySelector("[data-completion-banner]");
        if (banner) banner.hidden = percentage !== 100;
    };

    const refreshFromApi = id =>
        fetch(`/api/progress/modules/${id}`)
            .then(response => response.ok ? response.json() : Promise.reject())
            .then(data => {
                if (!Array.isArray(data.completedSections)) return;

                const completed = new Set(
                    data.completedSections.filter(key => sectionKeys.includes(key))
                );

                saveCompleted(id, completed);
                progressByModule.set(id, completed);
                updateSummaryCards(id, completed);
                updateOverallProgress();
                updateDashboardFocus();
            })
            .catch(() => {
                // Use saved browser progress until the backend is connected.
            });

    if (!workspace) {
        const ids = new Set(
            [...document.querySelectorAll("[data-module-progress]")]
                .map(card => Number(card.dataset.moduleProgress))
                .filter(id => id >= 1 && id <= 8)
        );

        if (document.querySelector("[data-overall-progress]")) {
            for (let id = 1; id <= 8; id++) ids.add(id);
        }

        // Read all saved modules before deciding what the dashboard should show.
        ids.forEach(id => {
            const completed = readCompleted(id);
            progressByModule.set(id, completed);
            updateSummaryCards(id, completed);
        });

        updateOverallProgress();
        updateDashboardFocus();

        ids.forEach(id => { void refreshFromApi(id); });
        return;
    }

    const completed = readCompleted(moduleId, workspaceKeys);
    updateModulePage(completed);
    updateSummaryCards(moduleId, completed);

    fetch(`/api/progress/modules/${moduleId}`)
        .then(response => response.ok ? response.json() : Promise.reject())
        .then(data => {
            if (!Array.isArray(data.completedSections)) return;

            completed.clear();
            data.completedSections
                .filter(key => workspaceKeys.includes(key))
                .forEach(key => completed.add(key));

            saveCompleted(moduleId, completed);
            updateModulePage(completed);
            updateSummaryCards(moduleId, completed);
        })
        .catch(() => {
            // Keep the learner's saved browser progress.
        });

    document.querySelectorAll("[data-complete-section]").forEach(button => {
        button.addEventListener("click", async () => {
            const key = button.dataset.completeSection;
            if (!workspaceKeys.includes(key)) return;

            if (completed.has(key)) completed.delete(key);
            else completed.add(key);

            saveCompleted(moduleId, completed);
            updateModulePage(completed);
            updateSummaryCards(moduleId, completed);

            try {
                const response = await fetch(
                    `/api/progress/modules/${moduleId}`,
                    {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({
                            sectionKey: key,
                            completed: completed.has(key)
                        })
                    }
                );

                if (!response.ok) return;

                const data = await response.json();
                if (!Array.isArray(data.completedSections)) return;

                completed.clear();
                data.completedSections
                    .filter(item => workspaceKeys.includes(item))
                    .forEach(item => completed.add(item));

                saveCompleted(moduleId, completed);
                updateModulePage(completed);
                updateSummaryCards(moduleId, completed);
            } catch {
                // The section remains saved in this browser.
            }
        });
    });
})();