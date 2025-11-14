using System;
using System.Security.Cryptography;

namespace ABC_Retail_Part1.Helpers
{
    public static class PasswordHelper
    {
        // Format stored: {iterations}.{base64Salt}.{base64Hash}
        public static string HashPassword(string password, int iterations = 100_000)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string stored)
        {
            try
            {
                var parts = stored.Split('.', 3);
                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var storedHash = Convert.FromBase64String(parts[2]);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(hash, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}