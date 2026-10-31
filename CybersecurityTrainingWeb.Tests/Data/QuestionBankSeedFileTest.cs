using CybersecurityTrainingWeb.Data;

namespace CybersecurityTrainingWeb.Tests.Data
{
    // Tests whether the ReadSileFile method in QuestionBankSeeder.cs properly reads the banks, questions, and options
    // TODO: Make tests for checking whether the JSON file gets properly seeded
    // to the database
    public class ReadSeedFileTest
        {
            // Tests whether the ReadSeedFile method properly reads 24 banks
            [Fact]
            public void ReadSeedFile_HasTwentyFourBanks()
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "question_banks.json");
                var banks = QuestionBankSeeder.ReadSeedFile(path);
                Assert.Equal(24, banks.Count);
            }

            // Tests whether all banks have 10 questions
            [Fact]
            public void ReadSeedFile_AllBanksHaveTenQuestions()
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "question_banks.json");
                var banks = QuestionBankSeeder.ReadSeedFile(path);
                Assert.All(banks, bank => 
                {
                    Assert.Equal(10, bank.Questions.Count);
                });
            }

            // Tests whether all multiple-choice questions have 4 options
            // Currently, no scenario questions exist in the banks
            [Fact]
            public void ReadSeedFile_AllQuestionHaveFourOptions()
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "question_banks.json");
                var banks = QuestionBankSeeder.ReadSeedFile(path);
                var questions = banks
                    .SelectMany(bank => bank.Questions)
                    .Where(question => question.Type == "multipleChoice");

                Assert.All(questions, question =>
                {
                    Assert.Equal(4, question.Options.Count);
                });
            }
        }
}
