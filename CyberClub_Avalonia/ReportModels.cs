using System;

namespace CyberClubApp
{
    public class ReportSummary
    {
        public int TotalSessions { get; set; }
        public double TotalHours { get; set; }
        public double AverageMinutes { get; set; }
        public string? TopGamerName { get; set; }
        public int TopGamerSessions { get; set; }
        public double TopGamerHours { get; set; }
    }

    public class PcUsageRow
    {
        public string PcName { get; set; } = string.Empty;
        public bool IsVip { get; set; }
        public int SessionCount { get; set; }
        public double TotalHours { get; set; }
    }

    public class DailyRow
    {
        public DateTime Day { get; set; }
        public int SessionCount { get; set; }
        public double TotalHours { get; set; }

        public string DayDisplay => Day.ToString("dd.MM (ddd)");
    }
}

