using PeduliTransit.Core;
using UnityEngine;

namespace PeduliTransit.NPC
{
    public class NpcPassenger : MonoBehaviour
    {
        public NpcRole Role { get; private set; }
        public bool IsSitting { get; private set; }

        TextMesh _label;

        public void Setup(NpcRole role, bool sitting, Color color)
        {
            Role = role;
            IsSitting = sitting;

            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = color;
            }

            EnsureLabel(role.ToString());
        }

        void EnsureLabel(string text)
        {
            if (_label != null)
                return;

            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            _label = go.AddComponent<TextMesh>();
            _label.text = text;
            _label.characterSize = 0.08f;
            _label.fontSize = 48;
            _label.anchor = TextAnchor.LowerCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = Color.white;
        }

        void LateUpdate()
        {
            if (_label == null || Camera.main == null)
                return;
            _label.transform.rotation = Quaternion.LookRotation(
                _label.transform.position - Camera.main.transform.position);
        }

        public void Highlight(bool on)
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer == null)
                return;
            renderer.material.color = on
                ? Color.Lerp(renderer.material.color, Color.yellow, 0.55f)
                : renderer.material.color;
        }
    }
}
