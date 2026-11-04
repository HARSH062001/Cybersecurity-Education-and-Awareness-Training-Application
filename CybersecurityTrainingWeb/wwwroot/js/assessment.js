(() => {
    const assessment = document.querySelector("[data-assessment]");
    if (!assessment) return;

    const attemptId = assessment.dataset.attemptId;
    const panels = [...assessment.querySelectorAll("[data-question-panel]")];
    const navigation = [...assessment.querySelectorAll("[data-question-nav]")];
    const total = panels.length;
    const storageKey = `cybertraining.assessment.attempt.${attemptId}.v1`;

    let current = 0;
    let responses = {};

    try {
        responses = JSON.parse(sessionStorage.getItem(storageKey) || "{}");
    } catch {
        responses = {};
    }

    const answeredCount = () =>
        panels.filter(panel => {
            const key = panel.querySelector("[data-answer-input]")?.dataset.answerInput;
            return key && String(responses[key] || "").trim();
        }).length;

    const update = () => {
        panels.forEach((panel, index) => {
            panel.hidden = index !== current;
        });

        navigation.forEach((button, index) => {
            const isCurrent = index === current;
            const key = panels[index].querySelector("[data-answer-input]")?.dataset.answerInput;
            const isAnswered = key && String(responses[key] || "").trim();

            button.classList.toggle("is-current", isCurrent);
            button.classList.toggle("is-answered", !isCurrent && Boolean(isAnswered));
            button.setAttribute("aria-current", isCurrent ? "step" : "false");
        });

        assessment.querySelectorAll("[data-assessment-count]").forEach(value => {
            value.textContent = `Question ${current + 1} of ${total}`;
        });

        assessment.querySelectorAll("[data-assessment-answered]").forEach(value => {
            value.textContent = `${answeredCount()} answered`;
        });

        assessment.querySelectorAll("[data-assessment-bar]").forEach(value => {
            value.style.width = `${((current + 1) / total) * 100}%`;
        });

        const panel = panels[current];
        panel.querySelector("[data-previous-question]")
            ?.toggleAttribute("disabled", current === 0);
        panel.querySelector("[data-next-question]")
            ?.toggleAttribute("hidden", current === total - 1);
        panel.querySelector("[data-submit-assessment]")
            ?.toggleAttribute("hidden", current !== total - 1);
    };

    assessment.querySelectorAll("[data-answer-input]").forEach(input => {
        const key = input.dataset.answerInput;

        if (input.type === "radio" && responses[key] === input.value) {
            input.checked = true;
        }

        if (input.tagName === "TEXTAREA") {
            input.value = responses[key] || "";
        }

        const saveResponse = () => {
            responses[key] = input.type === "radio"
                ? assessment.querySelector(`input[name='${input.name}']:checked`)?.value || ""
                : input.value;

            sessionStorage.setItem(storageKey, JSON.stringify(responses));
            update();
        };

        input.addEventListener("input", saveResponse);
        input.addEventListener("change", saveResponse);
    });

    navigation.forEach((button, index) => {
        button.addEventListener("click", () => {
            current = index;
            update();
            panels[current].scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        });
    });

    assessment.querySelectorAll("[data-previous-question]").forEach(button => {
        button.addEventListener("click", () => {
            if (current > 0) {
                current--;
                update();
            }
        });
    });

    assessment.querySelectorAll("[data-next-question]").forEach(button => {
        button.addEventListener("click", () => {
            if (current < total - 1) {
                current++;
                update();
            }
        });
    });

    assessment.querySelectorAll("[data-submit-assessment]").forEach(button => {
        button.addEventListener("click", async () => {
            const answered = answeredCount();

            if (!window.confirm(
                `Submit ${answered} of ${total} answers for marking?`
            )) {
                return;
            }

            const csrfToken =
                document.querySelector('meta[name="csrf-token"]')?.content;

            if (!csrfToken) {
                window.alert("Please sign in again before submitting.");
                return;
            }

            const answers = panels.map(panel => {
                const input = panel.querySelector("[data-answer-input]");
                const key = input?.dataset.answerInput;

                return {
                    questionNumber: Number(panel.dataset.questionNumber),
                    selectedOptionCode: key ? (responses[key] || null) : null
                };
            });

            button.disabled = true;
            button.textContent = "Submitting...";

            try {
                const response = await fetch(
                    `/api/assessments/attempts/${encodeURIComponent(attemptId)}/submit`,
                    {
                        method: "POST",
                        credentials: "same-origin",
                        headers: {
                            "Content-Type": "application/json",
                            "X-CSRF-TOKEN": csrfToken
                        },
                        body: JSON.stringify({ answers })
                    }
                );

                const result = await response.json().catch(() => ({}));

                if (!response.ok) {
                    throw new Error(
                        result.error ||
                        result.title ||
                        "Submission failed. Please try again."
                    );
                }

                sessionStorage.removeItem(storageKey);

                window.location.assign(
                    `/Results?attemptId=${encodeURIComponent(result.attemptId)}`
                );
            } catch (error) {
                window.alert(
                    error.message || "Submission failed. Please try again."
                );
                button.disabled = false;
                button.textContent = "Submit assessment";
            }
        });
    });

    update();
})();