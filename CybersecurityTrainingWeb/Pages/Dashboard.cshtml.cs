using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingWeb.Pages;

public class DashboardModel : PageModel
{
    public List<ModuleInfo> Modules { get; private set; } = [];

    public void OnGet()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "module-content.json");

        if (!System.IO.File.Exists(path))
            return;

        var records = JsonSerializer.Deserialize<List<ModuleRecord>>(
            System.IO.File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        // Use the same module names as the learning catalogue.
        Modules = records
            .Where(record => record.Id is >= 1 and <= 8)
            .OrderBy(record => record.Id)
            .Select(record => new ModuleInfo(
                record.Id,
                record.Title.Replace($"Module {record.Id} – ", "")))
            .ToList();
    }

    public record ModuleInfo(int Id, string Title);

    private sealed class ModuleRecord
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }
}