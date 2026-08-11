using UnityEngine;

namespace PeduliTransit.NPC
{
    /// <summary>
    /// Procedural sit pose for Casual1 / NPC rigs. Snaps hips onto the seat cushion.
    /// </summary>
    public class NpcSitPose : MonoBehaviour
    {
        Animator _animator;
        Transform _hips;
        Transform _spine;
        Transform _thighL;
        Transform _shinL;
        Transform _thighR;
        Transform _shinR;

        bool _sitting;
        bool _bound;
        Vector3 _seatSurface;
        Vector3 _seatForward;

        Quaternion _hipsRest, _spineRest, _thighLRest, _shinLRest, _thighRRest, _shinRRest;
        bool _restCached;

        public bool IsSitting => _sitting;

        public void Bind(Animator animator)
        {
            _animator = animator != null ? animator : GetComponentInChildren<Animator>();
            ResolveBones();
            CacheRest();
            _bound = _hips != null && (_thighL != null || _thighR != null);
        }

        public void Sit(Vector3 seatSurface, Vector3 seatForward)
        {
            if (!_bound)
                Bind(_animator);

            _seatSurface = seatSurface;
            _seatForward = seatForward;
            _seatForward.y = 0f;
            if (_seatForward.sqrMagnitude < 0.001f)
                _seatForward = transform.forward;
            _seatForward.Normalize();

            _sitting = true;
            if (_animator != null)
                _animator.enabled = false;

            // Face the seat, plant root under the cushion, then bend legs.
            transform.rotation = Quaternion.LookRotation(_seatForward, Vector3.up);
            if (_hips != null)
            {
                Vector3 hipOffset = _hips.position - transform.position;
                transform.position = _seatSurface - new Vector3(hipOffset.x, 0f, hipOffset.z) - Vector3.up * 0.02f;
            }
            else
            {
                transform.position = _seatSurface - Vector3.up * 0.35f;
            }

            ApplyLocalSitPose();
            SnapHipsToSeat(true);
        }

        public void Stand()
        {
            _sitting = false;
            RestoreRest();
            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.speed = 0f;
            }
        }

        void LateUpdate()
        {
            if (!_sitting)
                return;
            if (!_bound)
                Bind(_animator);
            ApplyLocalSitPose();
            SnapHipsToSeat(false);
        }

        void SnapHipsToSeat(bool force)
        {
            if (_hips == null)
                return;

            Vector3 desired = _seatSurface + Vector3.up * 0.01f;
            Vector3 delta = desired - _hips.position;
            if (force)
                transform.position += delta;
            else
                transform.position += new Vector3(delta.x * 0.4f, delta.y, delta.z * 0.4f);
        }

        void ApplyLocalSitPose()
        {
            // Thigh forward fold + shin tuck — Casual1 / Blender Y-up local X.
            if (_thighL != null)
                _thighL.localRotation = _thighLRest * Quaternion.Euler(78f, 6f, 0f);
            if (_thighR != null)
                _thighR.localRotation = _thighRRest * Quaternion.Euler(78f, -6f, 0f);
            if (_shinL != null)
                _shinL.localRotation = _shinLRest * Quaternion.Euler(90f, 0f, 0f);
            if (_shinR != null)
                _shinR.localRotation = _shinRRest * Quaternion.Euler(90f, 0f, 0f);
            if (_hips != null)
                _hips.localRotation = _hipsRest * Quaternion.Euler(-8f, 0f, 0f);
            if (_spine != null)
                _spine.localRotation = _spineRest * Quaternion.Euler(6f, 0f, 0f);
        }

        void CacheRest()
        {
            if (_restCached)
                return;
            if (_hips != null) _hipsRest = _hips.localRotation;
            if (_spine != null) _spineRest = _spine.localRotation;
            if (_thighL != null) _thighLRest = _thighL.localRotation;
            if (_shinL != null) _shinLRest = _shinL.localRotation;
            if (_thighR != null) _thighRRest = _thighR.localRotation;
            if (_shinR != null) _shinRRest = _shinR.localRotation;
            _restCached = true;
        }

        void RestoreRest()
        {
            if (!_restCached)
                return;
            if (_hips != null) _hips.localRotation = _hipsRest;
            if (_spine != null) _spine.localRotation = _spineRest;
            if (_thighL != null) _thighL.localRotation = _thighLRest;
            if (_shinL != null) _shinL.localRotation = _shinLRest;
            if (_thighR != null) _thighR.localRotation = _thighRRest;
            if (_shinR != null) _shinR.localRotation = _shinRRest;
        }

        void ResolveBones()
        {
            if (_animator != null && _animator.isHuman)
            {
                _hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
                _spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
                _thighL = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                _shinL = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                _thighR = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                _shinR = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            }

            _hips ??= FindBone("Hip", "Hips", "B-hips", "pelvis", "Pelvis");
            _spine ??= FindBone("Spine", "B-spine");
            _thighL ??= FindBone("ThighL", "Thigh.L", "LeftUpLeg", "LeftUpperLeg", "B-thigh.L");
            _thighR ??= FindBone("ThighR", "Thigh.R", "RightUpLeg", "RightUpperLeg", "B-thigh.R");
            _shinL ??= FindBone("ShinL", "Shin.L", "LeftLeg", "LeftLowerLeg", "B-shin.L");
            _shinR ??= FindBone("ShinR", "Shin.R", "RightLeg", "RightLowerLeg", "B-shin.R");
        }

        Transform FindBone(params string[] names)
        {
            var all = GetComponentsInChildren<Transform>(true);
            foreach (var name in names)
            {
                foreach (var t in all)
                {
                    if (t != null && string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase))
                        return t;
                }
            }
            foreach (var name in names)
            {
                foreach (var t in all)
                {
                    if (t != null && t.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return t;
                }
            }
            return null;
        }
    }
}
