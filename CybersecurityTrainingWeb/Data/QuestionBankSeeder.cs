// Loads validated question-bank data and adds missing versions to the database.
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Data;

public static class QuestionBankSeeder
{
    public static void Seed(DbContext context, string path)
{
    var banks = ReadSeedFile(path);

            var existingKeys = context.Set<AssessmentBank>()
            .Select(b => new { b.ModuleId, b.Code, b.Version })
            .AsEnumerable()
            .Select(b => (b.ModuleId, b.Code, b.Version))
            .ToHashSet();

        var added = 0;

        foreach (var bank in banks)
        {
            if (existingKeys.Contains(
                (bank.ModuleId, bank.Bank, bank.Version)))
            {
                continue;
            }

            var entity = new AssessmentBank
            {
                ModuleId = bank.ModuleId,
                Code = bank.Bank,
                Version = bank.Version,
                IsPublished = true,
                Questions = bank.Questions.Select(q => new AssessmentQuestion
                {
                    Number = q.Number,
                    Text = q.Text,
                    Type = q.Type,
                    CorrectOptionCode = q.CorrectAnswer,
                    Explanation = q.Explanation,
                    Options = q.Options.Select(o => new AssessmentOption
                    {
                        Code = o.Code,
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };

            context.Set<AssessmentBank>().Add(entity);
            added++;
        }

        if (added > 0)
        {
            context.SaveChanges();
        }

        Console.WriteLine($"Added {added} banks");
    }

    public sealed class OptionJsonDto
    {
        public string Code { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public sealed class QuestionJsonDto
    {
        public string Id { get; set; } = "";
        public int Number { get; set; }
        public string Text { get; set; } = "";
        public string Type { get; set; } = "";
        public List<OptionJsonDto> Options { get; set; } = [];
        public string CorrectAnswer { get; set; } = "";
        public string Explanation { get; set; } = "";
    }

    public sealed class BankJsonDto
    {
        public int ModuleId { get; set; }
        public string Bank { get; set; } = "";
        public int Version { get; set; } = 1;
        public List<QuestionJsonDto> Questions { get; set; } = [];
    }

    public static List<BankJsonDto> ReadSeedFile(string path)
    {
        var text = File.ReadAllText(path);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var banks = JsonSerializer.Deserialize<List<BankJsonDto>>(
            text,
            options) ?? [];

        foreach (var bank in banks)
        {
            foreach (var question in bank.Questions
                         .Where(q => q.Type == "multipleChoice"))
            {
                var hasCorrectOption = question.Options.Any(
                    option => option.Code == question.CorrectAnswer);

                if (!hasCorrectOption)
                {
                    throw new InvalidDataException(
                        $"Module {bank.ModuleId}, bank {bank.Bank}, " +
                        $"question {question.Number} has no matching correct option.");
                }

                if (string.IsNullOrWhiteSpace(question.Explanation))
                {
                    throw new InvalidDataException(
                        $"Module {bank.ModuleId}, bank {bank.Bank}, " +
                        $"question {question.Number} has no explanation.");
                }
            }
        }

        return banks;
    }
}
