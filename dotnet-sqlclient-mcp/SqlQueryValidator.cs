using System.Text.RegularExpressions;

internal static class SqlQueryValidator
{
    private static readonly Regex MultipleStatementsRegex = new(@";\s*\S", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DdlRegex = new(@"\b(CREATE|ALTER|DROP|TRUNCATE|RENAME|EXEC|EXECUTE|SP_|XP_)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static void Validate(string query, params string[] expectedStartKeywords)
    {
        string trimmed = query.TrimStart();

        if (DdlRegex.IsMatch(trimmed))
        {
            throw new ArgumentException("DDL and stored procedure statements are not allowed.");
        }

        if (MultipleStatementsRegex.IsMatch(trimmed))
        {
            throw new ArgumentException("Multiple statements in a single call are not allowed.");
        }

        bool startsWithExpectedKeyword = expectedStartKeywords.Any(keyword => trimmed.StartsWith(keyword, StringComparison.OrdinalIgnoreCase));
        if (!startsWithExpectedKeyword)
        {
            throw new ArgumentException($"Query must start with one of the following keywords: {string.Join(", ", expectedStartKeywords)}");
        }
    }
}