using System.Collections;
using UnityEngine;

namespace PeduliTransit.World
{

    public class VehicleDoor : MonoBehaviour
    {
        public Transform LeftLeaf;
        public Transform RightLeaf;
        public Vector3 OutsidePoint;
        public Vector3 InsidePoint;

        Vector3 _leftClosed;
        Vector3 _rightClosed;
        Vector3 _leftOpen;
        Vector3 _rightOpen;
        bool _open;
        Coroutine _anim;

        public bool IsOpen => _open;

        public void Init(Transform left, Transform right, Vector3 outside, Vector3 inside)
        {
            LeftLeaf = left;
            RightLeaf = right;
            OutsidePoint = outside;
            InsidePoint = inside;

            if (LeftLeaf != null)
            {
                _leftClosed = LeftLeaf.localPosition;
                _leftOpen = _leftClosed + new Vector3(0f, 0f, 0.85f);
            }

            if (RightLeaf != null)
            {
                _rightClosed = RightLeaf.localPosition;
                _rightOpen = _rightClosed + new Vector3(0f, 0f, -0.85f);
            }
        }

        public IEnumerator Open(float duration = 0.55f)
        {
            if (_open)
                yield break;
            yield return Animate(true, duration);
            _open = true;
        }

        public IEnumerator Close(float duration = 0.45f)
        {
            if (!_open)
                yield break;
            yield return Animate(false, duration);
            _open = false;
        }

        IEnumerator Animate(bool opening, float duration)
        {
            if (_anim != null)
                StopCoroutine(_anim);

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
    }
}
