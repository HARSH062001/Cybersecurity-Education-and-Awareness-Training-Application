using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CybersecurityTrainingApplication.Web.Pages;
public class ModuleModel : PageModel
{
    public int ModuleId { get; private set; } public string Title { get; private set; }=""; public string Purpose { get; private set; }=""; public string ModuleContent { get; private set; }=""; public string[] Banks=>["A","B","C"];
    public IActionResult OnGet(int id=1){ ModuleId=id; var data=Data[id is >=1 and <=8?id:1]; Title=data.title; Purpose=data.purpose; ModuleContent=data.content; return Page(); }
    private static readonly Dictionary<int,(string title,string purpose,string content)> Data = new(){
        [1]=("Cybersecurity Fundamentals","Build practical employee habits for protecting accounts, devices, information and business services.","Learn confidentiality, integrity and availability, common threats and vulnerabilities, account protection, safe information handling, device security and incident reporting."),
        [2]=("Phishing and Social Engineering","Recognise deceptive messages and social pressure before they cause harm.","Learn how urgency, authority, impersonation, links and attachments are used to influence workplace decisions."),
        [3]=("Password and Identity Security","Protect accounts through strong authentication and safe access decisions.","Learn passphrases, multi-factor authentication, password managers, account recovery and least privilege."),
        [4]=("Data Security and Privacy","Handle business and personal information carefully throughout its lifecycle.","Learn classification, need-to-know access, approved sharing, retention, secure disposal and privacy reporting."),
        [5]=("Network and Internet Security","Use networks and online services safely at work and remotely.","Learn secure connections, HTTPS, public Wi-Fi, VPN use, browser safety, downloads and online scams."),
        [6]=("Device and Physical Security","Protect laptops, phones, removable media and physical workspaces.","Learn screen locking, updates, endpoint protection, lost-device response, backups and safe home working."),
        [7]=("AI Literacy and AI Security","Understand AI risks while using approved tools responsibly.","Learn about inaccurate outputs, sensitive information, prompts, deepfakes, bias, copyright and human review. This module contains no AI-powered application features."),
        [8]=("Incident Reporting and Cyber Hygiene","Build reliable daily security habits and report suspicious events quickly.","Learn what to report, how to preserve useful details, follow containment instructions, maintain updates and apply daily cyber hygiene.")};
}
