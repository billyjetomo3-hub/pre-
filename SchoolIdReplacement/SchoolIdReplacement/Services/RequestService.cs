using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SchoolIdReplacement.Data;
using SchoolIdReplacement.Models;

namespace SchoolIdReplacement.Services
{
    public static class RequestService
    {
        public static DataTable ListAll(string statusFilter, string search)
        {
            var where = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                where.Add("r.Status = @status");
                parameters.Add(Sql.P("@status", statusFilter));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                where.Add("(r.RequestNo LIKE @search OR s.StudentNo LIKE @search OR s.FirstName LIKE @search OR s.LastName LIKE @search OR r.RequestType LIKE @search)");
                parameters.Add(Sql.P("@search", "%" + search.Trim() + "%"));
            }

            var sql =
                "SELECT r.Id, r.RequestNo AS [Request No], s.StudentNo AS [Student No], " +
                "       s.LastName + ', ' + s.FirstName AS [Student], r.RequestType AS [Type], " +
                "       r.Status, r.PaymentStatus AS [Payment], r.FeeAmount AS [Fee], " +
                "       CONVERT(NVARCHAR(20), r.DateRequested, 107) AS [Date Filed] " +
                "FROM dbo.Requests r " +
                "INNER JOIN dbo.Students s ON s.Id = r.StudentId " +
                (where.Count > 0 ? "WHERE " + string.Join(" AND ", where.ToArray()) + " " : string.Empty) +
                "ORDER BY CASE r.Status WHEN 'Pending' THEN 0 WHEN 'Approved' THEN 1 WHEN 'Ready for Pickup' THEN 2 ELSE 3 END, r.DateRequested DESC";

            return Sql.Query(sql, parameters.ToArray());
        }

        public static DataTable ListForStudent(int studentId)
        {
            return Sql.Query(
                "SELECT r.Id, r.RequestNo AS [Request No], r.RequestType AS [Type], r.Status, " +
                "       r.PaymentStatus AS [Payment], r.FeeAmount AS [Fee], " +
                "       CONVERT(NVARCHAR(20), r.DateRequested, 107) AS [Date Filed], " +
                "       CONVERT(NVARCHAR(20), r.DateReleased, 107) AS [Date Released] " +
                "FROM dbo.Requests r WHERE r.StudentId = @studentId ORDER BY r.DateRequested DESC",
                Sql.P("@studentId", studentId));
        }

        public static DataTable RecentForStudent(int studentId, int limit)
        {
            return Sql.Query(
                "SELECT TOP (@limit) r.Id, r.RequestNo AS [Request No], r.RequestType AS [Type], r.Status, " +
                "       r.PaymentStatus AS [Payment], CONVERT(NVARCHAR(20), r.DateRequested, 107) AS [Date Filed] " +
                "FROM dbo.Requests r WHERE r.StudentId = @studentId ORDER BY r.DateRequested DESC",
                Sql.P("@limit", limit),
                Sql.P("@studentId", studentId));
        }

        public static IdRequest Get(int requestId)
        {
            var table = Sql.Query(
                "SELECT r.*, s.StudentNo, s.FirstName, s.MiddleName, s.LastName " +
                "FROM dbo.Requests r INNER JOIN dbo.Students s ON s.Id = r.StudentId WHERE r.Id = @id",
                Sql.P("@id", requestId));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            var row = table.Rows[0];
            return new IdRequest
            {
                Id = Convert.ToInt32(row["Id"]),
                RequestNo = Convert.ToString(row["RequestNo"]),
                StudentId = Convert.ToInt32(row["StudentId"]),
                StudentNo = Convert.ToString(row["StudentNo"]),
                StudentName = (Convert.ToString(row["FirstName"]) + " " + Convert.ToString(row["LastName"])).Trim(),
                RequestType = Convert.ToString(row["RequestType"]),
                Reason = Convert.ToString(row["Reason"]),
                Status = Convert.ToString(row["Status"]),
                FeeAmount = Convert.ToDecimal(row["FeeAmount"]),
                PaymentStatus = Convert.ToString(row["PaymentStatus"]),
                OrNumber = row["OrNumber"] == DBNull.Value ? null : Convert.ToString(row["OrNumber"]),
                Remarks = row["Remarks"] == DBNull.Value ? null : Convert.ToString(row["Remarks"]),
                DateRequested = Convert.ToDateTime(row["DateRequested"]),
                DateUpdated = row["DateUpdated"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DateUpdated"]),
                DateReleased = row["DateReleased"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DateReleased"]),
                ProcessedBy = row["ProcessedBy"] == DBNull.Value ? null : Convert.ToString(row["ProcessedBy"])
            };
        }

        public static IdRequest Create(int studentId, string requestType, string reason)
        {
            if (studentId <= 0)
            {
                throw new InvalidOperationException("Please select a valid student.");
            }

            if (string.IsNullOrWhiteSpace(requestType))
            {
                throw new InvalidOperationException("Please choose the type of replacement.");
            }

            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 15)
            {
                throw new InvalidOperationException("Please describe the reason in at least 15 characters so the registrar can evaluate it.");
            }

            var active = Sql.Scalar(
                "SELECT COUNT(*) FROM dbo.Requests WHERE StudentId = @studentId AND Status IN (N'Pending', N'Approved', N'Ready for Pickup')",
                Sql.P("@studentId", studentId));

            if (Convert.ToInt32(active) > 0)
            {
                throw new InvalidOperationException("You already have an open request. Please wait for it to be released before filing another one.");
            }

            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    var requestNo = NextRequestNumber();
                    var id = Convert.ToInt32(Sql.Scalar(
                        "INSERT INTO dbo.Requests (RequestNo, StudentId, RequestType, Reason, Status, FeeAmount, PaymentStatus) " +
                        "OUTPUT INSERTED.Id VALUES (@requestNo, @studentId, @requestType, @reason, @status, @fee, @payment)",
                        Sql.P("@requestNo", requestNo),
                        Sql.P("@studentId", studentId),
                        Sql.P("@requestType", requestType.Trim()),
                        Sql.P("@reason", reason.Trim()),
                        Sql.P("@status", RequestStatus.Pending),
                        Sql.P("@fee", RequestTypes.FeeFor(requestType)),
                        Sql.P("@payment", PaymentStatus.Unpaid)));

                    AuditService.Write(Session.Current, "Request filed", requestNo + " - " + requestType);
                    return Get(id);
                }
                catch (SqlException ex)
                {
                    if (ex.Number != 2601 && ex.Number != 2627)
                    {
                        throw;
                    }
                }
            }

            throw new InvalidOperationException("The request number could not be generated. Please try again.");
        }

        public static void Approve(int requestId, string processedBy, string remarks)
        {
            Transition(requestId, RequestStatus.Pending, RequestStatus.Approved, processedBy, remarks, false);

            var request = Get(requestId);
            AuditService.Write(Session.Current, "Request approved", request.RequestNo);
        }

        public static void Reject(int requestId, string processedBy, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                throw new InvalidOperationException("Please write a short note explaining why the request is rejected.");
            }

            Transition(requestId, RequestStatus.Pending, RequestStatus.Rejected, processedBy, remarks, false);

            var request = Get(requestId);
            AuditService.Write(Session.Current, "Request rejected", request.RequestNo);
        }

        public static void MarkPaid(int requestId, string processedBy, string orNumber)
        {
            if (string.IsNullOrWhiteSpace(orNumber))
            {
                throw new InvalidOperationException("Please enter the official receipt number.");
            }

            var rows = Sql.Execute(
                "UPDATE dbo.Requests SET PaymentStatus = @paid, OrNumber = @orNumber, ProcessedBy = @by, DateUpdated = SYSDATETIME() " +
                "WHERE Id = @id AND Status = @approved",
                Sql.P("@paid", PaymentStatus.Paid),
                Sql.P("@orNumber", orNumber.Trim()),
                Sql.P("@by", processedBy),
                Sql.P("@id", requestId),
                Sql.P("@approved", RequestStatus.Approved));

            if (rows == 0)
            {
                throw new InvalidOperationException("Only approved requests can be marked as paid. Refresh the list and try again.");
            }

            var request = Get(requestId);
            AuditService.Write(Session.Current, "Payment recorded", request.RequestNo + " - OR " + orNumber.Trim());
        }

        public static void MarkReady(int requestId, string processedBy, string remarks)
        {
            var request = Get(requestId);
            if (request == null)
            {
                throw new InvalidOperationException("The request could not be found.");
            }

            if (request.PaymentStatus != PaymentStatus.Paid)
            {
                throw new InvalidOperationException("The replacement fee must be paid before the card can be marked ready for pickup.");
            }

            Transition(requestId, RequestStatus.Approved, RequestStatus.ReadyForPickup, processedBy, remarks, false);
            AuditService.Write(Session.Current, "Card printed", request.RequestNo + " is ready for pickup");
        }

        public static void Release(int requestId, string processedBy, string remarks)
        {
            Transition(requestId, RequestStatus.ReadyForPickup, RequestStatus.Released, processedBy, remarks, true);

            var request = Get(requestId);
            AuditService.Write(Session.Current, "Card released", request.RequestNo + " released to the student");
        }

        public static void Cancel(int requestId, string remarks)
        {
            Transition(requestId, RequestStatus.Pending, RequestStatus.Cancelled, Session.Current == null ? "student" : Session.Current.Username, remarks, false);

            var request = Get(requestId);
            AuditService.Write(Session.Current, "Request cancelled", request.RequestNo);
        }

        public static DashboardStats GetStatistics()
        {
            var stats = new DashboardStats();
            var table = Sql.Query(
                "SELECT " +
                " (SELECT COUNT(*) FROM dbo.Students) AS TotalStudents, " +
                " (SELECT COUNT(*) FROM dbo.Students WHERE IsActive = 1) AS ActiveStudents, " +
                " (SELECT COUNT(*) FROM dbo.Requests) AS TotalRequests, " +
                " (SELECT COUNT(*) FROM dbo.Requests WHERE Status = @pending) AS Pending, " +
                " (SELECT COUNT(*) FROM dbo.Requests WHERE Status = @approved) AS Approved, " +
                " (SELECT COUNT(*) FROM dbo.Requests WHERE Status = @ready) AS Ready, " +
                " (SELECT COUNT(*) FROM dbo.Requests WHERE Status = @released AND DateReleased >= DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1)) AS ReleasedThisMonth, " +
                " (SELECT ISNULL(SUM(FeeAmount), 0) FROM dbo.Requests WHERE PaymentStatus = @paid AND DateUpdated >= DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1)) AS Collected, " +
                " (SELECT COUNT(*) FROM dbo.Requests WHERE Status = @approved AND PaymentStatus = @unpaid) AS UnpaidApproved",
                Sql.P("@pending", RequestStatus.Pending),
                Sql.P("@approved", RequestStatus.Approved),
                Sql.P("@ready", RequestStatus.ReadyForPickup),
                Sql.P("@released", RequestStatus.Released),
                Sql.P("@paid", PaymentStatus.Paid),
                Sql.P("@unpaid", PaymentStatus.Unpaid));

            if (table.Rows.Count > 0)
            {
                var row = table.Rows[0];
                stats.TotalStudents = Convert.ToInt32(row["TotalStudents"]);
                stats.ActiveStudents = Convert.ToInt32(row["ActiveStudents"]);
                stats.TotalRequests = Convert.ToInt32(row["TotalRequests"]);
                stats.Pending = Convert.ToInt32(row["Pending"]);
                stats.Approved = Convert.ToInt32(row["Approved"]);
                stats.ReadyForPickup = Convert.ToInt32(row["Ready"]);
                stats.ReleasedThisMonth = Convert.ToInt32(row["ReleasedThisMonth"]);
                stats.CollectedThisMonth = Convert.ToDecimal(row["Collected"]);
                stats.UnpaidApproved = Convert.ToInt32(row["UnpaidApproved"]);
            }

            return stats;
        }

        private static void Transition(int requestId, string expectedStatus, string newStatus, string processedBy, string remarks, bool released)
        {
            var rows = Sql.Execute(
                "UPDATE dbo.Requests SET Status = @newStatus, ProcessedBy = @by, DateUpdated = SYSDATETIME(), " +
                "Remarks = CASE WHEN @remarks IS NULL THEN Remarks ELSE @remarks END, " +
                "DateReleased = CASE WHEN @released = 1 THEN SYSDATETIME() ELSE DateReleased END " +
                "WHERE Id = @id AND Status = @expected",
                Sql.P("@newStatus", newStatus),
                Sql.P("@by", processedBy),
                Sql.P("@remarks", string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim()),
                Sql.P("@released", released ? 1 : 0),
                Sql.P("@id", requestId),
                Sql.P("@expected", expectedStatus));

            if (rows == 0)
            {
                throw new InvalidOperationException("This request was already updated. Refresh the list and try again.");
            }
        }

        private static string NextRequestNumber()
        {
            var prefix = "REQ-" + DateTime.Now.Year + "-";
            var max = Sql.Scalar(
                "SELECT MAX(RequestNo) FROM dbo.Requests WHERE RequestNo LIKE @prefix",
                Sql.P("@prefix", prefix + "%"));

            var next = 1;
            var value = max == null || max == DBNull.Value ? null : Convert.ToString(max);
            if (!string.IsNullOrEmpty(value))
            {
                var dash = value.LastIndexOf('-');
                if (dash >= 0 && dash < value.Length - 1)
                {
                    int parsed;
                    if (int.TryParse(value.Substring(dash + 1), out parsed))
                    {
                        next = parsed + 1;
                    }
                }
            }

            return prefix + next.ToString("D4");
        }
    }
}
