using System.ComponentModel.DataAnnotations;
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CybersecurityTrainingWeb.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string Email { get; private set; } = "";

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email ?? "";
        Input = new ProfileInput
        {
            FullName = user.FullName,
            Department = user.Department,
            JobTitle = user.JobTitle
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email ?? "";

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.FullName = Clean(Input.FullName);
        user.Department = Clean(Input.Department);
        user.JobTitle = Clean(Input.JobTitle);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        TempData["ProfileStatus"] = "Your profile has been saved.";
        return RedirectToPage();
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public class ProfileInput
    {
        [Display(Name = "Full name")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Job title")]
        [StringLength(100)]
        public string? JobTitle { get; set; }
    }
}