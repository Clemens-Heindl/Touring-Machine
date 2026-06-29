using System.Security.Cryptography;
using System.Text;

namespace TourPlannerAPI.Utilities
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (password is null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hash);
        }

        public static bool VerifyPassword(string passwordHash, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(passwordHash),
                Encoding.UTF8.GetBytes(storedHash));
        }
    }
}
