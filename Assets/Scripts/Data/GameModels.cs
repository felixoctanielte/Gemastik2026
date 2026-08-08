using System;
using PeduliTransit.Core;

namespace PeduliTransit.Data
{
    [Serializable]
    public class PlayerProfile
    {
        public string username = "";
        public int totalScore;
        public int correctReports;
        public int correctInitiatives;
        public int wrongChoices;
        public int timeouts;
        public int sessionsPlayed;
    }

    [Serializable]
    public class GameSettingsData
    {
        public float masterVolume = 0.8f;
        public float mouseSensitivity = 2f;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public int totalScore;
        public int correctReports;
        public int correctInitiatives;
        public string lastMode;
        public long updatedAtUnix;
    }

    [Serializable]
    public class IncidentDefinition
    {
        public string id;
        public string title;
        public EventCategory category;
        public NpcRole npcRole;
        public string introStory;
        public string decisionPrompt;
        public float timeLimit = 10f;
        public int scoreYes = 10;
        public int scoreNo = -30;
        public int scoreTimeout = -50;
        public string storyAfterYes;
        public string storyAfterNo;
        public string storyAfterTimeout;
        public bool correctIsYes = true;
    }

    [Serializable]
    public class SessionStats
    {
        public TransportMode mode;
        public int currentPoints;
        public int correctReports;
        public int correctInitiatives;
        public int wrongChoices;
        public int timeouts;
        public int eventsCompleted;
    }
}
