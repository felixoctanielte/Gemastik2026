using System;
using System.Collections;
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
        GameObject _resultRoot;

        Text _scoreText;
        Text _modeText;
        Text _storyTitle;
        Text _storyBody;
        Text _promptText;
        Text _timerText;
        Image _timerFill;
        Text _resultBody;

        Action _onStoryContinue;
        Action<DecisionOutcome> _onDecision;
        Coroutine _timerRoutine;
        bool _decisionLocked;

        public void Init(Transform canvas)
        {
            _canvas = canvas;
            BuildHud();
            BuildStory();
            BuildPopup();
            BuildResult();
            HideAll();
        }

        void BuildHud()
        {
            var root = UiTheme.MakePanel(_canvas, "Hud", new Color(0f, 0f, 0f, 0f));
            UiTheme.Stretch(root.rectTransform);
            _hudRoot = root.gameObject;

            _modeText = UiTheme.MakeText(root.transform, "MODE", 20, FontStyle.Bold, TextAnchor.UpperLeft, UiTheme.Teal);
            UiTheme.SetAnchored(_modeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -50f), new Vector2(300f, -16f));

            _scoreText = UiTheme.MakeText(root.transform, "Skor: 0", 22, FontStyle.Bold, TextAnchor.UpperRight,
                UiTheme.Accent);
            UiTheme.SetAnchored(_scoreText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-220f, -50f), new Vector2(-20f, -16f));

            var hint = UiTheme.MakeText(root.transform, "WASD gerak | Mouse lihat | ESC lepas kursor", 16,
                FontStyle.Normal, TextAnchor.LowerCenter, UiTheme.Muted);
            UiTheme.SetAnchored(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-280f, 12f), new Vector2(280f, 40f));
        }

        void BuildStory()
        {
            var dim = UiTheme.MakePanel(_canvas, "Story", new Color(0f, 0f, 0f, 0.72f));
            UiTheme.Stretch(dim.rectTransform);
            _storyRoot = dim.gameObject;

            var card = UiTheme.MakePanel(dim.transform, "Card", UiTheme.Panel);
            var rt = card.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(640f, 320f);

            _storyTitle = UiTheme.MakeText(card.transform, "Misi", 26, FontStyle.Bold, TextAnchor.UpperCenter,
                UiTheme.Accent);
            _storyTitle.rectTransform.anchoredPosition = new Vector2(0f, -30f);

            _storyBody = UiTheme.MakeText(card.transform, "", 18, FontStyle.Normal, TextAnchor.UpperLeft, UiTheme.Text);
            UiTheme.SetAnchored(_storyBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-280f, -40f), new Vector2(280f, 100f));

            var cont = UiTheme.MakeButton(card.transform, "LANJUT", UiTheme.Accent, new Vector2(180f, 46f));
            cont.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -120f);
            cont.onClick.AddListener(() =>
            {
                _storyRoot.SetActive(false);
                _onStoryContinue?.Invoke();
            });
        }

        void BuildPopup()
        {
            var dim = UiTheme.MakePanel(_canvas, "Popup", new Color(0f, 0f, 0f, 0.75f));
            UiTheme.Stretch(dim.rectTransform);
            _popupRoot = dim.gameObject;

            var card = UiTheme.MakePanel(dim.transform, "Card", UiTheme.Panel);
            var rt = card.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620f, 280f);

            _promptText = UiTheme.MakeText(card.transform, "Pertanyaan?", 22, FontStyle.Bold, TextAnchor.UpperCenter);
            UiTheme.SetAnchored(_promptText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-280f, -90f), new Vector2(280f, -24f));

            _timerText = UiTheme.MakeText(card.transform, "10.0", 20, FontStyle.Bold, TextAnchor.MiddleCenter,
                UiTheme.Accent);
            _timerText.rectTransform.anchoredPosition = new Vector2(0f, 10f);

            var barBg = UiTheme.MakePanel(card.transform, "TimerBg", new Color(1f, 1f, 1f, 0.15f));
            barBg.rectTransform.sizeDelta = new Vector2(420f, 14f);
            barBg.rectTransform.anchoredPosition = new Vector2(0f, -20f);

            _timerFill = UiTheme.MakePanel(barBg.transform, "Fill", UiTheme.Teal);
            UiTheme.Stretch(_timerFill.rectTransform);
            _timerFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            _timerFill.type = Image.Type.Filled;
            _timerFill.fillMethod = Image.FillMethod.Horizontal;
            _timerFill.fillAmount = 1f;

            var yes = UiTheme.MakeButton(card.transform, "YA", UiTheme.Good, new Vector2(160f, 48f));
            yes.GetComponent<RectTransform>().anchoredPosition = new Vector2(-110f, -90f);
            yes.onClick.AddListener(() => Resolve(DecisionOutcome.Yes));

            var no = UiTheme.MakeButton(card.transform, "TIDAK", UiTheme.Danger, new Vector2(160f, 48f));
            no.GetComponent<RectTransform>().anchoredPosition = new Vector2(110f, -90f);
            no.onClick.AddListener(() => Resolve(DecisionOutcome.No));
        }

        void BuildResult()
        {
            var dim = UiTheme.MakePanel(_canvas, "Result", new Color(0f, 0f, 0f, 0.8f));
            UiTheme.Stretch(dim.rectTransform);
            _resultRoot = dim.gameObject;

            var card = UiTheme.MakePanel(dim.transform, "Card", UiTheme.Panel);
            card.rectTransform.sizeDelta = new Vector2(560f, 360f);
            card.rectTransform.anchorMin = card.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            UiTheme.MakeText(card.transform, "SESI SELESAI", 28, FontStyle.Bold, TextAnchor.UpperCenter, UiTheme.Accent)
                .rectTransform.anchoredPosition = new Vector2(0f, -32f);

            _resultBody = UiTheme.MakeText(card.transform, "", 18, FontStyle.Normal, TextAnchor.UpperLeft);
            UiTheme.SetAnchored(_resultBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-220f, -40f), new Vector2(220f, 110f));

            var hub = UiTheme.MakeButton(card.transform, "KEMBALI KE HUB", UiTheme.Teal, new Vector2(240f, 48f));
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
            _onStoryContinue = onContinue;
            _storyTitle.text = title;
            _storyBody.text = body;
            _storyRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowDecision(string prompt, float seconds, Action<DecisionOutcome> onDecision)
        {
            _onDecision = onDecision;
            _decisionLocked = false;
            _promptText.text = prompt;
            _popupRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_timerRoutine != null)
                StopCoroutine(_timerRoutine);
            _timerRoutine = StartCoroutine(TimerRoutine(seconds));
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
            if (_resultRoot) _resultRoot.SetActive(false);
        }
    }
}
