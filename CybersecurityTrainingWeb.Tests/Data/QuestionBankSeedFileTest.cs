using CybersecurityTrainingWeb.Data;

namespace CybersecurityTrainingWeb.Tests.Data;

public class ReadSeedFileTest
{
    private static List<QuestionBankSeeder.BankJsonDto> ReadBanks()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "question_banks.json");

        return QuestionBankSeeder.ReadSeedFile(path);
    }

    [Fact]
    public void ReadSeedFile_HasTwentyFourBanks()
    {
        Assert.Equal(24, ReadBanks().Count);
    }

    [Fact]
    public void ReadSeedFile_AllBanksHaveTenQuestions()
    {
        Assert.All(
            ReadBanks(),
            bank => Assert.Equal(10, bank.Questions.Count));
    }

    [Fact]
    public void ReadSeedFile_AllMultipleChoiceQuestionsHaveFourOptions()
    {
        var questions = ReadBanks()
            .SelectMany(bank => bank.Questions)
            .Where(question => question.Type == "multipleChoice");

        Assert.All(
            questions,
            question => Assert.Equal(4, question.Options.Count));
    }

    [Fact]
    public void ReadSeedFile_EveryMultipleChoiceQuestionHasAnswerAndExplanation()
    {
        foreach (var bank in ReadBanks())
        {
            foreach (var question in bank.Questions
                         .Where(q => q.Type == "multipleChoice"))
            {
                Assert.Contains(
                    question.Options,
                    option => option.Code == question.CorrectAnswer);

                Assert.False(
                    string.IsNullOrWhiteSpace(question.Explanation),
                    $"Missing explanation: module {bank.ModuleId}, " +
                    $"bank {bank.Bank}, question {question.Number}");
            }
        }
    }
}