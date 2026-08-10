using System;
using System.Collections;
using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.Data;
using UnityEngine;
using UnityEngine.UI;

namespace PeduliTransit.UI
{
    public class GameplayUI : MonoBehaviour
    {
        Transform _canvas;
        GameObject _hudRoot;
        GameObject _storyRoot;
        GameObject _popupRoot;
        GameObject _choiceRoot;
        GameObject _resultRoot;

        Text _scoreText;
        Text _modeText;
        Text _storyTitle;
        Text _storyBody;
        Image _storyPortrait;
        Text _promptText;
        Text _timerText;
        Image _timerFill;
        Text _resultBody;
        Text _choicePrompt;

        public Sprite defaultSpeakerPortrait;
        public Font customFont;

        PhoneWhatsAppUI _phone;

        Action _onStoryContinue;
        Action<DecisionOutcome> _onDecision;
        Action<bool> _onPriorityChoice;
        Coroutine _timerRoutine;
        bool _decisionLocked;

        public void Init(Transform canvas)
        {
            _canvas = canvas;
            UiTheme.EnsureFullscreenCanvas(canvas);

            if (defaultSpeakerPortrait == null)
                defaultSpeakerPortrait = UiAssets.EduPortrait;

            BuildHud();
            BuildStory();
            BuildPopup();
            BuildPriorityChoice();
            BuildResult();

            _phone = gameObject.GetComponent<PhoneWhatsAppUI>();
            if (_phone == null)
                _phone = gameObject.AddComponent<PhoneWhatsAppUI>();
            _phone.Build(canvas);

            HideAll();
        }

        void BuildHud()
        {
            var root = UiTheme.MakePanel(_canvas, "Hud", new Color(0f, 0f, 0f, 0f));
            UiTheme.Stretch(root.rectTransform);
            root.raycastTarget = false;
            _hudRoot = root.gameObject;

            _modeText = UiTheme.MakeText(root.transform, "MODE", 20, FontStyle.Bold, TextAnchor.UpperLeft, UiTheme.Teal);
            _modeText.raycastTarget = false;
            UiTheme.SetAnchored(_modeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -50f), new Vector2(300f, -16f));

            _scoreText = UiTheme.MakeText(root.transform, "Skor: 0", 22, FontStyle.Bold, TextAnchor.UpperRight,
                UiTheme.Accent);
            _scoreText.raycastTarget = false;
            UiTheme.SetAnchored(_scoreText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-220f, -50f), new Vector2(-20f, -16f));

            var hintBar = UiTheme.MakePanel(root.transform, "CameraHint", new Color(0.04f, 0.08f, 0.1f, 0.78f));
            hintBar.raycastTarget = false;
            UiTheme.SetAnchored(hintBar.rectTransform, new Vector2(0.08f, 0f), new Vector2(0.92f, 0f),
                new Vector2(0f, 12f), new Vector2(0f, 54f));

            var hint = UiTheme.MakeText(hintBar.transform,
                "Kamera: tahan KLIK KANAN + geser = putar | WASD = geser | Scroll = zoom | Q/E = naik/turun", 16,
                FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.78f, 0.35f, 1f));
            hint.raycastTarget = false;
            UiTheme.Stretch(hint.rectTransform);
            hint.rectTransform.offsetMin = new Vector2(10f, 4f);
            hint.rectTransform.offsetMax = new Vector2(-10f, -4f);

            var exit = UiTheme.MakeButton(root.transform, "HUB", UiTheme.Danger, new Vector2(100f, 40f));
            var ert = exit.GetComponent<RectTransform>();
            ert.anchorMin = ert.anchorMax = new Vector2(0f, 1f);
            ert.pivot = new Vector2(0f, 1f);
            ert.anchoredPosition = new Vector2(20f, -70f);
            exit.onClick.AddListener(() =>
            {
                PeduliTransit.Bootstrap.GameBootstrap.Instance?.ReturnToHubFromResult();
            });
        }

        void BuildStory()
        {
            var dim = UiTheme.MakePanel(_canvas, "Story", new Color(0f, 0f, 0f, 0.45f));
            UiTheme.Stretch(dim.rectTransform);
            _storyRoot = dim.gameObject;

            Sprite roundedSprite = UiTheme.RoundedSprite;

            var borderGO = new GameObject("CardGlow", typeof(RectTransform), typeof(Image));
            borderGO.transform.SetParent(dim.transform, false);
            var borderImg = borderGO.GetComponent<Image>();
            borderImg.sprite = roundedSprite;
            borderImg.type = Image.Type.Sliced;
            borderImg.raycastTarget = false;
            borderImg.color = new Color(UiTheme.Accent.r, UiTheme.Accent.g, UiTheme.Accent.b, 0.28f);

            var cardGO = new GameObject("Card", typeof(RectTransform), typeof(Image));
            cardGO.transform.SetParent(dim.transform, false);
            var cardImg = cardGO.GetComponent<Image>();
            cardImg.sprite = roundedSprite;
            cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(0.04f, 0.08f, 0.10f, 0.66f);

            var brt = borderGO.GetComponent<RectTransform>();
            var rt = cardGO.GetComponent<RectTransform>();
            ResponsiveUI.FitBottomStoryCard(brt, rt);

            var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGO.transform.SetParent(cardGO.transform, false);
            _storyPortrait = portraitGO.GetComponent<Image>();
            _storyPortrait.preserveAspect = true;
            _storyPortrait.raycastTarget = false;
            _storyPortrait.sprite = defaultSpeakerPortrait;
            ResponsiveUI.FitPortraitForStory(_storyPortrait.rectTransform, rt);

            var nameTagGO = new GameObject("NameTag", typeof(RectTransform), typeof(Image));
            nameTagGO.transform.SetParent(cardGO.transform, false);
            var nameTagImg = nameTagGO.GetComponent<Image>();
            nameTagImg.sprite = roundedSprite;
            nameTagImg.type = Image.Type.Sliced;
            nameTagImg.color = UiTheme.Accent;
            var ntrt = nameTagGO.GetComponent<RectTransform>();
            ntrt.anchorMin = new Vector2(0f, 1f);
            ntrt.anchorMax = new Vector2(0f, 1f);
            ntrt.pivot = new Vector2(0f, 1f);
            ntrt.sizeDelta = new Vector2(300f, 36f);
            ntrt.anchoredPosition = new Vector2(250f, -16f);

            _storyTitle = UiTheme.MakeText(nameTagGO.transform, "Misi", 18, FontStyle.Bold, TextAnchor.MiddleCenter,
                Color.white);
            UiTheme.Stretch(_storyTitle.rectTransform);
            AddOutline(_storyTitle, new Color(0f, 0f, 0f, 0.5f));
            if (customFont != null) _storyTitle.font = customFont;

            _storyBody = UiTheme.MakeText(cardGO.transform, "", 20, FontStyle.Normal, TextAnchor.UpperLeft,
                new Color(0.96f, 0.97f, 0.98f, 1f));

            UiTheme.SetAnchored(_storyBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(250f, 20f), new Vector2(-20f, -58f));
            _storyBody.lineSpacing = 1.2f;
            _storyBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _storyBody.verticalOverflow = VerticalWrapMode.Overflow;
            AddOutline(_storyBody, new Color(0f, 0f, 0f, 0.55f));
            if (customFont != null) _storyBody.font = customFont;

            var cont = UiTheme.MakeButton(cardGO.transform, "LANJUT", UiTheme.Accent, new Vector2(150f, 44f));
            var crt = cont.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(1f, 0f);
            crt.anchorMax = new Vector2(1f, 0f);
            crt.pivot = new Vector2(1f, 0f);
            crt.anchoredPosition = new Vector2(-18f, 14f);
            cont.onClick.AddListener(() =>
            {
                _storyRoot.SetActive(false);
                _onStoryContinue?.Invoke();
            });
        }

        static void AddOutline(Text text, Color color)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1.2f, -1.2f);
        }

        void BuildPopup()
        {
            var dim = UiTheme.MakePanel(_canvas, "Popup", new Color(0f, 0f, 0f, 0.75f));
            UiTheme.Stretch(dim.rectTransform);
            _popupRoot = dim.gameObject;

            var card = UiTheme.MakeResponsiveGlassCard(dim.transform, "Card",
                0.42f, 0.30f, 420f, 720f, 240f, 340f);

            _promptText = UiTheme.MakeText(card.transform, "Pertanyaan?", 20, FontStyle.Bold, TextAnchor.UpperCenter);
            UiTheme.SetAnchored(_promptText.rectTransform, new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.92f),
                Vector2.zero, Vector2.zero);
            _promptText.horizontalOverflow = HorizontalWrapMode.Wrap;

            _timerText = UiTheme.MakeText(card.transform, "10.0", 18, FontStyle.Bold, TextAnchor.MiddleCenter,
                UiTheme.Accent);
            _timerText.rectTransform.anchoredPosition = new Vector2(0f, 8f);

            var barBg = UiTheme.MakePanel(card.transform, "TimerBg", new Color(1f, 1f, 1f, 0.15f));
            barBg.rectTransform.sizeDelta = new Vector2(360f, 12f);
            barBg.rectTransform.anchoredPosition = new Vector2(0f, -18f);

            _timerFill = UiTheme.MakePanel(barBg.transform, "Fill", UiTheme.Teal);
            UiTheme.Stretch(_timerFill.rectTransform);
            _timerFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            _timerFill.type = Image.Type.Filled;
            _timerFill.fillMethod = Image.FillMethod.Horizontal;
            _timerFill.fillAmount = 1f;

            var yes = UiTheme.MakeButton(card.transform, "YA", UiTheme.Good, new Vector2(140f, 44f));
            yes.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100f, -85f);
            yes.onClick.AddListener(() => Resolve(DecisionOutcome.Yes));

            var no = UiTheme.MakeButton(card.transform, "TIDAK", UiTheme.Danger, new Vector2(140f, 44f));
            no.GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, -85f);
            no.onClick.AddListener(() => Resolve(DecisionOutcome.No));
        }

        void BuildPriorityChoice()
        {
            var dim = UiTheme.MakePanel(_canvas, "PriorityChoice", new Color(0f, 0f, 0f, 0.75f));
            UiTheme.Stretch(dim.rectTransform);
            _choiceRoot = dim.gameObject;

            var card = UiTheme.MakeResponsiveGlassCard(dim.transform, "Card",
                0.44f, 0.32f, 440f, 760f, 260f, 360f);

            _choicePrompt = UiTheme.MakeText(card.transform, "Pilih tindakan", 20, FontStyle.Bold,
                TextAnchor.UpperCenter);
            UiTheme.SetAnchored(_choicePrompt.rectTransform, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.92f),
                Vector2.zero, Vector2.zero);
            _choicePrompt.horizontalOverflow = HorizontalWrapMode.Wrap;

            var negur = UiTheme.MakeButton(card.transform, "TEGUR SENDIRI", UiTheme.Accent, new Vector2(200f, 48f));
            negur.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120f, -30f);
            negur.onClick.AddListener(() =>
            {
                _choiceRoot.SetActive(false);
                _onPriorityChoice?.Invoke(true);
            });

            var wa = UiTheme.MakeButton(card.transform, "LAPOR WA", new Color(0.07f, 0.54f, 0.47f, 1f),
                new Vector2(200f, 48f));
            wa.GetComponent<RectTransform>().anchoredPosition = new Vector2(120f, -30f);
            wa.onClick.AddListener(() =>
            {
                _choiceRoot.SetActive(false);
                _onPriorityChoice?.Invoke(false);
            });

            var skip = UiTheme.MakeButton(card.transform, "ABAIKAN", UiTheme.Danger, new Vector2(150f, 38f));
            skip.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
            skip.onClick.AddListener(() =>
            {
                _choiceRoot.SetActive(false);
                _onDecision?.Invoke(DecisionOutcome.No);
            });
        }

        void BuildResult()
        {
            var dim = UiTheme.MakePanel(_canvas, "Result", new Color(0f, 0f, 0f, 0.8f));
            UiTheme.Stretch(dim.rectTransform);
            _resultRoot = dim.gameObject;

            var card = UiTheme.MakeResponsiveGlassCard(dim.transform, "Card",
                0.38f, 0.42f, 400f, 640f, 300f, 440f);

            UiTheme.MakeText(card.transform, "SESI SELESAI", 26, FontStyle.Bold, TextAnchor.UpperCenter, UiTheme.Accent)
                .rectTransform.anchoredPosition = new Vector2(0f, -28f);

            _resultBody = UiTheme.MakeText(card.transform, "", 17, FontStyle.Normal, TextAnchor.UpperLeft);
            UiTheme.SetAnchored(_resultBody.rectTransform, new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.78f),
                Vector2.zero, Vector2.zero);
            _resultBody.horizontalOverflow = HorizontalWrapMode.Wrap;

            var hub = UiTheme.MakeButton(card.transform, "KEMBALI KE HUB", UiTheme.Teal, new Vector2(220f, 44f));
            hub.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f);
            hub.onClick.AddListener(() =>
            {
                _resultRoot.SetActive(false);
                PeduliTransit.Bootstrap.GameBootstrap.Instance?.ReturnToHubFromResult();
            });
        }

        public void ShowHud(string modeLabel, int score)
        {
            HideAll();
            _hudRoot.SetActive(true);
            _modeText.text = modeLabel;
            UpdateScore(score);
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Skor: {score}";
        }

        public void ShowStory(string title, string body, Action onContinue)
        {
            ShowStory(title, body, onContinue, null);
        }

        public void ShowStory(string title, string body, Action onContinue, Sprite portrait)
        {
            _onStoryContinue = onContinue;
            _storyTitle.text = title;
            _storyBody.text = body;
            if (_storyPortrait != null)
                _storyPortrait.sprite = portrait != null ? portrait : defaultSpeakerPortrait;

            _storyRoot.SetActive(true);
            PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.StoryAdvance);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowDecision(string prompt, float seconds, Action<DecisionOutcome> onDecision)
        {
            _onDecision = onDecision;
            _decisionLocked = false;
            _promptText.text = prompt;
            _popupRoot.SetActive(true);
            PeduliTransit.Audio.AudioManager.Instance?.PlayUi(PeduliTransit.Audio.SfxId.UiClick);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_timerRoutine != null)
                StopCoroutine(_timerRoutine);
            _timerRoutine = StartCoroutine(TimerRoutine(seconds));
        }

        public void ShowPriorityActionChoice(string prompt, Action<bool> onChoice, Action<DecisionOutcome> onSkip)
        {
            _onPriorityChoice = onChoice;
            _onDecision = onSkip;
            _choicePrompt.text = prompt;
            _choiceRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowWhatsAppReport(IncidentDefinition incident, Action<DecisionOutcome> onDecision)
        {
            _onDecision = onDecision;
            _decisionLocked = false;

            _phone.Show(
                incident.whatsappContactName,
                incident.contactSubtitle,
                incident.reportOptions,
                incident.timeLimit,
                onPick: opt =>
                {
                    if (_decisionLocked) return;
                    _decisionLocked = true;
                    onDecision?.Invoke(opt != null && opt.isCorrect
                        ? DecisionOutcome.Yes
                        : DecisionOutcome.WrongReport);
                },
                onCancel: () =>
                {
                    if (_decisionLocked) return;
                    _decisionLocked = true;
                    onDecision?.Invoke(DecisionOutcome.Cancel);
                },
                onTimeout: () =>
                {
                    if (_decisionLocked) return;
                    _decisionLocked = true;
                    onDecision?.Invoke(DecisionOutcome.Timeout);
                }
            );
        }

        IEnumerator TimerRoutine(float seconds)
        {
            float left = seconds;
            while (left > 0f && !_decisionLocked)
            {
                left -= Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(left / seconds);
                _timerText.text = $"{Mathf.Max(0f, left):0.0}s";
                _timerFill.fillAmount = t;
                _timerFill.color = t < 0.3f ? UiTheme.Danger : UiTheme.Teal;
                yield return null;
            }

            if (!_decisionLocked)
                Resolve(DecisionOutcome.Timeout);
        }

        void Resolve(DecisionOutcome outcome)
        {
            if (_decisionLocked)
                return;

            _decisionLocked = true;
            if (_timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
            }

            _popupRoot.SetActive(false);
            _onDecision?.Invoke(outcome);
        }

        public void ShowResult(SessionStats session, int totalScore)
        {
            HideAll();
            _resultRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _resultBody.text =
                $"Mode: {session.mode}\n" +
                $"Skor sesi: {session.currentPoints}\n" +
                $"Total skor akun: {totalScore}\n\n" +
                $"Lapor benar: {session.correctReports}\n" +
                $"Inisiatif benar: {session.correctInitiatives}\n" +
                $"Salah: {session.wrongChoices}\n" +
                $"Timeout: {session.timeouts}\n" +
                $"Event selesai: {session.eventsCompleted}";
        }

        public void HideAll()
        {
            if (_hudRoot) _hudRoot.SetActive(false);
            if (_storyRoot) _storyRoot.SetActive(false);
            if (_popupRoot) _popupRoot.SetActive(false);
            if (_choiceRoot) _choiceRoot.SetActive(false);
            if (_resultRoot) _resultRoot.SetActive(false);
            _phone?.Hide();
        }
    }
}
