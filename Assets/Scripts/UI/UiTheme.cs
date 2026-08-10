using UnityEngine;
using UnityEngine.UI;

namespace PeduliTransit.UI
{
    public static class UiTheme
    {
        public static readonly Color BgDeep = new Color(0.06f, 0.14f, 0.16f, 0.94f);
        public static readonly Color Panel = new Color(0.09f, 0.22f, 0.24f, 0.96f);
        public static readonly Color Accent = new Color(0.95f, 0.55f, 0.18f, 1f);
        public static readonly Color AccentDark = new Color(0.75f, 0.38f, 0.08f, 1f);
        public static readonly Color Teal = new Color(0.18f, 0.72f, 0.68f, 1f);
        public static readonly Color Danger = new Color(0.85f, 0.28f, 0.28f, 1f);
        public static readonly Color Good = new Color(0.28f, 0.72f, 0.42f, 1f);
        public static readonly Color Text = new Color(0.95f, 0.96f, 0.94f, 1f);
        public static readonly Color Muted = new Color(0.72f, 0.78f, 0.76f, 1f);

        // Dark, semi-transparent "glass" tint used for all cards/bars across screens.
        public static readonly Color Glass = new Color(0.04f, 0.08f, 0.10f, 0.70f);

        public static Font DefaultFont
        {
            get
            {
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null)
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return font;
            }
        }

        static Sprite _roundedSprite;
        static bool _roundedSpriteLookedUp;

        // Built-in Unity sprite with soft rounded corners + 9-slice borders. Reused everywhere
        // so every panel/card/button/bar in the game shares the same rounded look for free.
        //
        // IMPORTANT: this must be "UI/Skin/UISprite.psd" — the plain WHITE rounded sprite the
        // default Unity UI system uses for Image/Button/Panel, which tints predictably.
        // "UI/Skin/Background.psd" is the OLD legacy IMGUI skin box texture, which is baked
        // with its own tan/brown shading — tinting it dark still comes out muddy brown, which
        // is exactly the discoloration bug. Falls back to null (plain square corners) if
        // neither builtin resource name resolves on a given Unity version, so this never
        // breaks the build — worst case you just lose the rounded corners.
        public static Sprite RoundedSprite
        {
            get
            {
                if (!_roundedSpriteLookedUp)
                {
                    _roundedSpriteLookedUp = true;
                    _roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                    if (_roundedSprite == null)
                        _roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
                }
                return _roundedSprite;
            }
        }

        // Default width/height given to a Text element when the caller only sets an
        // anchoredPosition (point anchor) and never an explicit size. Without this, a fresh
        // RectTransform defaults to a tiny 100x100 box, which forces longer labels like
        // "PEDULI TRANSIT" to wrap across multiple lines and overlap whatever sits below them.
        static readonly Vector2 DefaultTextSize = new Vector2(560f, 64f);

        public static Text MakeText(Transform parent, string content, int size, FontStyle style = FontStyle.Normal,
            TextAnchor anchor = TextAnchor.MiddleCenter, Color? color = null)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = DefaultTextSize;
            var text = go.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color ?? Text;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Image MakePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        // Rounded, tinted "glass" panel — the same building block used for every card/bar/dialog.
        // Anchored to center by default; caller can re-anchor afterwards if it needs to sit
        // somewhere other than the middle (e.g. a full-width top/bottom bar).
        public static Image MakeGlassPanel(Transform parent, string name, Vector2 size, Color? tint = null)
        {
            var img = MakePanel(parent, name, tint ?? Glass);
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            return img;
        }

        // Soft glowing outline sitting just behind a card — gives every screen the same
        // "floating glass card with a faint accent halo" feel. Non-interactive.
        public static Image MakeGlowBorder(Transform parent, Vector2 size, Color? color = null)
        {
            var glowColor = color ?? new Color(Accent.r, Accent.g, Accent.b, 0.3f);
            var go = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            img.color = glowColor;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            return img;
        }

        // Convenience: creates a glow + glass card as one pair, both centered on parent.
        // Returns the card; the glow sits behind it automatically (created first).
        public static Image MakeGlassCard(Transform parent, string name, Vector2 size, float glowPadding = 16f,
            Color? tint = null, Color? glowColor = null)
        {
            MakeGlowBorder(parent, size + new Vector2(glowPadding, glowPadding), glowColor);
            return MakeGlassPanel(parent, name, size, tint);
        }

        // Character/mascot artwork — used for the login screen, the hub, and the story
        // dialog box so every screen can show the same friendly face consistently.
        public static Image MakePortrait(Transform parent, Sprite sprite, Vector2 size)
        {
            var go = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.rectTransform.sizeDelta = size;
            return img;
        }

        public static Button MakeButton(Transform parent, string label, Color bg, Vector2 size)
        {
            var img = MakePanel(parent, label + "Btn", bg);
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            var rt = img.rectTransform;
            rt.sizeDelta = size;

            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(bg, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(bg, Color.black, 0.15f);
            btn.colors = colors;

            var text = MakeText(img.transform, label, 22, FontStyle.Bold);
            AddOutline(text, new Color(0f, 0f, 0f, 0.5f));
            Stretch(text.rectTransform);

            return btn;
        }

        public static InputField MakeInput(Transform parent, string placeholder, Vector2 size)
        {
            var bg = MakePanel(parent, "Input", new Color(1f, 1f, 1f, 0.12f));
            bg.sprite = RoundedSprite;
            bg.type = Image.Type.Sliced;
            bg.rectTransform.sizeDelta = size;

            var input = bg.gameObject.AddComponent<InputField>();
            var text = MakeText(bg.transform, "", 22, FontStyle.Normal, TextAnchor.MiddleLeft);
            var ph = MakeText(bg.transform, placeholder, 22, FontStyle.Italic, TextAnchor.MiddleLeft, Muted);

            var textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(14f, 6f);
            textRt.offsetMax = new Vector2(-14f, -6f);

            var phRt = ph.rectTransform;
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(14f, 6f);
            phRt.offsetMax = new Vector2(-14f, -6f);

            input.textComponent = text;
            input.placeholder = ph;
            input.characterLimit = 16;
            return input;
        }

        // Subtle dark outline behind text so it stays readable over glass/busy backgrounds.
        // Shared by every screen instead of each UI script keeping its own private copy.
        public static void AddOutline(Text text, Color color)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1.2f, -1.2f);
        }

        public static void Stretch(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        public static void SetAnchored(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        // Makes sure the Canvas this UI lives on actually fills the screen and scales
        // consistently across resolutions/aspect ratios, instead of relying on whatever
        // Canvas settings happened to be left in the scene. Safe to call every time a
        // screen builds itself — it no-ops nicely on a deliberate World Space canvas.
        public static void EnsureFullscreenCanvas(Transform canvasTransform)
        {
            var canvas = canvasTransform.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                return;

            // Only take over scaling if there is NO CanvasScaler yet. If one already exists
            // (configured by hand in the Inspector, or by another script), leave it alone —
            // overwriting it is what made everything look wrong at other resolutions.
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}