using System;
using System.Collections.Generic;
using System.Linq;
using PeduliTransit.Data;
using UnityEngine;

namespace PeduliTransit.Managers
{
    public static class SaveSystem
    {
        const string ProfileKey = "PeduliTransit_Profile";
        const string SettingsKey = "PeduliTransit_Settings";
        const string LeaderboardKey = "PeduliTransit_Leaderboard";

        public static PlayerProfile LoadProfile()
        {
            if (!PlayerPrefs.HasKey(ProfileKey))
                return new PlayerProfile();

            try
            {
                return JsonUtility.FromJson<PlayerProfile>(PlayerPrefs.GetString(ProfileKey))
                       ?? new PlayerProfile();
            }
            catch
            {
                return new PlayerProfile();
            }
        }

        public static void SaveProfile(PlayerProfile profile)
        {
            PlayerPrefs.SetString(ProfileKey, JsonUtility.ToJson(profile));
            PlayerPrefs.Save();
        }

        public static GameSettingsData LoadSettings()
        {
            if (!PlayerPrefs.HasKey(SettingsKey))
                return new GameSettingsData();

            try
            {
                return JsonUtility.FromJson<GameSettingsData>(PlayerPrefs.GetString(SettingsKey))
                       ?? new GameSettingsData();
            }
            catch
            {
                return new GameSettingsData();
            }
        }

        public static void SaveSettings(GameSettingsData settings)
        {
            PlayerPrefs.SetString(SettingsKey, JsonUtility.ToJson(settings));
            PlayerPrefs.Save();
            if (PeduliTransit.Audio.AudioManager.Instance != null)
                PeduliTransit.Audio.AudioManager.Instance.ApplyVolumes();
            else
                AudioListener.volume = settings.muteAll ? 0f : Mathf.Clamp01(settings.masterVolume);
        }

        public static List<LeaderboardEntry> LoadLeaderboard()
        {
            if (!PlayerPrefs.HasKey(LeaderboardKey))
                return new List<LeaderboardEntry>();

            try
            {
                var wrap = JsonUtility.FromJson<LeaderboardWrapper>(PlayerPrefs.GetString(LeaderboardKey));
                return wrap?.entries?.ToList() ?? new List<LeaderboardEntry>();
            }
            catch
            {
                return new List<LeaderboardEntry>();
            }
        }

        public static void SaveLeaderboard(List<LeaderboardEntry> entries)
        {
            var wrap = new LeaderboardWrapper { entries = entries.ToArray() };
            PlayerPrefs.SetString(LeaderboardKey, JsonUtility.ToJson(wrap));
            PlayerPrefs.Save();
        }

        [Serializable]
        class LeaderboardWrapper
        {
            public LeaderboardEntry[] entries;
        }
    }
}
