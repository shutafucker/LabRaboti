namespace CyberClubApp
{
    public static class PasswordHelper
    {
        // workFactor=12 — рекомендованный минимум OWASP, ~100-300мс на хеш
        private const int WorkFactor = 12;

        public static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

        public static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Если в базе лежит не BCrypt-строка (старые данные, повреждение)
                return false;
            }
        }
    }
}

