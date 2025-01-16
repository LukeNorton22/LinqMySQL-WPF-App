using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqMySQL;

public class ParsedSql
{
    public List<string> SelectColumns { get; set; } = new List<string>();
    public string Table { get; set; }
    public string WhereClause { get; set; }
    public string OrderByClause { get; set; }
    public List<JoinClause> Joins { get; set; } = new List<JoinClause>();
}

public class JoinClause
{
    public string Table { get; set; }
    public string OnCondition { get; set; }
}
