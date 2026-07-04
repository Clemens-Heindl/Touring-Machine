namespace TourPlannerAPI.Utilities
{
    /// <summary>
    /// Hashes and verifies passwords with BCrypt (per-password salt, adaptive
    /// work factor). Hashing happens on the server; the client only ever sends
    /// the raw password over HTTPS.
    /// </summary>
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password must not be empty.", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Stored value is not a valid BCrypt hash (e.g. legacy data).
                return false;
            }
        }
    }
}
