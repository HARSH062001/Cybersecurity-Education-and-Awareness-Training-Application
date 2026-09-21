(() => {
    const workspace = document.querySelector("[data-module-learning]");
    const sectionKeys = ["purpose", "outcomes", "terms", "concepts", "workplace", "behaviours", "review"];
    const moduleId = Number(workspace?.dataset.moduleLearning || 0);
    const keysForWorkspace = workspace ? [...workspace.querySelectorAll("[data-progress-section]")].map(section => section.dataset.progressSection).filter(Boolean) : sectionKeys;
    const storageKey = `cybertraining.module.${moduleId}.sections.v1`;

    const loadCompleted = () => {
        try {
            const stored = JSON.parse(localStorage.getItem(storageKey) || "[]");
            return new Set(stored.filter(key => keysForWorkspace.includes(key)));
        } catch {
            return new Set();
        }
    };

    const saveCompleted = completed => {
        localStorage.setItem(storageKey, JSON.stringify([...completed]));
        window.dispatchEvent(new CustomEvent("moduleprogresschanged", { detail: { moduleId } }));
    };

    const replaceCompleted = (completed, keys) => {
        completed.clear();
        keys.filter(key => keysForWorkspace.includes(key)).forEach(key => completed.add(key));
        if (moduleId) saveCompleted(completed);
    };

    const getProgress = (completed, total = keysForWorkspace.length) => Math.round((completed.size / total) * 100);

    const updateSummaryCards = (summaryModuleId, completedKeys) => {
        const percentage = getProgress(completedKeys, sectionKeys.length);
        document.querySelectorAll(`[data-module-progress='${summaryModuleId}']`).forEach(element => {
            element.style.setProperty("--module-progress", percentage);
            element.querySelectorAll("[data-progress-value]").forEach(value => value.textContent = `${percentage}%`);
            element.querySelectorAll("[data-progress-sections]").forEach(value => value.textContent = `${completedKeys.size} of ${sectionKeys.length} sections complete`);
            element.querySelectorAll("[data-module-bar]").forEach(value => value.style.width = `${percentage}%`);
            element.querySelectorAll("[data-module-status]").forEach(value => value.textContent = percentage === 100 ? "Complete" : percentage > 0 ? "In progress" : "Not started");
            const action = element.querySelector("[data-progress-action]");
            if (action) action.textContent = percentage === 0 ? `Start Module ${summaryModuleId}` : percentage === 100 ? `Review Module ${summaryModuleId}` : `Continue Module ${summaryModuleId}`;
        });
    };

    const progressByModule = new Map();
    const updateOverallProgress = () => {
        if (!document.querySelector("[data-overall-progress]")) return;
        const completedSections = [...progressByModule.values()].reduce((total, completed) => total + completed.size, 0);
        const completedModules = [...progressByModule.values()].filter(completed => completed.size === sectionKeys.length).length;
        const percentage = Math.round((completedSections / (8 * sectionKeys.length)) * 100);
        document.querySelectorAll("[data-overall-progress]").forEach(element => {
            element.querySelectorAll("[data-overall-percent]").forEach(value => value.textContent = `${percentage}%`);
            element.querySelectorAll("[data-overall-detail]").forEach(value => value.textContent = `${completedModules} of 8 modules completed`);
            element.querySelectorAll("[data-overall-bar]").forEach(value => value.style.width = `${percentage}%`);
        });
    };

    const updateModulePage = completed => {
        if (!workspace) return;

        const percentage = getProgress(completed);
        workspace.querySelector("[data-progress-ring]")?.style.setProperty("--progress", percentage);
        workspace.querySelectorAll("[data-progress-percent]").forEach(element => element.textContent = `${percentage}%`);
        workspace.querySelectorAll("[data-progress-count]").forEach(element => element.textContent = `${completed.size} of ${sectionKeys.length} sections`);

        keysForWorkspace.forEach(key => {
            const isComplete = completed.has(key);
            const section = workspace.querySelector(`[data-progress-section='${key}']`);
            const navigationLink = workspace.querySelector(`[data-section-link='${key}']`);
            section?.classList.toggle("is-complete", isComplete);
            navigationLink?.classList.toggle("is-complete", isComplete);
            const status = section?.querySelector("[data-section-status]");
            const label = section?.querySelector("[data-button-label]");
            const button = section?.querySelector("[data-complete-section]");
            if (status) status.textContent = isComplete ? "Completed" : "Not completed";
            if (label) label.textContent = isComplete ? "Completed — undo" : "Mark section complete";
            button?.setAttribute("aria-pressed", isComplete.toString());
        });

        const banner = workspace.querySelector("[data-completion-banner]");
        if (banner) banner.hidden = percentage !== 100;
    };

    const loadModuleProgress = summaryModuleId => {
        const summaryStorageKey = `cybertraining.module.${summaryModuleId}.sections.v1`;
        let summaryCompleted = new Set();
        try { summaryCompleted = new Set(JSON.parse(localStorage.getItem(summaryStorageKey) || "[]").filter(key => sectionKeys.includes(key))); } catch { /* Empty progress is a valid first state. */ }
        updateSummaryCards(summaryModuleId, summaryCompleted);
        progressByModule.set(summaryModuleId, summaryCompleted);
        updateOverallProgress();
        return fetch(`/api/progress/modules/${summaryModuleId}`)
            .then(response => response.ok ? response.json() : Promise.reject())
            .then(data => {
                summaryCompleted = new Set((data.completedSections || []).filter(key => sectionKeys.includes(key)));
                localStorage.setItem(summaryStorageKey, JSON.stringify([...summaryCompleted]));
                updateSummaryCards(summaryModuleId, summaryCompleted);
                progressByModule.set(summaryModuleId, summaryCompleted);
                updateOverallProgress();
            })
            .catch(() => { /* Device storage remains available while the API is unavailable. */ });
    };

    if (!workspace) {
        const summaryModuleIds = new Set([...document.querySelectorAll("[data-module-progress]")].map(card => Number(card.dataset.moduleProgress)));
        if (document.querySelector("[data-overall-progress]")) for (let summaryModuleId = 1; summaryModuleId <= 8; summaryModuleId++) summaryModuleIds.add(summaryModuleId);
        summaryModuleIds.forEach(loadModuleProgress);
        return;
    }

    const completed = loadCompleted();
    updateModulePage(completed);
    updateSummaryCards(moduleId, completed);

    fetch(`/api/progress/modules/${moduleId}`)
        .then(response => response.ok ? response.json() : Promise.reject())
        .then(data => {
            replaceCompleted(completed, data.completedSections || []);
            updateModulePage(completed);
            updateSummaryCards(moduleId, completed);
        })
        .catch(() => { /* Device storage remains available while the API is unavailable. */ });

    document.querySelectorAll("[data-complete-section]").forEach(button => {
        button.addEventListener("click", async () => {
            const key = button.dataset.completeSection;
            if (!key) return;
            completed.has(key) ? completed.delete(key) : completed.add(key);
            saveCompleted(completed);
            updateModulePage(completed);
            updateSummaryCards(moduleId, completed);
            try {
                const response = await fetch(`/api/progress/modules/${moduleId}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ sectionKey: key, completed: completed.has(key) })
                });
                if (!response.ok) return;
                const data = await response.json();
                replaceCompleted(completed, data.completedSections || []);
                updateModulePage(completed);
                updateSummaryCards(moduleId, completed);
            } catch {
                /* Keep the local completion state and retry through the next learner action. */
            }
        });
    });
})();
