// Checks that learners can see only their own assessment history.
using System.Net;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CybersecurityTrainingWeb.Tests;

public class AssessmentHistoryIsolationTests
{
    [Fact]
    public async Task LearnersSeeOnlyTheirOwnAttempts()
    {
        using var factory = new AssessmentTestFactory();
        factory.PrepareDatabase();

        long learnerAAttemptId;
        long learnerBAttemptId;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var module1Bank = await db.AssessmentBanks
                .FirstAsync(bank => bank.ModuleId == 1);

            var module2Bank = await db.AssessmentBanks
                .FirstAsync(bank => bank.ModuleId == 2);

            var learnerAAttempt = new AssessmentAttempt
            {
                UserId = "test-learner-a",
                AssessmentBankId = module1Bank.Id,
                Status = AssessmentAttemptStatus.Completed,
                Score = 3,
                TotalQuestions = 10
            };

            var learnerBAttempt = new AssessmentAttempt
            {
                UserId = "test-learner-b",
                AssessmentBankId = module2Bank.Id,
                Status = AssessmentAttemptStatus.Completed,
                Score = 5,
                TotalQuestions = 10
            };

            db.AssessmentAttempts.AddRange(
                learnerAAttempt,
                learnerBAttempt);

            await db.SaveChangesAsync();

            learnerAAttemptId = learnerAAttempt.Id;
            learnerBAttemptId = learnerBAttempt.Id;
        }

        using var learnerA = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        learnerA.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserHeader,
            "test-learner-a");

        var responseA = await learnerA.GetAsync("/AssessmentHistory");
        Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);

        var pageA = await responseA.Content.ReadAsStringAsync();
        Assert.Contains($"attemptId={learnerAAttemptId}", pageA);
        Assert.DoesNotContain($"attemptId={learnerBAttemptId}", pageA);

        using var learnerB = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        learnerB.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserHeader,
            "test-learner-b");

        var responseB = await learnerB.GetAsync("/AssessmentHistory");
        Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);

        var pageB = await responseB.Content.ReadAsStringAsync();
        Assert.Contains($"attemptId={learnerBAttemptId}", pageB);
        Assert.DoesNotContain($"attemptId={learnerAAttemptId}", pageB);
    }
}