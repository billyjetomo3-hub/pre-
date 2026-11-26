using System;

namespace SchoolIdReplacement.Models
{
    /// <summary>
    /// School details printed on the ID card. Change these values to rebrand the system.
    /// </summary>
    public static class SchoolProfile
    {
        public const string Name = "Sunrise Polytechnic College";

        public const string Address = "123 Mabini Street, Quezon City";

        public const string Contact = "registrar@sunrisepoly.edu.ph  -  (02) 8123-4567";

        public static string SchoolYear
        {
            get
            {
                var now = DateTime.Now;
                var start = now.Month >= 6 ? now.Year : now.Year - 1;
                return start + " - " + (start + 1);
            }
        }

        public static string CardFooter
        {
            get { return "Valid for SY " + SchoolYear; }
        }
    }
}
