using System;
using System.Data;
using System.IO;
using System.Text;
using SchoolIdReplacement.Data;

namespace SchoolIdReplacement.Services
{
    public static class ReportService
    {
        public static DataTable SummaryByStatus(DateTime from, DateTime to)
        {
            return Sql.Query(
                "SELECT r.Status AS [Status], COUNT(*) AS [Requests], ISNULL(SUM(r.FeeAmount), 0) AS [Total Fees] " +
                "FROM dbo.Requests r WHERE r.DateRequested >= @from AND r.DateRequested < @to " +
                "GROUP BY r.Status ORDER BY COUNT(*) DESC",
                Sql.P("@from", from.Date),
                Sql.P("@to", to.Date.AddDays(1)));
        }

        public static DataTable SummaryByType(DateTime from, DateTime to)
        {
            return Sql.Query(
                "SELECT r.RequestType AS [Replacement Type], COUNT(*) AS [Requests], " +
                "       SUM(CASE WHEN r.PaymentStatus = N'Paid' THEN 1 ELSE 0 END) AS [Paid], " +
                "       ISNULL(SUM(r.FeeAmount), 0) AS [Assessed Fees] " +
                "FROM dbo.Requests r WHERE r.DateRequested >= @from AND r.DateRequested < @to " +
                "GROUP BY r.RequestType ORDER BY COUNT(*) DESC",
                Sql.P("@from", from.Date),
                Sql.P("@to", to.Date.AddDays(1)));
        }

        public static DataTable RecentActivity(int limit)
        {
            return Sql.Query(
                "SELECT TOP (@limit) CONVERT(NVARCHAR(20), a.CreatedAt, 120) AS [When], a.Username AS [User], " +
                "       a.Action, a.Details FROM dbo.AuditLogs a ORDER BY a.CreatedAt DESC",
                Sql.P("@limit", limit));
        }

        public static void ExportCsv(DataTable table, string filePath)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            var builder = new StringBuilder();

            for (var column = 0; column < table.Columns.Count; column++)
            {
                if (column > 0)
                {
                    builder.Append(',');
                }
                builder.Append(Escape(table.Columns[column].ColumnName));
            }
            builder.AppendLine();

            foreach (DataRow row in table.Rows)
            {
                for (var column = 0; column < table.Columns.Count; column++)
                {
                    if (column > 0)
                    {
                        builder.Append(',');
                    }
                    builder.Append(Escape(row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column])));
                }
                builder.AppendLine();
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
