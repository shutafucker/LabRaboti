using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CyberClubApp
{
    public class ReportService
    {
        public async Task<ReportSummary> GetSummaryAsync(DateTime from, DateTime to)
        {
            using var db = new AppDbContext();

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Computer)
                .Where(s => s.StartTime >= from && s.StartTime < to && s.EndTime != null)
                .ToListAsync();

            if (sessions.Count == 0)
                return new ReportSummary();

            var totalMinutes = sessions.Sum(s => (s.EndTime!.Value - s.StartTime).TotalMinutes);

            var topGamer = sessions
                .GroupBy(s => s.GamerName)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Minutes = g.Sum(s => (s.EndTime!.Value - s.StartTime).TotalMinutes)
                })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.Minutes)
                .First();

            return new ReportSummary
            {
                TotalSessions = sessions.Count,
                TotalHours = Math.Round(totalMinutes / 60.0, 1),
                AverageMinutes = Math.Round(totalMinutes / sessions.Count, 0),
                TopGamerName = topGamer.Name,
                TopGamerSessions = topGamer.Count,
                TopGamerHours = Math.Round(topGamer.Minutes / 60.0, 1)
            };
        }

        public async Task<List<PcUsageRow>> GetTopPcsAsync(DateTime from, DateTime to, int top = 5)
        {
            using var db = new AppDbContext();

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Computer)
                .Where(s => s.StartTime >= from && s.StartTime < to && s.EndTime != null)
                .ToListAsync();

            var rows = sessions
                .GroupBy(s => new { s.ComputerId, s.Computer.Name, s.Computer.IsVip })
                .Select(g => new PcUsageRow
                {
                    PcName = g.Key.Name,
                    IsVip = g.Key.IsVip,
                    SessionCount = g.Count(),
                    TotalHours = Math.Round(
                        g.Sum(s => (s.EndTime!.Value - s.StartTime).TotalMinutes) / 60.0, 1)
                })
                .OrderByDescending(r => r.SessionCount)
                .Take(top)
                .ToList();

            return rows;
        }

        public async Task<List<DailyRow>> GetDailyAsync(DateTime from, DateTime to)
        {
            using var db = new AppDbContext();

            var sessions = await db.Sessions
                .AsNoTracking()
                .Where(s => s.StartTime >= from && s.StartTime < to && s.EndTime != null)
                .Select(s => new { s.StartTime, s.EndTime })
                .ToListAsync();

            return sessions
                .GroupBy(s => s.StartTime.Date)
                .Select(g => new DailyRow
                {
                    Day = g.Key,
                    SessionCount = g.Count(),
                    TotalHours = Math.Round(
                        g.Sum(s => (s.EndTime!.Value - s.StartTime).TotalMinutes) / 60.0, 1)
                })
                .OrderBy(r => r.Day)
                .ToList();
        }
    }
}

