// Stores the first assessment due date and repeat interval for a module.
namespace CybersecurityTrainingWeb.Data;

// A module schedule supplies the first deadline and recurring assessment interval.
public sealed class ModuleSchedule
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public DateTime FirstDueAtUtc { get; set; }
    public int RepeatEveryDays { get; set; }
}
