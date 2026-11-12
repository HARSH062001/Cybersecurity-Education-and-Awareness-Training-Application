// Loads and handles server-side data for administrator assignment of module owners.
using CybersecurityTrainingWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Pages;

// Only administrators may manage module-owner assignments.
[Authorize(Roles = "Admin")]
public class AdminModuleOwnersModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminModuleOwnersModel(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Keep these IDs and titles aligned with Data/module-content.json.
    public static IReadOnlyList<ModuleChoice> Modules { get; } =
    [
        new(1, "Module 1 – Cybersecurity Fundamentals"),
        new(2, "Module 2 – Phishing and Social Engineering"),
        new(3, "Module 3 – Password and Identity Security"),
        new(4, "Module 4 – Data Security and Privacy"),
        new(5, "Module 5 – Network and Internet Security"),
        new(6, "Module 6 – Device and Physical Security"),
        new(7, "Module 7 – AI Literacy and AI Security"),
        new(8, "Module 8 – Incident Reporting and Cyber Hygiene")
    ];

    [BindProperty]
    public int ModuleId { get; set; }

    [BindProperty]
    public string OwnerUserId { get; set; } = "";

    public List<UserChoice> Users { get; private set; } = [];
    public List<AssignmentRow> Assignments { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadPageDataAsync();
        Response.Headers.CacheControl = "no-store";
    }

    // Add an owner assignment and ensure the account has the ModuleOwner role.
    public async Task<IActionResult> OnPostAssignAsync()
    {
        if (ModuleId is < 1 or > 8)
        {
            ModelState.AddModelError(
                nameof(ModuleId),
                "Choose a valid module.");
        }

        var owner = await _userManager.FindByIdAsync(OwnerUserId);

        if (owner is null)
        {
            ModelState.AddModelError(
                nameof(OwnerUserId),
                "Choose a registered account.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            return Page();
        }

        var alreadyAssigned = await _db.ModuleOwnerAssignments
            .AnyAsync(assignment =>
                assignment.ModuleId == ModuleId &&
                assignment.OwnerUserId == OwnerUserId);

        if (alreadyAssigned)
        {
            ModelState.AddModelError(
                nameof(OwnerUserId),
                "That account already owns this module.");

            await LoadPageDataAsync();
            return Page();
        }

        // The role gates owner-only pages; the assignment scopes their modules.
        if (!await _userManager.IsInRoleAsync(owner!, "ModuleOwner"))
        {
            var roleResult = await _userManager.AddToRoleAsync(
                owner!,
                "ModuleOwner");

            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not assign the ModuleOwner role: " +
                    string.Join(
                        ", ",
                        roleResult.Errors.Select(error =>
                            error.Description)));

                await LoadPageDataAsync();
                return Page();
            }
        }

        _db.ModuleOwnerAssignments.Add(new ModuleOwnerAssignment
        {
            ModuleId = ModuleId,
            OwnerUserId = owner!.Id
        });

        await _db.SaveChangesAsync();

        StatusMessage = "Module owner assigned.";
        return RedirectToPage();
    }

    // Remove one module assignment. The account keeps its role for now.
    public async Task<IActionResult> OnPostRemoveAsync(long assignmentId)
    {
        var assignment = await _db.ModuleOwnerAssignments
            .SingleOrDefaultAsync(item => item.Id == assignmentId);

        if (assignment is not null)
        {
            _db.ModuleOwnerAssignments.Remove(assignment);
            await _db.SaveChangesAsync();
            StatusMessage = "Module owner assignment removed.";
        }

        return RedirectToPage();
    }

    private async Task LoadPageDataAsync()
    {
        Users = await _db.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Select(user => new UserChoice(
                user.Id,
                user.FullName,
                user.Email))
            .ToListAsync();

        Assignments = await _db.ModuleOwnerAssignments
            .AsNoTracking()
            .Include(assignment => assignment.Owner)
            .OrderBy(assignment => assignment.ModuleId)
            .ThenBy(assignment => assignment.Owner.Email)
            .Select(assignment => new AssignmentRow(
                assignment.Id,
                assignment.ModuleId,
                assignment.Owner.FullName,
                assignment.Owner.Email))
            .ToListAsync();
    }

    public sealed record ModuleChoice(int Id, string Title);

    public sealed record UserChoice(
        string Id,
        string? FullName,
        string? Email);

    public sealed record AssignmentRow(
        long Id,
        int ModuleId,
        string? FullName,
        string? Email);
}