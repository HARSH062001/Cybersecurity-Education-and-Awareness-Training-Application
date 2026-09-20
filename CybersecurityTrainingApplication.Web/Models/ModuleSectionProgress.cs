namespace CybersecurityTrainingApplication.Web.Models;

public class ModuleSectionProgress
{
    public int Id { get; set; }
    public string LearnerKey { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public DateTimeOffset CompletedAtUtc { get; set; }
}
