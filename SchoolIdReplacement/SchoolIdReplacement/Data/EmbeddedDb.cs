using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using SchoolIdReplacement.Models;

namespace SchoolIdReplacement.Data
{
    /// <summary>
    /// Zero-dependency, persistent XML-backed DataSet engine used automatically when SQL Server / LocalDB
    /// is not installed on the target PC or laptop. Supports every query, filter, report, and workflow in the app.
    /// </summary>
    internal static class EmbeddedDb
    {
        private static readonly object SyncLock = new object();
        private static DataSet _ds;
        private static string _filePath;

        public static void Initialize(string filePath)
        {
            lock (SyncLock)
            {
                _filePath = filePath;
                _ds = BuildSchema();

                if (File.Exists(_filePath))
                {
                    try
                    {
                        _ds.ReadXml(_filePath, XmlReadMode.IgnoreSchema);
                    }
                    catch
                    {
                        _ds = BuildSchema();
                    }
                }
                else
                {
                    Save();
                }
            }
        }

        private static DataSet BuildSchema()
        {
            var ds = new DataSet("SchoolIdReplacementDb");

            var users = new DataTable("Users");
            var uId = users.Columns.Add("Id", typeof(int));
            uId.AutoIncrement = true;
            uId.AutoIncrementSeed = 1;
            uId.AutoIncrementStep = 1;
            users.PrimaryKey = new[] { uId };
            users.Columns.Add("Username", typeof(string));
            users.Columns.Add("PasswordHash", typeof(string));
            users.Columns.Add("PasswordSalt", typeof(string));
            users.Columns.Add("Role", typeof(string));
            users.Columns.Add("FullName", typeof(string));
            users.Columns.Add("IsActive", typeof(bool));
            users.Columns.Add("LastLoginAt", typeof(DateTime));
            users.Columns.Add("CreatedAt", typeof(DateTime));
            ds.Tables.Add(users);

            var students = new DataTable("Students");
            var sId = students.Columns.Add("Id", typeof(int));
            sId.AutoIncrement = true;
            sId.AutoIncrementSeed = 1;
            sId.AutoIncrementStep = 1;
            students.PrimaryKey = new[] { sId };
            students.Columns.Add("UserId", typeof(int));
            students.Columns.Add("StudentNo", typeof(string));
            students.Columns.Add("FirstName", typeof(string));
            students.Columns.Add("MiddleName", typeof(string));
            students.Columns.Add("LastName", typeof(string));
            students.Columns.Add("Course", typeof(string));
            students.Columns.Add("YearLevel", typeof(string));
            students.Columns.Add("Section", typeof(string));
            students.Columns.Add("Email", typeof(string));
            students.Columns.Add("ContactNo", typeof(string));
            students.Columns.Add("Address", typeof(string));
            students.Columns.Add("PhotoPath", typeof(string));
            students.Columns.Add("IsActive", typeof(bool));
            students.Columns.Add("CreatedAt", typeof(DateTime));
            ds.Tables.Add(students);

            var requests = new DataTable("Requests");
            var rId = requests.Columns.Add("Id", typeof(int));
            rId.AutoIncrement = true;
            rId.AutoIncrementSeed = 1;
            rId.AutoIncrementStep = 1;
            requests.PrimaryKey = new[] { rId };
            requests.Columns.Add("RequestNo", typeof(string));
            requests.Columns.Add("StudentId", typeof(int));
            requests.Columns.Add("RequestType", typeof(string));
            requests.Columns.Add("Reason", typeof(string));
            requests.Columns.Add("Status", typeof(string));
            requests.Columns.Add("FeeAmount", typeof(decimal));
            requests.Columns.Add("PaymentStatus", typeof(string));
            requests.Columns.Add("OrNumber", typeof(string));
            requests.Columns.Add("Remarks", typeof(string));
            requests.Columns.Add("DateRequested", typeof(DateTime));
            requests.Columns.Add("DateUpdated", typeof(DateTime));
            requests.Columns.Add("DateReleased", typeof(DateTime));
            requests.Columns.Add("ProcessedBy", typeof(string));
            ds.Tables.Add(requests);

            var logs = new DataTable("AuditLogs");
            var lId = logs.Columns.Add("Id", typeof(int));
            lId.AutoIncrement = true;
            lId.AutoIncrementSeed = 1;
            lId.AutoIncrementStep = 1;
            logs.PrimaryKey = new[] { lId };
            logs.Columns.Add("UserId", typeof(int));
            logs.Columns.Add("Username", typeof(string));
            logs.Columns.Add("Action", typeof(string));
            logs.Columns.Add("Details", typeof(string));
            logs.Columns.Add("CreatedAt", typeof(DateTime));
            ds.Tables.Add(logs);

            return ds;
        }

        private static void Save()
        {
            if (string.IsNullOrEmpty(_filePath)) return;
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            _ds.WriteXml(_filePath, XmlWriteMode.WriteSchema);
        }

        private static Dictionary<string, object> ParamMap(SqlParameter[] parameters)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p != null)
                    {
                        dict[p.ParameterName] = p.Value == DBNull.Value ? null : p.Value;
                    }
                }
            }
            return dict;
        }

        private static string Str(Dictionary<string, object> p, string key)
        {
            object v;
            return p.TryGetValue(key, out v) && v != null ? Convert.ToString(v) : null;
        }

        private static int Int(Dictionary<string, object> p, string key)
        {
            object v;
            return p.TryGetValue(key, out v) && v != null ? Convert.ToInt32(v) : 0;
        }

        private static decimal Dec(Dictionary<string, object> p, string key)
        {
            object v;
            return p.TryGetValue(key, out v) && v != null ? Convert.ToDecimal(v) : 0m;
        }

        private static DateTime Dt(Dictionary<string, object> p, string key)
        {
            object v;
            return p.TryGetValue(key, out v) && v != null ? Convert.ToDateTime(v) : DateTime.Now;
        }

        private static string LikePattern(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            return raw.Trim('%').ToLowerInvariant();
        }

        public static object Scalar(string sql, SqlParameter[] parameters)
        {
            lock (SyncLock)
            {
                var p = ParamMap(parameters);
                var q = sql.Trim();

                if (q.StartsWith("SELECT COUNT(*) FROM dbo.Users WHERE Username", StringComparison.OrdinalIgnoreCase))
                {
                    var u = Str(p, "@username") ?? string.Empty;
                    return _ds.Tables["Users"].AsEnumerable()
                        .Count(r => string.Equals(r.Field<string>("Username"), u, StringComparison.OrdinalIgnoreCase));
                }

                if (q.StartsWith("SELECT COUNT(*) FROM dbo.Users", StringComparison.OrdinalIgnoreCase))
                {
                    return _ds.Tables["Users"].Rows.Count;
                }

                if (q.StartsWith("SELECT Username FROM dbo.Users WHERE Id", StringComparison.OrdinalIgnoreCase))
                {
                    var id = Int(p, "@id");
                    var row = _ds.Tables["Users"].Rows.Find(id);
                    return row == null ? string.Empty : row["Username"];
                }

                if (q.StartsWith("SELECT COUNT(*) FROM dbo.Students WHERE StudentNo = @studentNo AND Id <> @id", StringComparison.OrdinalIgnoreCase))
                {
                    var sno = Str(p, "@studentNo") ?? string.Empty;
                    var id = Int(p, "@id");
                    return _ds.Tables["Students"].AsEnumerable()
                        .Count(r => r.Field<int>("Id") != id && string.Equals(r.Field<string>("StudentNo"), sno, StringComparison.OrdinalIgnoreCase));
                }

                if (q.StartsWith("SELECT COUNT(*) FROM dbo.Students WHERE StudentNo = @studentNo", StringComparison.OrdinalIgnoreCase))
                {
                    var sno = Str(p, "@studentNo") ?? string.Empty;
                    return _ds.Tables["Students"].AsEnumerable()
                        .Count(r => string.Equals(r.Field<string>("StudentNo"), sno, StringComparison.OrdinalIgnoreCase));
                }

                if (q.StartsWith("SELECT COUNT(*) FROM dbo.Requests WHERE StudentId = @studentId", StringComparison.OrdinalIgnoreCase))
                {
                    var sid = Int(p, "@studentId");
                    var openStatuses = new[] { RequestStatus.Pending, RequestStatus.Approved, RequestStatus.ReadyForPickup };
                    return _ds.Tables["Requests"].AsEnumerable()
                        .Count(r => r.Field<int>("StudentId") == sid && openStatuses.Contains(r.Field<string>("Status")));
                }

                if (q.StartsWith("SELECT MAX(RequestNo) FROM dbo.Requests", StringComparison.OrdinalIgnoreCase))
                {
                    var prefix = LikePattern(Str(p, "@prefix"));
                    return _ds.Tables["Requests"].AsEnumerable()
                        .Select(r => r.Field<string>("RequestNo"))
                        .Where(n => n != null && n.ToLowerInvariant().StartsWith(prefix))
                        .OrderByDescending(n => n)
                        .FirstOrDefault();
                }

                if (q.StartsWith("INSERT INTO dbo.Users", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Users"].NewRow();
                    row["Username"] = Str(p, "@username");
                    row["PasswordHash"] = Str(p, "@hash");
                    row["PasswordSalt"] = Str(p, "@salt");
                    row["Role"] = Str(p, "@role");
                    row["FullName"] = Str(p, "@fullName");
                    row["IsActive"] = true;
                    row["CreatedAt"] = DateTime.Now;
                    _ds.Tables["Users"].Rows.Add(row);
                    Save();
                    return Convert.ToInt32(row["Id"]);
                }

                if (q.StartsWith("INSERT INTO dbo.Students", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Students"].NewRow();
                    row["UserId"] = p.ContainsKey("@userId") && p["@userId"] != null ? (object)Int(p, "@userId") : DBNull.Value;
                    row["StudentNo"] = Str(p, "@studentNo");
                    row["FirstName"] = Str(p, "@firstName");
                    row["MiddleName"] = (object)Str(p, "@middleName") ?? DBNull.Value;
                    row["LastName"] = Str(p, "@lastName");
                    row["Course"] = (object)Str(p, "@course") ?? DBNull.Value;
                    row["YearLevel"] = (object)Str(p, "@yearLevel") ?? DBNull.Value;
                    row["Section"] = (object)Str(p, "@section") ?? DBNull.Value;
                    row["Email"] = (object)Str(p, "@email") ?? DBNull.Value;
                    row["ContactNo"] = (object)Str(p, "@contactNo") ?? DBNull.Value;
                    row["Address"] = (object)Str(p, "@address") ?? DBNull.Value;
                    row["PhotoPath"] = (object)Str(p, "@photoPath") ?? DBNull.Value;
                    row["IsActive"] = true;
                    row["CreatedAt"] = DateTime.Now;
                    _ds.Tables["Students"].Rows.Add(row);
                    Save();
                    return Convert.ToInt32(row["Id"]);
                }

                if (q.StartsWith("INSERT INTO dbo.Requests", StringComparison.OrdinalIgnoreCase))
                {
                    var reqNo = Str(p, "@requestNo");
                    var dup = _ds.Tables["Requests"].AsEnumerable()
                        .Any(r => string.Equals(r.Field<string>("RequestNo"), reqNo, StringComparison.OrdinalIgnoreCase));
                    if (dup)
                    {
                        throw new InvalidOperationException("Duplicate RequestNo");
                    }

                    var row = _ds.Tables["Requests"].NewRow();
                    row["RequestNo"] = reqNo;
                    row["StudentId"] = Int(p, "@studentId");
                    row["RequestType"] = Str(p, "@requestType");
                    row["Reason"] = (object)Str(p, "@reason") ?? DBNull.Value;
                    row["Status"] = Str(p, "@status");
                    row["FeeAmount"] = Dec(p, "@fee");
                    row["PaymentStatus"] = Str(p, "@payment");
                    row["DateRequested"] = DateTime.Now;
                    _ds.Tables["Requests"].Rows.Add(row);
                    Save();
                    return Convert.ToInt32(row["Id"]);
                }

                throw new NotSupportedException("Unsupported scalar query in EmbeddedDb: " + q);
            }
        }

        public static int Execute(string sql, SqlParameter[] parameters)
        {
            lock (SyncLock)
            {
                var p = ParamMap(parameters);
                var q = sql.Trim();

                if (q.StartsWith("INSERT INTO dbo.AuditLogs", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["AuditLogs"].NewRow();
                    row["UserId"] = p.ContainsKey("@userId") && p["@userId"] != null ? (object)Int(p, "@userId") : DBNull.Value;
                    row["Username"] = (object)Str(p, "@username") ?? DBNull.Value;
                    row["Action"] = Str(p, "@action");
                    row["Details"] = (object)Str(p, "@details") ?? DBNull.Value;
                    row["CreatedAt"] = DateTime.Now;
                    _ds.Tables["AuditLogs"].Rows.Add(row);
                    Save();
                    return 1;
                }

                if (q.StartsWith("INSERT INTO dbo.Requests", StringComparison.OrdinalIgnoreCase))
                {
                    var days = Int(p, "@days");
                    var updatedDays = Int(p, "@updatedDays");
                    var releasedDays = Int(p, "@releasedDays");
                    var status = Str(p, "@status");
                    var releasedStatus = Str(p, "@released");

                    var row = _ds.Tables["Requests"].NewRow();
                    row["RequestNo"] = Str(p, "@requestNo");
                    row["StudentId"] = Int(p, "@studentId");
                    row["RequestType"] = Str(p, "@requestType");
                    row["Reason"] = (object)Str(p, "@reason") ?? DBNull.Value;
                    row["Status"] = status;
                    row["FeeAmount"] = Dec(p, "@fee");
                    row["PaymentStatus"] = Str(p, "@paymentStatus");
                    row["OrNumber"] = (object)Str(p, "@orNumber") ?? DBNull.Value;
                    row["Remarks"] = (object)Str(p, "@remarks") ?? DBNull.Value;
                    row["DateRequested"] = DateTime.Now.AddDays(-days);
                    row["DateUpdated"] = DateTime.Now.AddDays(-updatedDays);
                    row["DateReleased"] = status == releasedStatus ? (object)DateTime.Now.AddDays(-releasedDays) : DBNull.Value;
                    _ds.Tables["Requests"].Rows.Add(row);
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Users SET LastLoginAt", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Users"].Rows.Find(Int(p, "@id"));
                    if (row == null) return 0;
                    row["LastLoginAt"] = DateTime.Now;
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Users SET PasswordHash", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Users"].Rows.Find(Int(p, "@id"));
                    if (row == null) return 0;
                    row["PasswordHash"] = Str(p, "@hash");
                    row["PasswordSalt"] = Str(p, "@salt");
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Users SET FullName", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Users"].Rows.Find(Int(p, "@id"));
                    if (row == null) return 0;
                    row["FullName"] = Str(p, "@fullName");
                    row["Username"] = Str(p, "@username");
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Users SET IsActive", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Users"].Rows.Find(Int(p, "@id"));
                    if (row == null) return 0;
                    row["IsActive"] = Int(p, "@active") == 1;
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Students SET IsActive", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Students"].Rows.Find(Int(p, "@id"));
                    if (row == null) return 0;
                    row["IsActive"] = Int(p, "@active") == 1;
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Students SET StudentNo", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Students"].Rows.Find(Int(p, "@id"));
                    if (row == null) return 0;
                    row["StudentNo"] = Str(p, "@studentNo");
                    row["FirstName"] = Str(p, "@firstName");
                    row["MiddleName"] = (object)Str(p, "@middleName") ?? DBNull.Value;
                    row["LastName"] = Str(p, "@lastName");
                    row["Course"] = (object)Str(p, "@course") ?? DBNull.Value;
                    row["YearLevel"] = (object)Str(p, "@yearLevel") ?? DBNull.Value;
                    row["Section"] = (object)Str(p, "@section") ?? DBNull.Value;
                    row["Email"] = (object)Str(p, "@email") ?? DBNull.Value;
                    row["ContactNo"] = (object)Str(p, "@contactNo") ?? DBNull.Value;
                    row["Address"] = (object)Str(p, "@address") ?? DBNull.Value;
                    row["PhotoPath"] = (object)Str(p, "@photoPath") ?? DBNull.Value;
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Requests SET PaymentStatus", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Requests"].Rows.Find(Int(p, "@id"));
                    if (row == null || !string.Equals(Convert.ToString(row["Status"]), Str(p, "@approved"), StringComparison.OrdinalIgnoreCase))
                    {
                        return 0;
                    }
                    row["PaymentStatus"] = Str(p, "@paid");
                    row["OrNumber"] = Str(p, "@orNumber");
                    row["ProcessedBy"] = Str(p, "@by");
                    row["DateUpdated"] = DateTime.Now;
                    Save();
                    return 1;
                }

                if (q.StartsWith("UPDATE dbo.Requests SET Status", StringComparison.OrdinalIgnoreCase))
                {
                    var row = _ds.Tables["Requests"].Rows.Find(Int(p, "@id"));
                    if (row == null || !string.Equals(Convert.ToString(row["Status"]), Str(p, "@expected"), StringComparison.OrdinalIgnoreCase))
                    {
                        return 0;
                    }
                    row["Status"] = Str(p, "@newStatus");
                    row["ProcessedBy"] = Str(p, "@by");
                    row["DateUpdated"] = DateTime.Now;
                    var remarks = Str(p, "@remarks");
                    if (remarks != null)
                    {
                        row["Remarks"] = remarks;
                    }
                    if (Int(p, "@released") == 1)
                    {
                        row["DateReleased"] = DateTime.Now;
                    }
                    Save();
                    return 1;
                }

                return 0;
            }
        }

        public static DataTable Query(string sql, SqlParameter[] parameters)
        {
            lock (SyncLock)
            {
                var p = ParamMap(parameters);
                var q = sql.Trim();

                // 1. AuthService.Login
                if (q.StartsWith("SELECT Id, Username, PasswordHash, PasswordSalt, Role, FullName, IsActive FROM dbo.Users", StringComparison.OrdinalIgnoreCase))
                {
                    var username = Str(p, "@username") ?? string.Empty;
                    var result = _ds.Tables["Users"].Clone();
                    foreach (var r in _ds.Tables["Users"].AsEnumerable()
                        .Where(r => string.Equals(r.Field<string>("Username"), username, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.ImportRow(r);
                    }
                    return result;
                }

                // 2. AuthService.ChangePassword
                if (q.StartsWith("SELECT PasswordHash, PasswordSalt FROM dbo.Users WHERE Id", StringComparison.OrdinalIgnoreCase))
                {
                    var id = Int(p, "@id");
                    var result = new DataTable();
                    result.Columns.Add("PasswordHash", typeof(string));
                    result.Columns.Add("PasswordSalt", typeof(string));
                    var r = _ds.Tables["Users"].Rows.Find(id);
                    if (r != null)
                    {
                        result.Rows.Add(r["PasswordHash"], r["PasswordSalt"]);
                    }
                    return result;
                }

                // 3. AuthService student lookup
                if (q.StartsWith("SELECT Id, StudentNo FROM dbo.Students WHERE UserId", StringComparison.OrdinalIgnoreCase))
                {
                    var uid = Int(p, "@userId");
                    var result = new DataTable();
                    result.Columns.Add("Id", typeof(int));
                    result.Columns.Add("StudentNo", typeof(string));
                    var r = _ds.Tables["Students"].AsEnumerable()
                        .FirstOrDefault(row => !row.IsNull("UserId") && row.Field<int>("UserId") == uid);
                    if (r != null)
                    {
                        result.Rows.Add(r["Id"], r["StudentNo"]);
                    }
                    return result;
                }

                // 4. StudentService.Get / GetByUserId
                if (q.StartsWith("SELECT Id, UserId, StudentNo, FirstName, MiddleName, LastName", StringComparison.OrdinalIgnoreCase))
                {
                    var result = _ds.Tables["Students"].Clone();
                    IEnumerable<DataRow> rows = _ds.Tables["Students"].AsEnumerable();
                    if (p.ContainsKey("@id"))
                    {
                        var id = Int(p, "@id");
                        rows = rows.Where(r => r.Field<int>("Id") == id);
                    }
                    else if (p.ContainsKey("@userId"))
                    {
                        var uid = Int(p, "@userId");
                        rows = rows.Where(r => !r.IsNull("UserId") && r.Field<int>("UserId") == uid);
                    }
                    foreach (var r in rows) result.ImportRow(r);
                    return result;
                }

                // 5. StudentService.List
                if (q.StartsWith("SELECT s.Id, s.StudentNo AS [Student No]", StringComparison.OrdinalIgnoreCase))
                {
                    var term = p.ContainsKey("@like") ? LikePattern(Str(p, "@like")) : string.Empty;
                    var result = new DataTable();
                    result.Columns.Add("Id", typeof(int));
                    result.Columns.Add("Student No", typeof(string));
                    result.Columns.Add("Name", typeof(string));
                    result.Columns.Add("Course", typeof(string));
                    result.Columns.Add("Year", typeof(string));
                    result.Columns.Add("Section", typeof(string));
                    result.Columns.Add("Email", typeof(string));
                    result.Columns.Add("Contact", typeof(string));
                    result.Columns.Add("Username", typeof(string));
                    result.Columns.Add("Active", typeof(bool));

                    var query = _ds.Tables["Students"].AsEnumerable()
                        .Where(s =>
                            string.IsNullOrEmpty(term) ||
                            (s.Field<string>("StudentNo") ?? "").ToLowerInvariant().Contains(term) ||
                            (s.Field<string>("FirstName") ?? "").ToLowerInvariant().Contains(term) ||
                            (s.Field<string>("LastName") ?? "").ToLowerInvariant().Contains(term) ||
                            (s.Field<string>("Course") ?? "").ToLowerInvariant().Contains(term))
                        .OrderBy(s => s.Field<string>("StudentNo"));

                    foreach (var s in query)
                    {
                        var user = s.IsNull("UserId") ? null : _ds.Tables["Users"].Rows.Find(s.Field<int>("UserId"));
                        result.Rows.Add(
                            s["Id"],
                            s["StudentNo"],
                            s.Field<string>("LastName") + ", " + s.Field<string>("FirstName"),
                            s["Course"],
                            s["YearLevel"],
                            s["Section"],
                            s["Email"],
                            s["ContactNo"],
                            user == null ? (object)DBNull.Value : user["Username"],
                            s["IsActive"]);
                    }
                    return result;
                }

                // 6. RequestService.ListAll
                if (q.StartsWith("SELECT r.Id, r.RequestNo AS [Request No], s.StudentNo AS [Student No]", StringComparison.OrdinalIgnoreCase))
                {
                    var statusFilter = Str(p, "@status");
                    var term = p.ContainsKey("@search") ? LikePattern(Str(p, "@search")) : string.Empty;

                    var result = new DataTable();
                    result.Columns.Add("Id", typeof(int));
                    result.Columns.Add("Request No", typeof(string));
                    result.Columns.Add("Student No", typeof(string));
                    result.Columns.Add("Student", typeof(string));
                    result.Columns.Add("Type", typeof(string));
                    result.Columns.Add("Status", typeof(string));
                    result.Columns.Add("Payment", typeof(string));
                    result.Columns.Add("Fee", typeof(decimal));
                    result.Columns.Add("Date Filed", typeof(string));

                    var joined = from r in _ds.Tables["Requests"].AsEnumerable()
                                 join s in _ds.Tables["Students"].AsEnumerable()
                                 on r.Field<int>("StudentId") equals s.Field<int>("Id")
                                 where (string.IsNullOrEmpty(statusFilter) || r.Field<string>("Status") == statusFilter)
                                 where (string.IsNullOrEmpty(term) ||
                                        (r.Field<string>("RequestNo") ?? "").ToLowerInvariant().Contains(term) ||
                                        (s.Field<string>("StudentNo") ?? "").ToLowerInvariant().Contains(term) ||
                                        (s.Field<string>("FirstName") ?? "").ToLowerInvariant().Contains(term) ||
                                        (s.Field<string>("LastName") ?? "").ToLowerInvariant().Contains(term) ||
                                        (r.Field<string>("RequestType") ?? "").ToLowerInvariant().Contains(term))
                                 orderby StatusRank(r.Field<string>("Status")), r.Field<DateTime>("DateRequested") descending
                                 select new { r, s };

                    foreach (var item in joined)
                    {
                        result.Rows.Add(
                            item.r["Id"],
                            item.r["RequestNo"],
                            item.s["StudentNo"],
                            item.s.Field<string>("LastName") + ", " + item.s.Field<string>("FirstName"),
                            item.r["RequestType"],
                            item.r["Status"],
                            item.r["PaymentStatus"],
                            item.r["FeeAmount"],
                            item.r.Field<DateTime>("DateRequested").ToString("MMM dd, yyyy", CultureInfo.InvariantCulture));
                    }
                    return result;
                }

                // 7. RequestService.ListForStudent
                if (q.StartsWith("SELECT r.Id, r.RequestNo AS [Request No], r.RequestType AS [Type], r.Status,        r.PaymentStatus", StringComparison.OrdinalIgnoreCase) ||
                    q.Contains("CONVERT(NVARCHAR(20), r.DateReleased, 107) AS [Date Released]"))
                {
                    var sid = Int(p, "@studentId");
                    var result = new DataTable();
                    result.Columns.Add("Id", typeof(int));
                    result.Columns.Add("Request No", typeof(string));
                    result.Columns.Add("Type", typeof(string));
                    result.Columns.Add("Status", typeof(string));
                    result.Columns.Add("Payment", typeof(string));
                    result.Columns.Add("Fee", typeof(decimal));
                    result.Columns.Add("Date Filed", typeof(string));
                    result.Columns.Add("Date Released", typeof(string));

                    var rows = _ds.Tables["Requests"].AsEnumerable()
                        .Where(r => r.Field<int>("StudentId") == sid)
                        .OrderByDescending(r => r.Field<DateTime>("DateRequested"));

                    foreach (var r in rows)
                    {
                        result.Rows.Add(
                            r["Id"],
                            r["RequestNo"],
                            r["RequestType"],
                            r["Status"],
                            r["PaymentStatus"],
                            r["FeeAmount"],
                            r.Field<DateTime>("DateRequested").ToString("MMM dd, yyyy", CultureInfo.InvariantCulture),
                            r.IsNull("DateReleased") ? (object)DBNull.Value : r.Field<DateTime>("DateReleased").ToString("MMM dd, yyyy", CultureInfo.InvariantCulture));
                    }
                    return result;
                }

                // 8. RequestService.RecentForStudent
                if (q.StartsWith("SELECT TOP (@limit) r.Id, r.RequestNo AS [Request No]", StringComparison.OrdinalIgnoreCase))
                {
                    var sid = Int(p, "@studentId");
                    var limit = Int(p, "@limit");
                    var result = new DataTable();
                    result.Columns.Add("Id", typeof(int));
                    result.Columns.Add("Request No", typeof(string));
                    result.Columns.Add("Type", typeof(string));
                    result.Columns.Add("Status", typeof(string));
                    result.Columns.Add("Payment", typeof(string));
                    result.Columns.Add("Date Filed", typeof(string));

                    var rows = _ds.Tables["Requests"].AsEnumerable()
                        .Where(r => r.Field<int>("StudentId") == sid)
                        .OrderByDescending(r => r.Field<DateTime>("DateRequested"))
                        .Take(limit);

                    foreach (var r in rows)
                    {
                        result.Rows.Add(
                            r["Id"],
                            r["RequestNo"],
                            r["RequestType"],
                            r["Status"],
                            r["PaymentStatus"],
                            r.Field<DateTime>("DateRequested").ToString("MMM dd, yyyy", CultureInfo.InvariantCulture));
                    }
                    return result;
                }

                // 9. RequestService.Get
                if (q.StartsWith("SELECT r.*, s.StudentNo, s.FirstName, s.MiddleName, s.LastName", StringComparison.OrdinalIgnoreCase))
                {
                    var id = Int(p, "@id");
                    var result = _ds.Tables["Requests"].Clone();
                    result.Columns.Add("StudentNo", typeof(string));
                    result.Columns.Add("FirstName", typeof(string));
                    result.Columns.Add("MiddleName", typeof(string));
                    result.Columns.Add("LastName", typeof(string));

                    var r = _ds.Tables["Requests"].Rows.Find(id);
                    if (r != null)
                    {
                        var s = _ds.Tables["Students"].Rows.Find(r.Field<int>("StudentId"));
                        var row = result.NewRow();
                        foreach (DataColumn col in _ds.Tables["Requests"].Columns)
                        {
                            row[col.ColumnName] = r[col.ColumnName];
                        }
                        if (s != null)
                        {
                            row["StudentNo"] = s["StudentNo"];
                            row["FirstName"] = s["FirstName"];
                            row["MiddleName"] = s["MiddleName"];
                            row["LastName"] = s["LastName"];
                        }
                        result.Rows.Add(row);
                    }
                    return result;
                }

                // 10. RequestService.GetStatistics
                if (q.Contains("AS TotalStudents") && q.Contains("AS UnpaidApproved"))
                {
                    var now = DateTime.Now;
                    var monthStart = new DateTime(now.Year, now.Month, 1);
                    var students = _ds.Tables["Students"].AsEnumerable().ToList();
                    var requests = _ds.Tables["Requests"].AsEnumerable().ToList();

                    var result = new DataTable();
                    result.Columns.Add("TotalStudents", typeof(int));
                    result.Columns.Add("ActiveStudents", typeof(int));
                    result.Columns.Add("TotalRequests", typeof(int));
                    result.Columns.Add("Pending", typeof(int));
                    result.Columns.Add("Approved", typeof(int));
                    result.Columns.Add("Ready", typeof(int));
                    result.Columns.Add("ReleasedThisMonth", typeof(int));
                    result.Columns.Add("Collected", typeof(decimal));
                    result.Columns.Add("UnpaidApproved", typeof(int));

                    result.Rows.Add(
                        students.Count,
                        students.Count(s => s.Field<bool>("IsActive")),
                        requests.Count,
                        requests.Count(r => r.Field<string>("Status") == RequestStatus.Pending),
                        requests.Count(r => r.Field<string>("Status") == RequestStatus.Approved),
                        requests.Count(r => r.Field<string>("Status") == RequestStatus.ReadyForPickup),
                        requests.Count(r => r.Field<string>("Status") == RequestStatus.Released && !r.IsNull("DateReleased") && r.Field<DateTime>("DateReleased") >= monthStart),
                        requests.Where(r => r.Field<string>("PaymentStatus") == PaymentStatus.Paid && !r.IsNull("DateUpdated") && r.Field<DateTime>("DateUpdated") >= monthStart).Sum(r => r.Field<decimal>("FeeAmount")),
                        requests.Count(r => r.Field<string>("Status") == RequestStatus.Approved && r.Field<string>("PaymentStatus") == PaymentStatus.Unpaid));

                    return result;
                }

                // 11. ReportService.SummaryByStatus
                if (q.StartsWith("SELECT r.Status AS [Status], COUNT(*) AS [Requests]", StringComparison.OrdinalIgnoreCase))
                {
                    var from = Dt(p, "@from");
                    var to = Dt(p, "@to");
                    var result = new DataTable();
                    result.Columns.Add("Status", typeof(string));
                    result.Columns.Add("Requests", typeof(int));
                    result.Columns.Add("Total Fees", typeof(decimal));

                    var groups = _ds.Tables["Requests"].AsEnumerable()
                        .Where(r => r.Field<DateTime>("DateRequested") >= from && r.Field<DateTime>("DateRequested") < to)
                        .GroupBy(r => r.Field<string>("Status"))
                        .OrderByDescending(g => g.Count());

                    foreach (var g in groups)
                    {
                        result.Rows.Add(g.Key, g.Count(), g.Sum(r => r.Field<decimal>("FeeAmount")));
                    }
                    return result;
                }

                // 12. ReportService.SummaryByType
                if (q.StartsWith("SELECT r.RequestType AS [Replacement Type]", StringComparison.OrdinalIgnoreCase))
                {
                    var from = Dt(p, "@from");
                    var to = Dt(p, "@to");
                    var result = new DataTable();
                    result.Columns.Add("Replacement Type", typeof(string));
                    result.Columns.Add("Requests", typeof(int));
                    result.Columns.Add("Paid", typeof(int));
                    result.Columns.Add("Assessed Fees", typeof(decimal));

                    var groups = _ds.Tables["Requests"].AsEnumerable()
                        .Where(r => r.Field<DateTime>("DateRequested") >= from && r.Field<DateTime>("DateRequested") < to)
                        .GroupBy(r => r.Field<string>("RequestType"))
                        .OrderByDescending(g => g.Count());

                    foreach (var g in groups)
                    {
                        result.Rows.Add(
                            g.Key,
                            g.Count(),
                            g.Count(r => r.Field<string>("PaymentStatus") == PaymentStatus.Paid),
                            g.Sum(r => r.Field<decimal>("FeeAmount")));
                    }
                    return result;
                }

                // 13. ReportService.RecentActivity
                if (q.StartsWith("SELECT TOP (@limit) CONVERT(NVARCHAR(20), a.CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    var limit = Int(p, "@limit");
                    var result = new DataTable();
                    result.Columns.Add("When", typeof(string));
                    result.Columns.Add("User", typeof(string));
                    result.Columns.Add("Action", typeof(string));
                    result.Columns.Add("Details", typeof(string));

                    var rows = _ds.Tables["AuditLogs"].AsEnumerable()
                        .OrderByDescending(a => a.Field<DateTime>("CreatedAt"))
                        .Take(limit);

                    foreach (var a in rows)
                    {
                        result.Rows.Add(
                            a.Field<DateTime>("CreatedAt").ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            a["Username"],
                            a["Action"],
                            a["Details"]);
                    }
                    return result;
                }

                return new DataTable();
            }
        }

        private static int StatusRank(string status)
        {
            switch (status)
            {
                case RequestStatus.Pending: return 0;
                case RequestStatus.Approved: return 1;
                case RequestStatus.ReadyForPickup: return 2;
                default: return 3;
            }
        }
    }
}
