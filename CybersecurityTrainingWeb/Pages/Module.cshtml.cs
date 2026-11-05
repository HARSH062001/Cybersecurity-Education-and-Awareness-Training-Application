using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CybersecurityTrainingWeb.Utils;
using Microsoft.AspNetCore.Identity;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class ModuleModel : PageModel
{
    public int ModuleId { get; private set; }
    public string Title { get; private set; } = "";
    public string Purpose { get; private set; } = "";
    public bool HasPassed { get; set; }
    public bool HasAttempted { get; set; }
    public bool HasCompletedAllSections { get; private set; }
    public long? BestAttemptId { get; private set; }
    public int? BestScore { get; private set; }
    public int? BestTotalQuestions { get; private set; }
    public int? BestPercentage { get; private set; }
    public List<ContentBlock> Blocks { get; private set; } = [];
    public List<LearningSection> LearningSections { get; private set; } = [];
    public string[] Banks => ["A", "B", "C"];
    private readonly UserManager<ApplicationUser> _userManager; //get user information
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;


    public ModuleModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _configuration = configuration;
    }
    public async Task<IActionResult> OnGetAsync(int id = 1)
    {
        if (id is < 1 or > 8) return RedirectToPage("/Modules");

        var path = Path.Combine(AppContext.BaseDirectory, "Data", "module-content.json");
        var records = System.IO.File.Exists(path) ? JsonSerializer.Deserialize<List<ModuleContentRecord>>(System.IO.File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [] : [];
        var record = records.FirstOrDefault(x => x.Id == id) ?? records.FirstOrDefault(x => x.Id == 1);
        if (record is null) return Page();
        ModuleId = record.Id;
        Title = record.Title.Replace($"Module {record.Id} – ", "");
        Blocks = record.Blocks;
        Purpose = Blocks.FirstOrDefault(x => x.Kind == "paragraph")?.Text ?? "Build practical cybersecurity knowledge and safe workplace habits.";
        LearningSections = BuildLearningSections(Blocks);

        // Get user id
        var userId = _userManager.GetUserId(User);
        if (userId is null) // Just in case the user is not logged in (should not happen)
        {
            return Unauthorized();
        }

        var completedSections = await _dbContext.SectionProgressEntries.AsNoTracking()
            .Where(progress => progress.UserId == userId && progress.ModuleId == ModuleId)
            .Select(progress => progress.SectionKey)
            .Distinct()
            .ToListAsync();
        HasCompletedAllSections = LearningSections.Count > 0 &&
            LearningSections.All(section => completedSections.Contains(section.Key));

        var bestAttempt = await _dbContext.AssessmentAttempts.AsNoTracking()
            .Where(attempt => attempt.UserId == userId &&
                              attempt.Bank.ModuleId == ModuleId &&
                              attempt.Status == AssessmentAttemptStatus.Completed &&
                              attempt.Score != null &&
                              attempt.TotalQuestions != null &&
                              attempt.TotalQuestions > 0)
            .OrderByDescending(attempt => attempt.Score * 100.0 / attempt.TotalQuestions)
            .ThenByDescending(attempt => attempt.SubmittedAtUtc)
            .Select(attempt => new
            {
                attempt.Id,
                Score = attempt.Score!.Value,
                TotalQuestions = attempt.TotalQuestions!.Value
            })
            .FirstOrDefaultAsync();
        HasAttempted = bestAttempt is not null;
        BestAttemptId = bestAttempt?.Id;
        BestScore = bestAttempt?.Score;
        BestTotalQuestions = bestAttempt?.TotalQuestions;
        BestPercentage = bestAttempt is null
            ? null
            : (int)Math.Round(100.0 * bestAttempt.Score / bestAttempt.TotalQuestions);
        var passingPercentage = ResultUtils.AssessmentRules.GetPassingPercentage(_configuration);
        HasPassed = bestAttempt is not null && ResultUtils.AssessmentRules.IsPassed(
            bestAttempt.Score, bestAttempt.TotalQuestions, passingPercentage);

        return Page();
    }

    private static List<LearningSection> BuildLearningSections(List<ContentBlock> blocks)
    {
        var tables = blocks.Where(x => x.Kind == "table").ToList();
        var content = blocks.Where(x => x.Kind != "table").ToList();

        List<ContentBlock> Before(string end) => content.TakeWhile(x => x.Text != end).ToList();
        List<ContentBlock> Between(string start, string end) => content.SkipWhile(x => x.Text != start).Skip(1).TakeWhile(x => x.Text != end).ToList();
        List<ContentBlock> From(string start) => content.SkipWhile(x => x.Text != start).ToList();

        return
        [
            new("purpose", "Purpose and importance", "Why this matters in everyday work", Before("Learning outcomes")),
            new("outcomes", "Learning outcomes", "What you should be able to do", Between("Learning outcomes", "Key terms")),
            new("terms", "Key terms", "Plain-language definitions", tables.Count > 0 ? [tables[0]] : []),
            new("concepts", "Core concepts", "The knowledge behind safer decisions", Between("Main topics", "Workplace examples")),
            new("workplace", "Workplace examples", "How the risks appear during normal work", Between("Workplace examples", "Business impact examples")),
            new("behaviours", "Business impact and safe behaviours", "Actions to use every day", AddTables(Between("Business impact examples", "Assessment coverage"), tables, 1, 2)),
            new("review", "Review and next steps", "Confirm coverage and revisit sources", From("Assessment coverage"))
        ];
    }

    private static List<ContentBlock> AddTables(List<ContentBlock> blocks, List<ContentBlock> tables, params int[] tableIndexes)
    {
        foreach (var tableIndex in tableIndexes)
        {
            if (tables.Count > tableIndex) blocks.Add(tables[tableIndex]);
        }
        return blocks;
    }
    public sealed class ModuleContentRecord { public int Id { get; set; } public string Title { get; set; } = ""; public List<ContentBlock> Blocks { get; set; } = []; }
    public sealed class ContentBlock { public string Kind { get; set; } = "paragraph"; public string Text { get; set; } = ""; public List<List<string>> Rows { get; set; } = []; }
    public sealed record LearningSection(string Key, string Title, string Summary, List<ContentBlock> Blocks);
}
