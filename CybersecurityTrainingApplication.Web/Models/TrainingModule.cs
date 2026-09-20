namespace CybersecurityTrainingApplication.Web.Models;

public class TrainingModule
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<QuestionBank> QuestionBanks { get; set; } = [];
}
public class QuestionBank
{
    public int Id { get; set; }
    public int TrainingModuleId { get; set; }
    public TrainingModule TrainingModule { get; set; } = null!;
    public string BankCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = [];
}
public class Question
{
    public int Id { get; set; }
    public int QuestionBankId { get; set; }
    public QuestionBank QuestionBank { get; set; } = null!;
    public string QuestionText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string BusinessImpact { get; set; } = string.Empty;
    public string ReferenceText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<AnswerOption> AnswerOptions { get; set; } = [];
}
public class AnswerOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string OptionCode { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
