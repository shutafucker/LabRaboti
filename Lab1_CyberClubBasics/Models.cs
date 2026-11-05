using System;
using System.Collections.Generic;

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
    }

    // Игровая сессия
    public class Session
    {
        public int Id { get; set; }
        public string GamerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }

        // Внешний ключ: за каким ПК сидел игрок
        public int ComputerId { get; set; }
        public Computer Computer { get; set; } = null!;
    }
}

