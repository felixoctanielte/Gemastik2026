using System;
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
            // Single-scene prototype: tetap di hierarchy, jangan DDOL
            Profile = SaveSystem.LoadProfile();
            Settings = SaveSystem.LoadSettings();
            AudioListener.volume = Mathf.Clamp01(Settings.masterVolume);
            Session = new SessionStats();
        }

        public void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        public void Login(string username)
        {
            Profile.username = username.Trim();
            SaveSystem.SaveProfile(Profile);
            SetState(GameState.Hub);
        }

        public bool HasSavedUser => !string.IsNullOrWhiteSpace(Profile.username);

        public void BeginSession(TransportMode mode)
        {
            CurrentMode = mode;
            Session = new SessionStats { mode = mode };
            SetState(GameState.Playing);
        }

        public void ApplyDecision(EventCategory category, DecisionOutcome outcome, int delta)
        {
            Session.currentPoints += delta;
            Session.eventsCompleted++;

            bool correct = outcome == DecisionOutcome.Yes;

            if (outcome == DecisionOutcome.Timeout)
            {
                Session.timeouts++;
                Profile.timeouts++;
            }
            else if (!correct)
            {
                Session.wrongChoices++;
                Profile.wrongChoices++;
            }
            else if (category == EventCategory.Report)
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

        public void EndSession()
        {
            Profile.totalScore += Session.currentPoints;
            Profile.sessionsPlayed++;
            SaveSystem.SaveProfile(Profile);
            Leaderboard.Submit(Profile, CurrentMode.ToString());
            SetState(GameState.Result);
        }

        public void SaveSettings()
        {
            SaveSystem.SaveSettings(Settings);
        }

        public void ReturnToHub()
        {
            SetState(GameState.Hub);
        }
    }
}
