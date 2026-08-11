using UnityEngine;

namespace PeduliTransit.Player
{
    /// <summary>
    /// MC locomotion: solid ground stick, Space jump, Idle clip + procedural walking legs (no Kevin retarget = no "berenang").
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        public float moveSpeed = 3.2f;
        public float gravity = -30f;
        public float jumpSpeed = 6.2f;
        public float turnSpeed = 12f;
        public float groundStickForce = -6f;
        public float groundCheckDistance = 0.35f;

        CharacterController _controller;
        FreeLookCamera _camera;
        Animator _animator;
        float _verticalVelocity;
        bool _moveEnabled = true;
        bool _grounded;

        enum AnimState { Idle, Walk, Jump }
        AnimState _animState = AnimState.Idle;
        float _jumpLock;

        Transform _thighL, _thighR, _shinL, _shinR;
        Quaternion _thighL0, _thighR0, _shinL0, _shinR0;
        bool _hasLegs;
        float _walkPhase;
        int _legAxis; // 0=X 1=Z — detected once

        public bool MoveEnabled
        {
            get => _moveEnabled;
            set => _moveEnabled = value;
        }

        public void Init(FreeLookCamera camera)
        {
            _controller = GetComponent<CharacterController>();
            _camera = camera;
            _animator = GetComponentInChildren<Animator>();

            if (_controller != null)
            {
                _controller.height = 1.55f;
                _controller.radius = 0.25f;
                _controller.center = new Vector3(0f, 0.78f, 0f);
                _controller.stepOffset = 0.2f;
                _controller.skinWidth = 0.08f;
                _controller.minMoveDistance = 0f;
                _controller.slopeLimit = 45f;
            }

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                _animator.speed = 1f;
                _animator.enabled = true;
            }

            CacheLegs();
            SnapToGround(2.5f);
            PlayIdleOnly();
            _animState = AnimState.Idle;
        }

        public void SnapToGround(float maxDrop = 3f)
        {
            if (_controller == null)
                return;

            // Lift a bit then cast down to solid ground (ignores triggers).
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            if (Physics.SphereCast(origin, 0.2f, Vector3.down, out RaycastHit hit, maxDrop + 1.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                // Ignore self
                if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                    return;

                _controller.enabled = false;
                transform.position = new Vector3(transform.position.x, hit.point.y + 0.02f, transform.position.z);
                _controller.enabled = true;
                _verticalVelocity = groundStickForce;
                _grounded = true;
            }
        }

        void CacheLegs()
        {
            _thighL = FindBone("ThighL");
            _thighR = FindBone("ThighR");
            _shinL = FindBone("ShinL");
            _shinR = FindBone("ShinR");
            _hasLegs = _thighL != null && _thighR != null;
            if (!_hasLegs)
                return;

            _thighL0 = _thighL.localRotation;
            _thighR0 = _thighR.localRotation;
            if (_shinL) _shinL0 = _shinL.localRotation;
            if (_shinR) _shinR0 = _shinR.localRotation;

            // Casual1 Blender rig typically swings thighs on local X.
            _legAxis = 0;
        }

        Transform FindBone(string name)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == name)
                    return t;
            }
            return null;
        }

        void Update()
        {
            if (_controller == null)
                _controller = GetComponent<CharacterController>();
            if (_controller == null || !_controller.enabled)
                return;

            _grounded = ProbeGround();

            if (!_moveEnabled)
            {
                StickGravityOnly();
                RestoreLegs();
                PlayIdleOnly();
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
            bool walking = input.sqrMagnitude > 0.01f;

            Vector3 move = Vector3.zero;
            if (walking)
            {
                float yaw = _camera != null ? _camera.transform.eulerAngles.y : transform.eulerAngles.y;
                move = Quaternion.Euler(0f, yaw, 0f) * input;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move, Vector3.up),
                    turnSpeed * Time.deltaTime);
            }

            if (_grounded && _jumpLock <= 0f && _verticalVelocity < 0f)
                _verticalVelocity = groundStickForce;

            if (_grounded && _jumpLock <= 0f && Input.GetKeyDown(KeyCode.Space))
            {
                _verticalVelocity = jumpSpeed;
                _grounded = false;
                _jumpLock = 0.35f;
                _animState = AnimState.Jump;
            }

            if (_jumpLock > 0f)
                _jumpLock -= Time.deltaTime;

            _verticalVelocity += gravity * Time.deltaTime;
            // Clamp endless fall
            if (_verticalVelocity < -40f)
                _verticalVelocity = -40f;

            Vector3 velocity = move * moveSpeed;
            velocity.y = _verticalVelocity;
            CollisionFlags flags = _controller.Move(velocity * Time.deltaTime);
            if ((flags & CollisionFlags.Below) != 0)
            {
                _grounded = true;
                if (_verticalVelocity < 0f)
                    _verticalVelocity = groundStickForce;
            }

            // Rescue if we somehow fell far below the interior floor.
            if (transform.position.y < -5f)
                SnapToGround(20f);

            if (_jumpLock > 0f)
                _animState = AnimState.Jump;
            else
                _animState = walking ? AnimState.Walk : AnimState.Idle;

            // Animator: keep Idle only (matches Casual1). Never play Kevin Walk/Jump (causes "berenang").
            PlayIdleOnly();
        }

        void LateUpdate()
        {
            if (!_moveEnabled || !_hasLegs)
                return;

            if (_animState == AnimState.Walk && _grounded)
                ApplyWalkLegs();
            else if (_animState == AnimState.Jump)
                ApplyJumpLegs();
            else
                RestoreLegs();
        }

        void StickGravityOnly()
        {
            if (_grounded && _verticalVelocity < 0f)
                _verticalVelocity = groundStickForce;
            _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }

        bool ProbeGround()
        {
            Vector3 origin = transform.position + Vector3.up * 0.15f;
            float dist = (_controller != null ? _controller.skinWidth : 0.08f) + groundCheckDistance;
            if (Physics.SphereCast(origin, 0.18f, Vector3.down, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                    return _controller != null && _controller.isGrounded;
                return true;
            }
            return _controller != null && _controller.isGrounded;
        }

        void PlayIdleOnly()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            _animator.enabled = true;
            _animator.applyRootMotion = false;
            // Keep Idle playing slowly so upper body isn't T-pose, but don't retarget walk/jump.
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                if (_animator.HasState(0, Animator.StringToHash("Idle")))
                    _animator.CrossFadeInFixedTime("Idle", 0.15f, 0, 0f);
            }
            _animator.speed = _animState == AnimState.Idle ? 1f : 0.15f;
        }

        void ApplyWalkLegs()
        {
            _walkPhase += Time.deltaTime * 9f;
            float a = Mathf.Sin(_walkPhase) * 32f;
            float b = Mathf.Sin(_walkPhase + Mathf.PI) * 32f;
            float kneeL = Mathf.Max(0f, Mathf.Sin(_walkPhase)) * 40f;
            float kneeR = Mathf.Max(0f, Mathf.Sin(_walkPhase + Mathf.PI)) * 40f;
            SetThigh(_thighL, _thighL0, a);
            SetThigh(_thighR, _thighR0, b);
            SetShin(_shinL, _shinL0, kneeL);
            SetShin(_shinR, _shinR0, kneeR);
        }

        void ApplyJumpLegs()
        {
            SetThigh(_thighL, _thighL0, 25f);
            SetThigh(_thighR, _thighR0, 25f);
            SetShin(_shinL, _shinL0, 40f);
            SetShin(_shinR, _shinR0, 40f);
        }

        void RestoreLegs()
        {
            if (_thighL) _thighL.localRotation = _thighL0;
            if (_thighR) _thighR.localRotation = _thighR0;
            if (_shinL) _shinL.localRotation = _shinL0;
            if (_shinR) _shinR.localRotation = _shinR0;
            _walkPhase = 0f;
        }

        void SetThigh(Transform t, Quaternion rest, float deg)
        {
            if (t == null) return;
            t.localRotation = rest * (_legAxis == 0
                ? Quaternion.Euler(deg, 0f, 0f)
                : Quaternion.Euler(0f, 0f, deg));
        }

        void SetShin(Transform t, Quaternion rest, float deg)
        {
            if (t == null) return;
            t.localRotation = rest * (_legAxis == 0
                ? Quaternion.Euler(deg, 0f, 0f)
                : Quaternion.Euler(0f, 0f, deg));
        }
    }
}
