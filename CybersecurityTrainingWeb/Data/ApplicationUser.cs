using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CybersecurityTrainingWeb.Data;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [StringLength(100)]
    public string? JobTitle { get; set; }
}