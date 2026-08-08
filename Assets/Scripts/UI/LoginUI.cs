using System;
using PeduliTransit.Core;
using PeduliTransit.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace PeduliTransit.UI
{
    public class LoginUI : MonoBehaviour
    {
        InputField _input;
        Text _error;
        Action _onLoggedIn;

        public void Build(Transform canvas, Action onLoggedIn)
        {
            _onLoggedIn = onLoggedIn;

            var root = UiTheme.MakePanel(canvas, "LoginRoot", UiTheme.BgDeep);
            UiTheme.Stretch(root.rectTransform);

            var card = UiTheme.MakePanel(root.transform, "Card", UiTheme.Panel);
            var cardRt = card.rectTransform;
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(460f, 340f);

            UiTheme.MakeText(card.transform, "PEDULI TRANSIT", 34, FontStyle.Bold, TextAnchor.UpperCenter, UiTheme.Accent)
                .rectTransform.anchoredPosition = new Vector2(0f, -36f);

            UiTheme.MakeText(card.transform, "Masukkan username untuk mulai", 18, FontStyle.Normal,
                    TextAnchor.UpperCenter, UiTheme.Muted)
                .rectTransform.anchoredPosition = new Vector2(0f, -80f);

            _input = UiTheme.MakeInput(card.transform, "Username...", new Vector2(320f, 48f));
            _input.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 10f);

            var gm = GameManager.Instance;
            if (gm.HasSavedUser)
                _input.text = gm.Profile.username;

            var loginBtn = UiTheme.MakeButton(card.transform, "LOGIN", UiTheme.Accent, new Vector2(220f, 52f));
            loginBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -70f);
            loginBtn.onClick.AddListener(Submit);

            _error = UiTheme.MakeText(card.transform, "", 16, FontStyle.Normal, TextAnchor.LowerCenter, UiTheme.Danger);
            _error.rectTransform.anchoredPosition = new Vector2(0f, 28f);
        }

        void Submit()
        {
            var name = _input != null ? _input.text.Trim() : "";
            if (string.IsNullOrEmpty(name) || name.Length < 3)
            {
                _error.text = "Username minimal 3 karakter.";
                return;
            }

            GameManager.Instance.Login(name);
            _onLoggedIn?.Invoke();
        }
    }
}
