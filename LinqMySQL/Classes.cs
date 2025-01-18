using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqMySQL;

public class ParsedSql
{
    // Columns selected in the SELECT clause
    public List<string> SelectColumns { get; set; } = new List<string>();

    // The table in the FROM clause
    public string Table { get; set; } = string.Empty;

    // WHERE clause condition
    public string WhereClause { get; set; } = string.Empty;

    // ORDER BY clause condition
    public string OrderByClause { get; set; } = string.Empty;

    // List of JOIN clauses
    public List<JoinClause> Joins { get; set; } = new List<JoinClause>();

    // DISTINCT flag for SELECT DISTINCT queries
    public bool IsDistinct { get; set; }

    // GROUP BY clause
    public string GroupByClause { get; set; } = string.Empty;

    // HAVING clause
    public string HavingClause { get; set; } = string.Empty;

    // LIMIT value for limiting the number of rows
    public int? Limit { get; set; }

    // IN clause values
    public string InClause { get; set; } = string.Empty;

    // BETWEEN clause values (list of two values for range)
    public List<string> BetweenClause { get; set; } = new List<string>();

    // LIKE clause value
    public string LikeClause { get; set; } = string.Empty;

    // IS NULL clause flag
    public bool IsNullClause { get; set; }

    // EXISTS clause condition
    public string ExistsClause { get; set; } = string.Empty;

    // ANY clause condition
    public string AnyClause { get; set; } = string.Empty;

    // ALL clause condition
    public string AllClause { get; set; } = string.Empty;

    // UNION clause condition
    public string UnionClause { get; set; } = string.Empty;

    // INTERSECT clause condition
    public string IntersectClause { get; set; } = string.Empty;

    // EXCEPT clause condition
    public string ExceptClause { get; set; } = string.Empty;

    // FETCH clause condition
    public string FetchClause { get; set; } = string.Empty;

    // WITH clause (Common Table Expression)
    public string WithClause { get; set; } = string.Empty;

    // SHOW clause (metadata query, like SHOW TABLES)
    public string ShowClause { get; set; } = string.Empty;

    // EXPLAIN clause for query analysis
    public string ExplainClause { get; set; } = string.Empty;
}

public class JoinClause
{
    public string JoinType { get; set; } = string.Empty;
    public string LeftTable { get; set; } = string.Empty;
    public string RightTable { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string LeftProperty { get; set; } = string.Empty; // e.g., u.employee_id
    public string RightProperty { get; set; } = string.Empty; // e.g., e.id
}
