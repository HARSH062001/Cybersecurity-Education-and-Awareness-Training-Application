namespace CybersecurityTrainingWeb.Services;

/// <summary>
/// Optionally takes in a bank returns. Returns a new bank code following the rules:
/// 1. If not bank code is passed, returns a random bank code (A,b or C )
/// 2. If bank code is passed, returns a new bank code
/// 3. 
/// </summary>
public static class BankPicker
{
    // Returns either A, B, or C based on the banks that have been attempted. 
    // If no banks have been attempted, returns a random bank code
    public static string PickBank (IReadOnlyList<string> banksAttempted)
    {
        // Get the count of each bank code attempted
        var aCount = banksAttempted.Count(b => b == "A");
        var bCount = banksAttempted.Count(b => b == "B");
        var cCount = banksAttempted.Count(b => b == "C");

        var lastBank = banksAttempted.LastOrDefault(""); // the last bank attempted or empty string
        var leastAttemptedCount = Math.Min(aCount, Math.Min(bCount, cCount)); // value of least attempted bank code

        var bankDictionary = new Dictionary<string, int>
        {
            {"A", aCount},
            {"B", bCount},
            {"C", cCount}
        };

        // Get the bank codes that have been attempted the least
        var leastAttemptedBanks = bankDictionary
            .Where(item => item.Value == leastAttemptedCount)
            .Select(item => item.Key)
            .ToList();

        // if more than one bank code has been attempted the least, remove the last bank attempted from the list of least attempted banks and return a random bank code from the remaining least attempted banks
        if (leastAttemptedBanks.Count > 1)
        {
            leastAttemptedBanks.Remove(lastBank);
            var random = new Random();
            var randomIndex = random.Next(leastAttemptedBanks.Count);
            return leastAttemptedBanks[randomIndex];
        }

        // if only one, return that bank code
        else if(leastAttemptedBanks.Count == 1)
        {
            return leastAttemptedBanks[0];
        }
        else
        {
            return "A"; // default to A for edge cases
        }
    }
}