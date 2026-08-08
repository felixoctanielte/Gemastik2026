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

        public static Text MakeText(Transform parent, string content, int size, FontStyle style = FontStyle.Normal,
            TextAnchor anchor = TextAnchor.MiddleCenter, Color? color = null)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
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

        public static Button MakeButton(Transform parent, string label, Color bg, Vector2 size)
        {
            var img = MakePanel(parent, label + "Btn", bg);
            var rt = img.rectTransform;
            rt.sizeDelta = size;

            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(bg, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(bg, Color.black, 0.15f);
            btn.colors = colors;

            var text = MakeText(img.transform, label, 22, FontStyle.Bold);
            Stretch(text.rectTransform);

            return btn;
        }

        public static InputField MakeInput(Transform parent, string placeholder, Vector2 size)
        {
            var bg = MakePanel(parent, "Input", new Color(1f, 1f, 1f, 0.12f));
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
    }
}
