using System.ComponentModel.DataAnnotations;

namespace CybersecurityTrainingWeb.Data;

public sealed class AssessmentBank
{
    public long Id { get; set; }

    [Range(1, 8)]
    public int ModuleId { get; set; }

    [Required, StringLength(1)]
    public string Code { get; set; } = ""; // A, B, or C

    public int Version { get; set; } = 1;
    public bool IsPublished { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<AssessmentQuestion> Questions { get; set; } =
        new List<AssessmentQuestion>();
}

public sealed class AssessmentQuestion
{
    public long Id { get; set; }

    public long AssessmentBankId { get; set; }
    public AssessmentBank Bank { get; set; } = null!;

    public int Number { get; set; }

    [Required, StringLength(2000)]
    public string Text { get; set; } = "";

    [Required, StringLength(32)]
    public string Type { get; set; } = "multipleChoice";

    // Keep marking information on the server; never include it
    // in the learner-facing question response.
    [StringLength(8)]
    public string? CorrectOptionCode { get; set; }

    [StringLength(4000)]
    public string? Explanation { get; set; }

    // Scenario/free-text answers need a reviewed marking rubric.
    [StringLength(4000)]
    public string? ReviewRubric { get; set; }

    public ICollection<AssessmentOption> Options { get; set; } =
        new List<AssessmentOption>();
}

public sealed class AssessmentOption
{
    public long Id { get; set; }

    public long AssessmentQuestionId { get; set; }
    public AssessmentQuestion Question { get; set; } = null!;

    [Required, StringLength(8)]
    public string Code { get; set; } = "";

    [Required, StringLength(1000)]
    public string Text { get; set; } = "";
}