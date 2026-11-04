using System.ComponentModel.DataAnnotations;

namespace CybersecurityTrainingWeb.Data;

// Records which modules a learner has enrolled in and when.
public sealed class ModuleEnrollment
{
    public long Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public ApplicationUser User { get; set; } = null!;

    // Module IDs match the IDs used by the learning content.
    [Range(1, 8)]
    public int ModuleId { get; set; }

    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
}