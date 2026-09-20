using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CybersecurityTrainingApplication.Web.Pages;
public class ModulesModel : PageModel
{
    public List<ModuleCard> Modules { get; } = [
        new(1,"Cybersecurity Fundamentals","Core security goals, threats and safe employee behaviours."), new(2,"Phishing and Social Engineering","Recognise deceptive messages and social pressure."), new(3,"Password and Identity Security","Protect accounts, passwords and access permissions."), new(4,"Data Security and Privacy","Handle business and personal information safely."), new(5,"Network and Internet Security","Use networks and online services safely."), new(6,"Device and Physical Security","Protect devices, workplaces and removable media."), new(7,"AI Literacy and AI Security","Use AI tools responsibly and protect sensitive information."), new(8,"Incident Reporting and Cyber Hygiene","Build daily habits and report incidents early.")
    ];
    public record ModuleCard(int Number,string Title,string Summary);
}
