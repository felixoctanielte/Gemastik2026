using System;
using System.Text;
using PeduliTransit.Core;
using PeduliTransit.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PeduliTransit.UI
{
    public class HubUI : MonoBehaviour
    {
        Action<TransportMode> _onPlay;
        GameObject _root;
        GameObject _overlay;

        public void Build(Transform canvas, Action<TransportMode> onPlay)
        {
            _onPlay = onPlay;

            var root = UiTheme.MakePanel(canvas, "HubRoot", new Color(0.05f, 0.12f, 0.14f, 0.55f));
            UiTheme.Stretch(root.rectTransform);
            _root = root.gameObject;

            var brand = UiTheme.MakeText(root.transform, "PEDULI TRANSIT", 42, FontStyle.Bold,
                TextAnchor.UpperLeft, UiTheme.Accent);
            UiTheme.SetAnchored(brand.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -90f), new Vector2(520f, -30f));

            var sub = UiTheme.MakeText(root.transform, "Pilih moda transportasi untuk menjalankan misi kepedulian.",
                18, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Muted);
            UiTheme.SetAnchored(sub.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -140f), new Vector2(620f, -95f));

            CreateModeButton(root.transform, "KRL", TransportMode.Krl, new Vector2(-260f, -20f));
            CreateModeButton(root.transform, "BUS", TransportMode.Bus, new Vector2(0f, -20f));
            CreateModeButton(root.transform, "ANGKUTAN\nUMUM", TransportMode.AngkutanUmum, new Vector2(260f, -20f));

            float y = -210f;
            CreateSideButton(root.transform, "SETTINGS", () => ShowSettings(), new Vector2(-260f, y));
            CreateSideButton(root.transform, "PROFILE", () => ShowProfile(), new Vector2(-87f, y));
            CreateSideButton(root.transform, "LEADERBOARD", () => ShowLeaderboard(), new Vector2(87f, y));
            CreateSideButton(root.transform, "EXIT", ExitGame, new Vector2(260f, y), UiTheme.Danger);
        }

        void CreateModeButton(Transform parent, string label, TransportMode mode, Vector2 pos)
        {
            var btn = UiTheme.MakeButton(parent, label, UiTheme.Teal, new Vector2(200f, 120f));
            btn.GetComponent<RectTransform>().anchoredPosition = pos;
            btn.onClick.AddListener(() => _onPlay?.Invoke(mode));
        }

        void CreateSideButton(Transform parent, string label, Action action, Vector2 pos, Color? color = null)
        {
            var btn = UiTheme.MakeButton(parent, label, color ?? UiTheme.AccentDark, new Vector2(160f, 48f));
            btn.GetComponent<RectTransform>().anchoredPosition = pos;
            btn.onClick.AddListener(() => action());
        }

        void ClearOverlay()
        {
            if (_overlay != null)
                Destroy(_overlay);
            _overlay = null;
        }

        GameObject MakeOverlayCard(string title, out Transform content)
        {
            ClearOverlay();
            var overlay = UiTheme.MakePanel(_root.transform, "Overlay", new Color(0f, 0f, 0f, 0.65f));
            UiTheme.Stretch(overlay.rectTransform);
            _overlay = overlay.gameObject;

            var card = UiTheme.MakePanel(overlay.transform, "Card", UiTheme.Panel);
            var rt = card.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(560f, 420f);

            UiTheme.MakeText(card.transform, title, 28, FontStyle.Bold, TextAnchor.UpperCenter, UiTheme.Accent)
                .rectTransform.anchoredPosition = new Vector2(0f, -28f);

            var close = UiTheme.MakeButton(card.transform, "TUTUP", UiTheme.AccentDark, new Vector2(140f, 42f));
            close.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -180f);
            close.onClick.AddListener(ClearOverlay);

            content = card.transform;
            return _overlay;
        }

        void ShowProfile()
        {
            MakeOverlayCard("PROFILE", out var content);
            var p = GameManager.Instance.Profile;
            var sb = new StringBuilder();
            sb.AppendLine($"Username: {p.username}");
            sb.AppendLine($"Total Skor: {p.totalScore}");
            sb.AppendLine($"Lapor Benar: {p.correctReports}");
            sb.AppendLine($"Inisiatif Benar: {p.correctInitiatives}");
            sb.AppendLine($"Salah Pilih: {p.wrongChoices}");
            sb.AppendLine($"Timeout: {p.timeouts}");
            sb.AppendLine($"Sesi Dimainkan: {p.sessionsPlayed}");

            var body = UiTheme.MakeText(content, sb.ToString(), 20, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Text);
            UiTheme.SetAnchored(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220f, -40f), new Vector2(220f, 130f));
        }

        void ShowLeaderboard()
        {
            MakeOverlayCard("LEADERBOARD", out var content);
            var svc = GameManager.Instance.Leaderboard;
            var byScore = svc.GetByScore();
            var byCare = svc.GetByCareActions();
            var need = svc.GetNeedsImprovement();

            var sb = new StringBuilder();
            sb.AppendLine("=== Top Skor ===");
            AppendBoard(sb, byScore, e => $"{e.totalScore} pts");
            sb.AppendLine();
            sb.AppendLine("=== Top Lapor & Inisiatif ===");
            AppendBoard(sb, byCare, e => $"{e.correctReports + e.correctInitiatives} aksi");
            sb.AppendLine();
            sb.AppendLine("=== Perlu Lebih Peka ===");
            AppendBoard(sb, need, e => $"{e.totalScore} pts");

            var body = UiTheme.MakeText(content, sb.ToString(), 16, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Text);
            UiTheme.SetAnchored(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-240f, -150f), new Vector2(240f, 140f));
        }

        static void AppendBoard(StringBuilder sb, System.Collections.Generic.List<Data.LeaderboardEntry> list,
            Func<Data.LeaderboardEntry, string> metric)
        {
            if (list.Count == 0)
            {
                sb.AppendLine("(masih kosong)");
                return;
            }

            int max = Mathf.Min(5, list.Count);
            for (int i = 0; i < max; i++)
            {
                var e = list[i];
                sb.AppendLine($"{i + 1}. {e.username} — {metric(e)}");
            }
        }

        void ShowSettings()
        {
            MakeOverlayCard("SETTINGS", out var content);
            var settings = GameManager.Instance.Settings;

            var volLabel = UiTheme.MakeText(content, $"Volume: {settings.masterVolume:0.00}", 18,
                FontStyle.Normal, TextAnchor.MiddleCenter);
            volLabel.rectTransform.anchoredPosition = new Vector2(0f, 80f);

            CreateAdjust(content, "VOL -", new Vector2(-120f, 30f), () =>
            {
                settings.masterVolume = Mathf.Clamp01(settings.masterVolume - 0.1f);
                GameManager.Instance.SaveSettings();
                volLabel.text = $"Volume: {settings.masterVolume:0.00}";
            });
            CreateAdjust(content, "VOL +", new Vector2(120f, 30f), () =>
            {
                settings.masterVolume = Mathf.Clamp01(settings.masterVolume + 0.1f);
                GameManager.Instance.SaveSettings();
                volLabel.text = $"Volume: {settings.masterVolume:0.00}";
            });

            var sensLabel = UiTheme.MakeText(content, $"Sens Kamera: {settings.mouseSensitivity:0.0}", 18,
                FontStyle.Normal, TextAnchor.MiddleCenter);
            sensLabel.rectTransform.anchoredPosition = new Vector2(0f, -20f);

            CreateAdjust(content, "SENS -", new Vector2(-120f, -70f), () =>
            {
                settings.mouseSensitivity = Mathf.Clamp(settings.mouseSensitivity - 0.5f, 0.5f, 8f);
                GameManager.Instance.SaveSettings();
                sensLabel.text = $"Sens Kamera: {settings.mouseSensitivity:0.0}";
            });
            CreateAdjust(content, "SENS +", new Vector2(120f, -70f), () =>
            {
                settings.mouseSensitivity = Mathf.Clamp(settings.mouseSensitivity + 0.5f, 0.5f, 8f);
                GameManager.Instance.SaveSettings();
                sensLabel.text = $"Sens Kamera: {settings.mouseSensitivity:0.0}";
            });
        }

        void CreateAdjust(Transform parent, string label, Vector2 pos, Action action)
        {
            var btn = UiTheme.MakeButton(parent, label, UiTheme.Teal, new Vector2(120f, 40f));
            btn.GetComponent<RectTransform>().anchoredPosition = pos;
            btn.onClick.AddListener(() => action());
        }

        void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetVisible(bool visible)
        {
            if (_root != null)
                _root.SetActive(visible);
            if (!visible)
                ClearOverlay();
        }
    }
}
