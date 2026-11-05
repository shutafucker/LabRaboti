using System.ComponentModel.DataAnnotations;

namespace CyberClubApp
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        // Здесь лежит BCrypt-хеш, а не открытый пароль
        [Required, MaxLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        // "Admin" или "Operator"
        [Required, MaxLength(20)]
        public string Role { get; set; } = "Operator";
    }
}

