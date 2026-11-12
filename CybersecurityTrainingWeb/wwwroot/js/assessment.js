// Manages assessment navigation, answer saving, resume and submission in the browser.
(() => {
    const assessment = document.querySelector("[data-assessment]");
    if (!assessment) return;

    const attemptId = assessment.dataset.attemptId;
    const panels = [...assessment.querySelectorAll("[data-question-panel]")];
    const navigation = [...assessment.querySelectorAll("[data-question-nav]")];
    const total = panels.length;
    const storageKey = `cybertraining.assessment.attempt.${attemptId}.v1`;
    const saveStatus = assessment.querySelector("[data-answer-save-status]");
    const pendingSaves = new Map();

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

    const panelForKey = key => panels.find(panel =>
        panel.querySelector("[data-answer-input]")?.dataset.answerInput === key);

    // Queue saves per question so a slower earlier request cannot overwrite a newer selection.
    const persistAnswer = (key, selectedOptionCode) => {
        const panel = panelForKey(key);
        const csrfToken = document.querySelector('meta[name="csrf-token"]')?.content;
        if (!panel || !csrfToken || !selectedOptionCode) {
            if (saveStatus) saveStatus.textContent = "Your answer is saved in this browser session.";
            return Promise.resolve(false);
        }

        const previousSave = pendingSaves.get(key) || Promise.resolve();
        const nextSave = previousSave.catch(() => {}).then(async () => {
            const response = await fetch(
                `/api/assessments/attempts/${encodeURIComponent(attemptId)}/answers`,
                {
                    method: "POST",
                    credentials: "same-origin",
                    headers: {
                        "Content-Type": "application/json",
                        "X-CSRF-TOKEN": csrfToken
                    },
                    body: JSON.stringify({
                        questionNumber: Number(panel.dataset.questionNumber),
                        selectedOptionCode
                    })
                }
            );
            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.error || "Answer save failed.");
            }
        });

        pendingSaves.set(key, nextSave);
        if (saveStatus) saveStatus.textContent = "Saving your answer…";

        return nextSave.then(() => {
            if (responses[key] === selectedOptionCode && saveStatus) {
                saveStatus.textContent = "Your answers are saved to your account.";
            }
            return true;
        }).catch(() => {
            if (responses[key] === selectedOptionCode && saveStatus) {
                saveStatus.textContent = "Could not sync this answer. It remains in this browser session.";
            }
            return false;
        }).finally(() => {
            if (pendingSaves.get(key) === nextSave) pendingSaves.delete(key);
        });
    };

    // Restore the account’s saved selections when the learner resumes this attempt.
    const restoreSavedAnswers = async () => {
        try {
            const response = await fetch(
                `/api/assessments/attempts/${encodeURIComponent(attemptId)}/answers`,
                { credentials: "same-origin", cache: "no-store" }
            );
            if (!response.ok) throw new Error("Could not load saved answers.");

            const saved = await response.json();
            const serverResponses = {};
            (saved.answers || []).forEach(answer => {
                const panel = panels.find(item =>
                    Number(item.dataset.questionNumber) === answer.questionNumber);
                const key = panel?.querySelector("[data-answer-input]")?.dataset.answerInput;
                if (key && answer.selectedOptionCode) {
                    serverResponses[key] = answer.selectedOptionCode;
                }
            });

            responses = { ...serverResponses, ...responses };
            assessment.querySelectorAll("[data-answer-input]").forEach(input => {
                input.checked = input.type === "radio" && responses[input.dataset.answerInput] === input.value;
            });
            sessionStorage.setItem(storageKey, JSON.stringify(responses));
            update();

            for (const [key, answer] of Object.entries(responses)) {
                if (answer && serverResponses[key] !== answer) {
                    void persistAnswer(key, answer);
                }
            }
            if (!Object.keys(responses).length && saveStatus) {
                saveStatus.textContent = "No saved answers yet.";
            }
        } catch {
            if (saveStatus) {
                saveStatus.textContent = "Saved answers could not be loaded. This browser session is still available.";
            }
        }
    };

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
        const submitButton = panel.querySelector("[data-submit-assessment]");
        if (submitButton) {
            const allAnswered = answeredCount() === total;
            submitButton.toggleAttribute("hidden", current !== total - 1);
            submitButton.disabled = !allAnswered;
        }
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
            void persistAnswer(key, responses[key]);
        };

        input.addEventListener(input.type === "radio" ? "change" : "input", saveResponse);
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

            if (answered !== total) {
                window.alert(`Answer all ${total} questions before submitting.`);
                return;
            }

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
                if (pendingSaves.size) {
                    if (saveStatus) saveStatus.textContent = "Saving your answers before submission…";
                    await Promise.all([...pendingSaves.values()].map(save => save.catch(() => {})));
                }

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
    void restoreSavedAnswers();
})();
