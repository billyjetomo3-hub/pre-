using System;
using System.Security.Cryptography;

namespace SchoolIdReplacement.Security
{
    /// <summary>
    /// PBKDF2 password hashing (salt + 10,000 iterations). Plain passwords are never stored.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static void Create(string password, out string hash, out string salt)
        {
            if (password == null)
            {
                password = string.Empty;
            }

            var saltBytes = new byte[SaltSize];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(saltBytes);
            }

            hash = Convert.ToBase64String(Derive(password, saltBytes, Iterations));
            salt = Convert.ToBase64String(saltBytes);
        }

        public static bool Verify(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
            {
                return false;
            }

            byte[] expected;
            byte[] saltBytes;
            try
            {
                expected = Convert.FromBase64String(hash);
                saltBytes = Convert.FromBase64String(salt);
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = Derive(password ?? string.Empty, saltBytes, Iterations);
            if (actual.Length != expected.Length)
            {
                return false;
            }

            var difference = 0;
            for (var i = 0; i < actual.Length; i++)
            {
                difference |= actual[i] ^ expected[i];
            }

            return difference == 0;
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return derive.GetBytes(HashSize);
            }
        }
    }
}
