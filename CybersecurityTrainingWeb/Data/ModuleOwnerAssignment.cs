// Links an authorised owner to a module they manage.
using System.ComponentModel.DataAnnotations;

namespace CybersecurityTrainingWeb.Data;

// Connects a module to a user who is allowed to manage it.
public sealed class ModuleOwnerAssignment
{
    public long Id { get; set; }

    // Module IDs match the IDs used by the learning content.
    [Range(1, 8)]
    public int ModuleId { get; set; }

    [Required]
    public string OwnerUserId { get; set; } = "";

    public ApplicationUser Owner { get; set; } = null!;
}
