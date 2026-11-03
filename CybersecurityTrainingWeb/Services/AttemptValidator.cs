using CybersecurityTrainingWeb.Data;
using CybersecurityTrainingWeb.Pages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Services;
/*
    This class validates the answers submitted by the user and if validated,
    creates AssessmentAttemptAnswer records in the database for each answer submitted.
    It also updates the AssessmentAttempt record to mark it as completed.
*/
public static class AttemptValidator
{
    public static async Task<AssessmentAttempt?> ValidateAndMarkAttempt(
        ApplicationDbContext dbContext,
        string userId,
        long attemptId,
        Dictionary<long, string> answers
    )
    {
        // Get assessment attempt, and the related bank/questions/options from the database
        var assessmentAttempt = await dbContext.AssessmentAttempts
            .Where(attempt => attempt.Id == attemptId)
            .Where(attempt => attempt.UserId == userId)
            .Where(attempt => attempt.Status == AssessmentAttemptStatus.InProgress)
            .Include(attempt => attempt.Bank)
            .ThenInclude(bank => bank.Questions)
            .ThenInclude(question => question.Options)
            .FirstOrDefaultAsync();

        if (assessmentAttempt is null)
        {
            return null; // Invalid attempt
        }

        // Check if each answer's ID exists in the bank's questions and if the answer code is valid for that question
        var answerDictionary = new Dictionary<long, List<string>>();
        // Populate the answer dictionary with the questions and their options associated
        // with the attempt
        foreach (var question in assessmentAttempt.Bank.Questions)
        {
            answerDictionary[question.Id] = question.Options.Select(o => o.Code).ToList();
        }

        // Check if total answered questions match the total questions in the bank
        // Since the answers variable is a dictionary, all answers are unique (No duplicates answers to a single question)
        if (answers.Count != answerDictionary.Count)
        {
            return null; // Invalid number of answers
        }

        // Validate the user's answers. If any answer is invalid, return null
        foreach (var answer in answers)
        {
            if (!answerDictionary.ContainsKey(answer.Key) || !answerDictionary[answer.Key].Contains(answer.Value))
            {
                return null; // Invalid answer
            }
        }

        // Mark each answers, and create AssessmentAttemptAnswer records in the database
        foreach (var question in assessmentAttempt.Bank.Questions)
        {
            var answerCode = answers[question.Id];
            var attemptAnswer = new AssessmentAnswer
            {
                AssessmentAttemptId = assessmentAttempt.Id,
                AssessmentQuestionId = question.Id,
                SelectedOptionCode = answerCode.ToUpperInvariant(),
                IsCorrect = answerCode.Equals(question.CorrectOptionCode),
                PointsAwarded = answerCode.Equals(question.CorrectOptionCode) ? 1 : 0,
            };
            dbContext.AssessmentAnswers.Add(attemptAnswer);
        }

        // Update the assessment attempt to mark it as completed
        assessmentAttempt.Status = AssessmentAttemptStatus.Completed;
        assessmentAttempt.SubmittedAtUtc = DateTime.UtcNow;
        assessmentAttempt.Score = assessmentAttempt.Answers.Count(a => a.IsCorrect == true);
        assessmentAttempt.TotalQuestions = assessmentAttempt.Bank.Questions.Count;
        assessmentAttempt.MarkedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return assessmentAttempt;
    }
}