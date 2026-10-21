using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingWeb.Pages;

public class ModulesModel : PageModel
{
    public List<ModuleCard> Modules { get; private set; } = [];

    public void OnGet()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "module-content.json");
        if (!System.IO.File.Exists(path)) return;

        var records = JsonSerializer.Deserialize<List<ModuleRecord>>(
            System.IO.File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        Modules = records
            .OrderBy(record => record.Id)
            .Select(record => new ModuleCard(
                record.Id,
                record.Title.Replace($"Module {record.Id} – ", ""),
                record.Blocks.FirstOrDefault(block => block.Kind == "paragraph")?.Text
                    ?? "Build practical cybersecurity knowledge and safer workplace habits."))
            .ToList();
    }

    public sealed class ModuleRecord
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<ModuleBlock> Blocks { get; set; } = [];
    }

    public sealed class ModuleBlock
    {
        public string Kind { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public record ModuleCard(int Number, string Title, string Summary);
}
