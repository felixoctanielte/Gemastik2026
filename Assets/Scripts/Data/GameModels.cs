using System;
using System.Collections.Generic;
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
        public float bgmVolume = 0.65f;
        public float sfxVolume = 0.85f;
        public float uiVolume = 0.9f;
        public bool muteAll;
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
    public class ReportOption
    {
        public string id;
        public string buttonLabel;
        public string chatPreview;
        public bool isCorrect;
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
        public float timeLimit = 12f;
        public int scoreYes = 10;
        public int scoreNo = -30;
        public int scoreTimeout = -50;
        public int scoreWrongReport = -10;
        public int scoreNegur = 10;
        public int scoreCancel = 0;
        public string storyAfterYes;
        public string storyAfterNo;
        public string storyAfterTimeout;
        public string storyAfterWrongReport;
        public string storyAfterNegur;
        public string storyAfterCancel;
        public bool correctIsYes = true;
        public bool allowsNegur;
        public bool escalateOnCorrect;
        public bool usesWhatsApp = true;
        public List<ReportOption> reportOptions = new List<ReportOption>();
        public string whatsappContactName = "Petugas";
        public string contactSubtitle = "online";
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
