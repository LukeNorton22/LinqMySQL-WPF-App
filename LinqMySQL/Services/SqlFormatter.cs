using System.Text.RegularExpressions;

public static class SqlFormatter
{
    public static string Format(string rawSql)
    {
        if (string.IsNullOrWhiteSpace(rawSql))
            return string.Empty;

        // Basic formatting rules
        string formattedSql = Regex.Replace(rawSql, @"\s+", " "); // Remove extra spaces
        formattedSql = Regex.Replace(
            formattedSql,
            @"(?i)\b(SELECT|FROM|WHERE|ORDER BY|JOIN|ON|GROUP BY|HAVING)\b",
            match => match.Value.ToUpper()
        ); // Capitalize keywords
        formattedSql = formattedSql
            .Replace(",", ",\n    ")
            .Replace("SELECT", "SELECT\n    ")
            .Replace("FROM", "\nFROM\n    ");

        return formattedSql.Trim();
    }
}
