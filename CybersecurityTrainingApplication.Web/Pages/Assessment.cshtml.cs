using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingApplication.Web.Pages;

public class AssessmentModel : PageModel
{
    public int ModuleId { get; private set; }
    public string Bank { get; private set; } = "A";
    public string ModuleTitle { get; private set; } = "Cybersecurity Fundamentals";
    public List<PreviewQuestion> Questions { get; private set; } = [];

    public IActionResult OnGet(int moduleId = 1, string? bank = null)
    {
        if (moduleId is < 1 or > 8) return RedirectToPage("/Modules");
        if (bank is null || bank.Length != 1 || bank[0] is < 'A' or > 'C') bank = ((char)('A' + Random.Shared.Next(3))).ToString();
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "assessment-preview.json");
        var banks = System.IO.File.Exists(path)
            ? JsonSerializer.Deserialize<List<PreviewBank>>(System.IO.File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? []
            : [];
        var preview = banks.FirstOrDefault(item => item.ModuleId == moduleId && string.Equals(item.Bank, bank, StringComparison.OrdinalIgnoreCase));
        if (preview is null) return RedirectToPage("/Modules");

        ModuleId = moduleId;
        Bank = bank.ToUpperInvariant();
        ModuleTitle = ModuleTitles[moduleId - 1];
        Questions = preview.Questions.OrderBy(question => question.Number).ToList();
        return Page();
    }

    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];

    public sealed class PreviewBank { public int ModuleId { get; set; } public string Bank { get; set; } = ""; public List<PreviewQuestion> Questions { get; set; } = []; }
    public sealed class PreviewQuestion { public string Id { get; set; } = ""; public int Number { get; set; } public string Text { get; set; } = ""; public string Type { get; set; } = "multipleChoice"; public List<PreviewOption> Options { get; set; } = []; }
    public sealed class PreviewOption { public string Code { get; set; } = ""; public string Text { get; set; } = ""; }
}
