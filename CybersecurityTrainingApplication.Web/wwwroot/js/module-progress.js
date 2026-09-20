(() => {
    const storageKey = "cybertraining.module.1.sections.v1";
    const sectionKeys = ["purpose", "outcomes", "terms", "concepts", "workplace", "behaviours", "review"];

    const loadCompleted = () => {
        try {
            const stored = JSON.parse(localStorage.getItem(storageKey) || "[]");
            return new Set(stored.filter(key => sectionKeys.includes(key)));
        } catch {
            return new Set();
        }
    };

    const saveCompleted = completed => {
        localStorage.setItem(storageKey, JSON.stringify([...completed]));
        window.dispatchEvent(new CustomEvent("moduleprogresschanged", { detail: { moduleId: 1 } }));
    };

    const replaceCompleted = (completed, keys) => {
        completed.clear();
        keys.filter(key => sectionKeys.includes(key)).forEach(key => completed.add(key));
        saveCompleted(completed);
    };

    const getProgress = completed => Math.round((completed.size / sectionKeys.length) * 100);

    const updateModulePage = completed => {
        const workspace = document.querySelector("[data-module-learning='1']");
        if (!workspace) return;

        const percentage = getProgress(completed);
        workspace.querySelector("[data-progress-ring]")?.style.setProperty("--progress", percentage);
        workspace.querySelectorAll("[data-progress-percent]").forEach(element => element.textContent = `${percentage}%`);
        workspace.querySelectorAll("[data-progress-count]").forEach(element => element.textContent = `${completed.size} of ${sectionKeys.length} sections`);

        sectionKeys.forEach(key => {
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

    const updateSummaryPages = completed => {
        const percentage = getProgress(completed);
        document.querySelectorAll("[data-module-one-progress]").forEach(element => {
            element.style.setProperty("--module-progress", percentage);
            element.querySelectorAll("[data-progress-value]").forEach(value => value.textContent = `${percentage}%`);
            element.querySelectorAll("[data-progress-sections]").forEach(value => value.textContent = `${completed.size} of ${sectionKeys.length} sections complete`);
            const action = element.querySelector("[data-progress-action]");
            if (action) action.textContent = percentage === 0 ? "Start Module 1" : percentage === 100 ? "Review Module 1" : "Continue Module 1";
        });
    };

    const completed = loadCompleted();
    updateModulePage(completed);
    updateSummaryPages(completed);

    fetch("/api/progress/modules/1")
        .then(response => response.ok ? response.json() : Promise.reject())
        .then(data => {
            replaceCompleted(completed, data.completedSections || []);
            updateModulePage(completed);
            updateSummaryPages(completed);
        })
        .catch(() => { /* Device storage remains available while the API is unavailable. */ });

    document.querySelectorAll("[data-complete-section]").forEach(button => {
        button.addEventListener("click", async () => {
            const key = button.dataset.completeSection;
            if (!key) return;
            completed.has(key) ? completed.delete(key) : completed.add(key);
            saveCompleted(completed);
            updateModulePage(completed);
            updateSummaryPages(completed);
            try {
                const response = await fetch("/api/progress/modules/1", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ sectionKey: key, completed: completed.has(key) })
                });
                if (!response.ok) return;
                const data = await response.json();
                replaceCompleted(completed, data.completedSections || []);
                updateModulePage(completed);
                updateSummaryPages(completed);
            } catch {
                /* Keep the local completion state and retry through the next learner action. */
            }
        });
    });
})();
