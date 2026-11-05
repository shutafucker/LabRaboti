using System;

namespace CyberClubApp
{
    public class AuditLog
    {
        public long Id { get; set; }
        public string TableName { get; set; } = string.Empty;   // "Computers" или "Sessions"
        public string Action { get; set; } = string.Empty;      // "INSERT", "UPDATE", "DELETE"
        public string? OldData { get; set; }                     // JSON старых данных (null для INSERT)
        public string? NewData { get; set; }                     // JSON новых данных (null для DELETE)
        public string? ChangedBy { get; set; }                   // логин пользователя
        public DateTime ChangedAt { get; set; }                  // время изменения
    }
}

