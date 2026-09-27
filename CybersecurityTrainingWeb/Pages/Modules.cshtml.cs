using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingWeb.Pages;

public class ModulesModel : PageModel
{
    public List<ModuleCard> Modules { get; } =
    [
        new(
            1,
            "Cybersecurity Fundamentals",
            "Core security goals, threats and safe employee behaviours."
        ),
        new(
            2,
            "Phishing and Social Engineering",
            "Recognise deceptive messages and social pressure."
        )
    ];

    public record ModuleCard(int Number, string Title, string Summary);
}