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
        var selectMatch = Regex.Match(rawSql, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase);
        if (selectMatch.Success)
        {
            var columns = selectMatch.Groups[1].Value.Split(',');
            parsedSql.SelectColumns = new List<string>();
            foreach (var column in columns)
            {
                parsedSql.SelectColumns.Add(column.Trim());
            }
        }

        // Match DISTINCT clause
        var distinctMatch = Regex.Match(rawSql, @"SELECT\s+DISTINCT", RegexOptions.IgnoreCase);
        parsedSql.IsDistinct = distinctMatch.Success;

        // Match FROM clause
        var fromMatch = Regex.Match(rawSql, @"FROM\s+(\S+)", RegexOptions.IgnoreCase);
        if (fromMatch.Success)
        {
            parsedSql.Table = fromMatch.Groups[1].Value;
        }

        // Match JOIN clauses (extract properties in ON condition)
        var joinMatches = Regex.Matches(
            rawSql,
            @"(\w+)\s+JOIN\s+(\S+)\s+AS\s+(\w+)\s+ON\s+(\w+\.\w+)\s*(=|<|>|<=|>=|<>)\s*(\w+\.\w+)",
            RegexOptions.IgnoreCase
        );

        foreach (Match joinMatch in joinMatches)
        {
            // Extract table names and column names, excluding aliases
            var leftTableColumn = joinMatch.Groups[4].Value; // e.g., u.employee_id
            var rightTableColumn = joinMatch.Groups[6].Value; // e.g., e.id

            // Remove alias (before the dot in the left and right columns)
            var leftColumn = leftTableColumn.Split('.')[1]; // Get the column name after the table name
            var rightColumn = rightTableColumn.Split('.')[1]; // Get the column name after the table name

            parsedSql.Joins.Add(
                new JoinClause
                {
                    JoinType = joinMatch.Groups[1].Value.ToUpper(), // JOIN type (INNER, LEFT, etc.)
                    RightTable = joinMatch.Groups[1].Value, // Table name (without alias)
                    LeftTable = joinMatch.Groups[2].Value, // Table name (without alias)
                    LeftProperty = leftColumn, // Left column name (without alias)
                    RightProperty = rightColumn // Right column name (without alias)
                }
            );
        }

        // Match WHERE clause
        var whereMatch = Regex.Match(
            rawSql,
            @"WHERE\s+(.*?)(?=\s+GROUP BY|\s+ORDER BY|\s+HAVING|\s*$)",
            RegexOptions.IgnoreCase
        );
        if (whereMatch.Success)
        {
            parsedSql.WhereClause = whereMatch.Groups[1].Value.Trim();
        }

        // Match GROUP BY clause
        var groupByMatch = Regex.Match(
            rawSql,
            @"GROUP BY\s+(.*?)(?=\s+HAVING|\s+ORDER BY|\s*$)",
            RegexOptions.IgnoreCase
        );
        if (groupByMatch.Success)
        {
            parsedSql.GroupByClause = groupByMatch.Groups[1].Value.Trim();
        }

        // Match HAVING clause
        var havingMatch = Regex.Match(
            rawSql,
            @"HAVING\s+(.*?)(?=\s+ORDER BY|\s*$)",
            RegexOptions.IgnoreCase
        );
        if (havingMatch.Success)
        {
            parsedSql.HavingClause = havingMatch.Groups[1].Value.Trim();
        }

        // Match ORDER BY clause
        var orderByMatch = Regex.Match(
            rawSql,
            @"ORDER BY\s+(.*?)(?=\s+LIMIT|\s*$)",
            RegexOptions.IgnoreCase
        );
        if (orderByMatch.Success)
        {
            parsedSql.OrderByClause = orderByMatch.Groups[1].Value.Trim();
        }

        // Match LIMIT clause
        var limitMatch = Regex.Match(rawSql, @"LIMIT\s+(\d+)", RegexOptions.IgnoreCase);
        if (limitMatch.Success)
        {
            parsedSql.Limit = int.Parse(limitMatch.Groups[1].Value);
        }

        // Match IN clause
        var inMatch = Regex.Match(rawSql, @"IN\s*\((.*?)\)", RegexOptions.IgnoreCase);
        if (inMatch.Success)
        {
            parsedSql.InClause = inMatch.Groups[1].Value.Trim();
        }

        // Match BETWEEN clause
        var betweenMatch = Regex.Match(
            rawSql,
            @"BETWEEN\s+(.*?)\s+AND\s+(.*)",
            RegexOptions.IgnoreCase
        );
        if (betweenMatch.Success)
        {
            parsedSql.BetweenClause = new List<string>
            {
                betweenMatch.Groups[1].Value.Trim(),
                betweenMatch.Groups[2].Value.Trim()
            };
        }

        // Match LIKE clause
        var likeMatch = Regex.Match(rawSql, @"LIKE\s+'(.*?)'", RegexOptions.IgnoreCase);
        if (likeMatch.Success)
        {
            parsedSql.LikeClause = likeMatch.Groups[1].Value.Trim();
        }

        // Match IS NULL clause
        var isNullMatch = Regex.Match(rawSql, @"IS\s+NULL", RegexOptions.IgnoreCase);
        if (isNullMatch.Success)
        {
            parsedSql.IsNullClause = true;
        }

        // Match EXISTS clause
        var existsMatch = Regex.Match(rawSql, @"EXISTS\s+\(.*?\)", RegexOptions.IgnoreCase);
        if (existsMatch.Success)
        {
            parsedSql.ExistsClause = existsMatch.Value.Trim();
        }

        // Match ANY and ALL clauses
        var anyMatch = Regex.Match(rawSql, @"ANY\s+\(.*?\)", RegexOptions.IgnoreCase);
        if (anyMatch.Success)
        {
            parsedSql.AnyClause = anyMatch.Value.Trim();
        }

        var allMatch = Regex.Match(rawSql, @"ALL\s+\(.*?\)", RegexOptions.IgnoreCase);
        if (allMatch.Success)
        {
            parsedSql.AllClause = allMatch.Value.Trim();
        }

        // Match UNION, INTERSECT, and EXCEPT clauses
        var unionMatch = Regex.Match(rawSql, @"UNION\s+(ALL)?", RegexOptions.IgnoreCase);
        if (unionMatch.Success)
        {
            parsedSql.UnionClause = unionMatch.Value.Trim();
        }

        var intersectMatch = Regex.Match(rawSql, @"INTERSECT", RegexOptions.IgnoreCase);
        if (intersectMatch.Success)
        {
            parsedSql.IntersectClause = intersectMatch.Value.Trim();
        }

        var exceptMatch = Regex.Match(rawSql, @"EXCEPT", RegexOptions.IgnoreCase);
        if (exceptMatch.Success)
        {
            parsedSql.ExceptClause = exceptMatch.Value.Trim();
        }

        // Match FETCH clause
        var fetchMatch = Regex.Match(
            rawSql,
            @"FETCH\s+(FIRST|NEXT)\s+\d+\s+ROWS\s+ONLY",
            RegexOptions.IgnoreCase
        );
        if (fetchMatch.Success)
        {
            parsedSql.FetchClause = fetchMatch.Value.Trim();
        }

        // Match WITH clause (CTE)
        var withMatch = Regex.Match(
            rawSql,
            @"WITH\s+(\w+)\s+AS\s+\(.*?\)",
            RegexOptions.IgnoreCase
        );
        if (withMatch.Success)
        {
            parsedSql.WithClause = withMatch.Value.Trim();
        }

        // Match SHOW clause (for metadata queries)
        var showMatch = Regex.Match(
            rawSql,
            @"SHOW\s+(TABLES|COLUMNS|DATABASES|VIEWS|INDEXES)",
            RegexOptions.IgnoreCase
        );
        if (showMatch.Success)
        {
            parsedSql.ShowClause = showMatch.Value.Trim();
        }

        // Match EXPLAIN clause
        var explainMatch = Regex.Match(rawSql, @"EXPLAIN\s+(.*)", RegexOptions.IgnoreCase);
        if (explainMatch.Success)
        {
            parsedSql.ExplainClause = explainMatch.Groups[1].Value.Trim();
        }

        return parsedSql;
    }
}
