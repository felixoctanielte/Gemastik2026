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

        // Assign your app logo/mascot sprite here for a nicer login screen.
        public Sprite appLogo;

        // Optional: assign a nicer imported .ttf/.otf for a better-looking font.
        // Leave empty to keep Unity's default font.
        public Font customFont;

        public void Build(Transform canvas, Action onLoggedIn)
        {
            _onLoggedIn = onLoggedIn;

            UiTheme.EnsureFullscreenCanvas(canvas);

            // --- Root: fills the ENTIRE screen, this is the fullscreen background layer. ---
            var root = UiTheme.MakePanel(canvas, "LoginRoot", UiTheme.BgDeep);
            UiTheme.Stretch(root.rectTransform);

            Sprite roundedSprite = UiTheme.RoundedSprite;

            // --- Soft glow blob behind everything, just for a bit of atmosphere/depth. ---
            var glowGO = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowGO.transform.SetParent(root.transform, false);
            var glowImg = glowGO.GetComponent<Image>();
            glowImg.sprite = roundedSprite;
            glowImg.type = Image.Type.Sliced;
            glowImg.raycastTarget = false;
            glowImg.color = new Color(UiTheme.Accent.r, UiTheme.Accent.g, UiTheme.Accent.b, 0.14f);
            var glrt = glowGO.GetComponent<RectTransform>();
            glrt.anchorMin = glrt.anchorMax = new Vector2(0.5f, 0.5f);
            glrt.sizeDelta = new Vector2(680f, 620f);

            // --- Thin accent border sitting just behind the card (creates a soft outline/glow),
            //     same technique as the story dialog card glow in GameplayUI. ---
            var borderGO = new GameObject("CardGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGO.transform.SetParent(root.transform, false);
            var borderImg = borderGO.GetComponent<Image>();
            borderImg.sprite = roundedSprite;
            borderImg.type = Image.Type.Sliced;
            borderImg.raycastTarget = false;
            borderImg.color = new Color(UiTheme.Accent.r, UiTheme.Accent.g, UiTheme.Accent.b, 0.35f);
            var brt = borderGO.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(492f, 472f);

            // --- Card: rounded, semi-transparent glass panel (instead of flat solid color). ---
            var cardGO = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGO.transform.SetParent(root.transform, false);
            var card = cardGO.GetComponent<Image>();
            card.sprite = roundedSprite;
            card.type = Image.Type.Sliced;
            card.color = new Color(0.04f, 0.08f, 0.10f, 0.72f);
            var cardRt = cardGO.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(480f, 460f);

            bool hasLogo = appLogo != null;

            // --- Logo (mascot), centered near the top of the card. ---
            if (hasLogo)
            {
                var logoGO = new GameObject("Logo", typeof(RectTransform), typeof(Image));
                logoGO.transform.SetParent(card.transform, false);
                var logoImg = logoGO.GetComponent<Image>();
                logoImg.sprite = appLogo;
                logoImg.preserveAspect = true;
                logoImg.raycastTarget = false;
                var lrt = logoGO.GetComponent<RectTransform>();
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 1f);
                lrt.pivot = new Vector2(0.5f, 1f);
                lrt.sizeDelta = new Vector2(100f, 100f);
                lrt.anchoredPosition = new Vector2(0f, -20f);
            }

            float titleY = hasLogo ? 96f : 172f;
            float subtitleY = hasLogo ? 54f : 130f;

            var title = UiTheme.MakeText(card.transform, "PEDULI TRANSIT", 36, FontStyle.Bold, TextAnchor.UpperCenter,
                UiTheme.Accent);
            title.rectTransform.anchoredPosition = new Vector2(0f, titleY);
            UiTheme.AddOutline(title, new Color(0f, 0f, 0f, 0.5f));
            if (customFont != null) title.font = customFont;

            var subtitle = UiTheme.MakeText(card.transform, "Masukkan username untuk mulai", 18, FontStyle.Normal,
                TextAnchor.UpperCenter, UiTheme.Muted);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, subtitleY);
            if (customFont != null) subtitle.font = customFont;

            // --- Input field, restyled to match the rounded/glass look. ---
            _input = UiTheme.MakeInput(card.transform, "Username...", new Vector2(360f, 54f));
            _input.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -10f);

            var inputBg = _input.GetComponent<Image>();
            if (inputBg != null)
                inputBg.color = new Color(1f, 1f, 1f, 0.10f);

            if (_input.textComponent != null)
            {
                _input.textComponent.fontSize = 20;
                _input.textComponent.color = Color.white;
                if (customFont != null) _input.textComponent.font = customFont;
            }
            var placeholderText = _input.placeholder as Text;
            if (placeholderText != null)
            {
                placeholderText.fontSize = 20;
                placeholderText.color = new Color(1f, 1f, 1f, 0.45f);
                if (customFont != null) placeholderText.font = customFont;
            }

            var gm = GameManager.Instance;
            if (gm.HasSavedUser)
                _input.text = gm.Profile.username;

            // --- Login button: rounded, bigger + bolder label. ---
            var loginBtn = UiTheme.MakeButton(card.transform, "LOGIN", UiTheme.Accent, new Vector2(260f, 56f));
            loginBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -90f);

            var btnText = loginBtn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.fontSize = 22;
                btnText.fontStyle = FontStyle.Bold;
                if (customFont != null) btnText.font = customFont;
            }
            loginBtn.onClick.AddListener(Submit);

            _error = UiTheme.MakeText(card.transform, "", 16, FontStyle.Normal, TextAnchor.LowerCenter, UiTheme.Danger);
            _error.rectTransform.anchoredPosition = new Vector2(0f, -168f);
            if (customFont != null) _error.font = customFont;
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