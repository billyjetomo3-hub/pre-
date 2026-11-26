using System;
using System.Data;
using SchoolIdReplacement.Data;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Security;

namespace SchoolIdReplacement.Services
{
    public static class AuthService
    {
        public static Session Login(string username, string password)
        {
            username = (username ?? string.Empty).Trim();

            if (username.Length == 0 || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Please enter your username and password.");
            }

            var table = Sql.Query(
                "SELECT Id, Username, PasswordHash, PasswordSalt, Role, FullName, IsActive FROM dbo.Users WHERE Username = @username",
                Sql.P("@username", username));

            if (table.Rows.Count == 0)
            {
                throw new InvalidOperationException("Incorrect username or password.");
            }

            var row = table.Rows[0];

            if (!Convert.ToBoolean(row["IsActive"]))
            {
                throw new InvalidOperationException("This account has been deactivated. Please contact the registrar.");
            }

            if (!PasswordHasher.Verify(password, Convert.ToString(row["PasswordHash"]), Convert.ToString(row["PasswordSalt"])))
            {
                throw new InvalidOperationException("Incorrect username or password.");
            }

            var session = new Session
            {
                UserId = Convert.ToInt32(row["Id"]),
                Username = Convert.ToString(row["Username"]),
                FullName = Convert.ToString(row["FullName"]),
                Role = Convert.ToString(row["Role"])
            };

            if (session.Role == Roles.Student)
            {
                var student = Sql.Query(
                    "SELECT Id, StudentNo FROM dbo.Students WHERE UserId = @userId",
                    Sql.P("@userId", session.UserId));

                if (student.Rows.Count > 0)
                {
                    session.StudentId = Convert.ToInt32(student.Rows[0]["Id"]);
                    session.StudentNo = Convert.ToString(student.Rows[0]["StudentNo"]);
                }
            }

            Sql.Execute("UPDATE dbo.Users SET LastLoginAt = SYSDATETIME() WHERE Id = @id", Sql.P("@id", session.UserId));
            AuditService.Write(session, "Sign in", session.Username + " signed in as " + session.Role);

            return session;
        }

        public static void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var table = Sql.Query("SELECT PasswordHash, PasswordSalt FROM dbo.Users WHERE Id = @id", Sql.P("@id", userId));
            if (table.Rows.Count == 0)
            {
                throw new InvalidOperationException("The account could not be found.");
            }

            if (!PasswordHasher.Verify(currentPassword, Convert.ToString(table.Rows[0]["PasswordHash"]), Convert.ToString(table.Rows[0]["PasswordSalt"])))
            {
                throw new InvalidOperationException("Your current password is incorrect.");
            }

            ValidatePassword(newPassword);
            StorePassword(userId, newPassword);
        }

        public static void ResetPassword(int userId, string newPassword)
        {
            ValidatePassword(newPassword);
            StorePassword(userId, newPassword);
        }

        public static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                throw new InvalidOperationException("The password must be at least 6 characters long.");
            }
        }

        public static bool UsernameExists(string username)
        {
            var count = Sql.Scalar("SELECT COUNT(*) FROM dbo.Users WHERE Username = @username", Sql.P("@username", username));
            return Convert.ToInt32(count) > 0;
        }

        private static void StorePassword(int userId, string password)
        {
            string hash;
            string salt;
            PasswordHasher.Create(password, out hash, out salt);

            Sql.Execute(
                "UPDATE dbo.Users SET PasswordHash = @hash, PasswordSalt = @salt WHERE Id = @id",
                Sql.P("@hash", hash),
                Sql.P("@salt", salt),
                Sql.P("@id", userId));

            var username = Convert.ToString(Sql.Scalar("SELECT Username FROM dbo.Users WHERE Id = @id", Sql.P("@id", userId)));
            AuditService.Write(Session.Current, "Password changed", "Password updated for " + username);
        }
    }
}
