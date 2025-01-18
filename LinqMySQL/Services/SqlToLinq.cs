using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinqMySQL.Services
{
    public class SqlToLinq
    {
        public static string ConvertSqlToLinq(ParsedSql parsedSql)
        {
            var linqQuery = $"var query = dbContext.{parsedSql.Table}";

            // Apply Joins
            foreach (var join in parsedSql.Joins)
            {
                // Generate dynamic aliases based on the first letter of table names
                var leftAlias = join.LeftTable.Substring(0, 1).ToLower(); // First letter of LeftTable
                var rightAlias = join.RightTable.Substring(0, 1).ToLower(); // First letter of RightTable

                linqQuery +=
                    $"\n    .Join(dbContext.{join.LeftTable}, {leftAlias} => {leftAlias}.{join.LeftProperty}, {rightAlias} => {rightAlias}.{join.RightProperty}, ({leftAlias}, {rightAlias}) => new {{ {leftAlias}, {rightAlias} }})";
            }

            // Handle Select after Joins
            if (parsedSql.SelectColumns.Count > 0)
            {
                var selectColumns = string.Join(
                    ", ",
                    parsedSql.SelectColumns.Select(c =>
                    {
                        // Check if the column has a table alias prefix (e.g., 'x.name' or 'y.name')
                        if (c.Contains("."))
                        {
                            var parts = c.Split('.');
                            var alias = parts[0]; // 'x' or 'y'
                            var column = parts[1]; // 'name' or 'id'

                            // Ensure that alias is correctly matched to the join
                            return $"x.{alias}.{column}";
                        }
                        return $"x.{c}"; // Default case, for un-aliased columns
                    })
                );

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
                string linqWhere = ConvertWhereClauseToLinq(parsedSql.WhereClause, parsedSql);
                linqQuery += $"\n    .Where(x => {linqWhere})";
            }

            // Apply Order By
            if (!string.IsNullOrEmpty(parsedSql.OrderByClause))
            {
                linqQuery += $"\n    .OrderBy(x => {parsedSql.OrderByClause})";
            }

            linqQuery += ";";

            return linqQuery;
        }

        public static string ConvertWhereClauseToLinq(string whereClause, ParsedSql parsedSql)
        {
            var firstColumn = true;
            // Step 1: Replace column names with LINQ property references (e.g., "name" -> "x.Name")
            whereClause = Regex.Replace(
                whereClause,
                @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b", // Match words that look like column names
                match =>
                {
                    string column = match.Value;
                    string[] sqlKeywords = { "AND", "OR", "NOT", "LIKE", "IN", "IS", "NULL" };

                    // Check if the match is not a SQL keyword and is not already prefixed with 'x.'
                    if (!column.StartsWith("x.") && !sqlKeywords.Contains(column.ToUpper()))
                    {
                        // Check if the match is followed by an equal sign or any comparison operator
                        // and is not followed by a quote (which means it's not a literal value

                        if (
                            match.Index + match.Length < whereClause.Length
                            && !"=<>!'".Contains(whereClause[match.Index + match.Length])
                            && firstColumn
                        )
                        {
                            firstColumn = false;
                            return $"x.{column}"; // Add 'x.' only if it's not a comparison operator
                        }
                    }

                    // Return the original match if it's a SQL keyword, already valid expression, or a literal
                    return column;
                }
            );

            // Step 2: Replace SQL operators with LINQ operators
            whereClause = whereClause.Replace("=", "==").Replace("<>", "!=");

            // Step 3: Handle IN, LIKE, and other conditions
            whereClause = Regex.Replace(
                whereClause,
                @"IN\s*\(([^)]+)\)",
                m => $"IN({m.Groups[1].Value})"
            );
            whereClause = Regex.Replace(
                whereClause,
                @"LIKE\s+'(.*?)'",
                m => $"LIKE \"{m.Groups[1].Value}\""
            );

            // Step 4: Ensure values are properly wrapped in single quotes if needed
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

        // Helper method to remove table alias (if any) from a column name
        private static string RemoveTableAlias(string column)
        {
            var parts = column.Split('.');
            return parts.Length > 1 ? parts[1] : parts[0]; // Remove table alias if present
        }

        // Helper method to determine the correct alias or table for the column
        private static string GetTableAliasForSelect(string column)
        {
            var parts = column.Split('.');
            return parts.Length > 1 ? parts[0] : "x"; // Return the alias or 'x' if no alias
        }
    }
}
