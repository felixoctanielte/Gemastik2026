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
        public Sprite mascot;

        public void Build(Transform canvas, Action<TransportMode> onPlay)
        {
            _onPlay = onPlay;

            if (mascot == null)
                mascot = UiAssets.EduPortrait;

            UiTheme.EnsureFullscreenCanvas(canvas);

            var root = UiTheme.MakePanel(canvas, "HubRoot", new Color(0.05f, 0.12f, 0.14f, 0.55f));
            UiTheme.Stretch(root.rectTransform);
            _root = root.gameObject;

            var brand = UiTheme.MakeText(root.transform, "PEDULI TRANSIT", 40, FontStyle.Bold,
                TextAnchor.UpperLeft, UiTheme.Accent);
            UiTheme.SetAnchored(brand.rectTransform, new Vector2(0.03f, 0.88f), new Vector2(0.55f, 0.98f),
                Vector2.zero, Vector2.zero);

            var sub = UiTheme.MakeText(root.transform,
                "Pilih moda. Lapor lewat WhatsApp (ketuk pesan). Salah lapor −10. Kursi prioritas: tegur atau WA.",
                17, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Muted);
            UiTheme.SetAnchored(sub.rectTransform, new Vector2(0.03f, 0.78f), new Vector2(0.58f, 0.88f),
                Vector2.zero, Vector2.zero);
            sub.horizontalOverflow = HorizontalWrapMode.Wrap;

            if (mascot != null)
            {
                var portrait = UiTheme.MakePortrait(root.transform, mascot, new Vector2(240f, 310f));
                var prt = portrait.rectTransform;
                prt.anchorMin = new Vector2(0.72f, 0.25f);
                prt.anchorMax = new Vector2(0.97f, 0.85f);
                prt.offsetMin = Vector2.zero;
                prt.offsetMax = Vector2.zero;
            }

            var modeRow = new GameObject("ModeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            modeRow.transform.SetParent(root.transform, false);
            var mrt = modeRow.GetComponent<RectTransform>();
            UiTheme.SetAnchored(mrt, new Vector2(0.05f, 0.42f), new Vector2(0.68f, 0.62f),
                Vector2.zero, Vector2.zero);
            var hlg = modeRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(8, 8, 8, 8);

            CreateModeButton(modeRow.transform, "KRL", TransportMode.Krl);
            CreateModeButton(modeRow.transform, "BUS", TransportMode.Bus);
            CreateModeButton(modeRow.transform, "ANGKUTAN\nUMUM", TransportMode.AngkutanUmum);

            var sideRow = new GameObject("SideRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            sideRow.transform.SetParent(root.transform, false);
            var srt = sideRow.GetComponent<RectTransform>();
            UiTheme.SetAnchored(srt, new Vector2(0.05f, 0.28f), new Vector2(0.68f, 0.40f),
                Vector2.zero, Vector2.zero);
            var sh = sideRow.GetComponent<HorizontalLayoutGroup>();
            sh.spacing = 12f;
            sh.childAlignment = TextAnchor.MiddleCenter;
            sh.childForceExpandWidth = true;
            sh.childForceExpandHeight = true;

            CreateSideButton(sideRow.transform, "SETTINGS", ShowSettings);
            CreateSideButton(sideRow.transform, "PROFILE", ShowProfile);
            CreateSideButton(sideRow.transform, "LEADERBOARD", ShowLeaderboard);
            CreateSideButton(sideRow.transform, "EXIT", ExitGame, UiTheme.Danger);
        }

        void CreateModeButton(Transform parent, string label, TransportMode mode)
        {
            var btn = UiTheme.MakeButton(parent, label, UiTheme.Teal, new Vector2(0f, 0f));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minHeight = 90f;
            le.preferredHeight = 110f;
            btn.onClick.AddListener(() => _onPlay?.Invoke(mode));
        }

        void CreateSideButton(Transform parent, string label, Action action, Color? color = null)
        {
            var btn = UiTheme.MakeButton(parent, label, color ?? UiTheme.AccentDark, new Vector2(0f, 0f));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minHeight = 40f;
            le.preferredHeight = 46f;
            var text = btn.GetComponentInChildren<Text>();
            if (text != null) text.fontSize = 16;
            btn.onClick.AddListener(() => action());
        }

        void ClearOverlay()
        {
            if (_overlay != null)
                Destroy(_overlay);
            _overlay = null;
        }

        GameObject MakeOverlayCard(string title, out Transform content, float heightFrac = 0.50f)
        {
            ClearOverlay();
            PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);

            var overlay = UiTheme.MakePanel(_root.transform, "Overlay", new Color(0f, 0f, 0f, 0.65f));
            UiTheme.Stretch(overlay.rectTransform);
            _overlay = overlay.gameObject;

            var card = UiTheme.MakeResponsiveGlassCard(overlay.transform, "Card",
                0.46f, heightFrac, 460f, 820f, 340f, 700f);

            UiTheme.MakeText(card.transform, title, 26, FontStyle.Bold, TextAnchor.UpperCenter, UiTheme.Accent)
                .rectTransform.anchoredPosition = new Vector2(0f, -24f);

            var close = UiTheme.MakeButton(card.transform, "TUTUP", UiTheme.AccentDark, new Vector2(140f, 40f));
            var crt = close.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(0f, 18f);
            close.onClick.AddListener(() =>
            {
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiBack);
                ClearOverlay();
            });

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

            var body = UiTheme.MakeText(content, sb.ToString(), 18, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Text);
            UiTheme.SetAnchored(body.rectTransform, new Vector2(0.1f, 0.20f), new Vector2(0.9f, 0.78f),
                Vector2.zero, Vector2.zero);
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

            var body = UiTheme.MakeText(content, sb.ToString(), 15, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Text);
            UiTheme.SetAnchored(body.rectTransform, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.78f),
                Vector2.zero, Vector2.zero);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
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
            MakeOverlayCard("SETTINGS", out var content, heightFrac: 0.72f);
            var settings = GameManager.Instance.Settings;
            float y = 180f;

            var nameInput = UiTheme.MakeInput(content, "Username baru...", new Vector2(300f, 42f));
            nameInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, y);
            nameInput.text = GameManager.Instance.Profile.username ?? "";

            var renameBtn = UiTheme.MakeButton(content, "GANTI NAMA", UiTheme.Accent, new Vector2(150f, 42f));
            renameBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, y);
            renameBtn.onClick.AddListener(() =>
            {
                GameManager.Instance.RenameUser(nameInput.text);
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiConfirm);
            });

            y -= 58f;
            var muteLabel = UiTheme.MakeText(content, settings.muteAll ? "Mute: ON" : "Mute: OFF", 17,
                FontStyle.Bold, TextAnchor.MiddleCenter);
            muteLabel.rectTransform.anchoredPosition = new Vector2(0f, y);
            CreateAdjust(content, "MUTE", new Vector2(180f, y), () =>
            {
                settings.muteAll = !settings.muteAll;
                GameManager.Instance.SaveSettings();
                muteLabel.text = settings.muteAll ? "Mute: ON" : "Mute: OFF";
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);
            });

            y -= 50f;
            CreateVolumeRow(content, "Master", () => settings.masterVolume, v => settings.masterVolume = v, y);
            y -= 48f;
            CreateVolumeRow(content, "BGM / Musik", () => settings.bgmVolume, v => settings.bgmVolume = v, y);
            y -= 48f;
            CreateVolumeRow(content, "SFX / Suara game", () => settings.sfxVolume, v => settings.sfxVolume = v, y);
            y -= 48f;
            CreateVolumeRow(content, "UI / Klik tombol", () => settings.uiVolume, v => settings.uiVolume = v, y);

            y -= 56f;
            var sensLabel = UiTheme.MakeText(content, $"Sens Kamera: {settings.mouseSensitivity:0.0}", 17,
                FontStyle.Normal, TextAnchor.MiddleCenter);
            sensLabel.rectTransform.anchoredPosition = new Vector2(0f, y);
            CreateAdjust(content, "-", new Vector2(-170f, y), () =>
            {
                settings.mouseSensitivity = Mathf.Clamp(settings.mouseSensitivity - 0.5f, 0.5f, 8f);
                GameManager.Instance.SaveSettings();
                sensLabel.text = $"Sens Kamera: {settings.mouseSensitivity:0.0}";
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);
            });
            CreateAdjust(content, "+", new Vector2(170f, y), () =>
            {
                settings.mouseSensitivity = Mathf.Clamp(settings.mouseSensitivity + 0.5f, 0.5f, 8f);
                GameManager.Instance.SaveSettings();
                sensLabel.text = $"Sens Kamera: {settings.mouseSensitivity:0.0}";
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);
            });

            y -= 54f;
            var tip = UiTheme.MakeText(content,
                "Isi clip audio ke Resources/Audio/Bgm & Sfx (nama = enum), atau drag ke AudioLibrary asset.",
                13, FontStyle.Italic, TextAnchor.MiddleCenter, UiTheme.Muted);
            tip.rectTransform.anchoredPosition = new Vector2(0f, y);
            tip.rectTransform.sizeDelta = new Vector2(520f, 40f);
            tip.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        void CreateVolumeRow(Transform parent, string title, Func<float> getValue, Action<float> setValue, float y)
        {
            var label = UiTheme.MakeText(parent, $"{title}: {getValue():0.00}", 17,
                FontStyle.Normal, TextAnchor.MiddleCenter);
            label.rectTransform.anchoredPosition = new Vector2(0f, y);

            CreateAdjust(parent, "-", new Vector2(-170f, y), () =>
            {
                float v = Mathf.Clamp01(getValue() - 0.1f);
                setValue(v);
                GameManager.Instance.SaveSettings();
                label.text = $"{title}: {v:0.00}";
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);
            });
            CreateAdjust(parent, "+", new Vector2(170f, y), () =>
            {
                float v = Mathf.Clamp01(getValue() + 0.1f);
                setValue(v);
                GameManager.Instance.SaveSettings();
                label.text = $"{title}: {v:0.00}";
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);
            });
        }

        void CreateAdjust(Transform parent, string label, Vector2 pos, Action action)
        {
            var btn = UiTheme.MakeButton(parent, label, UiTheme.Teal, new Vector2(90f, 36f));
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
