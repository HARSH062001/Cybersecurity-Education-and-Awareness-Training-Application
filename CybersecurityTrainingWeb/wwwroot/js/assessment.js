(() => {
    const assessment = document.querySelector("[data-assessment]");
    if (!assessment) return;

    const moduleId = assessment.dataset.moduleId;
    const bank = assessment.dataset.bank;
    const panels = [...assessment.querySelectorAll("[data-question-panel]")];
    const navigation = [...assessment.querySelectorAll("[data-question-nav]")];
    const total = panels.length;
    const storageKey = `cybertraining.assessment.${moduleId}.${bank}.v1`;
    let current = 0;
    let responses = {};

    try { responses = JSON.parse(sessionStorage.getItem(storageKey) || "{}"); } catch { responses = {}; }

    const answeredCount = () => Object.values(responses).filter(value => String(value || "").trim()).length;

    const update = () => {
        panels.forEach((panel, index) => panel.hidden = index !== current);
        navigation.forEach((button, index) => {
            button.classList.toggle("is-active", index === current);
            button.classList.toggle("is-answered", Boolean(responses[panels[index].querySelector("[data-answer-input]")?.dataset.answerInput]));
            button.setAttribute("aria-current", index === current ? "step" : "false");
        });
        assessment.querySelectorAll("[data-assessment-count]").forEach(value => value.textContent = `Question ${current + 1} of ${total}`);
        assessment.querySelectorAll("[data-assessment-answered]").forEach(value => value.textContent = `${answeredCount()} answered`);
        assessment.querySelectorAll("[data-assessment-bar]").forEach(value => value.style.width = `${((current + 1) / total) * 100}%`);
        const panel = panels[current];
        panel.querySelector("[data-previous-question]")?.toggleAttribute("disabled", current === 0);
        panel.querySelector("[data-next-question]")?.toggleAttribute("hidden", current === total - 1);
        panel.querySelector("[data-submit-assessment]")?.toggleAttribute("hidden", current !== total - 1);
    };

    assessment.querySelectorAll("[data-answer-input]").forEach(input => {
        const key = input.dataset.answerInput;
        if (input.type === "radio" && responses[key] === input.value) input.checked = true;
        if (input.tagName === "TEXTAREA") input.value = responses[key] || "";
        input.addEventListener("input", () => {
            responses[key] = input.type === "radio" ? assessment.querySelector(`input[name='${input.name}']:checked`)?.value || "" : input.value;
            sessionStorage.setItem(storageKey, JSON.stringify(responses));
            update();
        });
        input.addEventListener("change", () => {
            responses[key] = input.type === "radio" ? assessment.querySelector(`input[name='${input.name}']:checked`)?.value || "" : input.value;
            sessionStorage.setItem(storageKey, JSON.stringify(responses));
            update();
        });
    });

    navigation.forEach((button, index) => button.addEventListener("click", () => { current = index; update(); panels[current].scrollIntoView({ behavior: "smooth", block: "start" }); }));
    assessment.querySelectorAll("[data-previous-question]").forEach(button => button.addEventListener("click", () => { if (current > 0) { current--; update(); } }));
    assessment.querySelectorAll("[data-next-question]").forEach(button => button.addEventListener("click", () => { if (current < total - 1) { current++; update(); } }));
    assessment.querySelectorAll("[data-submit-assessment]").forEach(button => button.addEventListener("click", () => {
        const answered = answeredCount();
        const confirmation = window.confirm(`Submit this frontend preview with ${answered} of ${total} responses? The backend will calculate the final result when connected.`);
        if (confirmation) window.location.assign(`/Results?moduleId=${encodeURIComponent(moduleId)}&bank=${encodeURIComponent(bank)}&answered=${answered}`);
    }));

    update();
})();
