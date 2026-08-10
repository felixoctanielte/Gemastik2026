using UnityEngine;
using UnityEngine.UI;

namespace PeduliTransit.UI
{

    public static class ResponsiveUI
    {
        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;

        public static void ApplyCanvasScaler(Canvas canvas)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                return;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            scaler.matchWidthOrHeight = 0.5f;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        public static RectTransform FitCenterCard(RectTransform rt, float widthFrac, float heightFrac,
            float minW, float maxW, float minH, float maxH)
        {
            float w = Mathf.Clamp(RefWidth * widthFrac, minW, maxW);
            float h = Mathf.Clamp(RefHeight * heightFrac, minH, maxH);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        public static void FitPhoneBezel(RectTransform rt)
        {
            float h = Mathf.Clamp(RefHeight * 0.78f, 560f, 820f);
            float w = Mathf.Clamp(h * 0.52f, 300f, 440f);

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
        }

        public static void FitBottomStoryCard(RectTransform glow, RectTransform card)
        {
            float sidePad = 0.06f;
            float bottom = 36f;
            float height = Mathf.Clamp(RefHeight * 0.28f, 220f, 320f);

            void Apply(RectTransform rt, float expand)
            {
                rt.anchorMin = new Vector2(sidePad, 0f);
                rt.anchorMax = new Vector2(1f - sidePad, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.offsetMin = new Vector2(-expand, bottom - expand * 0.3f);
                rt.offsetMax = new Vector2(expand, bottom + height + expand * 0.3f);
            }

            if (glow != null) Apply(glow, 6f);
            if (card != null) Apply(card, 0f);
        }

        public static void FitPortraitForStory(RectTransform portrait, RectTransform card)
        {
            if (portrait == null || card == null) return;
            float cardH = card.rect.height > 10f ? card.rect.height : 260f;
            float h = Mathf.Clamp(cardH * 1.05f, 180f, 280f);
            float w = h * 0.82f;
            portrait.anchorMin = new Vector2(0f, 1f);
            portrait.anchorMax = new Vector2(0f, 1f);
            portrait.pivot = new Vector2(0f, 0f);
            portrait.sizeDelta = new Vector2(w, h);
            portrait.anchoredPosition = new Vector2(Mathf.Max(16f, card.rect.width * 0.03f), -36f);
        }

        public static int FontSize(int baseSize)
        {

            return Mathf.Max(14, baseSize);
        }
    }
}
