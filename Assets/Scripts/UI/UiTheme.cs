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

        public static Image MakeGlassCard(Transform parent, string name, Vector2 size, float glowPadding = 16f,
            Color? tint = null, Color? glowColor = null)
        {
            MakeGlowBorder(parent, size + new Vector2(glowPadding, glowPadding), glowColor);
            return MakeGlassPanel(parent, name, size, tint);
        }

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

        public static void EnsureFullscreenCanvas(Transform canvasTransform)
        {
            var canvas = canvasTransform.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                return;

            ResponsiveUI.ApplyCanvasScaler(canvas);
        }

        public static Image MakeResponsiveGlassCard(Transform parent, string name,
            float widthFrac, float heightFrac, float minW, float maxW, float minH, float maxH,
            float glowPadding = 16f, Color? tint = null, Color? glowColor = null)
        {
            float w = Mathf.Clamp(ResponsiveUI.RefWidth * widthFrac, minW, maxW);
            float h = Mathf.Clamp(ResponsiveUI.RefHeight * heightFrac, minH, maxH);
            MakeGlowBorder(parent, new Vector2(w + glowPadding, h + glowPadding), glowColor);
            return MakeGlassPanel(parent, name, new Vector2(w, h), tint);
        }
    }
}