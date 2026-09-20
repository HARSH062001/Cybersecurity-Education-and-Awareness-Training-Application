using CybersecurityTrainingApplication.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingApplication.Web.Data;

public static class TrainingSeedData
{
    public static async Task InitialiseAsync(TrainingDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ModuleSectionProgress" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ModuleSectionProgress" PRIMARY KEY AUTOINCREMENT,
                "LearnerKey" TEXT NOT NULL,
                "ModuleId" INTEGER NOT NULL,
                "SectionKey" TEXT NOT NULL,
                "CompletedAtUtc" TEXT NOT NULL
            );
            """);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ModuleSectionProgress_LearnerKey_ModuleId_SectionKey"
            ON "ModuleSectionProgress" ("LearnerKey", "ModuleId", "SectionKey");
            """);
        if (await db.TrainingModules.AnyAsync()) return;
        var module = new TrainingModule
        {
            Title = "Cybersecurity Fundamentals",
            Purpose = "Build practical employee habits for protecting accounts, devices, information and business services.",
            Content = "This module introduces confidentiality, integrity and availability, common threats and vulnerabilities, account protection, safe information handling, device security and incident reporting. Employees should pause before unusual requests, use approved tools and report mistakes or suspicious activity promptly."
        };
        foreach (var code in new[] { "A", "B", "C" })
        {
            var bank = new QuestionBank { BankCode = code, Title = $"Question Bank {code}" };
            bank.Questions.Add(new Question
            {
                DisplayOrder = 1,
                QuestionText = "Which security goal means that only authorised people can view information?",
                Explanation = "Confidentiality protects information from unauthorised disclosure.",
                BusinessImpact = "A confidentiality failure can cause privacy harm, fraud and loss of trust.",
                ReferenceText = "NIST Cybersecurity Framework 1.1",
                AnswerOptions = [
                    new AnswerOption { OptionCode = "A", OptionText = "Confidentiality", IsCorrect = true },
                    new AnswerOption { OptionCode = "B", OptionText = "Availability" },
                    new AnswerOption { OptionCode = "C", OptionText = "Integrity" },
                    new AnswerOption { OptionCode = "D", OptionText = "Convenience" }]
            });
            module.QuestionBanks.Add(bank);
        }
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();
    }
}
