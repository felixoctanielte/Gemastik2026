using System;
using System.Collections;
using System.Collections.Generic;
using PeduliTransit.Data;
using UnityEngine;
using UnityEngine.UI;

namespace PeduliTransit.UI
{

    public class PhoneWhatsAppUI : MonoBehaviour
    {
        GameObject _root;
        GameObject _phone;
        Text _contactName;
        Text _contactStatus;
        Text _timerText;
        Image _timerFill;
        Transform _chatContent;
        Transform _optionContent;
        Action<ReportOption> _onPick;
        Action _onCancel;
        Action _onTimeout;
        Coroutine _timer;
        bool _locked;
        readonly List<GameObject> _spawnedBubbles = new List<GameObject>();
        readonly List<GameObject> _spawnedOptions = new List<GameObject>();

        static readonly Color Bezel = new Color(0.08f, 0.08f, 0.1f, 1f);
        static readonly Color ScreenBg = new Color(0.11f, 0.16f, 0.15f, 1f);
        static readonly Color HeaderGreen = new Color(0.07f, 0.54f, 0.47f, 1f);
        static readonly Color BubbleOut = new Color(0.18f, 0.55f, 0.44f, 1f);
        static readonly Color BubbleIn = new Color(0.22f, 0.25f, 0.28f, 1f);
        static readonly Color OptionBg = new Color(0.14f, 0.18f, 0.2f, 1f);

        public void Build(Transform canvas)
        {
            if (_root != null)
            {
                Destroy(_root);
                _root = null;
            }

            var dim = UiTheme.MakePanel(canvas, "PhoneOverlay", new Color(0f, 0f, 0f, 0.72f));
            UiTheme.Stretch(dim.rectTransform);
            _root = dim.gameObject;
            UiTheme.EnsureFullscreenCanvas(canvas);

            var bezel = UiTheme.MakePanel(dim.transform, "PhoneBezel", Bezel);
            bezel.sprite = UiTheme.RoundedSprite;
            bezel.type = Image.Type.Sliced;
            var brt = bezel.rectTransform;
            ResponsiveUI.FitPhoneBezel(brt);
            _phone = bezel.gameObject;

            var notch = UiTheme.MakePanel(bezel.transform, "Notch", new Color(0.05f, 0.05f, 0.06f, 1f));
            notch.sprite = UiTheme.RoundedSprite;
            notch.type = Image.Type.Sliced;
            var nrt = notch.rectTransform;
            nrt.anchorMin = new Vector2(0.5f, 1f);
            nrt.anchorMax = new Vector2(0.5f, 1f);
            nrt.pivot = new Vector2(0.5f, 1f);
            nrt.sizeDelta = new Vector2(160f, 22f);
            nrt.anchoredPosition = new Vector2(0f, -10f);

            var screen = UiTheme.MakePanel(bezel.transform, "Screen", ScreenBg);
            var srt = screen.rectTransform;
            UiTheme.SetAnchored(srt, Vector2.zero, Vector2.one, new Vector2(14f, 28f), new Vector2(-14f, -28f));

            var status = UiTheme.MakeText(screen.transform, "09:41        5G  🔋", 14, FontStyle.Bold,
                TextAnchor.MiddleCenter, Color.white);
            UiTheme.SetAnchored(status.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(8f, -28f), new Vector2(-8f, -4f));

            var header = UiTheme.MakePanel(screen.transform, "Header", HeaderGreen);
            UiTheme.SetAnchored(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -78f), new Vector2(0f, -28f));

            var avatar = UiTheme.MakePanel(header.transform, "Avatar", new Color(0.85f, 0.9f, 0.88f, 1f));
            avatar.sprite = UiTheme.RoundedSprite;
            avatar.type = Image.Type.Sliced;
            var art = avatar.rectTransform;
            art.anchorMin = art.anchorMax = new Vector2(0f, 0.5f);
            art.pivot = new Vector2(0f, 0.5f);
            art.sizeDelta = new Vector2(36f, 36f);
            art.anchoredPosition = new Vector2(12f, 0f);

            _contactName = UiTheme.MakeText(header.transform, "Petugas", 18, FontStyle.Bold, TextAnchor.MiddleLeft,
                Color.white);
            UiTheme.SetAnchored(_contactName.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f),
                new Vector2(58f, 0f), new Vector2(-10f, -4f));

            _contactStatus = UiTheme.MakeText(header.transform, "online", 13, FontStyle.Normal, TextAnchor.MiddleLeft,
                new Color(0.85f, 0.95f, 0.9f, 1f));
            UiTheme.SetAnchored(_contactStatus.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.5f),
                new Vector2(58f, 4f), new Vector2(-10f, 0f));

            var timerRow = UiTheme.MakePanel(screen.transform, "TimerRow", new Color(0f, 0f, 0f, 0.25f));
            UiTheme.SetAnchored(timerRow.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -108f), new Vector2(0f, -78f));

            _timerText = UiTheme.MakeText(timerRow.transform, "18s", 14, FontStyle.Bold, TextAnchor.MiddleLeft,
                UiTheme.Accent);
            UiTheme.SetAnchored(_timerText.rectTransform, new Vector2(0f, 0f), new Vector2(0.3f, 1f),
                new Vector2(10f, 0f), new Vector2(0f, 0f));

            var barBg = UiTheme.MakePanel(timerRow.transform, "BarBg", new Color(1f, 1f, 1f, 0.15f));
            UiTheme.SetAnchored(barBg.rectTransform, new Vector2(0.28f, 0.35f), new Vector2(0.96f, 0.65f),
                Vector2.zero, Vector2.zero);
            _timerFill = UiTheme.MakePanel(barBg.transform, "Fill", HeaderGreen);
            UiTheme.Stretch(_timerFill.rectTransform);
            _timerFill.type = Image.Type.Filled;
            _timerFill.fillMethod = Image.FillMethod.Horizontal;
            _timerFill.fillAmount = 1f;

            var chatArea = new GameObject("ChatArea", typeof(RectTransform), typeof(Image), typeof(Mask));
            chatArea.transform.SetParent(screen.transform, false);
            var chatImg = chatArea.GetComponent<Image>();
            chatImg.color = new Color(0.86f, 0.83f, 0.76f, 0.35f);
            chatArea.GetComponent<Mask>().showMaskGraphic = true;
            UiTheme.SetAnchored(chatArea.GetComponent<RectTransform>(), new Vector2(0f, 0.28f), new Vector2(1f, 1f),
                new Vector2(6f, 0f), new Vector2(-6f, -112f));

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            content.transform.SetParent(chatArea.transform, false);
            var crt = content.GetComponent<RectTransform>();
            UiTheme.Stretch(crt, 8f);
            crt.pivot = new Vector2(0.5f, 1f);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 8f;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _chatContent = content.transform;

            var optTitle = UiTheme.MakeText(screen.transform, "Pilih pesan siap kirim:", 14, FontStyle.Bold,
                TextAnchor.MiddleLeft, Color.white);
            UiTheme.SetAnchored(optTitle.rectTransform, new Vector2(0f, 0.28f), new Vector2(1f, 0.28f),
                new Vector2(12f, -8f), new Vector2(-12f, 18f));

            var optArea = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup));
            optArea.transform.SetParent(screen.transform, false);
            UiTheme.SetAnchored(optArea.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.28f),
                new Vector2(8f, 48f), new Vector2(-8f, -22f));
            var ovlg = optArea.GetComponent<VerticalLayoutGroup>();
            ovlg.spacing = 6f;
            ovlg.childControlHeight = true;
            ovlg.childControlWidth = true;
            ovlg.childForceExpandHeight = false;
            ovlg.childForceExpandWidth = true;
            _optionContent = optArea.transform;

            var cancel = UiTheme.MakeButton(screen.transform, "Tutup", UiTheme.Danger, new Vector2(120f, 34f));
            var cancelRt = cancel.GetComponent<RectTransform>();
            cancelRt.anchorMin = cancelRt.anchorMax = new Vector2(0.5f, 0f);
            cancelRt.anchoredPosition = new Vector2(0f, 14f);
            cancel.onClick.AddListener(() =>
            {
                if (_locked) return;
                _locked = true;
                PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.PhoneCancel);
                Close();
                _onCancel?.Invoke();
            });

            _root.SetActive(false);
        }

        public void Show(string contactName, string status, List<ReportOption> options, float seconds,
            Action<ReportOption> onPick, Action onCancel, Action onTimeout = null)
        {
            _onPick = onPick;
            _onCancel = onCancel;
            _onTimeout = onTimeout;
            _locked = false;
            _contactName.text = contactName;
            _contactStatus.text = status;

            ClearSpawned(_spawnedBubbles);
            ClearSpawned(_spawnedOptions);

            AddBubble("Ketuk salah satu pesan di bawah. Tidak perlu mengetik.", incoming: true);

            if (options != null)
            {
                foreach (var opt in options)
                    AddOptionButton(opt);
            }

            _root.SetActive(true);
            PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.PhoneOpen);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_timer != null)
                StopCoroutine(_timer);
            _timer = StartCoroutine(TimerRoutine(seconds));
        }

        void AddBubble(string message, bool incoming)
        {
            var row = new GameObject(incoming ? "In" : "Out", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_chatContent, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = incoming ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            h.padding = new RectOffset(4, 4, 0, 0);
            h.childForceExpandWidth = true;

            var pad = new GameObject("Pad", typeof(RectTransform), typeof(LayoutElement));
            pad.transform.SetParent(row.transform, false);
            pad.GetComponent<LayoutElement>().flexibleWidth = incoming ? 0.05f : 0.25f;

            var bubble = UiTheme.MakePanel(row.transform, "Bubble", incoming ? BubbleIn : BubbleOut);
            bubble.sprite = UiTheme.RoundedSprite;
            bubble.type = Image.Type.Sliced;
            var le = bubble.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.flexibleWidth = 0.7f;

            var text = UiTheme.MakeText(bubble.transform, message, 14, FontStyle.Normal,
                incoming ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, Color.white);
            UiTheme.Stretch(text.rectTransform, 8f);

            var pad2 = new GameObject("Pad2", typeof(RectTransform), typeof(LayoutElement));
            pad2.transform.SetParent(row.transform, false);
            pad2.GetComponent<LayoutElement>().flexibleWidth = incoming ? 0.25f : 0.05f;

            _spawnedBubbles.Add(row);
        }

        void AddOptionButton(ReportOption opt)
        {
            var btn = UiTheme.MakeButton(_optionContent, opt.buttonLabel, OptionBg, new Vector2(0f, 40f));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 40f;
            le.flexibleWidth = 1f;

            var label = btn.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.fontSize = 13;
                label.alignment = TextAnchor.MiddleLeft;
                UiTheme.SetAnchored(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 2f),
                    new Vector2(-12f, -2f));
            }

            var captured = opt;
            btn.onClick.AddListener(() => Pick(captured));
            _spawnedOptions.Add(btn.gameObject);
        }

        void Pick(ReportOption opt)
        {
            if (_locked || opt == null)
                return;
            _locked = true;

            AddBubble(opt.chatPreview, incoming: false);
            AddBubble("Pesan terkirim ✓✓", incoming: true);
            PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.PhoneSend);

            if (_timer != null)
            {
                StopCoroutine(_timer);
                _timer = null;
            }

            StartCoroutine(DelayClose(() =>
            {
                Close();
                _onPick?.Invoke(opt);
            }));
        }

        IEnumerator DelayClose(Action done)
        {
            yield return new WaitForSecondsRealtime(0.55f);
            done?.Invoke();
        }

        IEnumerator TimerRoutine(float seconds)
        {
            float left = seconds;
            while (left > 0f && !_locked)
            {
                left -= Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(left / seconds);
                _timerText.text = $"{Mathf.Max(0f, left):0.0}s";
                _timerFill.fillAmount = t;
                _timerFill.color = t < 0.3f ? UiTheme.Danger : HeaderGreen;
                yield return null;
            }

            if (!_locked)
            {
                _locked = true;
                Close();
                if (_onTimeout != null)
                    _onTimeout.Invoke();
                else
                    _onCancel?.Invoke();
            }
        }

        public void Hide()
        {
            _locked = true;
            _onPick = null;
            _onCancel = null;
            _onTimeout = null;
            Close();
        }

        void Close()
        {
            if (_timer != null)
            {
                StopCoroutine(_timer);
                _timer = null;
            }

            if (_root != null)
                _root.SetActive(false);
        }

        static void ClearSpawned(List<GameObject> list)
        {
            foreach (var go in list)
            {
                if (go == null) continue;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEngine.Object.DestroyImmediate(go);
                else
#endif
                    go.SetActive(false);
                Destroy(go);
            }

            list.Clear();
        }

        public bool IsOpen => _root != null && _root.activeSelf;
    }
}
