using System.Collections;
using PeduliTransit.Core;
using PeduliTransit.World;
using UnityEngine;

namespace PeduliTransit.NPC
{
    public class NpcPassenger : MonoBehaviour
    {
        public NpcRole Role { get; private set; }
        public bool IsSitting { get; private set; }
        public bool IsPriorityEligible { get; private set; }
        public SeatSlot AssignedSeat { get; private set; }
        public bool IsResponder { get; private set; }
        public bool IsExiting { get; private set; }

        TextMesh _label;
        Animator _animator;
        RuntimeAnimatorController _controller;
        NpcSitPose _sitPose;
        bool _usingCharacterModel;

        [SerializeField] float walkSpeed = 1.35f;
        bool _walking;
        Vector3? _moveTarget;
        System.Action _onArrive;

        public void Setup(
            NpcRole role,
            bool sitting,
            Color color,
            RuntimeAnimatorController animatorController = null,
            bool tintMaterial = true,
            bool usingCharacterModel = false)
        {
            Role = role;
            IsSitting = sitting;
            _controller = animatorController;
            _usingCharacterModel = usingCharacterModel;
            IsPriorityEligible = IsPriorityRole(role);
            IsResponder = role == NpcRole.Security
                || role == NpcRole.TicketOfficer
                || role == NpcRole.DriverAssistant;

            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                _animator = gameObject.AddComponent<Animator>();

            if (animatorController != null)
            {
                _animator.runtimeAnimatorController = animatorController;
                _animator.applyRootMotion = false;
            }

            _sitPose = GetComponent<NpcSitPose>();
            if (_sitPose == null)
                _sitPose = gameObject.AddComponent<NpcSitPose>();
            _sitPose.Bind(_animator);

            if (tintMaterial)
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.color = color;
                }
            }

            EnsureLabel(DisplayName(role));
            ApplyPose(sitting);
        }

        public static bool IsPriorityRole(NpcRole role)
        {
            return role == NpcRole.Pregnant
                || role == NpcRole.Elderly
                || role == NpcRole.Disability
                || role == NpcRole.CarryingChild;
        }

        static string DisplayName(NpcRole role)
        {
            return role switch
            {
                NpcRole.LoudTalking => "Ramai",
                NpcRole.PrioritySeatAbuse => "Salah duduk",
                NpcRole.PhoneVolume => "HP keras",
                NpcRole.HarassmentHint => "Mencurigakan",
                NpcRole.Fighting => "Berantem",
                NpcRole.Pregnant => "Ibu hamil",
                NpcRole.CarryingChild => "Gendong anak",
                NpcRole.Disability => "Disabilitas",
                NpcRole.Elderly => "Lansia",
                NpcRole.Security => "Satpam",
                NpcRole.TicketOfficer => "Petugas",
                NpcRole.DriverAssistant => "Kondektur",
                _ => "Penumpang"
            };
        }

        public void AssignSeat(SeatSlot seat)
        {
            if (seat == null)
                return;

            if (AssignedSeat == seat && seat.Occupant == this)
            {
                SitAt(seat);
                return;
            }

            if (AssignedSeat != null)
                AssignedSeat.Vacate(this);

            if (seat.IsOccupied && seat.Occupant != this)
            {
                AssignedSeat = null;
                ApplyPose(false);
                return;
            }

            if (!seat.TryOccupy(this))
            {
                AssignedSeat = null;
                ApplyPose(false);
                return;
            }

            AssignedSeat = seat;
            SitAt(seat);
        }

        public void VacateSeat()
        {
            if (AssignedSeat != null)
            {
                AssignedSeat.Vacate(this);
                AssignedSeat = null;
            }

            ApplyPose(false);
        }

        public void SitAt(SeatSlot seat)
        {
            if (seat == null)
                return;

            AssignedSeat = seat;
            transform.position = seat.SitWorldPosition;
            transform.rotation = seat.SitFacing;
            ApplyPose(true);
        }

        public void StandAt(Vector3 worldPos, Quaternion facing)
        {
            VacateSeat();
            transform.position = worldPos;
            transform.rotation = facing;
            ApplyPose(false);
            SetWalking(false);
        }

        void ApplyPose(bool sitting)
        {
            IsSitting = sitting;
            SetWalking(false);

            if (!_usingCharacterModel)
            {
                transform.localScale = sitting
                    ? new Vector3(0.7f, 0.55f, 0.7f)
                    : new Vector3(0.7f, 0.9f, 0.7f);
                if (_sitPose != null)
                    _sitPose.Stand();
                return;
            }

            if (sitting)
            {
                Vector3 surface = AssignedSeat != null
                    ? AssignedSeat.SitSurfaceWorld
                    : transform.position + Vector3.up * 0.45f;
                Vector3 forward = AssignedSeat != null
                    ? AssignedSeat.SitForward
                    : transform.forward;
                _sitPose?.Sit(surface, forward);
            }
            else
            {
                _sitPose?.Stand();
                if (_animator != null)
                    _animator.enabled = true;
            }
        }

        public void SetWalking(bool walking)
        {
            _walking = walking;

            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            if (IsSitting)
            {
                _animator.enabled = false;
                return;
            }

            _animator.enabled = true;

            if (walking)
            {
                int walkHash = Animator.StringToHash("HumanF@Walk01_Forward");
                if (_animator.HasState(0, walkHash))
                    _animator.Play(walkHash, 0, 0f);
                _animator.speed = 1f;
            }
            else
            {
                int idleHash = Animator.StringToHash("HumanF@Idle01");
                if (_animator.HasState(0, idleHash))
                {
                    _animator.Play(idleHash, 0, 0f);
                    _animator.speed = 1f;
                }
                else
                {
                    _animator.speed = 0f;
                }
            }
        }

        public void WalkTo(Vector3 worldTarget, System.Action onArrive = null)
        {
            VacateSeat();
            _moveTarget = worldTarget;
            _onArrive = onArrive;
            FaceToward(worldTarget);
            SetWalking(true);
        }

        public IEnumerator WalkToRoutine(Vector3 worldTarget, float arriveDist = 0.35f)
        {
            bool done = false;
            WalkTo(worldTarget, () => done = true);
            float guard = 12f;
            while (!done && guard > 0f)
            {
                guard -= Time.unscaledDeltaTime;
                yield return null;
            }

            _moveTarget = null;
            SetWalking(false);
            transform.position = worldTarget;
        }

        void FaceToward(Vector3 worldTarget)
        {
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        void Update()
        {
            if (_moveTarget.HasValue)
            {
                Vector3 target = _moveTarget.Value;
                Vector3 from = transform.position;
                Vector3 to = Vector3.MoveTowards(from, target, walkSpeed * Time.deltaTime);
                to.y = target.y;

                // Simple collision: don't walk through walls / solid props.
                Vector3 delta = to - from;
                float dist = delta.magnitude;
                if (dist > 0.0001f)
                {
                    Vector3 dir = delta / dist;
                    if (Physics.CapsuleCast(
                            from + Vector3.up * 0.3f,
                            from + Vector3.up * 1.2f,
                            0.18f,
                            dir,
                            out RaycastHit hit,
                            dist + 0.05f,
                            ~0,
                            QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                        {
                            // Stop short of the obstacle; mark arrived so routines don't hang forever.
                            _moveTarget = null;
                            SetWalking(false);
                            var blockedCb = _onArrive;
                            _onArrive = null;
                            blockedCb?.Invoke();
                            return;
                        }
                    }
                }

                transform.position = to;
                FaceToward(target);

                if (Vector3.Distance(transform.position, target) <= 0.3f)
                {
                    transform.position = target;
                    _moveTarget = null;
                    SetWalking(false);
                    var cb = _onArrive;
                    _onArrive = null;
                    cb?.Invoke();
                }
            }
        }

        void EnsureLabel(string text)
        {
            if (_label != null)
            {
                _label.text = text;
                return;
            }

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

        public void SetLabel(string text)
        {
            EnsureLabel(text);
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
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer == null || renderer.material == null)
                    continue;

                if (on)
                    renderer.material.color = Color.Lerp(renderer.material.color, Color.yellow, 0.45f);
            }
        }

        public void MarkExiting()
        {
            IsExiting = true;
            VacateSeat();
        }
    }
}
