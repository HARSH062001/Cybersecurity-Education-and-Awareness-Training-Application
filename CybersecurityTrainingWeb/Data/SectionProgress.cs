using System.ComponentModel.DataAnnotations;

namespace CybersecurityTrainingWeb.Data;

public sealed class SectionProgress
{
    public long Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public int ModuleId { get; set; }

    [Required]
    [StringLength(40)]
    public string SectionKey { get; set; } = "";

    public DateTime CompletedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}