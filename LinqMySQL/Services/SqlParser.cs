using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LinqMySQL;

public static class SqlParser
{
    public static ParsedSql Parse(string rawSql)
    {
        if (string.IsNullOrWhiteSpace(rawSql))
        {
            throw new ArgumentException("SQL query cannot be empty.");
        }

        var parsedSql = new ParsedSql();

        // Match SELECT clause
        var selectMatch = Regex.Match(
            rawSql,
            @"SELECT\s+(.*?)(?=\s+FROM)",
            RegexOptions.IgnoreCase
        );
        if (selectMatch.Success)
        {
            var columns = selectMatch.Groups[1].Value.Split(',');
            parsedSql.SelectColumns = new List<string>();
            foreach (var column in columns)
            {
                parsedSql.SelectColumns.Add(column.Trim());
            }
        }

        // Match FROM clause
        var fromMatch = Regex.Match(rawSql, @"FROM\s+(\S+)", RegexOptions.IgnoreCase);
        if (fromMatch.Success)
        {
            parsedSql.Table = fromMatch.Groups[1].Value;
        }

        // Match WHERE clause
        var whereMatch = Regex.Match(
            rawSql,
            @"WHERE\s+(.*?)(?=\s+ORDER BY|\s*$)",
            RegexOptions.IgnoreCase
        );
        if (whereMatch.Success)
        {
            parsedSql.WhereClause = whereMatch.Groups[1].Value.Trim();
        }

        // Match ORDER BY clause
        var orderByMatch = Regex.Match(rawSql, @"ORDER BY\s+(.*)", RegexOptions.IgnoreCase);
        if (orderByMatch.Success)
        {
            parsedSql.OrderByClause = orderByMatch.Groups[1].Value.Trim();
        }

        return parsedSql;
    }
}
