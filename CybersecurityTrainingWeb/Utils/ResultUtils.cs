// Provides shared rules for assessment eligibility, passing and attempt history.
namespace CybersecurityTrainingWeb.Utils;

using CybersecurityTrainingWeb.Data;
using Microsoft.EntityFrameworkCore;

public static class ResultUtils
{
    public static class AssessmentRules
    {
        public const int DefaultPassingPercentage = 90;
        public static readonly string[] RequiredSectionKeys =
            ["purpose", "outcomes", "terms", "concepts", "workplace", "behaviours", "review"];

        public static int GetPassingPercentage(IConfiguration configuration) =>
            Math.Clamp(configuration.GetValue<int?>("Assessment:PassingPercentage")
                       ?? DefaultPassingPercentage, 1, 100);

        public static bool IsPassed(int? score, int? total, int passingPercentage) =>
            score is not null && total is > 0 &&
            score.Value * 100.0 / total.Value >= passingPercentage;

        public static async Task<bool> HasCompletedAllSectionsAsync(
            ApplicationDbContext dbContext,
            string userId,
            int moduleId)
        {
            var completedCount = await dbContext.SectionProgressEntries.AsNoTracking()
                .Where(progress => progress.UserId == userId &&
                                   progress.ModuleId == moduleId &&
                                   RequiredSectionKeys.Contains(progress.SectionKey))
                .Select(progress => progress.SectionKey)
                .Distinct()
                .CountAsync();

            return completedCount == RequiredSectionKeys.Length;
        }
    }
    // Gets the most recent completed attempt with its questions and saved answers.
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
            .ThenInclude(question => question.Options)
            .Include(attempt => attempt.Answers)
            .Where(attempt => attempt.Status == AssessmentAttemptStatus.Completed)
            .OrderByDescending(attempt => attempt.SubmittedAtUtc)
            .FirstOrDefaultAsync();

        return finishedAttempt;
    }

    // Returns a boolean whether a student has passed a specific module
    public static Task<bool> HasUserPassed(
        ApplicationDbContext dbContext,
        int moduleId,
        string userId,
        int passingPercentage = AssessmentRules.DefaultPassingPercentage
    )
    {
        return dbContext.AssessmentAttempts.AnyAsync(attempt =>
        attempt.UserId == userId &&
        attempt.Status == AssessmentAttemptStatus.Completed &&
        attempt.Score != null && attempt.TotalQuestions != null &&
        attempt.Score * 100.0 >= attempt.TotalQuestions * passingPercentage &&
        attempt.Bank.ModuleId == moduleId
        );
    }

    // Returns whether the user has attempted and finished a result (without reading bank, questions, options and answers)
    public static Task<bool> HasFinishedAttempt(
    ApplicationDbContext dbContext, string userId, int moduleId)
    {
        return dbContext.AssessmentAttempts.AnyAsync(attempt =>
        attempt.UserId == userId &&
        attempt.Bank.ModuleId == moduleId &&
        attempt.Status == AssessmentAttemptStatus.Completed);
    }

}
