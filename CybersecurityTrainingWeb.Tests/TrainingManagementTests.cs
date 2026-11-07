using System.Net;
using System.Security.Claims;
using CybersecurityTrainingWeb.Data;
using CybersecurityTrainingWeb.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CybersecurityTrainingWeb.Tests;

public class TrainingManagementTests
{
    private static PageContext Context(string userId, string role = "") => new()
    {
        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role) }, "Test")) }
    };

    [Fact]
    public async Task OwnerCannotScheduleUnassignedModule()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var page = new TrainingManagementModel(db) { PageContext = Context("test-learner-a", "ModuleOwner"), ModuleId = 2, DueAtUtc = DateTime.UtcNow };
        Assert.IsType<ForbidResult>(await page.OnPostScheduleAsync());
        Assert.Empty(db.ModuleSchedules);
    }

    [Fact]
    public async Task OwnerCannotEnrolLearnerIntoUnassignedModule()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var page = new TrainingManagementModel(db) { PageContext = Context("test-learner-a", "ModuleOwner"), ModuleId = 2, LearnerEmail = "learner-b@example.test" };
        Assert.IsType<ForbidResult>(await page.OnPostEnrollAsync());
        Assert.Empty(db.ModuleEnrollments);
    }

    [Fact]
    public async Task AssignedOwnerCanSaveScheduleAndEnrolWithoutDuplicates()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ModuleOwnerAssignments.Add(new ModuleOwnerAssignment { ModuleId = 1, OwnerUserId = "test-learner-a" });
        await db.SaveChangesAsync();
        var page = new TrainingManagementModel(db) { PageContext = Context("test-learner-a", "ModuleOwner"), ModuleId = 1,
            LearnerEmail = "learner-b@example.test", DueAtUtc = DateTime.UtcNow.AddDays(3), RepeatEveryDays = 30 };
        page.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(page.HttpContext,
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        Assert.IsType<RedirectToPageResult>(await page.OnPostEnrollAsync());
        Assert.IsType<RedirectToPageResult>(await page.OnPostEnrollAsync());
        Assert.Single(db.ModuleEnrollments);
        Assert.IsType<RedirectToPageResult>(await page.OnPostScheduleAsync());
        Assert.Equal(30, Assert.Single(db.ModuleSchedules).RepeatEveryDays);
        page.RepeatEveryDays = 7;
        await page.OnPostScheduleAsync();
        Assert.Equal(7, Assert.Single(db.ModuleSchedules).RepeatEveryDays);
    }

    [Fact]
    public async Task NotificationsExcludeOtherLearnersAssignments()
    {
        using var factory = new AssessmentTestFactory(); factory.PrepareDatabase();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ModuleEnrollments.AddRange(new ModuleEnrollment { UserId = "test-learner-a", ModuleId = 1 }, new ModuleEnrollment { UserId = "test-learner-b", ModuleId = 2 });
        db.ModuleSchedules.Add(new ModuleSchedule { ModuleId = 1, FirstDueAtUtc = DateTime.UtcNow.AddDays(-1), RepeatEveryDays = 7 });
        await db.SaveChangesAsync();
        var page = new NotificationsModel(db, scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()) { PageContext = Context("test-learner-a") };
        await page.OnGetAsync();
        var notice = Assert.Single(page.Notices);
        Assert.Equal(1, notice.ModuleId);
        Assert.Contains("overdue", notice.Message);
    }
}
