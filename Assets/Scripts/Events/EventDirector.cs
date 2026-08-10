using System.Collections;
using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.Data;
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
        FreeLookCamera _camera;
        List<IncidentDefinition> _queue;
        int _index;
        System.Action _onSessionComplete;
        TransportMode _mode;

        public void Begin(TransportMode mode, GameplayUI ui, VehicleInteriorBuilder world,
            FreeLookCamera camera, System.Action onSessionComplete)
        {
            _ui = ui;
            _world = world;
            _camera = camera;
            _onSessionComplete = onSessionComplete;
            _mode = mode;

            _queue = IncidentCatalog.GetForMode(mode);
            _queue = PickSessionEvents(_queue, 5);
            _index = 0;

            string modeLabel = mode switch
            {
                TransportMode.Krl => "MODE: KRL",
                TransportMode.Bus => "MODE: BUS",
                _ => "MODE: ANGKUTAN UMUM"
            };

            _ui.ShowHud(modeLabel, GameManager.Instance.Session.currentPoints);
            SetCam(true);

            StartCoroutine(RunIntroThenEvents());
        }

        void SetCam(bool enabled)
        {
            if (_camera != null)
                _camera.LookEnabled = enabled;
        }

        static List<IncidentDefinition> PickSessionEvents(List<IncidentDefinition> all, int count)
        {
            var reports = all.FindAll(e => e.category == EventCategory.Report);
            var initiatives = all.FindAll(e => e.category == EventCategory.Initiative);

            var preferred = new List<IncidentDefinition>();
            foreach (var r in reports)
            {
                if (r.npcRole == NpcRole.HarassmentHint || r.npcRole == NpcRole.Fighting ||
                    r.npcRole == NpcRole.PrioritySeatAbuse)
                    preferred.Add(r);
            }

            var picked = new List<IncidentDefinition>();
            foreach (var p in preferred)
            {
                if (picked.Count >= count) break;
                if (!picked.Contains(p)) picked.Add(p);
            }

            int i = 0, j = 0;
            while (picked.Count < count && (i < reports.Count || j < initiatives.Count))
            {
                if (picked.Count % 2 == 0 && i < reports.Count)
                {
                    if (!picked.Contains(reports[i])) picked.Add(reports[i]);
                    i++;
                }
                else if (j < initiatives.Count)
                {
                    if (!picked.Contains(initiatives[j])) picked.Add(initiatives[j]);
                    j++;
                }
                else if (i < reports.Count)
                {
                    if (!picked.Contains(reports[i])) picked.Add(reports[i]);
                    i++;
                }
                else break;
            }

            return picked;
        }

        IEnumerator RunIntroThenEvents()
        {

            yield return ShowBlockingStory(
                "Pintu & Penumpang",
                "Perhatikan pintu. Penumpang masuk: kursi kosong → duduk; penuh → berdiri.\n" +
                "Kursi oranye = prioritas.\n\n" +
                "Kamera: tahan KLIK KANAN + geser mouse = lihat sekitar | WASD = geser | Scroll = zoom | Q/E = naik/turun.");

            SetCam(true);
            if (_world != null)
                yield return StartCoroutine(_world.IntroBoardDemo());

            string petugas = _mode switch
            {
                TransportMode.Krl => "satpam",
                TransportMode.Bus => "petugas karcis",
                _ => "anak buah pak sopir"
            };

            yield return ShowBlockingStory(
                "Misi Kepedulian",
                $"Amati lingkungan di kendaraan umum.\n\n" +
                $"Lapor gangguan lewat WhatsApp (ketuk pesan siap kirim).\n" +
                $"Salah jenis laporan = −10. Tutup tanpa kirim = 0 (tidak dihukum berat).\n\n" +
                $"Penanggung jawab: {petugas}.\n" +
                "Kursi prioritas: TEGUR sendiri atau LAPOR WA.");

            SetCam(true);
            yield return new WaitForSecondsRealtime(1.2f);
            yield return StartCoroutine(RunNext());
        }

        IEnumerator ShowBlockingStory(string title, string body)
        {
            SetCam(false);
            bool done = false;
            _ui.ShowStory(title, body, () => done = true);
            yield return new WaitUntil(() => done);
        }

        IEnumerator RunNext()
        {
            if (_index >= _queue.Count)
            {
                SetCam(false);
                GameManager.Instance.EndSession();
                _ui.ShowResult(GameManager.Instance.Session, GameManager.Instance.Profile.totalScore);
                _onSessionComplete?.Invoke();
                yield break;
            }

            var incident = _queue[_index];
            FocusNpc(incident.npcRole);

            SetCam(true);
            yield return new WaitForSecondsRealtime(1.0f);

            yield return ShowBlockingStory(
                $"[{(incident.category == EventCategory.Report ? "LAPOR" : "INISIATIF")}] {incident.title}",
                incident.introStory);

            DecisionOutcome? outcome = null;
            SetCam(false);

            if (incident.category == EventCategory.Report && incident.allowsNegur)
            {
                bool? wantNegur = null;
                bool skipped = false;
                _ui.ShowPriorityActionChoice(incident.decisionPrompt,
                    onChoice: negur => wantNegur = negur,
                    onSkip: o =>
                    {
                        skipped = true;
                        outcome = o;
                    });

                yield return new WaitUntil(() => wantNegur.HasValue || skipped);

                if (!(skipped && outcome.HasValue))
                {
                    if (wantNegur == true)
                    {
                        outcome = DecisionOutcome.Negur;
                        SetCam(true);
                        var abuser = _world?.FindByRole(NpcRole.PrioritySeatAbuse);
                        if (abuser != null && _world != null)
                            yield return StartCoroutine(_world.PlayerNegurPriorityRoutine(abuser));
                    }
                    else if (wantNegur == false)
                    {
                        _ui.ShowWhatsAppReport(incident, o => outcome = o);
                        yield return new WaitUntil(() => outcome.HasValue);

                        if (outcome == DecisionOutcome.Yes && _world != null)
                        {
                            SetCam(true);
                            var culprit = _world.FindByRole(incident.npcRole);
                            yield return StartCoroutine(_world.ResponderResolveRoutine(culprit, incident.escalateOnCorrect));
                        }
                    }
                }
            }
            else if (incident.category == EventCategory.Report && incident.usesWhatsApp)
            {
                _ui.ShowWhatsAppReport(incident, o => outcome = o);
                yield return new WaitUntil(() => outcome.HasValue);

                if (outcome == DecisionOutcome.Yes && _world != null)
                {
                    SetCam(true);
                    var culprit = _world.FindByRole(incident.npcRole);
                    yield return StartCoroutine(_world.ResponderResolveRoutine(culprit, incident.escalateOnCorrect));
                }
            }
            else
            {
                _ui.ShowDecision(incident.decisionPrompt, incident.timeLimit, o => outcome = o);
                yield return new WaitUntil(() => outcome.HasValue);

                if (outcome == DecisionOutcome.Yes && _world != null)
                {
                    SetCam(true);
                    yield return StartCoroutine(_world.GiveSeatTo(incident.npcRole));
                }
            }

            if (!outcome.HasValue)
                outcome = DecisionOutcome.Timeout;

            int delta = GameManager.Instance.ScoreFor(incident, outcome.Value);
            GameManager.Instance.ApplyDecision(incident.category, outcome.Value, delta);
            _ui.UpdateScore(GameManager.Instance.Session.currentPoints);

            yield return ShowBlockingStory(ResultTitle(outcome.Value), BuildAfterStory(incident, outcome.Value, delta));

            _index++;

            SetCam(true);
            yield return new WaitForSecondsRealtime(1.5f);
            yield return StartCoroutine(RunNext());
        }

        static string ResultTitle(DecisionOutcome outcome)
        {
            return outcome switch
            {
                DecisionOutcome.Yes => "Laporan Berhasil",
                DecisionOutcome.Negur => "Teguran Berhasil",
                DecisionOutcome.WrongReport => "Edukasi — Laporan Tidak Sesuai",
                DecisionOutcome.Cancel => "Laporan Dibatalkan",
                DecisionOutcome.No => "Perlu Refleksi",
                _ => "Edukasi — Waktu Habis"
            };
        }

        static string BuildAfterStory(IncidentDefinition incident, DecisionOutcome outcome, int delta)
        {
            string body = outcome switch
            {
                DecisionOutcome.Yes => incident.storyAfterYes,
                DecisionOutcome.Negur => string.IsNullOrEmpty(incident.storyAfterNegur)
                    ? incident.storyAfterYes
                    : incident.storyAfterNegur,
                DecisionOutcome.WrongReport => string.IsNullOrEmpty(incident.storyAfterWrongReport)
                    ? "Laporan tidak sesuai. Edukasi: sampaikan ke pihak berwajib dengan jenis kejadian yang tepat."
                    : incident.storyAfterWrongReport,
                DecisionOutcome.Cancel => string.IsNullOrEmpty(incident.storyAfterCancel)
                    ? "Kamu menutup chat tanpa mengirim. Tidak apa-apa mengamati dulu—tapi jika situasi berbahaya, segera lapor ke petugas."
                    : incident.storyAfterCancel,
                DecisionOutcome.No => incident.storyAfterNo,
                _ => incident.storyAfterTimeout
            };

            string sign = delta >= 0 ? $"+{delta}" : $"{delta}";
            return $"{body}\n\n({sign} poin)";
        }

        void FocusNpc(NpcRole role)
        {
            var npc = _world?.FindByRole(role);
            if (npc == null || _camera == null)
                return;

            npc.Highlight(true);
            _camera.FocusOn(npc.transform, preferredDistance: 4.2f);
            PeduliTransit.Audio.AudioManager.Instance?.PlayIncidentAmbience(role);
        }

        public void StopDirector()
        {
            StopAllCoroutines();
            SetCam(false);
        }
    }
}
