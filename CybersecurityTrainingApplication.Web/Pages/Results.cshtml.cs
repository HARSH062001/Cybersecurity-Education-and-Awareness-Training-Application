using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingApplication.Web.Pages;

public class ResultsModel : PageModel
{
    public int ModuleId { get; private set; } = 1;
    public string Bank { get; private set; } = "A";
    public int Answered { get; private set; }
    public string ModuleTitle => ModuleTitles[ModuleId - 1];

    public void OnGet(int moduleId = 1, string bank = "A", int answered = 0)
    {
        ModuleId = moduleId is >= 1 and <= 8 ? moduleId : 1;
        Bank = bank.Length == 1 && bank[0] is >= 'A' and <= 'C' ? bank.ToUpperInvariant() : "A";
        Answered = Math.Clamp(answered, 0, 10);
    }

    private static readonly string[] ModuleTitles =
    [
        "Cybersecurity Fundamentals", "Phishing and Social Engineering", "Password and Identity Security", "Data Security and Privacy",
        "Network and Internet Security", "Device and Physical Security", "AI Literacy and AI Security", "Incident Reporting and Cyber Hygiene"
    ];
}
