using System;
using PeduliTransit.Audio;
using PeduliTransit.Core;
using PeduliTransit.Data;
using UnityEngine;

namespace PeduliTransit.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public PlayerProfile Profile { get; private set; }
        public GameSettingsData Settings { get; private set; }
        public SessionStats Session { get; private set; }
        public TransportMode CurrentMode { get; private set; }
        public LeaderboardService Leaderboard { get; } = new LeaderboardService();

        public event Action<GameState> StateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Profile = SaveSystem.LoadProfile();
            Settings = SaveSystem.LoadSettings();
            Session = new SessionStats();

            AudioManager.EnsureExists();
            if (AudioManager.Instance != null)
                AudioManager.Instance.ApplyVolumes();
            else
                AudioListener.volume = Settings.muteAll ? 0f : Mathf.Clamp01(Settings.masterVolume);
        }

        public void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        public void Login(string username)
        {
            Profile.username = username.Trim();
            if (Profile.username.Length > 16)
                Profile.username = Profile.username.Substring(0, 16);
            SaveSystem.SaveProfile(Profile);
            SetState(GameState.Hub);
            AudioManager.Instance?.PlayUi(SfxId.LoginSuccess);
            AudioManager.Instance?.PlayBgm(BgmId.Hub);
        }

        public void RenameUser(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return;

            string cleaned = newName.Trim();
            if (cleaned.Length > 16)
                cleaned = cleaned.Substring(0, 16);

            Profile.username = cleaned;
            SaveSystem.SaveProfile(Profile);
            AudioManager.Instance?.PlayUi(SfxId.UiConfirm);
        }

        public bool HasSavedUser => !string.IsNullOrWhiteSpace(Profile.username);

        public void BeginSession(TransportMode mode)
        {
            CurrentMode = mode;
            Session = new SessionStats { mode = mode };
            SetState(GameState.Playing);
            AudioManager.Instance?.PlayUi(SfxId.ModeSelect);
            AudioManager.Instance?.PlayBgmForMode(mode);
        }

        public void ApplyDecision(EventCategory category, DecisionOutcome outcome, int delta)
        {
            Session.currentPoints += delta;
            Session.eventsCompleted++;
            AudioManager.Instance?.PlayDecisionFeedback(outcome);

            if (outcome == DecisionOutcome.Timeout)
            {
                Session.timeouts++;
                Profile.timeouts++;
                return;
            }

            if (outcome == DecisionOutcome.Cancel)
                return;

            bool correct = outcome == DecisionOutcome.Yes || outcome == DecisionOutcome.Negur;

            if (!correct)
            {
                Session.wrongChoices++;
                Profile.wrongChoices++;
                return;
            }

            if (category == EventCategory.Report || outcome == DecisionOutcome.Negur)
            {
                Session.correctReports++;
                Profile.correctReports++;
            }
            else
            {
                Session.correctInitiatives++;
                Profile.correctInitiatives++;
            }
        }

        public int ScoreFor(IncidentDefinition incident, DecisionOutcome outcome)
        {
            if (incident == null)
                return 0;

            return outcome switch
            {
                DecisionOutcome.Yes => incident.scoreYes,
                DecisionOutcome.Negur => incident.scoreNegur,
                DecisionOutcome.WrongReport => incident.scoreWrongReport,
                DecisionOutcome.Cancel => incident.scoreCancel,
                DecisionOutcome.No => incident.scoreNo,
                _ => incident.scoreTimeout
            };
        }

        public void EndSession()
        {
            Profile.totalScore += Session.currentPoints;
            Profile.sessionsPlayed++;
            SaveSystem.SaveProfile(Profile);
            Leaderboard.Submit(Profile, CurrentMode.ToString());
            SetState(GameState.Result);
            AudioManager.Instance?.PlaySfx(SfxId.SessionComplete);
            AudioManager.Instance?.PlayBgm(BgmId.Result);
        }

        public void SaveSettings()
        {
            SaveSystem.SaveSettings(Settings);
            AudioManager.Instance?.ApplyVolumes();
        }

        public void ReturnToHub()
        {
            SetState(GameState.Hub);
            AudioManager.Instance?.PlayBgm(BgmId.Hub);
        }
    }
}
