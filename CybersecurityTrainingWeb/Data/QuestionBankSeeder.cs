using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Data;

public static class QuestionBankSeeder
{
    public static void Seed(DbContext context)
    {
        var banks = ReadSeedFile();

        //Check if the bank exists in the db and if the versions match
        //If yes, then skip adding the database
        var existingKeys = context.Set<AssessmentBank>()
            .Select(b => new {b.ModuleId, b.Code, b.Version })
            .AsEnumerable()
            .Select(b => (b.ModuleId, b.Code, b.Version))
            .ToHashSet();

        var added = 0;
        foreach (var bank in banks)
        {
            if (existingKeys.Contains((bank.ModuleId, bank.Bank, bank.Version)))
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
                    AssessmentBankId = bank.ModuleId,
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
        context.SaveChanges();
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

    public static List<BankJsonDto> ReadSeedFile()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "question_banks.json");
        var text = File.ReadAllText(path);

        var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<BankJsonDto>>(text, options) ?? [];
    }
}