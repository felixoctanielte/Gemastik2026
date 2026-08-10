using PeduliTransit.Managers;
using UnityEngine;

namespace PeduliTransit.Player
{
    public class FreeLookCamera : MonoBehaviour
    {
        public float panSpeed = 8f;
        public float verticalPanSpeed = 5.5f;
        public float orbitSensitivity = 3.5f;
        public float zoomSpeed = 3f;
        public float minDistance = 1.5f;
        public float maxDistance = 14f;
        public float minPitch = 12f;
        public float maxPitch = 75f;

        Camera _cam;
        Vector3 _focus;
        float _yaw;
        float _pitch = 25f;
        float _distance = 5f;
        bool _lookEnabled = true;

        public Camera Cam => _cam;
        public Vector3 FocusPoint => _focus;

        public bool LookEnabled
        {
            get => _lookEnabled;
            set => _lookEnabled = value;
        }

        public void Init(Vector3 focus, float yawDegrees = 180f, float distance = 5f)
        {
            _cam = GetComponent<Camera>();
            if (_cam == null)
                _cam = gameObject.AddComponent<Camera>();
            _cam.enabled = true;
            _cam.nearClipPlane = 0.05f;
            if (_cam.fieldOfView < 50f)
                _cam.fieldOfView = 60f;

            _focus = focus;
            if (_focus.y < 0.8f)
                _focus.y = 1.2f;

            _yaw = yawDegrees;
            _pitch = 25f;
            _distance = Mathf.Clamp(distance, minDistance, maxDistance);
            _lookEnabled = true;
            ApplyTransform();
        }

        public void FocusOn(Transform target, float preferredDistance = -1f)
        {
            if (target == null)
                return;

            _focus = target.position + Vector3.up * 1.1f;
            if (_focus.y < 0.8f)
                _focus.y = 1.1f;

            if (preferredDistance > 0f)
                _distance = Mathf.Clamp(preferredDistance, minDistance, maxDistance);

            Vector3 toCam = transform.position - _focus;
            if (toCam.sqrMagnitude > 0.01f)
            {
                _yaw = Mathf.Atan2(toCam.x, toCam.z) * Mathf.Rad2Deg;
                float flat = new Vector3(toCam.x, 0f, toCam.z).magnitude;
                if (flat > 0.01f)
                    _pitch = Mathf.Clamp(Mathf.Atan2(toCam.y, flat) * Mathf.Rad2Deg, minPitch, maxPitch);
            }

            ApplyTransform();
        }

        void Update()
        {
            if (!_lookEnabled || _cam == null || !_cam.enabled)
                return;

            float sens = GameManager.Instance != null
                ? GameManager.Instance.Settings.mouseSensitivity
                : 2f;

            HandleOrbit(sens);
            HandlePan();
            HandleZoom();
            ApplyTransform();
        }

        void HandleOrbit(float sens)
        {
            if (!Input.GetMouseButton(1) && !Input.GetMouseButton(2))
                return;

            float mx = Input.GetAxis("Mouse X") * orbitSensitivity * sens;
            float my = Input.GetAxis("Mouse Y") * orbitSensitivity * sens;
            _yaw += mx;
            _pitch = Mathf.Clamp(_pitch - my, minPitch, maxPitch);
        }

        void HandlePan()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float up = 0f;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
                up += 1f;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))
                up -= 1f;

            if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f && Mathf.Abs(up) < 0.01f)
                return;

            Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 right = yawRot * Vector3.right;
            Vector3 forward = yawRot * Vector3.forward;
            Vector3 delta = (right * h + forward * v) * panSpeed * Time.unscaledDeltaTime;
            delta += Vector3.up * up * verticalPanSpeed * Time.unscaledDeltaTime;
            _focus += delta;
            _focus.y = Mathf.Clamp(_focus.y, 0.6f, 5f);
        }

        void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
                _distance = Mathf.Clamp(_distance - scroll * zoomSpeed * 4f, minDistance, maxDistance);
        }

        void ApplyTransform()
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _focus + rot * new Vector3(0f, 0f, -_distance);
            transform.rotation = rot;
        }
    }
}
