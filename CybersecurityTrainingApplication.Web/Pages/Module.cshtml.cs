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
    public string[] Banks => ["A", "B", "C"];
    public IActionResult OnGet(int id = 1)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "module-content.json");
        var records = System.IO.File.Exists(path) ? JsonSerializer.Deserialize<List<ModuleContentRecord>>(System.IO.File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [] : [];
        var record = records.FirstOrDefault(x => x.Id == id) ?? records.FirstOrDefault(x => x.Id == 1);
        if (record is null) return Page();
        ModuleId = record.Id; Title = record.Title; Blocks = record.Blocks;
        Purpose = Blocks.FirstOrDefault(x => x.Kind == "paragraph")?.Text ?? "Build practical cybersecurity knowledge and safe workplace habits.";
        return Page();
    }
    public sealed class ModuleContentRecord { public int Id { get; set; } public string Title { get; set; } = ""; public List<ContentBlock> Blocks { get; set; } = []; }
    public sealed class ContentBlock { public string Kind { get; set; } = "paragraph"; public string Text { get; set; } = ""; public List<List<string>> Rows { get; set; } = []; }
}
