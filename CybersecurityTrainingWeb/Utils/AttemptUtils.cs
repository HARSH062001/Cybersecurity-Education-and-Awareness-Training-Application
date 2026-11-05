namespace CybersecurityTrainingWeb.Utils;

public static class AttemptUtils
{
    /// <summary>
    /// Chooses among the least-used banks and avoids repeating the last bank
    /// when another bank is equally available. The caller still checks that
    /// the selected bank has a published version for the module.
    /// </summary>
    public static string PickBank(IReadOnlyList<string> banksAttempted)
    {
        string[] bankCodes = ["A", "B", "C"];
        var counts = bankCodes.ToDictionary(
            code => code,
            code => banksAttempted.Count(attempted =>
                string.Equals(attempted, code, StringComparison.OrdinalIgnoreCase)));

        var leastUsed = counts.Values.Min();
        var candidates = counts
            .Where(entry => entry.Value == leastUsed)
            .Select(entry => entry.Key)
            .ToList();

        if (candidates.Count > 1 && banksAttempted.Count > 0)
        {
            var lastBank = banksAttempted[^1];
            if (candidates.Count > 1)
            {
                candidates.RemoveAll(code =>
                    string.Equals(code, lastBank, StringComparison.OrdinalIgnoreCase));
            }
        }

        return candidates[Random.Shared.Next(candidates.Count)];
    }
}
