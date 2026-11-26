using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Security;

namespace SchoolIdReplacement.Data
{
    /// <summary>
    /// Automatically detects an available SQL Server / LocalDB instance on the host PC,
    /// or falls back to a zero-configuration local file database so the system runs on ANY Windows PC or laptop.
    /// </summary>
    public static class Database
    {
        private const string DefaultDatabaseName = "SchoolIdReplacementDb";

        private static string _connectionString;
        private static bool _useEmbeddedStore;
        private static string _serverDisplay = "Local Portable Store";
        private static string _databaseDisplay = DefaultDatabaseName;

        public static string ConnectionString
        {
            get { return _connectionString; }
        }

        public static bool UseEmbeddedStore
        {
            get { return _useEmbeddedStore; }
        }

        public static string DatabaseName
        {
            get { return _databaseDisplay; }
        }

        public static string ServerName
        {
            get { return _serverDisplay; }
        }

        public static string EmbeddedFilePath
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SchoolIdReplacement");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, DefaultDatabaseName + ".xml");
            }
        }

        public static void Initialize()
        {
            var configured = ConfigurationManager.ConnectionStrings["SchoolIdDb"];
            var raw = configured == null ? "Auto" : (configured.ConnectionString ?? "Auto").Trim();

            if (string.Equals(raw, "LocalFile", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(raw, "Portable", StringComparison.OrdinalIgnoreCase))
            {
                InitializeEmbeddedStore();
                return;
            }

            if (string.Equals(raw, "Auto", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(raw))
            {
                var candidates = new[]
                {
                    "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=" + DefaultDatabaseName + ";Integrated Security=True;Connect Timeout=4;MultipleActiveResultSets=True",
                    "Data Source=(LocalDB)\\v11.0;Initial Catalog=" + DefaultDatabaseName + ";Integrated Security=True;Connect Timeout=4;MultipleActiveResultSets=True",
                    "Data Source=.\\SQLEXPRESS;Initial Catalog=" + DefaultDatabaseName + ";Integrated Security=True;Connect Timeout=4;MultipleActiveResultSets=True",
                    "Data Source=localhost;Initial Catalog=" + DefaultDatabaseName + ";Integrated Security=True;Connect Timeout=4;MultipleActiveResultSets=True"
                };

                foreach (var candidate in candidates)
                {
                    if (TryConnectSqlServer(candidate))
                    {
                        return;
                    }
                }

                // Fallback: No SQL Server / LocalDB installed on this laptop/PC -> use embedded local database
                InitializeEmbeddedStore();
                return;
            }

            // Explicit SQL Server connection string specified by user
            if (!TryConnectSqlServer(raw))
            {
                throw new InvalidOperationException(
                    "Could not connect to the configured SQL Server in App.config. " +
                    "Set connectionString=\"Auto\" in App.config to automatically detect SQL Server or use the built-in portable database.");
            }
        }

        private static bool TryConnectSqlServer(string connStr)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connStr);
                if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                {
                    builder.InitialCatalog = DefaultDatabaseName;
                }

                var databaseName = builder.InitialCatalog;
                var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString);
                masterBuilder.InitialCatalog = "master";
                if (masterBuilder.ConnectTimeout > 5)
                {
                    masterBuilder.ConnectTimeout = 5;
                }

                using (var connection = new SqlConnection(masterBuilder.ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText =
                            "IF DB_ID(N'" + databaseName.Replace("'", "''") + "') IS NULL " +
                            "CREATE DATABASE [" + databaseName.Replace("]", "]]") + "];";
                        command.CommandTimeout = 60;
                        command.ExecuteNonQuery();
                    }
                }

                _useEmbeddedStore = false;
                _connectionString = builder.ConnectionString;
                _serverDisplay = builder.DataSource;
                _databaseDisplay = databaseName;

                CreateSchema();
                SeedSampleData();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void InitializeEmbeddedStore()
        {
            _useEmbeddedStore = true;
            _connectionString = "Embedded://" + EmbeddedFilePath;
            _serverDisplay = "Built-in Portable Storage (No SQL Install Required)";
            _databaseDisplay = Path.GetFileName(EmbeddedFilePath);

            EmbeddedDb.Initialize(EmbeddedFilePath);
            SeedSampleData();
        }

        private static void CreateSchema()
        {
            foreach (var batch in SplitBatches(SchemaScript))
            {
                if (batch.Trim().Length == 0)
                {
                    continue;
                }

                Sql.Execute(batch);
            }
        }

        private static string[] SplitBatches(string script)
        {
            var batches = new List<string>();
            var current = new StringBuilder();

            foreach (var line in script.Replace("\r\n", "\n").Split('\n'))
            {
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    batches.Add(current.ToString());
                    current.Length = 0;
                }
                else
                {
                    current.AppendLine(line);
                }
            }

            batches.Add(current.ToString());
            return batches.ToArray();
        }

        private static void SeedSampleData()
        {
            if (Convert.ToInt32(Sql.Scalar("SELECT COUNT(*) FROM dbo.Users")) > 0)
            {
                return;
            }

            string hash;
            string salt;

            PasswordHasher.Create("admin123", out hash, out salt);
            InsertUser("admin", "Registrar Administrator", hash, salt, Roles.Administrator);

            PasswordHasher.Create("student123", out hash, out salt);

            var juanUserId = InsertUser("2024-0001", "Juan Dela Cruz", hash, salt, Roles.Student);
            var juanId = InsertStudent(juanUserId, "2024-0001", "Juan", "Santos", "Dela Cruz",
                "BS Information Technology", "3rd Year", "BSIT-3A",
                "juan.delacruz@school.edu.ph", "0917 555 0142", "12 Mabini Street, Quezon City");

            var mariaUserId = InsertUser("2024-0002", "Maria Clara Reyes", hash, salt, Roles.Student);
            var mariaId = InsertStudent(mariaUserId, "2024-0002", "Maria Clara", "Lopez", "Reyes",
                "BS Accountancy", "2nd Year", "BSA-2B",
                "maria.reyes@school.edu.ph", "0918 222 7781", "48 Rizal Avenue, Pasig City");

            var joseUserId = InsertUser("2024-0003", "Jose Miguel Ramos", hash, salt, Roles.Student);
            var joseId = InsertStudent(joseUserId, "2024-0003", "Jose Miguel", "Bautista", "Ramos",
                "BS Civil Engineering", "4th Year", "BSCE-4A",
                "jose.ramos@school.edu.ph", "0995 431 6602", "7 Katipunan Road, Marikina City");

            var year = DateTime.Now.Year;

            InsertRequest("REQ-" + year + "-0001", juanId, RequestTypes.Lost,
                "Lost my ID inside the campus gymnasium after the intramurals opening ceremony.",
                RequestStatus.Released, PaymentStatus.Paid, "OR-" + year + "-004512", 21,
                "Replacement ID released to the student.");

            InsertRequest("REQ-" + year + "-0002", mariaId, RequestTypes.Damaged,
                "The card was bent and the printed details are no longer readable.",
                RequestStatus.ReadyForPickup, PaymentStatus.Paid, "OR-" + year + "-004638", 9,
                "New card printed. For pickup at the registrar's office.");

            InsertRequest("REQ-" + year + "-0003", joseId, RequestTypes.NameCorrection,
                "My middle name was misspelled on the previous ID and needs correction.",
                RequestStatus.Approved, PaymentStatus.Unpaid, null, 4,
                "Approved. Please settle the replacement fee at the cashier.");

            InsertRequest("REQ-" + year + "-0004", juanId, RequestTypes.PhotoRetake,
                "Requesting a new photo because the old one is faded.",
                RequestStatus.Pending, PaymentStatus.Unpaid, null, 2, null);

            InsertRequest("REQ-" + year + "-0005", mariaId, RequestTypes.InformationUpdate,
                "My course code changed from BSA-2A to BSA-2B.",
                RequestStatus.Pending, PaymentStatus.Unpaid, null, 1, null);

            InsertRequest("REQ-" + year + "-0006", joseId, RequestTypes.Lost,
                "ID was lost together with my wallet.",
                RequestStatus.Rejected, PaymentStatus.Unpaid, null, 12,
                "Rejected: please submit an affidavit of loss and re-file.");
        }

        private static int InsertUser(string username, string fullName, string hash, string salt, string role)
        {
            return Convert.ToInt32(Sql.Scalar(
                "INSERT INTO dbo.Users (Username, PasswordHash, PasswordSalt, Role, FullName) " +
                "OUTPUT INSERTED.Id VALUES (@username, @hash, @salt, @role, @fullName)",
                Sql.P("@username", username),
                Sql.P("@hash", hash),
                Sql.P("@salt", salt),
                Sql.P("@role", role),
                Sql.P("@fullName", fullName)));
        }

        private static int InsertStudent(int userId, string studentNo, string firstName, string middleName, string lastName,
            string course, string yearLevel, string section, string email, string contactNo, string address)
        {
            return Convert.ToInt32(Sql.Scalar(
                "INSERT INTO dbo.Students (UserId, StudentNo, FirstName, MiddleName, LastName, Course, YearLevel, Section, Email, ContactNo, Address) " +
                "OUTPUT INSERTED.Id VALUES (@userId, @studentNo, @firstName, @middleName, @lastName, @course, @yearLevel, @section, @email, @contactNo, @address)",
                Sql.P("@userId", userId),
                Sql.P("@studentNo", studentNo),
                Sql.P("@firstName", firstName),
                Sql.P("@middleName", middleName),
                Sql.P("@lastName", lastName),
                Sql.P("@course", course),
                Sql.P("@yearLevel", yearLevel),
                Sql.P("@section", section),
                Sql.P("@email", email),
                Sql.P("@contactNo", contactNo),
                Sql.P("@address", address)));
        }

        private static void InsertRequest(string requestNo, int studentId, string requestType, string reason,
            string status, string paymentStatus, string orNumber, int daysAgo, string remarks)
        {
            Sql.Execute(
                "INSERT INTO dbo.Requests (RequestNo, StudentId, RequestType, Reason, Status, FeeAmount, PaymentStatus, OrNumber, Remarks, DateRequested, DateUpdated, DateReleased) " +
                "VALUES (@requestNo, @studentId, @requestType, @reason, @status, @fee, @paymentStatus, @orNumber, @remarks, " +
                "DATEADD(DAY, -@days, SYSDATETIME()), DATEADD(DAY, -@updatedDays, SYSDATETIME()), " +
                "CASE WHEN @status = @released THEN DATEADD(DAY, -@releasedDays, SYSDATETIME()) ELSE NULL END)",
                Sql.P("@requestNo", requestNo),
                Sql.P("@studentId", studentId),
                Sql.P("@requestType", requestType),
                Sql.P("@reason", reason),
                Sql.P("@status", status),
                Sql.P("@fee", RequestTypes.FeeFor(requestType)),
                Sql.P("@paymentStatus", paymentStatus),
                Sql.P("@orNumber", orNumber),
                Sql.P("@remarks", remarks),
                Sql.P("@days", daysAgo),
                Sql.P("@updatedDays", Math.Max(0, daysAgo / 3)),
                Sql.P("@released", RequestStatus.Released),
                Sql.P("@releasedDays", Math.Max(0, daysAgo / 4)));
        }

        private const string SchemaScript = @"
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username      NVARCHAR(50)  NOT NULL,
        PasswordHash  NVARCHAR(200) NOT NULL,
        PasswordSalt  NVARCHAR(100) NOT NULL,
        Role          NVARCHAR(30)  NOT NULL,
        FullName      NVARCHAR(120) NOT NULL,
        IsActive      BIT           NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        LastLoginAt   DATETIME2     NULL,
        CreatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME())
    );
END;
GO
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Users_Username' AND object_id = OBJECT_ID(N'dbo.Users'))
BEGIN
    CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users(Username);
END;
GO
IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Students PRIMARY KEY,
        UserId      INT           NULL,
        StudentNo   NVARCHAR(30)  NOT NULL,
        FirstName   NVARCHAR(60)  NOT NULL,
        MiddleName  NVARCHAR(60)  NULL,
        LastName    NVARCHAR(60)  NOT NULL,
        Course      NVARCHAR(80)  NULL,
        YearLevel   NVARCHAR(20)  NULL,
        Section     NVARCHAR(40)  NULL,
        Email       NVARCHAR(120) NULL,
        ContactNo   NVARCHAR(30)  NULL,
        Address     NVARCHAR(200) NULL,
        PhotoPath   NVARCHAR(400) NULL,
        IsActive    BIT           NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT (1),
        CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Students_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END;
GO
IF OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Students_StudentNo' AND object_id = OBJECT_ID(N'dbo.Students'))
BEGIN
    CREATE UNIQUE INDEX UX_Students_StudentNo ON dbo.Students(StudentNo);
END;
GO
IF OBJECT_ID(N'dbo.Requests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Requests
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Requests PRIMARY KEY,
        RequestNo     NVARCHAR(30)   NOT NULL,
        StudentId     INT            NOT NULL,
        RequestType   NVARCHAR(40)   NOT NULL,
        Reason        NVARCHAR(500)  NULL,
        Status        NVARCHAR(30)   NOT NULL CONSTRAINT DF_Requests_Status DEFAULT (N'Pending'),
        FeeAmount     DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Requests_Fee DEFAULT (0),
        PaymentStatus NVARCHAR(20)   NOT NULL CONSTRAINT DF_Requests_Payment DEFAULT (N'Unpaid'),
        OrNumber      NVARCHAR(40)   NULL,
        Remarks       NVARCHAR(500)  NULL,
        DateRequested DATETIME2      NOT NULL CONSTRAINT DF_Requests_Requested DEFAULT (SYSDATETIME()),
        DateUpdated   DATETIME2      NULL,
        DateReleased  DATETIME2      NULL,
        ProcessedBy   NVARCHAR(120)  NULL,
        CONSTRAINT FK_Requests_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id)
    );
END;
GO
IF OBJECT_ID(N'dbo.Requests', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Requests_RequestNo' AND object_id = OBJECT_ID(N'dbo.Requests'))
BEGIN
    CREATE UNIQUE INDEX UX_Requests_RequestNo ON dbo.Requests(RequestNo);
END;
GO
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        UserId    INT           NULL,
        Username  NVARCHAR(50)  NULL,
        Action    NVARCHAR(80)  NOT NULL,
        Details   NVARCHAR(500) NULL,
        CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSDATETIME())
    );
END;
GO
";
    }
}
