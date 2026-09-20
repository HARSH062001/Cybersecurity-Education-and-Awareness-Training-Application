using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CybersecurityTrainingApplication.Web.Pages;
public class ModulesModel : PageModel
{
    public List<ModuleCard> Modules { get; } = [
        new(1,"Cybersecurity Fundamentals","Core security goals, threats and safe employee behaviours.",0), new(2,"Phishing and Social Engineering","Recognise deceptive messages and social pressure.",0), new(3,"Password and Identity Security","Protect accounts, passwords and access permissions.",0), new(4,"Data Security and Privacy","Handle business and personal information safely.",0), new(5,"Network and Internet Security","Use networks and online services safely.",0), new(6,"Device and Physical Security","Protect devices, workplaces and removable media.",0), new(7,"AI Literacy and AI Security","Use AI tools responsibly and protect sensitive information.",0), new(8,"Incident Reporting and Cyber Hygiene","Build daily habits and report incidents early.",0)
    ];
    public record ModuleCard(int Number,string Title,string Summary,int Progress);
}
