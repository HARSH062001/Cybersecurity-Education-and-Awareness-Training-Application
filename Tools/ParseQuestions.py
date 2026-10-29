"""
Converts the question documents into a JSON file containing the questions, 
answers and which module it belongs to

The script follows thing following regex patterns to extract the following:
Questions: "X. Questions text"
Options: "[A-D]. Option text"
Correct Answers: "X. [A-D] - Explanation text"
"""

import json
from pathlib import Path
import re

from docx import Document

ROOT = Path(__file__).resolve().parents[1] #location above where this file lives
SOURCE = ROOT / "02_Question Banks" #Location of the question banks
OUTPUT = ROOT / "CybersecurityTrainingWeb" / "Data" / "question_banks.json" #location where the JSON file will be

#REGEX patterns to match the questions, options, and correct answers
QUESTION =  re.compile(r"^(\d+)\.\s+(.+)$")
OPTION = re.compile(r"^([A-D])\.\s+(.+)$")
ANSWER = re.compile(r"^(\d+)\.\s+([A-D])\s\-\s+(.+)$")
STOPPER = "Trainer Answer Key and Explanations"

#CONSTANTS TO CHANGE IF ANY CHANGES TO THE MODULES' NUMBER
MODULE_COUNT = 8

def read_lines(path: Path) -> list[str]:
    """
    Reads the lines from the document and returns a list of strings if there
    is text in the line.
    """
    return [
        paragraph.text.strip() for paragraph in Document(path).paragraphs 
        if paragraph.text.strip()
        ]

def split_sections(lines: list[str]) -> tuple[list[str], list[str]]:
    """
    Splits the lines into two sections: questions and answers. The questions
    section is everything before the "Trainer Answer Key and Explanations"
    line, and the answers section is everything after that line.
    """
    key_index = next((index for index, text in enumerate(lines) if text.startswith(STOPPER)), len(lines))
    return lines[:key_index], lines[key_index + 1:]

def get_questions(
        question_list: list[tuple[int, int, str]], 
        question_section: list[str],
        module_id: int, 
        bank_code: str) -> list:

    questions = []
    for question_index, (start, number, text) in enumerate(question_list):
            end = question_list[question_index + 1][0] if question_index + 1 < len(question_list) else len(question_section)
            chunk = question_section[start:end]
            prompt_lines = text
            options = []
            for line in chunk:
                option = OPTION.match(line)
                if option:
                    options.append({"code": option.group(1), "text": option.group(2)})
    
            questions.append({
                "id": f"m{module_id}{bank_code.lower()}q{number}",
                "number": number,
                "text": prompt_lines,
                "type": "multipleChoice" if options else "scenario",
                "options": options,
            })

    return questions

def get_answers(answer_section: list[str]) -> dict:
    answer_dict = {}
    for line in answer_section:
        if match := ANSWER.match(line):
            answer_dict[int(match.group(1))] = {
                "correctAnswer": match.group(2),
                "explanation": match.group(3)
                }
    return answer_dict

def merge_questions_and_answers(questions, answer_dict):
    #add answer and explanation to each question
    for question in questions:
        answer = answer_dict.get(question["number"])
        if answer is None:
            raise ValueError(f"No answer found for question {question['number']}")
        question['correctAnswer'] = answer['correctAnswer']
        question['explanation'] = answer['explanation']


def parse_bank(path: Path, module_id: int, bank_code: str) -> dict:
    """
    Parses the question bank document and returns a dictionary containing the following:
    moduleId: The module number
    bank: The bank code (A, B, or C)
    questions: A list of dictionaries containing the following:
        id: The question ID (e.g. m1aq1)
        number: The question number (e.g. 1)
        text: The question text
        type: The question type (just multiple choice for now)
        options: A list of dictionaries containing the following:
            code: The option code (e.g. A)
            text: The option text
        correctAnswer: The correct answer code (e.g. A)
        explanation: The explanation text for the correct answer
    """
    # Get all the lines from the document and strip whitespace
    lines = read_lines(path)

    # Split into question and answer sections
    question_section, answer_section = split_sections(lines)

    # Get the index of the questions
    question_indexes_and_text = [
        (index, int(match.group(1)), match.group(2))
        for index,text in enumerate(question_section) if (match := QUESTION.match(text)) 
    ]

    #get questions
    questions = get_questions(question_indexes_and_text, question_section, module_id, bank_code)
    
    #Get the answers
    answer_dict = get_answers(answer_section)

    merge_questions_and_answers(questions, answer_dict)

    return {"moduleId": module_id, "bank": bank_code, "questions": questions}

def main() -> None:
    question_banks = []
    for module_id in range(1,MODULE_COUNT + 1):
        for bank_code in ("A", "B", "C"):
            source_file = SOURCE / f"Module {module_id}" / f"Question Bank {bank_code}.docx"
            question_banks.append(parse_bank(source_file, module_id, bank_code))

    OUTPUT.write_text(json.dumps(question_banks, indent=2), encoding="utf-8")
    print(f"Wrote {len(question_banks)} banks to {OUTPUT}")

if __name__ == "__main__":
    main()