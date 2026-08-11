using PeduliTransit.Core;
using PeduliTransit.NPC;
using UnityEngine;

namespace PeduliTransit.World
{
    public class SeatSlot : MonoBehaviour
    {
        public bool IsPriority;
        public bool IsOccupied => Occupant != null;
        public NpcPassenger Occupant { get; private set; }

        [Tooltip("Tinggi bantalan kursi dari pivot (lantai).")]
        public float cushionHeight = 0.38f;
        public float sitDepth = 0.06f;

        public Vector3 SitSurfaceWorld =>
            transform.position + Vector3.up * cushionHeight + transform.forward * sitDepth;

        // Root di dekat lantai kursi; hips di-snap ke cushion oleh NpcSitPose.
        public Vector3 SitWorldPosition =>
            transform.position + Vector3.up * 0.01f - transform.forward * 0.04f;

        public Quaternion SitFacing => transform.rotation;
        public Vector3 SitForward => transform.forward;

        public bool TryOccupy(NpcPassenger passenger)
        {
            if (passenger == null)
                return false;
            if (IsOccupied && Occupant != passenger)
                return false;

            Occupant = passenger;
            return true;
        }

        public void Vacate(NpcPassenger passenger = null)
        {
            if (passenger != null && Occupant != passenger)
                return;
            Occupant = null;
        }

        public bool OccupantAllowedOnPriority(NpcRole role)
        {
            return role == NpcRole.Pregnant
                || role == NpcRole.Elderly
                || role == NpcRole.Disability
                || role == NpcRole.CarryingChild;
        }
    }
}
