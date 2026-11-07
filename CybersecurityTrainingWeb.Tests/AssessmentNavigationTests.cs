using System.Net;
using CybersecurityTrainingWeb.Data;
using CybersecurityTrainingWeb.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CybersecurityTrainingWeb.Tests;

public class AssessmentNavigationTests
{
    private static HttpClient Client(AssessmentTestFactory factory, bool signedIn = true)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        if (signedIn) client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserHeader, "test-learner-a");
        return client;
    }

    private static void CompleteSections(ApplicationDbContext db)
    {
        db.SectionProgressEntries.AddRange(ResultUtils.AssessmentRules.RequiredSectionKeys.Select(key =>
            new SectionProgress { UserId = "test-learner-a", ModuleId = 1, SectionKey = key,
                CompletedAtUtc = DateTime.UtcNow }));
    }

    [Theory]
    [InlineData("/a-page-that-does-not-exist")]
    [InlineData("/Assessment?attemptId=9223372036854775807")]
    [InlineData("/Results?attemptId=9223372036854775807")]
    [InlineData("/Assessment?moduleId=999")]
    public async Task MissingPagesHaveRecoveryLinksAndKeep404(string url)
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var client = Client(factory);
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Page or assessment not found", html);
        Assert.Contains("Back to modules", html);
        Assert.Contains("href=\"/Modules\"", html);
    }

    [Fact]
    public async Task AnonymousMissingPageIsReadableAndApi404DoesNotBecomeHtml()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var client = Client(factory, false);
        var missing = await client.GetAsync("/missing-public-page");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Contains("Back to modules", await missing.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html");
        var api = await client.GetAsync("/api/missing-resource");
        Assert.Equal(HttpStatusCode.NotFound, api.StatusCode);
        Assert.DoesNotContain("Back to modules", await api.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ModuleEntryCreatesOnceAndRefreshResumesSameAttempt()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CompleteSections(db); await db.SaveChangesAsync();
        using var client = Client(factory);
        var first = await client.GetAsync("/Assessment?moduleId=1");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var attempt = await db.AssessmentAttempts.SingleAsync(a => a.UserId == "test-learner-a");
        var question = await db.AssessmentQuestions.FirstAsync(q => q.AssessmentBankId == attempt.AssessmentBankId);
        db.AssessmentAnswers.Add(new AssessmentAnswer { AssessmentAttemptId = attempt.Id,
            AssessmentQuestionId = question.Id, SelectedOptionCode = "B" });
        await db.SaveChangesAsync();
        var again = await client.GetAsync("/Assessment?moduleId=1");
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Contains($"data-attempt-id=\"{attempt.Id}\"", await again.Content.ReadAsStringAsync());
        Assert.Equal(1, await db.AssessmentAttempts.CountAsync());
        Assert.Equal("B", (await db.AssessmentAnswers.AsNoTracking().SingleAsync()).SelectedOptionCode);
    }

    [Fact]
    public async Task ModuleEntryResumesLatestOwnedAttemptAndHistoryUsesModuleLinks()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CompleteSections(db);
        var bank = await db.AssessmentBanks.FirstAsync(b => b.ModuleId == 1);
        var old = new AssessmentAttempt { UserId = "test-learner-a", AssessmentBankId = bank.Id,
            StartedAtUtc = DateTime.UtcNow.AddHours(-2) };
        var latest = new AssessmentAttempt { UserId = "test-learner-a", AssessmentBankId = bank.Id,
            StartedAtUtc = DateTime.UtcNow.AddHours(-1) };
        db.AssessmentAttempts.AddRange(old, latest, new AssessmentAttempt {
            UserId = "test-learner-b", AssessmentBankId = bank.Id, StartedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        using var client = Client(factory);
        var response = await client.GetAsync("/Assessment?moduleId=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"data-attempt-id=\"{latest.Id}\"", await response.Content.ReadAsStringAsync());
        var history = await (await client.GetAsync("/AssessmentHistory")).Content.ReadAsStringAsync();
        Assert.Contains("href=\"/Assessment?moduleId=1\"", history);
        Assert.DoesNotContain("href=\"/Assessment?attemptId=", history);
        var legacy = await client.GetAsync($"/Assessment?attemptId={old.Id}");
        Assert.Equal("/Assessment?moduleId=1", legacy.Headers.Location?.OriginalString);
        Assert.Equal(3, await db.AssessmentAttempts.CountAsync());
    }

    [Fact]
    public async Task AnotherLearnersAttemptReturnsGeneric404WithoutCreatingAnAttempt()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var bank = await db.AssessmentBanks.FirstAsync();
        var other = new AssessmentAttempt { UserId = "test-learner-b", AssessmentBankId = bank.Id };
        db.AssessmentAttempts.Add(other); await db.SaveChangesAsync();
        using var client = Client(factory);
        var response = await client.GetAsync($"/Assessment?attemptId={other.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page or assessment not found", await response.Content.ReadAsStringAsync());
        Assert.False(await db.AssessmentAttempts.AnyAsync(a => a.UserId == "test-learner-a"));
    }

    [Theory]
    [InlineData(3, HttpStatusCode.OK, 2)]
    [InlineData(9, HttpStatusCode.Redirect, 1)]
    public async Task FailedModuleCanRetryButPassedModuleStaysLocked(int score, HttpStatusCode status, int attemptCount)
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CompleteSections(db);
        var bank = await db.AssessmentBanks.FirstAsync(b => b.ModuleId == 1);
        var completed = new AssessmentAttempt { UserId = "test-learner-a", AssessmentBankId = bank.Id,
            Status = AssessmentAttemptStatus.Completed, Score = score, TotalQuestions = 10,
            SubmittedAtUtc = DateTime.UtcNow };
        db.AssessmentAttempts.Add(completed); await db.SaveChangesAsync();
        using var client = Client(factory);
        var response = await client.GetAsync("/Assessment?moduleId=1");
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(attemptCount, await db.AssessmentAttempts.CountAsync());
        var bookmark = await client.GetAsync($"/Assessment?attemptId={completed.Id}");
        Assert.Equal($"/Results?attemptId={completed.Id}", bookmark.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task IncompleteLearningCannotCreateAnAttempt()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var client = Client(factory);
        var response = await client.GetAsync("/Assessment?moduleId=1");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Module?id=1", response.Headers.Location?.OriginalString);
        using var scope = factory.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().AssessmentAttempts.AnyAsync());
    }
}
