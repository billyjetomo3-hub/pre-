namespace SchoolIdReplacement.Models
{
    /// <summary>
    /// The signed-in user. <see cref="Current"/> is set by the login form.
    /// </summary>
    public class Session
    {
        public static Session Current { get; set; }

        public static bool SignOutRequested { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public int? StudentId { get; set; }

        public string StudentNo { get; set; }

        public bool IsAdministrator
        {
            get { return Role == Roles.Administrator; }
        }

        public string Initials
        {
            get
            {
                var name = string.IsNullOrWhiteSpace(FullName) ? Username : FullName;
                if (string.IsNullOrWhiteSpace(name))
                {
                    return "?";
                }

                var parts = name.Trim().Split(' ');
                if (parts.Length == 1)
                {
                    return parts[0].Substring(0, 1).ToUpperInvariant();
                }

                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
            }
        }
    }
}
