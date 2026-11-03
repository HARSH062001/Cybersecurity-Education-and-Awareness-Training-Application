namespace CybersecurityTrainingWeb.Services;

using CybersecurityTrainingWeb.Data;
/*
    This class is responsible for finding the latest assessment attempt for a user and module, or creating a new one if none exists.
    It also ensures that the user is not attempting the same bank multiple times in a row.
*/
public static class AttemptFinder
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

        var newBank = BankPicker.PickBank(history);

        var newBankAttempt = CreateNewAttempt(dbContext, userId, moduleId, newBank);
        return newBankAttempt;
    }

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
}