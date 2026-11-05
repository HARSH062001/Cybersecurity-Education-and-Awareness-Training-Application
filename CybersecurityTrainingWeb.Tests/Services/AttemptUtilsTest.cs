using CybersecurityTrainingWeb.Utils;

namespace CybersecurityTrainingWeb.Tests.Data
{
    // Tests the AttemptUtils class
    // based on the banks that have been attempted
    public class AttemptUtilsTest
    {

        [Theory]
        [InlineData(new string[] { }, new string[] { "A", "B", "C" })]
        [InlineData(new[] { "A" }, new[] { "B", "C" })]
        [InlineData(new[] { "A", "B" }, new[] { "C" })]
        [InlineData(new[] { "A", "B", "C" }, new[] { "A", "B" })]
        [InlineData(new[] { "A", "B", "C", "A" }, new[] { "B", "C" })]
        [InlineData(new[] { "A", "B", "C", "A", "B" }, new[] { "C" })]
        [InlineData(new[] { "A", "B", "C", "A", "B", "C" }, new[] { "A", "B" })]
        [InlineData(new[] { "A", "B", "B", "C" }, new[] { "A" })] // This will never happen but just in case
        public void PickBank_ReturnsRandomBank(string[] attemptedBanks, string[] expectedBanks)
        {
            for (int i = 0; i < 10; i++) // Run the test multiple times to account for randomness
            {
                var bankCode = AttemptUtils.PickBank(attemptedBanks);
                Assert.Contains(bankCode, expectedBanks);
            }

        }
    }
}