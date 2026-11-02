namespace CybersecurityTrainingWeb.Services;
using CybersecurityTrainingWeb.Data;
public static class AttemptFinder
{
    // Returns the latest assessment attempt for the user and module, or creates a new one if none exists
    public static AssessmentAttempt
     GetOrCreateAttempt(ApplicationDbContext dbContext, string userId, int moduleId)
    {
        var latestAttempt = dbContext.AssessmentAttempts
            .Where(attempt => attempt.UserId == userId && attempt.Id == moduleId)
            .OrderBy(attempt => attempt.StartedAtUtc)
            .FirstOrDefault();

        // Check if the user is still attempting a bank 
        if (latestAttempt != null && latestAttempt.Status == 0)
        {
            return latestAttempt;
        }

        // If not, check if the user has tried to attempt the assessment
        var history = dbContext.AssessmentAttempts
            .Where(attempt => attempt.UserId == userId && attempt.Id == moduleId)
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