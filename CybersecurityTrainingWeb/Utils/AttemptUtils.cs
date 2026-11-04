using Microsoft.EntityFrameworkCore;
using CybersecurityTrainingWeb.Data;

namespace CybersecurityTrainingWeb.Utils;

/*
    This class is responsible helpers that can help for assessment functions
*/
public static class AttemptUtils
{
    // Returns the latest assessment attempt for the user and module, or creates a new one if none exists
    public static AssessmentAttempt
     GetOrCreateAttempt(ApplicationDbContext dbContext, string userId, int moduleId)
    {
        var inProgressAttempt = dbContext.AssessmentAttempts
            .Where(attempt => attempt.UserId == userId)
            .Where(attempt => attempt.Bank.ModuleId == moduleId)
            .FirstOrDefault(attempt => attempt.Status == AssessmentAttemptStatus.InProgress);

        // Check if the user is still attempting a bank     
        if (inProgressAttempt != null)
        {
            return inProgressAttempt;
        }

        // If not, check if the user has tried to attempt the assessment
        var history = dbContext.AssessmentAttempts
            .Where(attempt => attempt.UserId == userId)
            .Where(attempt => attempt.Bank.ModuleId == moduleId)
            .OrderBy(attempt => attempt.StartedAtUtc)
            .Select(attempt => attempt.Bank.Code)
            .ToList();

        var newBank = AttemptUtils.PickBank(history);

        var newBankAttempt = CreateNewAttempt(dbContext, userId, moduleId, newBank);
        return newBankAttempt;
    }

    // Used by GetOrCreateAttempt, this method creates a new attempt and saves it to database
    private static AssessmentAttempt CreateNewAttempt(ApplicationDbContext dbContext, string userId, int moduleId, string bankCode)
    {
        var newAttempt = new AssessmentAttempt
        {
            UserId = userId,
            AssessmentBankId = dbContext.AssessmentBanks
                .Where(bank => bank.ModuleId == moduleId && bank.Code == bankCode)
                .Where(bank => bank.IsPublished)
                .Select(bank => bank.Id)
                .FirstOrDefault(),
            Status = AssessmentAttemptStatus.InProgress,
            StartedAtUtc = DateTime.UtcNow
        };

        dbContext.AssessmentAttempts.Add(newAttempt);
        dbContext.SaveChanges();

        return newAttempt;
    }

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


    /// <summary>
    /// Optionally takes in a bank returns. Returns a new bank code following the rules:
    /// 1. If not bank code is passed, returns a random bank code (A,b or C )
    /// 2. If bank code is passed, returns a new bank code
    /// 3. Returns either A, B, or C based on the banks that have been attempted. 
    /// 4. If no banks have been attempted, returns a random bank code
    /// </summary>
    public static string PickBank(IReadOnlyList<string> banksAttempted)
    {
        // Get the count of each bank code attempted
        var aCount = banksAttempted.Count(b => b == "A");
        var bCount = banksAttempted.Count(b => b == "B");
        var cCount = banksAttempted.Count(b => b == "C");

        var lastBank = banksAttempted.LastOrDefault(""); // the last bank attempted or empty string
        var leastAttemptedCount = Math.Min(aCount, Math.Min(bCount, cCount)); // value of least attempted bank code

        var bankDictionary = new Dictionary<string, int>
        {
            {"A", aCount},
            {"B", bCount},
            {"C", cCount}
        };

        // Get the bank codes that have been attempted the least
        var leastAttemptedBanks = bankDictionary
            .Where(item => item.Value == leastAttemptedCount)
            .Select(item => item.Key)
            .ToList();

        // if more than one bank code has been attempted the least, remove the last bank attempted from the list of least attempted banks and return a random bank code from the remaining least attempted banks
        if (leastAttemptedBanks.Count > 1)
        {
            leastAttemptedBanks.Remove(lastBank);
            var random = new Random();
            var randomIndex = random.Next(leastAttemptedBanks.Count);
            return leastAttemptedBanks[randomIndex];
        }

        // if only one, return that bank code
        else if (leastAttemptedBanks.Count == 1)
        {
            return leastAttemptedBanks[0];
        }
        else
        {
            return "A"; // default to A for edge cases
        }
    }

}