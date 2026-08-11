using PeduliTransit.Managers;
using UnityEngine;

namespace PeduliTransit.Player
{
    /// <summary>
    /// Third-person camera locked to the MC. Orbit with RMB, zoom with scroll.
    /// SphereCast keeps the lens from clipping interior walls.
    /// </summary>
    public class FreeLookCamera : MonoBehaviour
    {
        public float orbitSensitivity = 3.5f;
        public float zoomSpeed = 3f;
        public float minDistance = 1.35f;
        public float maxDistance = 6.5f;
        public float minPitch = 8f;
        public float maxPitch = 68f;
        public float collisionRadius = 0.22f;
        public float collisionPadding = 0.18f;
        public float followHeight = 1.45f;
        public float focusSmooth = 14f;

        Camera _cam;
        Transform _followTarget;
        Transform _lookInterest;
        Vector3 _focus;
        float _yaw;
        float _pitch = 22f;
        float _distance = 3.4f;
        bool _lookEnabled = true;
        Collider[] _ignoredColliders = new Collider[0];

        public Camera Cam => _cam;
        public Vector3 FocusPoint => _focus;
        public Transform FollowTarget => _followTarget;

        public bool LookEnabled
        {
            get => _lookEnabled;
            set => _lookEnabled = value;
        }

        public void Init(Vector3 focus, float yawDegrees = 180f, float distance = 3.4f)
        {
            EnsureCamera();
            _focus = focus;
            if (_focus.y < 0.8f)
                _focus.y = 1.2f;

            _yaw = yawDegrees;
            _pitch = 22f;
            _distance = Mathf.Clamp(distance, minDistance, maxDistance);
            _lookEnabled = true;
            ApplyTransform(true);
        }

        public void SetFollowTarget(Transform target, Collider[] ignoreColliders = null)
        {
            _followTarget = target;
            _ignoredColliders = ignoreColliders ?? new Collider[0];
            if (_followTarget != null)
            {
                _focus = _followTarget.position + Vector3.up * followHeight;
                _yaw = _followTarget.eulerAngles.y;
            }
        }

        public void ClearLookInterest()
        {
            _lookInterest = null;
        }

        public void FocusOn(Transform target, float preferredDistance = -1f)
        {
            if (target == null)
                return;

            _lookInterest = target;

            if (_followTarget != null)
                _focus = _followTarget.position + Vector3.up * followHeight;
            else
            {
                _focus = target.position + Vector3.up * 1.1f;
                if (_focus.y < 0.8f)
                    _focus.y = 1.1f;
            }

            if (preferredDistance > 0f)
                _distance = Mathf.Clamp(preferredDistance, minDistance, maxDistance);

            Vector3 interest = target.position + Vector3.up * 1.05f;
            Vector3 flat = interest - _focus;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f)
                _yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;

            float vertical = interest.y - _focus.y;
            float horizontal = new Vector3(interest.x - _focus.x, 0f, interest.z - _focus.z).magnitude;
            if (horizontal > 0.05f)
                _pitch = Mathf.Clamp(Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg + 12f, minPitch, maxPitch);

            ApplyTransform(true);
        }

        void EnsureCamera()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null)
                _cam = gameObject.AddComponent<Camera>();
            _cam.enabled = true;
            _cam.nearClipPlane = 0.05f;
            if (_cam.fieldOfView < 50f)
                _cam.fieldOfView = 58f;
        }

        void LateUpdate()
        {
            if (_cam == null)
                EnsureCamera();

            if (!_lookEnabled || _cam == null || !_cam.enabled)
            {
                SyncFocusFromTarget(1f);
                ApplyTransform(false);
                return;
            }

            float sens = GameManager.Instance != null
                ? GameManager.Instance.Settings.mouseSensitivity
                : 2f;

            HandleOrbit(sens);
            HandleZoom();
            SyncFocusFromTarget(Time.unscaledDeltaTime * focusSmooth);
            ApplyTransform(false);
        }

        void SyncFocusFromTarget(float blend)
        {
            if (_followTarget == null)
                return;

            Vector3 desired = _followTarget.position + Vector3.up * followHeight;
            if (blend >= 1f)
                _focus = desired;
            else
                _focus = Vector3.Lerp(_focus, desired, Mathf.Clamp01(blend));
        }

        void HandleOrbit(float sens)
        {
            if (!Input.GetMouseButton(1) && !Input.GetMouseButton(2))
                return;

            float mx = Input.GetAxis("Mouse X") * orbitSensitivity * sens;
            float my = Input.GetAxis("Mouse Y") * orbitSensitivity * sens;
            _yaw += mx;
            _pitch = Mathf.Clamp(_pitch - my, minPitch, maxPitch);
            _lookInterest = null;
        }

        void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
                _distance = Mathf.Clamp(_distance - scroll * zoomSpeed * 4f, minDistance, maxDistance);
        }

        void ApplyTransform(bool instant)
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -_distance);
            Vector3 desired = _focus + offset;
            Vector3 finalPos = ResolveCollision(_focus, desired);

            if (instant)
                transform.position = finalPos;
            else
                transform.position = Vector3.Lerp(transform.position, finalPos, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));

            transform.rotation = rot;
        }

        Vector3 ResolveCollision(Vector3 pivot, Vector3 desired)
        {
            Vector3 delta = desired - pivot;
            float dist = delta.magnitude;
            if (dist < 0.001f)
                return pivot;

            Vector3 dir = delta / dist;
            var hits = Physics.SphereCastAll(
                pivot,
                collisionRadius,
                dir,
                dist,
                ~0,
                QueryTriggerInteraction.Ignore);

            float best = dist;
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider == null)
                    continue;
                if (ShouldIgnore(hit.collider))
                    continue;

                float allowed = Mathf.Max(0.35f, hit.distance - collisionPadding);
                if (allowed < best)
                    best = allowed;
            }

            return pivot + dir * Mathf.Clamp(best, 0.35f, dist);
        }

        bool ShouldIgnore(Collider col)
        {
            if (_followTarget != null && col.transform.IsChildOf(_followTarget))
                return true;

            if (_ignoredColliders != null)
            {
                for (int i = 0; i < _ignoredColliders.Length; i++)
                {
                    if (_ignoredColliders[i] == col)
                        return true;
                }
            }

            return false;
        }
    }
}
