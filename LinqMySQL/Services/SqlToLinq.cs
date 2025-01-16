using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinqMySQL.Services;

public class SqlToLinq
{
    public static string ConvertSqlToLinq(ParsedSql parsedSql)
    {
        var linqQuery = $"var query = dbContext.{parsedSql.Table}";

        // Apply Select
        if (parsedSql.SelectColumns.Count > 0)
        {
            var selectColumns = string.Join(", ", parsedSql.SelectColumns.Select(c => $"x.{c}"));

            if (parsedSql.SelectColumns.Count == 1)
            {
                linqQuery += $"\n    .Select(x =>  {selectColumns})";
            }
            else
            {
                linqQuery += $"\n    .Select(x => new {{ {selectColumns} }})";
            }
        }

        // Apply Where
        if (!string.IsNullOrEmpty(parsedSql.WhereClause))
        {
            string linqWhere = ConvertWhereClauseToLinq(parsedSql.WhereClause);
            linqQuery += $"\n    .Where(x => {linqWhere})";
        }

        // Apply Order By
        if (!string.IsNullOrEmpty(parsedSql.OrderByClause))
        {
            linqQuery += $"\n    .OrderBy(x => {parsedSql.OrderByClause})";
        }

        // Apply Joins
        foreach (var join in parsedSql.Joins)
        {
            linqQuery +=
                $"\n    .Join(dbContext.{join.Table}, x => x.{join.OnCondition}, y => y.{join.OnCondition}, (x, y) => new {{ x, y }})";
        }

        linqQuery += ";";

        return linqQuery;
    }

    public static string ConvertWhereClauseToLinq(string whereClause)
    {
        // Step 1: Replace column names with LINQ property references (e.g., "name" -> "x.Name")
        whereClause = Regex.Replace(
            whereClause,
            @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b",
            match =>
            {
                // Only replace if not already prefixed with 'x.' and not a SQL keyword
                string column = match.Value;
                string[] sqlKeywords = { "AND", "OR", "NOT", "LIKE", "IN", "IS", "NULL" };
                if (!column.StartsWith("x.") && !sqlKeywords.Contains(column.ToUpper()))
                {
                    return $"x.{column}";
                }
                return column;
            }
        );

        // Step 2: Replace SQL operators with LINQ operators
        whereClause = whereClause.Replace("=", "==").Replace("<>", "!=");

        // Step 3: Ensure values are properly wrapped in single quotes if needed
        whereClause = Regex.Replace(
            whereClause,
            @"==\s*([^'\s]+)",
            match =>
            {
                string value = match.Groups[1].Value;
                // Add quotes if value is not already quoted and not a property reference
                if (!value.StartsWith("'") && !value.Contains("x."))
                {
                    value = $"'{value}'";
                }
                return $"== {value}";
            }
        );

        return whereClause;
    }
}
