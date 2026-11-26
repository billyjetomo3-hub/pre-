using System;
using System.Data;
using SchoolIdReplacement.Data;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Security;

namespace SchoolIdReplacement.Services
{
    public static class StudentService
    {
        public static DataTable List(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return Sql.Query(
                    "SELECT s.Id, s.StudentNo AS [Student No], s.LastName + ', ' + s.FirstName AS [Name], " +
                    "       s.Course, s.YearLevel AS [Year], s.Section, s.Email, s.ContactNo AS [Contact], " +
                    "       u.Username, s.IsActive AS [Active] " +
                    "FROM dbo.Students s LEFT JOIN dbo.Users u ON u.Id = s.UserId ORDER BY s.StudentNo");
            }

            var like = "%" + search.Trim() + "%";
            return Sql.Query(
                "SELECT s.Id, s.StudentNo AS [Student No], s.LastName + ', ' + s.FirstName AS [Name], " +
                "       s.Course, s.YearLevel AS [Year], s.Section, s.Email, s.ContactNo AS [Contact], " +
                "       u.Username, s.IsActive AS [Active] " +
                "FROM dbo.Students s LEFT JOIN dbo.Users u ON u.Id = s.UserId " +
                "WHERE s.StudentNo LIKE @like OR s.FirstName LIKE @like OR s.LastName LIKE @like OR s.Course LIKE @like " +
                "ORDER BY s.StudentNo",
                Sql.P("@like", like));
        }

        public static Student Get(int studentId)
        {
            var table = Sql.Query(
                "SELECT Id, UserId, StudentNo, FirstName, MiddleName, LastName, Course, YearLevel, Section, " +
                "       Email, ContactNo, Address, PhotoPath, IsActive FROM dbo.Students WHERE Id = @id",
                Sql.P("@id", studentId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public static Student GetByUserId(int userId)
        {
            var table = Sql.Query(
                "SELECT Id, UserId, StudentNo, FirstName, MiddleName, LastName, Course, YearLevel, Section, " +
                "       Email, ContactNo, Address, PhotoPath, IsActive FROM dbo.Students WHERE UserId = @userId",
                Sql.P("@userId", userId));

            return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
        }

        public static int Create(Student student, string password)
        {
            Validate(student);
            AuthService.ValidatePassword(password);

            var exists = Convert.ToInt32(Sql.Scalar(
                "SELECT COUNT(*) FROM dbo.Students WHERE StudentNo = @studentNo",
                Sql.P("@studentNo", student.StudentNo.Trim())));

            if (exists > 0)
            {
                throw new InvalidOperationException("A student with this student number already exists.");
            }

            var username = string.IsNullOrWhiteSpace(student.StudentNo) ? string.Empty : student.StudentNo.Trim();
            if (AuthService.UsernameExists(username))
            {
                throw new InvalidOperationException("The login username \"" + username + "\" is already taken.");
            }

            string hash;
            string salt;
            PasswordHasher.Create(password, out hash, out salt);

            var userId = Convert.ToInt32(Sql.Scalar(
                "INSERT INTO dbo.Users (Username, PasswordHash, PasswordSalt, Role, FullName) " +
                "OUTPUT INSERTED.Id VALUES (@username, @hash, @salt, @role, @fullName)",
                Sql.P("@username", username),
                Sql.P("@hash", hash),
                Sql.P("@salt", salt),
                Sql.P("@role", Roles.Student),
                Sql.P("@fullName", student.FullName)));

            var id = Convert.ToInt32(Sql.Scalar(
                "INSERT INTO dbo.Students (UserId, StudentNo, FirstName, MiddleName, LastName, Course, YearLevel, Section, Email, ContactNo, Address, PhotoPath) " +
                "OUTPUT INSERTED.Id VALUES (@userId, @studentNo, @firstName, @middleName, @lastName, @course, @yearLevel, @section, @email, @contactNo, @address, @photoPath)",
                Sql.P("@userId", userId),
                Sql.P("@studentNo", student.StudentNo.Trim()),
                Sql.P("@firstName", student.FirstName.Trim()),
                Sql.P("@middleName", student.MiddleName),
                Sql.P("@lastName", student.LastName.Trim()),
                Sql.P("@course", student.Course),
                Sql.P("@yearLevel", student.YearLevel),
                Sql.P("@section", student.Section),
                Sql.P("@email", student.Email),
                Sql.P("@contactNo", student.ContactNo),
                Sql.P("@address", student.Address),
                Sql.P("@photoPath", student.PhotoPath)));

            AuditService.Write(Session.Current, "Student added", student.StudentNo + " - " + student.FullName);
            return id;
        }

        public static void Update(Student student)
        {
            Validate(student);

            var duplicate = Convert.ToInt32(Sql.Scalar(
                "SELECT COUNT(*) FROM dbo.Students WHERE StudentNo = @studentNo AND Id <> @id",
                Sql.P("@studentNo", student.StudentNo.Trim()),
                Sql.P("@id", student.Id)));

            if (duplicate > 0)
            {
                throw new InvalidOperationException("Another student already uses this student number.");
            }

            Sql.Execute(
                "UPDATE dbo.Students SET StudentNo = @studentNo, FirstName = @firstName, MiddleName = @middleName, " +
                "LastName = @lastName, Course = @course, YearLevel = @yearLevel, Section = @section, Email = @email, " +
                "ContactNo = @contactNo, Address = @address, PhotoPath = @photoPath WHERE Id = @id",
                Sql.P("@studentNo", student.StudentNo.Trim()),
                Sql.P("@firstName", student.FirstName.Trim()),
                Sql.P("@middleName", student.MiddleName),
                Sql.P("@lastName", student.LastName.Trim()),
                Sql.P("@course", student.Course),
                Sql.P("@yearLevel", student.YearLevel),
                Sql.P("@section", student.Section),
                Sql.P("@email", student.Email),
                Sql.P("@contactNo", student.ContactNo),
                Sql.P("@address", student.Address),
                Sql.P("@photoPath", student.PhotoPath),
                Sql.P("@id", student.Id));

            if (student.UserId.HasValue)
            {
                Sql.Execute(
                    "UPDATE dbo.Users SET FullName = @fullName, Username = @username WHERE Id = @id",
                    Sql.P("@fullName", student.FullName),
                    Sql.P("@username", student.StudentNo.Trim()),
                    Sql.P("@id", student.UserId.Value));
            }

            AuditService.Write(Session.Current, "Student updated", student.StudentNo + " - " + student.FullName);
        }

        public static void SetActive(int studentId, bool active)
        {
            var student = Get(studentId);
            if (student == null)
            {
                throw new InvalidOperationException("The student could not be found.");
            }

            Sql.Execute("UPDATE dbo.Students SET IsActive = @active WHERE Id = @id",
                Sql.P("@active", active ? 1 : 0),
                Sql.P("@id", studentId));

            if (student.UserId.HasValue)
            {
                Sql.Execute("UPDATE dbo.Users SET IsActive = @active WHERE Id = @id",
                    Sql.P("@active", active ? 1 : 0),
                    Sql.P("@id", student.UserId.Value));
            }

            AuditService.Write(Session.Current, active ? "Student activated" : "Student deactivated", student.StudentNo);
        }

        public static void ResetPassword(int studentId, string newPassword)
        {
            var student = Get(studentId);
            if (student == null || !student.UserId.HasValue)
            {
                throw new InvalidOperationException("This student has no login account.");
            }

            AuthService.ResetPassword(student.UserId.Value, newPassword);
            AuditService.Write(Session.Current, "Password reset", "Password reset for " + student.StudentNo);
        }

        private static void Validate(Student student)
        {
            if (student == null)
            {
                throw new InvalidOperationException("No student data was supplied.");
            }

            if (string.IsNullOrWhiteSpace(student.StudentNo))
            {
                throw new InvalidOperationException("The student number is required.");
            }

            if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
            {
                throw new InvalidOperationException("The first name and last name are required.");
            }
        }

        private static Student Map(DataRow row)
        {
            return new Student
            {
                Id = Convert.ToInt32(row["Id"]),
                UserId = row["UserId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["UserId"]),
                StudentNo = Convert.ToString(row["StudentNo"]),
                FirstName = Convert.ToString(row["FirstName"]),
                MiddleName = row["MiddleName"] == DBNull.Value ? null : Convert.ToString(row["MiddleName"]),
                LastName = Convert.ToString(row["LastName"]),
                Course = row["Course"] == DBNull.Value ? null : Convert.ToString(row["Course"]),
                YearLevel = row["YearLevel"] == DBNull.Value ? null : Convert.ToString(row["YearLevel"]),
                Section = row["Section"] == DBNull.Value ? null : Convert.ToString(row["Section"]),
                Email = row["Email"] == DBNull.Value ? null : Convert.ToString(row["Email"]),
                ContactNo = row["ContactNo"] == DBNull.Value ? null : Convert.ToString(row["ContactNo"]),
                Address = row["Address"] == DBNull.Value ? null : Convert.ToString(row["Address"]),
                PhotoPath = row["PhotoPath"] == DBNull.Value ? null : Convert.ToString(row["PhotoPath"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}
