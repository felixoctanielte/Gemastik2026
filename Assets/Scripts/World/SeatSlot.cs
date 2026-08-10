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

        public Vector3 SitWorldPosition => transform.position + Vector3.up * 0.05f;
        public Quaternion SitFacing => transform.rotation;

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
