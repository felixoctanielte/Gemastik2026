using PeduliTransit.Managers;
using UnityEngine;

namespace PeduliTransit.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public float moveSpeed = 4.5f;
        public float gravity = -18f;
        public Transform cameraPivot;

        CharacterController _controller;
        float _pitch;
        float _yaw;
        float _verticalVelocity;
        bool _lookEnabled = true;

        public bool LookEnabled
        {
            get => _lookEnabled;
            set
            {
                _lookEnabled = value;
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !value;
            }
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraPivot == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraPivot = cam.transform;
            }

            _yaw = transform.eulerAngles.y;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                LookEnabled = !LookEnabled;

            float sens = GameManager.Instance != null
                ? GameManager.Instance.Settings.mouseSensitivity
                : 2f;

            if (_lookEnabled && cameraPivot != null)
            {
                float mx = Input.GetAxis("Mouse X") * sens;
                float my = Input.GetAxis("Mouse Y") * sens;
                _yaw += mx;
                _pitch = Mathf.Clamp(_pitch - my, -80f, 80f);
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
            Vector3 world = transform.TransformDirection(input) * moveSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;
            world.y = _verticalVelocity;

            _controller.Move(world * Time.deltaTime);
        }
    }
}
