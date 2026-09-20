using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CybersecurityTrainingApplication.Web.Pages;
public class ResultsModel:PageModel{public string Bank{get;private set;}="A";public void OnGet(string bank="A"){Bank=bank;}}
