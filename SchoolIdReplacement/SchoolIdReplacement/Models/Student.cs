using System;

namespace SchoolIdReplacement.Models
{
    public class Student
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string StudentNo { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Course { get; set; }

        public string YearLevel { get; set; }

        public string Section { get; set; }

        public string Email { get; set; }

        public string ContactNo { get; set; }

        public string Address { get; set; }

        public string PhotoPath { get; set; }

        public bool IsActive { get; set; }

        public string FullName
        {
            get
            {
                var middle = string.IsNullOrWhiteSpace(MiddleName) ? string.Empty : " " + MiddleName.Trim();
                return (FirstName + middle + " " + LastName).Replace("  ", " ").Trim();
            }
        }

        public string Initials
        {
            get
            {
                var first = string.IsNullOrWhiteSpace(FirstName) ? "?" : FirstName.Substring(0, 1);
                var last = string.IsNullOrWhiteSpace(LastName) ? string.Empty : LastName.Substring(0, 1);
                return (first + last).ToUpperInvariant();
            }
        }

        public string CourseLine
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(Course))
                {
                    parts.Add(Course.Trim());
                }
                if (!string.IsNullOrWhiteSpace(YearLevel))
                {
                    parts.Add(YearLevel.Trim());
                }
                if (!string.IsNullOrWhiteSpace(Section))
                {
                    parts.Add(Section.Trim());
                }

                return string.Join(" - ", parts.ToArray());
            }
        }

        public Student Clone()
        {
            return (Student)MemberwiseClone();
        }
    }
}
