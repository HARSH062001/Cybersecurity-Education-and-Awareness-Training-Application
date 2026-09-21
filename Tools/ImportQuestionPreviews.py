"""Create learner-facing assessment preview data from the approved Word question banks.

Correct answers, explanations, business impacts and references remain in the source
documents for the backend import. This file intentionally exports only question text,
type and explicit answer options for the frontend prototype.
"""

import json
import re
from pathlib import Path

from docx import Document


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "02_Question Banks"
OUTPUT = ROOT / "CybersecurityTrainingApplication.Web" / "Data" / "assessment-preview.json"
QUESTION_MARKER = re.compile(r"^Question\s+(\d+)\s*$", re.IGNORECASE)
OPTION_MARKER = re.compile(r"^([A-D])\.\s+(.+)$")


def parse_bank(path: Path, module_id: int, bank_code: str) -> dict:
    lines = [paragraph.text.strip() for paragraph in Document(path).paragraphs if paragraph.text.strip()]
    question_indexes = [(index, int(match.group(1))) for index, text in enumerate(lines) if (match := QUESTION_MARKER.match(text))]
    questions = []

    for question_index, (start, number) in enumerate(question_indexes):
        end = question_indexes[question_index + 1][0] if question_index + 1 < len(question_indexes) else len(lines)
        chunk = lines[start + 1:end]
        prompt_lines = []
        options = []
        for line in chunk:
            if line.lower().startswith(("correct answer:", "explanation:", "business impact:", "reference:")):
                break
            option = OPTION_MARKER.match(line)
            if option:
                options.append({"code": option.group(1), "text": option.group(2)})
            else:
                prompt_lines.append(line)

        questions.append({
            "id": f"m{module_id}{bank_code.lower()}q{number}",
            "number": number,
            "text": " ".join(prompt_lines),
            "type": "multipleChoice" if options else "scenario",
            "options": options,
        })

    return {"moduleId": module_id, "bank": bank_code, "questions": questions}


def main() -> None:
    records = []
    for module_id in range(1, 9):
        for bank_code in ("A", "B", "C"):
            source_file = SOURCE / f"Module {module_id}" / f"Question Bank {bank_code}.docx"
            records.append(parse_bank(source_file, module_id, bank_code))
    OUTPUT.write_text(json.dumps(records, indent=2), encoding="utf-8")
    print(f"Wrote {len(records)} banks to {OUTPUT}")


if __name__ == "__main__":
    main()
