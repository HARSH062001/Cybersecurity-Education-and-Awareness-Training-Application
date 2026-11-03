namespace CybersecurityTrainingWeb.Services;

using CybersecurityTrainingWeb.Data;
using Microsoft.EntityFrameworkCore;

public static class ResultUtils
{
    public static class AssessmentRules
    {
        public const double PassingScore = 7;
    }
    // Gets the attempt of the user including the bank, the questions, and answers
    public static Task<AssessmentAttempt?> GetLatestFinishedAttempt(
        ApplicationDbContext dbContext,
        string userId,
        int moduleId
        )
    {
        // checks the following conditions: user matches, attempId matches, and status is completed
        var finishedAttempt = dbContext.AssessmentAttempts
            .Where(attempt => attempt.UserId == userId)
            .Where(attempt => attempt.Bank.ModuleId == moduleId)
            .Include(attempt => attempt.Bank)
            .ThenInclude(bank => bank.Questions)
            .Include(attempt => attempt.Answers)
            .OrderBy(attempt => attempt.SubmittedAtUtc)
            .LastOrDefaultAsync(attempt => attempt.Status == AssessmentAttemptStatus.Completed);

        return finishedAttempt;
    }

    // Returns a boolean whether a student has passed a specific module
    public static Task<bool> HasUserPassed(
        ApplicationDbContext dbContext,
        int moduleId,
        string userId
    )
    {
        return dbContext.AssessmentAttempts.AnyAsync(attempt =>
        attempt.UserId == userId &&
        attempt.Status == AssessmentAttemptStatus.Completed &&
        attempt.Score >= AssessmentRules.PassingScore &&
        attempt.Bank.ModuleId == moduleId
        );
    }
}