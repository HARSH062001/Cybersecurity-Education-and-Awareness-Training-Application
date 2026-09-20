using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CybersecurityTrainingApplication.Web.Pages;
public class AssessmentModel : PageModel
{
 public string Bank{get;private set;}="A"; public int QuestionNumber=>1; public int TotalQuestions=>10; public string Question=>"Which security goal means that only authorised people can view information?"; public Option[] Options=>[new("A","Confidentiality"),new("B","Availability"),new("C","Integrity"),new("D","Convenience")]; public void OnGet(string bank="A"){Bank=bank;}
 public record Option(string Code,string Text);
}
