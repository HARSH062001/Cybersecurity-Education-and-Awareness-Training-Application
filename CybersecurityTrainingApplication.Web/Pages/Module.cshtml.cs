using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CybersecurityTrainingApplication.Web.Pages;
public class ModuleModel : PageModel
{
    public int ModuleId { get; private set; }
    public string Title { get; private set; } = "";
    public string Purpose { get; private set; } = "";
    public List<ContentBlock> Blocks { get; private set; } = [];
    public List<LearningSection> LearningSections { get; private set; } = [];
    public string[] Banks => ["A", "B", "C"];
    public IActionResult OnGet(int id = 1)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "module-content.json");
        var records = System.IO.File.Exists(path) ? JsonSerializer.Deserialize<List<ModuleContentRecord>>(System.IO.File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [] : [];
        var record = records.FirstOrDefault(x => x.Id == id) ?? records.FirstOrDefault(x => x.Id == 1);
        if (record is null) return Page();
        ModuleId = record.Id;
        Title = record.Title.Replace($"Module {record.Id} – ", "");
        Blocks = record.Blocks;
        Purpose = Blocks.FirstOrDefault(x => x.Kind == "paragraph")?.Text ?? "Build practical cybersecurity knowledge and safe workplace habits.";
        if (ModuleId == 1) LearningSections = BuildModuleOneSections(Blocks);
        return Page();
    }

    private static List<LearningSection> BuildModuleOneSections(List<ContentBlock> blocks)
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
            new("concepts", "Core cybersecurity concepts", "The knowledge behind safer decisions", Between("Main topics", "Workplace examples")),
            new("workplace", "Workplace examples", "How the risks appear during normal work", AddTable(Between("Workplace examples", "Safe employee behaviours"), tables, 1)),
            new("behaviours", "Safe employee behaviours", "Actions to use every day", AddTable(Between("Safe employee behaviours", "Assessment coverage"), tables, 2)),
            new("review", "Review and next steps", "Confirm coverage and revisit sources", From("Assessment coverage"))
        ];
    }

    private static List<ContentBlock> AddTable(List<ContentBlock> blocks, List<ContentBlock> tables, int tableIndex)
    {
        if (tables.Count > tableIndex) blocks.Add(tables[tableIndex]);
        return blocks;
    }
    public sealed class ModuleContentRecord { public int Id { get; set; } public string Title { get; set; } = ""; public List<ContentBlock> Blocks { get; set; } = []; }
    public sealed class ContentBlock { public string Kind { get; set; } = "paragraph"; public string Text { get; set; } = ""; public List<List<string>> Rows { get; set; } = []; }
    public sealed record LearningSection(string Key, string Title, string Summary, List<ContentBlock> Blocks);
}
