using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingWeb.Pages;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class StatusModel : PageModel
{
    public int HttpStatusCode { get; private set; }
    public string Heading { get; private set; } = "Page or assessment not found";
    public string Message { get; private set; } =
        "This page or assessment is unavailable. The link may be out of date, or it may not be available to your account.";

    public void OnGet(int statusCode)
    {
        HttpStatusCode = statusCode is >= 400 and <= 599 ? statusCode : 404;
        Response.StatusCode = HttpStatusCode;
        if (HttpStatusCode != 404)
        {
            Heading = "This page is unavailable";
            Message = "We could not open this page. Return to the modules or try again later.";
        }
    }
}
