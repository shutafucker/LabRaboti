using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CyberClubApp
{
    // Компьютер в клубе
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "PC-01 (VIP)"
        public bool IsVip { get; set; }

        // История сессий этого ПК (связь 1 ко многим)
        public List<Session> Sessions { get; set; } = new();

        // Удобное вычисляемое свойство — ПК занят, если есть активная сессия
        [NotMapped]
        public bool IsOccupied => Sessions.Any(s => s.EndTime == null);
    }

    // Игровая сессия
    public class Session
    {
        public int Id { get; set; }
        public string GamerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }

        // null = сессия ещё идёт, дата = игрок ушёл
        public DateTime? EndTime { get; set; }

        // Внешний ключ: за каким ПК сидел игрок
        public int ComputerId { get; set; }
        public Computer Computer { get; set; } = null!;

        // Удобное вычисляемое свойство — НЕ колонка в базе
        [NotMapped]
        public bool IsActive => EndTime == null;

        [NotMapped]
        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;

        [NotMapped]
        public string StartTimeDisplay => StartTime.ToLocalTime().ToString("HH:mm");

        [NotMapped]
        public string DurationDisplay => Duration.ToString(@"hh\:mm");
    }
}

