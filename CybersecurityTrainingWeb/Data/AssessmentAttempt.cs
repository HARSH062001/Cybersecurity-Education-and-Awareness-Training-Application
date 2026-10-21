using System.ComponentModel.DataAnnotations;

namespace CybersecurityTrainingWeb.Data;

public enum AssessmentAttemptStatus
{
    InProgress,
    AwaitingReview,
    Completed
}

public sealed class AssessmentAttempt
{
    public long Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;

    public long AssessmentBankId { get; set; }
    public AssessmentBank Bank { get; set; } = null!;

    public AssessmentAttemptStatus Status { get; set; } =
        AssessmentAttemptStatus.InProgress;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? MarkedAtUtc { get; set; }

    // Remain null while an attempt needs scenario-answer review.
    public int? Score { get; set; }
    public int? TotalQuestions { get; set; }

    public ICollection<AssessmentAnswer> Answers { get; set; } =
        new List<AssessmentAnswer>();
}

public sealed class AssessmentAnswer
{
    public long Id { get; set; }

    public long AssessmentAttemptId { get; set; }
    public AssessmentAttempt Attempt { get; set; } = null!;

    public long AssessmentQuestionId { get; set; }
    public AssessmentQuestion Question { get; set; } = null!;

    [StringLength(8)]
    public string? SelectedOptionCode { get; set; }

    [StringLength(8000)]
    public string? WrittenResponse { get; set; }

    public bool? IsCorrect { get; set; }
    public int? PointsAwarded { get; set; }

    [StringLength(4000)]
    public string? Feedback { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }
}