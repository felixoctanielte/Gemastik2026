using System;
using System.Collections;
using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.Data;
using PeduliTransit.Events;
using PeduliTransit.Managers;
using PeduliTransit.NPC;
using PeduliTransit.Player;
using PeduliTransit.UI;
using PeduliTransit.World;
using UnityEngine;

namespace PeduliTransit.Events
{
    public class EventDirector : MonoBehaviour
    {
        GameplayUI _ui;
        VehicleInteriorBuilder _world;
        FirstPersonController _player;
        List<IncidentDefinition> _queue;
        int _index;
        Action _onSessionComplete;

        public void Begin(TransportMode mode, GameplayUI ui, VehicleInteriorBuilder world,
            FirstPersonController player, Action onSessionComplete)
        {
            _ui = ui;
            _world = world;
            _player = player;
            _onSessionComplete = onSessionComplete;

            _queue = IncidentCatalog.GetForMode(mode);
            // Demo: ambil 4 event bergantian report/inisiatif
            _queue = PickSessionEvents(_queue, 4);
            _index = 0;

            string modeLabel = mode switch
            {
                TransportMode.Krl => "MODE: KRL",
                TransportMode.Bus => "MODE: BUS",
                _ => "MODE: ANGKUTAN UMUM"
            };

            _ui.ShowHud(modeLabel, GameManager.Instance.Session.currentPoints);
            if (_player != null)
                _player.LookEnabled = true;

            StartCoroutine(RunIntroThenEvents(modeLabel));
        }

        static List<IncidentDefinition> PickSessionEvents(List<IncidentDefinition> all, int count)
        {
            var reports = all.FindAll(e => e.category == EventCategory.Report);
            var initiatives = all.FindAll(e => e.category == EventCategory.Initiative);
            var picked = new List<IncidentDefinition>();

            int i = 0, j = 0;
            while (picked.Count < count && (i < reports.Count || j < initiatives.Count))
            {
                if (picked.Count % 2 == 0 && i < reports.Count)
                    picked.Add(reports[i++]);
                else if (j < initiatives.Count)
                    picked.Add(initiatives[j++]);
                else if (i < reports.Count)
                    picked.Add(reports[i++]);
            }

            return picked;
        }

        IEnumerator RunIntroThenEvents(string modeLabel)
        {
            bool cont = false;
            _ui.ShowStory("Misi Kepedulian",
                "Kamu berada di dalam kendaraan umum.\n\n" +
                "Amati lingkungan: ada yang perlu dilaporkan (perilaku tidak sesuai) " +
                "dan ada yang perlu dibantu (beri tempat duduk).\n\n" +
                "Setiap keputusan berpoin. Kumpulkan skor sebanyak mungkin!",
                () => cont = true);

            if (_player != null)
                _player.LookEnabled = false;

            yield return new WaitUntil(() => cont);
            yield return StartCoroutine(RunNext());
        }

        IEnumerator RunNext()
        {
            if (_index >= _queue.Count)
            {
                GameManager.Instance.EndSession();
                _ui.ShowResult(GameManager.Instance.Session, GameManager.Instance.Profile.totalScore);
                _onSessionComplete?.Invoke();
                yield break;
            }

            var incident = _queue[_index];
            FocusNpc(incident.npcRole);

            bool storyDone = false;
            if (_player != null)
                _player.LookEnabled = false;

            string cat = incident.category == EventCategory.Report ? "LAPOR" : "INISIATIF";
            _ui.ShowStory($"[{cat}] {incident.title}", incident.introStory, () => storyDone = true);
            yield return new WaitUntil(() => storyDone);

            DecisionOutcome? outcome = null;
            _ui.ShowDecision(incident.decisionPrompt, incident.timeLimit, o => outcome = o);
            yield return new WaitUntil(() => outcome.HasValue);

            int delta = outcome.Value switch
            {
                DecisionOutcome.Yes => incident.scoreYes,
                DecisionOutcome.No => incident.scoreNo,
                _ => incident.scoreTimeout
            };

            // Untuk prototype edukasi: Ya dianggap tindakan peduli yang benar
            GameManager.Instance.ApplyDecision(incident.category, outcome.Value, delta);
            _ui.UpdateScore(GameManager.Instance.Session.currentPoints);

            string after = outcome.Value switch
            {
                DecisionOutcome.Yes => incident.storyAfterYes + $"\n\n(+{incident.scoreYes} poin)",
                DecisionOutcome.No => incident.storyAfterNo + $"\n\n({incident.scoreNo} poin)",
                _ => incident.storyAfterTimeout + $"\n\n({incident.scoreTimeout} poin)"
            };

            bool eduDone = false;
            string resultTitle = outcome.Value switch
            {
                DecisionOutcome.Yes => "Keputusan Baik",
                DecisionOutcome.No => "Perlu Refleksi",
                _ => "Edukasi — Waktu Habis"
            };
            _ui.ShowStory(resultTitle, after, () => eduDone = true);
            yield return new WaitUntil(() => eduDone);

            _index++;
            if (_player != null)
                _player.LookEnabled = true;

            yield return new WaitForSecondsRealtime(0.35f);
            yield return StartCoroutine(RunNext());
        }

        void FocusNpc(NpcRole role)
        {
            var npc = _world?.FindByRole(role);
            if (npc == null || _player == null)
                return;

            Vector3 to = npc.transform.position - _player.transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
                _player.transform.rotation = Quaternion.LookRotation(to.normalized);
        }

        public void StopDirector()
        {
            StopAllCoroutines();
        }
    }
}
