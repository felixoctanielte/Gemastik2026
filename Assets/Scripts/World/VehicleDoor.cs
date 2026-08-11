using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PeduliTransit.World
{
    public class VehicleDoor : MonoBehaviour
    {
        public Transform LeftLeaf;
        public Transform RightLeaf;
        public bool EnsurePassageCollidersDisabledWhileOpen = true;

        Vector3 _outsideLocal;
        Vector3 _insideLocal;
        Vector3 _leftClosed;
        Vector3 _rightClosed;
        Vector3 _leftOpen;
        Vector3 _rightOpen;
        bool _open;
        readonly List<Collider> _disabledColliders = new List<Collider>();

        public bool IsOpen => _open;
        public Vector3 OutsidePoint => transform.TransformPoint(_outsideLocal);
        public Vector3 InsidePoint => transform.TransformPoint(_insideLocal);

        public void Init(Transform left, Transform right, Vector3 outside, Vector3 inside, bool autoAxis = false)
        {
            LeftLeaf = left;
            RightLeaf = right;
            SetPassagePoints(outside, inside);

            Vector3 midWorld = Vector3.zero;
            int count = 0;
            if (LeftLeaf != null) { midWorld += LeftLeaf.position; count++; }
            if (RightLeaf != null) { midWorld += RightLeaf.position; count++; }
            if (count > 0) midWorld /= count;
            else midWorld = transform.position;

            if (LeftLeaf != null)
            {
                _leftClosed = LeftLeaf.localPosition;
                _leftOpen = _leftClosed + ComputeLocalSlide(LeftLeaf, midWorld, preferPositiveZ: true);
            }

            if (RightLeaf != null)
            {
                _rightClosed = RightLeaf.localPosition;
                _rightOpen = _rightClosed + ComputeLocalSlide(RightLeaf, midWorld, preferPositiveZ: false);
            }

            // Proxy double doors: big visible split on local Z.
            if (!autoAxis)
            {
                if (LeftLeaf != null)
                    _leftOpen = _leftClosed + new Vector3(0f, 0f, 1.2f);
                if (RightLeaf != null)
                    _rightOpen = _rightClosed + new Vector3(0f, 0f, -1.2f);
            }
        }

        public void SetPassagePoints(Vector3 outside, Vector3 inside)
        {
            _outsideLocal = transform.InverseTransformPoint(outside);
            _insideLocal = transform.InverseTransformPoint(inside);
        }

        static Vector3 ComputeLocalSlide(Transform leaf, Vector3 midWorld, bool preferPositiveZ)
        {
            float width = 0.9f;
            var rend = leaf.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var size = rend.bounds.size;
                width = Mathf.Clamp(Mathf.Max(size.x, size.z) * 0.85f, 0.5f, 2.4f);
            }

            Vector3 away = leaf.position - midWorld;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f)
                away = preferPositiveZ ? leaf.forward : -leaf.forward;
            away.Normalize();

            Vector3 worldDelta = away * width;
            Transform space = leaf.parent != null ? leaf.parent : leaf;
            return space.InverseTransformVector(worldDelta);
        }

        public IEnumerator Open(float duration = 0.7f)
        {
            if (_open)
                yield break;

            // Ensure leaves visible before animating.
            if (LeftLeaf != null) LeftLeaf.gameObject.SetActive(true);
            if (RightLeaf != null) RightLeaf.gameObject.SetActive(true);

            yield return Animate(true, duration);
            _open = true;
            if (EnsurePassageCollidersDisabledWhileOpen)
                SetPassageBlocked(false);
        }

        public IEnumerator Close(float duration = 0.55f)
        {
            if (!_open)
                yield break;
            if (EnsurePassageCollidersDisabledWhileOpen)
                SetPassageBlocked(true);
            yield return Animate(false, duration);
            _open = false;
        }

        IEnumerator Animate(bool opening, float duration)
        {
            float t = 0f;
            Vector3 l0 = LeftLeaf != null ? LeftLeaf.localPosition : Vector3.zero;
            Vector3 r0 = RightLeaf != null ? RightLeaf.localPosition : Vector3.zero;
            Vector3 l1 = opening ? _leftOpen : _leftClosed;
            Vector3 r1 = opening ? _rightOpen : _rightClosed;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                if (LeftLeaf != null)
                    LeftLeaf.localPosition = Vector3.Lerp(l0, l1, u);
                if (RightLeaf != null)
                    RightLeaf.localPosition = Vector3.Lerp(r0, r1, u);
                yield return null;
            }

            if (LeftLeaf != null)
                LeftLeaf.localPosition = l1;
            if (RightLeaf != null)
                RightLeaf.localPosition = r1;
            _open = opening;
        }

        void SetPassageBlocked(bool blocked)
        {
            if (blocked)
            {
                foreach (var col in _disabledColliders)
                {
                    if (col != null)
                        col.enabled = true;
                }
                _disabledColliders.Clear();
                return;
            }

            _disabledColliders.Clear();
            DisableLeafColliders(LeftLeaf);
            DisableLeafColliders(RightLeaf);
        }

        void DisableLeafColliders(Transform leaf)
        {
            if (leaf == null)
                return;
            foreach (var col in leaf.GetComponentsInChildren<Collider>(true))
            {
                if (col == null || !col.enabled)
                    continue;
                col.enabled = false;
                _disabledColliders.Add(col);
            }
        }
    }
}
