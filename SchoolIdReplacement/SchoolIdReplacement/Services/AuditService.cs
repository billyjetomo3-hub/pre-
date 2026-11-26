using SchoolIdReplacement.Data;
using SchoolIdReplacement.Models;

namespace SchoolIdReplacement.Services
{
    /// <summary>
    /// Records user actions for traceability. Logging never interrupts the user's work.
    /// </summary>
    public static class AuditService
    {
        public static void Write(Session session, string action, string details)
        {
            try
            {
                Sql.Execute(
                    "INSERT INTO dbo.AuditLogs (UserId, Username, Action, Details) VALUES (@userId, @username, @action, @details)",
                    Sql.P("@userId", session == null ? null : (object)session.UserId),
                    Sql.P("@username", session == null ? "system" : session.Username),
                    Sql.P("@action", action),
                    Sql.P("@details", details));
            }
            catch
            {
                // Auditing is best-effort: a logging failure must not break the workflow.
            }
        }
    }
}
