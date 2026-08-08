using System;
using System.Collections.Generic;
using System.Linq;
using PeduliTransit.Data;
using UnityEngine;

namespace PeduliTransit.Managers
{
    public class LeaderboardService
    {
        const int MaxEntries = 20;

        public void Submit(PlayerProfile profile, string modeName)
        {
            if (string.IsNullOrWhiteSpace(profile.username))
                return;

            var list = SaveSystem.LoadLeaderboard();
            var existing = list.FirstOrDefault(e =>
                string.Equals(e.username, profile.username, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                existing = new LeaderboardEntry { username = profile.username };
                list.Add(existing);
            }

            existing.totalScore = profile.totalScore;
            existing.correctReports = profile.correctReports;
            existing.correctInitiatives = profile.correctInitiatives;
            existing.lastMode = modeName;
            existing.updatedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            list = list
                .OrderByDescending(e => e.totalScore)
                .ThenByDescending(e => e.correctReports + e.correctInitiatives)
                .Take(MaxEntries)
                .ToList();

            SaveSystem.SaveLeaderboard(list);
        }

        public List<LeaderboardEntry> GetByScore()
        {
            return SaveSystem.LoadLeaderboard()
                .OrderByDescending(e => e.totalScore)
                .ToList();
        }

        public List<LeaderboardEntry> GetByCareActions()
        {
            return SaveSystem.LoadLeaderboard()
                .OrderByDescending(e => e.correctReports + e.correctInitiatives)
                .ThenByDescending(e => e.totalScore)
                .ToList();
        }

        public List<LeaderboardEntry> GetNeedsImprovement()
        {
            // Framing edukatif: skor terendah / paling sering perlu belajar lagi
            return SaveSystem.LoadLeaderboard()
                .OrderBy(e => e.totalScore)
                .ThenBy(e => e.correctReports + e.correctInitiatives)
                .ToList();
        }
    }
}
