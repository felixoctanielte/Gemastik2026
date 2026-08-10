using PeduliTransit.Core;
using UnityEngine;

namespace PeduliTransit.NPC
{
    public class NpcPassenger : MonoBehaviour
    {
        public NpcRole Role { get; private set; }
        public bool IsSitting { get; private set; }

        TextMesh _label;
        Animator _animator;

        [Header("Walking")]
        [SerializeField] float walkSpeed = 0.8f;

        bool _walking;

        public void Setup(
            NpcRole role,
            bool sitting,
            Color color,
            RuntimeAnimatorController animatorController = null,
            bool tintMaterial = true)
        {
            Role = role;
            IsSitting = sitting;

            // Animator
            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
                _animator = gameObject.AddComponent<Animator>();

            if (animatorController != null)
            {
                _animator.runtimeAnimatorController = animatorController;
                _animator.applyRootMotion = false;
            }

            // Material
            if (tintMaterial)
            {
                var renderer = GetComponentInChildren<Renderer>();

                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = color;
                }
            }

            EnsureLabel(role.ToString());

            // Kalau berdiri → jalan
            // Kalau duduk → diam
            SetWalking(!sitting);
        }

        void SetWalking(bool walking)
        {
            _walking = walking;

            if (_animator == null)
                return;

            if (_animator.runtimeAnimatorController == null)
                return;

            // Controller lu sekarang cuma punya animation Walk,
            // jadi langsung play state tersebut.
            _animator.Play("HumanF@Walk01_Forward", 0, 0f);
        }

        void Update()
        {
            if (!_walking)
                return;

            // Gerakkan NPC ke depan
            transform.position += transform.forward * walkSpeed * Time.deltaTime;
        }

        void EnsureLabel(string text)
        {
            if (_label != null)
                return;

            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.85f, 0f);

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