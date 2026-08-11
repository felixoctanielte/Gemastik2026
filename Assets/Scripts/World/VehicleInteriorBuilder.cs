using System.Collections;
using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.NPC;
using UnityEngine;

namespace PeduliTransit.World
{
    public class VehicleInteriorBuilder
    {
        readonly List<NpcPassenger> _npcs = new List<NpcPassenger>();
        readonly List<SeatSlot> _seats = new List<SeatSlot>();
        readonly List<Vector3> _standSpots = new List<Vector3>();
        GameObject _root;
        VehicleDoor _door;
        NpcPassenger _responder;
        TransportMode _mode;

        public GameObject[] CharacterPrefabs;
        public RuntimeAnimatorController NpcAnimatorController;

        public Transform Root => _root != null ? _root.transform : null;
        public IReadOnlyList<NpcPassenger> Npcs => _npcs;
        public IReadOnlyList<SeatSlot> Seats => _seats;
        public VehicleDoor Door => _door;
        public NpcPassenger Responder => _responder;
        public TransportMode Mode => _mode;

        public void Build(TransportMode mode, Transform parent)
        {
            Clear();
            _mode = mode;
            _root = new GameObject($"Interior_{mode}");
            _root.transform.SetParent(parent, false);

            Color floorColor = mode switch
            {
                TransportMode.Krl => new Color(0.25f, 0.28f, 0.32f),
                TransportMode.Bus => new Color(0.22f, 0.26f, 0.36f),
                _ => new Color(0.28f, 0.24f, 0.2f)
            };

            CreateBox("Floor", new Vector3(0f, 0f, 0f), new Vector3(14f, 0.2f, 6f), floorColor);
            CreateBox("Ceiling", new Vector3(0f, 3.2f, 0f), new Vector3(14f, 0.15f, 6f), floorColor * 0.8f);
            CreateBox("WallL", new Vector3(0f, 1.6f, 3.05f), new Vector3(14f, 3.2f, 0.15f), floorColor * 1.1f);
            CreateBox("WallR", new Vector3(0f, 1.6f, -3.05f), new Vector3(14f, 3.2f, 0.15f), floorColor * 1.1f);
            CreateBox("WallBack", new Vector3(-7.05f, 1.6f, 0f), new Vector3(0.15f, 3.2f, 6f), floorColor * 0.9f);

            CreateBox("WallFrontL", new Vector3(7.05f, 1.6f, 1.7f), new Vector3(0.15f, 3.2f, 2.4f), floorColor * 0.9f);
            CreateBox("WallFrontR", new Vector3(7.05f, 1.6f, -1.7f), new Vector3(0.15f, 3.2f, 2.4f), floorColor * 0.9f);

            BuildDoor();
            BuildSeats();
            BuildStandSpots();
            PlaceResponderStation(mode);
            SpawnCrowdWithSeating();
            var b = InteriorColliderUtility.ComputeRendererBounds(_root);
            RelayoutSeatsToInterior(b);
            InteriorDoorBinder.PlaceSideEntrance(_door, _root.transform, b);
            foreach (var npc in _npcs)
            {
                if (npc != null && npc.IsSitting && npc.AssignedSeat != null)
                    npc.SitAt(npc.AssignedSeat);
            }
        }

        public void BuildNpcsOnly(TransportMode mode, Transform parent)
        {
            Clear();
            _mode = mode;
            _root = new GameObject($"NPCs_{mode}");
            _root.transform.SetParent(parent, false);

            BuildDoor();
            BuildSeats();
            BuildStandSpots();
            PlaceResponderStation(mode);
            SpawnCrowdWithSeating();
            AlignCrowdToSiblingMesh(parent);
            BindDoorsToInteriorAsset(parent);
        }

        public void AlignCrowdToSiblingMesh(Transform interiorParent)
        {
            if (_root == null || interiorParent == null)
                return;

            bool hasBounds = false;
            Bounds b = new Bounds();
            foreach (var r in interiorParent.GetComponentsInChildren<Renderer>())
            {
                if (r == null || !r.enabled) continue;
                if (r.transform.IsChildOf(_root.transform)) continue;
                if (!hasBounds)
                {
                    b = r.bounds;
                    hasBounds = true;
                }
                else b.Encapsulate(r.bounds);
            }

            if (!hasBounds || b.size.magnitude < 1f || b.size.magnitude > 250f)
                return;

            Vector3 pos = new Vector3(b.center.x, Mathf.Clamp(b.min.y, -1f, 3f), b.center.z);
            _root.transform.position = pos;
        }

        public Vector3 GetViewFocus()
        {
            if (_root != null)
            {
                var p = _root.transform.TransformPoint(new Vector3(0f, 1.25f, 0f));
                if (p.y < 0.5f)
                    p.y = 1.25f;
                return p;
            }

            return new Vector3(0f, 1.25f, 0f);
        }

        void BuildDoor()
        {
            var doorRoot = new GameObject("Door");
            doorRoot.transform.SetParent(_root.transform, false);
            doorRoot.transform.localPosition = new Vector3(6.2f, 1.15f, 0f);

            var left = CreateBox("DoorLeft", doorRoot.transform.position + new Vector3(0f, 0f, 0.55f),
                new Vector3(0.12f, 2.3f, 1.05f), new Color(0.2f, 0.75f, 0.9f));
            left.transform.SetParent(doorRoot.transform, true);
            var right = CreateBox("DoorRight", doorRoot.transform.position + new Vector3(0f, 0f, -0.55f),
                new Vector3(0.12f, 2.3f, 1.05f), new Color(0.2f, 0.75f, 0.9f));
            right.transform.SetParent(doorRoot.transform, true);

            _door = doorRoot.AddComponent<VehicleDoor>();
            _door.Init(left.transform, right.transform,
                outside: doorRoot.transform.TransformPoint(new Vector3(1.6f, -1.15f, 0f)),
                inside: doorRoot.transform.TransformPoint(new Vector3(-1.6f, -1.15f, 0f)),
                autoAxis: false);
            _door.EnsurePassageCollidersDisabledWhileOpen = true;
        }

        void BindDoorsToInteriorAsset(Transform interiorParent)
        {
            if (_door == null || _root == null)
                return;

            GameObject assetRoot = null;
            foreach (Transform child in interiorParent)
            {
                if (child == _root.transform)
                    continue;
                assetRoot = child.gameObject;
                break;
            }

            Bounds b = assetRoot != null
                ? InteriorColliderUtility.ComputeRendererBounds(assetRoot)
                : InteriorColliderUtility.ComputeRendererBounds(_root);

            // Re-layout seats to the side benches of the real interior, then place door on the side wall.
            RelayoutSeatsToInterior(b);
            InteriorDoorBinder.PlaceSideEntrance(_door, _root.transform, b);

            // Re-apply sit on any already-spawned seated NPCs so they stick to the new slots.
            foreach (var npc in _npcs)
            {
                if (npc == null || !npc.IsSitting || npc.AssignedSeat == null)
                    continue;
                npc.SitAt(npc.AssignedSeat);
            }
        }

        void RelayoutSeatsToInterior(Bounds worldBounds)
        {
            if (_seats.Count == 0 || _root == null)
                return;

            bool longIsX = worldBounds.size.x >= worldBounds.size.z;
            float alongSize = longIsX ? worldBounds.size.x : worldBounds.size.z;
            float sideSize = longIsX ? worldBounds.size.z : worldBounds.size.x;
            float halfAlong = alongSize * 0.38f;
            float sideInset = Mathf.Max(0.55f, sideSize * 0.32f);
            float floorY = worldBounds.min.y;

            // Clear old visual cubes, keep SeatSlot components and retarget them.
            var visuals = new List<Transform>();
            foreach (Transform t in _root.transform)
            {
                if (t.name.StartsWith("SeatVis") || t.name.StartsWith("Backrest"))
                    visuals.Add(t);
            }
            foreach (var v in visuals)
                UnityEngine.Object.Destroy(v.gameObject);

            int perSide = Mathf.Max(3, _seats.Count / 2);
            int index = 0;
            for (int side = 0; side < 2; side++)
            {
                bool positiveSide = side == 0;
                Vector3 outDir = longIsX
                    ? (positiveSide ? Vector3.forward : Vector3.back)
                    : (positiveSide ? Vector3.right : Vector3.left);
                Vector3 along = longIsX ? Vector3.right : Vector3.forward;

                for (int i = 0; i < perSide && index < _seats.Count; i++, index++)
                {
                    float t = perSide == 1 ? 0f : (i / (float)(perSide - 1)) * 2f - 1f;
                    Vector3 pos = worldBounds.center + along * (t * halfAlong) + outDir * sideInset;
                    pos.y = floorY;

                    var slot = _seats[index];
                    slot.transform.position = pos;
                    // Face toward aisle (opposite of outDir).
                    slot.transform.rotation = Quaternion.LookRotation(-outDir, Vector3.up);
                    slot.cushionHeight = 0.42f;
                    slot.sitDepth = 0.05f;
                    slot.IsPriority = index >= _seats.Count - 4;

                    // Seat cushion visual flush with slot facing.
                    var cushion = CreateBox("SeatVis",
                        pos + Vector3.up * 0.42f - outDir * 0.05f,
                        new Vector3(0.75f, 0.1f, 0.55f),
                        slot.IsPriority ? new Color(0.8f, 0.4f, 0.2f) : new Color(0.32f, 0.38f, 0.42f));
                    cushion.transform.rotation = slot.transform.rotation;

                    var back = CreateBox("Backrest",
                        pos + Vector3.up * 0.75f + outDir * 0.22f,
                        new Vector3(0.75f, 0.55f, 0.08f),
                        new Color(0.28f, 0.32f, 0.36f));
                    back.transform.rotation = slot.transform.rotation;
                }
            }
        }

        void BuildSeats()
        {

            for (int i = -3; i <= 3; i++)
            {
                bool priority = i >= 2;
                CreateSeatSlot(new Vector3(i * 1.7f, 0.35f, 2.1f), true, priority);
                CreateSeatSlot(new Vector3(i * 1.7f, 0.35f, -2.1f), false, priority);
            }
        }

        void CreateSeatSlot(Vector3 pos, bool faceNegZ, bool priority)
        {
            // Seat pivot on floor; cushion surface ~0.45 above.
            Vector3 floorPos = new Vector3(pos.x, 0f, pos.z);
            Vector3 cushion = floorPos + Vector3.up * 0.42f;

            CreateBox("SeatVis", cushion, new Vector3(0.9f, 0.12f, 0.7f),
                priority ? new Color(0.75f, 0.35f, 0.2f) : new Color(0.35f, 0.4f, 0.45f));
            float backZ = faceNegZ ? 0.32f : -0.32f;
            CreateBox("Backrest", cushion + new Vector3(0f, 0.35f, backZ),
                new Vector3(0.9f, 0.7f, 0.1f), new Color(0.3f, 0.34f, 0.38f));

            var slotGo = new GameObject(priority ? "SeatPriority" : "SeatNormal");
            slotGo.transform.SetParent(_root.transform, false);
            slotGo.transform.position = floorPos;
            slotGo.transform.rotation = Quaternion.Euler(0f, faceNegZ ? 180f : 0f, 0f);
            var slot = slotGo.AddComponent<SeatSlot>();
            slot.IsPriority = priority;
            slot.cushionHeight = 0.38f;
            slot.sitDepth = 0.06f;
            _seats.Add(slot);
        }

        void BuildStandSpots()
        {
            for (int i = 0; i < 8; i++)
            {
                float x = -4.5f + i * 1.2f;
                _standSpots.Add(new Vector3(x, 0f, (i % 2 == 0) ? 0.55f : -0.55f));
            }
        }

        void PlaceResponderStation(TransportMode mode)
        {
            NpcRole role = mode switch
            {
                TransportMode.Krl => NpcRole.Security,
                TransportMode.Bus => NpcRole.TicketOfficer,
                _ => NpcRole.DriverAssistant
            };

            Vector3 station = mode == TransportMode.AngkutanUmum
                ? new Vector3(-5.8f, 0f, 0.9f)
                : new Vector3(-5.5f, 0f, 0f);

            _responder = CreateNpc(role, station, sitting: mode == TransportMode.AngkutanUmum);
            _responder.SetLabel(mode switch
            {
                TransportMode.Krl => "Satpam",
                TransportMode.Bus => "Petugas karcis",
                _ => "Anak buah sopir"
            });
            _npcs.Add(_responder);
        }

        void SpawnCrowdWithSeating()
        {

            var planned = new List<(NpcRole role, bool preferPriority, bool mustSit, bool mustStand)>
            {
                (NpcRole.PrioritySeatAbuse, true, true, false),
                (NpcRole.LoudTalking, false, false, false),
                (NpcRole.PhoneVolume, false, false, false),
                (NpcRole.HarassmentHint, false, false, false),
                (NpcRole.Fighting, false, false, true),

                (NpcRole.Pregnant, true, false, true),
                (NpcRole.Elderly, true, false, true),
                (NpcRole.CarryingChild, true, false, false),
                (NpcRole.Disability, true, false, false),
                (NpcRole.Normal, false, false, false),
                (NpcRole.Normal, false, false, false),
                (NpcRole.Normal, false, false, false),
            };

            int standIndex = 0;
            foreach (var p in planned)
            {
                SeatSlot seat = null;
                bool sitting = false;

                if (!p.mustStand)
                {
                    if (p.preferPriority || p.role == NpcRole.PrioritySeatAbuse)
                        seat = FindFreeSeat(priorityOnly: true) ?? FindFreeSeat(priorityOnly: false);
                    else
                        seat = FindFreeSeat(priorityOnly: false) ?? FindFreeSeat(priorityOnly: true);

                    if (seat != null && (p.mustSit || p.role == NpcRole.PrioritySeatAbuse || !IsCrowdSeatFullThreshold()))
                        sitting = true;
                    else
                        seat = null;
                }

                Vector3 pos;
                if (sitting && seat != null)
                    pos = seat.SitWorldPosition;
                else
                {
                    pos = standIndex < _standSpots.Count
                        ? _standSpots[standIndex++]
                        : new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-0.8f, 0.8f));
                }

                var npc = CreateNpc(p.role, pos, sitting);
                if (sitting && seat != null)
                    npc.AssignSeat(seat);
                else
                    npc.StandAt(pos, Quaternion.Euler(0f, pos.z > 0 ? 180f : 0f, 0f));

                _npcs.Add(npc);
            }
        }

        bool IsCrowdSeatFullThreshold()
        {
            int occupied = 0;
            foreach (var s in _seats)
                if (s.IsOccupied) occupied++;

            return occupied >= _seats.Count - 1;
        }

        SeatSlot FindFreeSeat(bool priorityOnly)
        {
            foreach (var s in _seats)
            {
                if (s.IsOccupied)
                    continue;
                if (priorityOnly && !s.IsPriority)
                    continue;
                if (!priorityOnly && s.IsPriority)
                    continue;
                return s;
            }

            return null;
        }

        public SeatSlot FindAnyFreeSeat(bool preferPriority = false)
        {
            if (preferPriority)
            {
                var p = FindFreeSeat(true);
                if (p != null) return p;
            }

            return FindFreeSeat(false) ?? FindFreeSeat(true);
        }

        public SeatSlot FindPrioritySeatWronglyOccupied()
        {
            foreach (var s in _seats)
            {
                if (!s.IsPriority || !s.IsOccupied)
                    continue;
                if (!s.OccupantAllowedOnPriority(s.Occupant.Role))
                    return s;
            }

            return null;
        }

        NpcPassenger CreateNpc(NpcRole role, Vector3 pos, bool sitting)
        {
            GameObject body;
            bool usingCharacterModel = CharacterPrefabs != null && CharacterPrefabs.Length > 0;

            if (usingCharacterModel)
            {
                var prefab = CharacterPrefabs[Random.Range(0, CharacterPrefabs.Length)];
                body = UnityEngine.Object.Instantiate(prefab, _root.transform);
                body.name = $"NPC_{role}";
                body.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
                body.transform.localRotation = Quaternion.Euler(0f, pos.z > 0 ? 180f : 0f, 0f);
            }
            else
            {
                body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = $"NPC_{role}";
                body.transform.SetParent(_root.transform, false);
                body.transform.position = pos;
                body.transform.localScale = sitting ? new Vector3(0.7f, 0.55f, 0.7f) : new Vector3(0.7f, 0.9f, 0.7f);
                UnityEngine.Object.Destroy(body.GetComponent<Collider>());
            }

            var npc = body.GetComponent<NpcPassenger>();
            if (npc == null)
                npc = body.AddComponent<NpcPassenger>();

            npc.Setup(
                role,
                sitting,
                ColorFor(role),
                NpcAnimatorController,
                tintMaterial: !usingCharacterModel,
                usingCharacterModel: usingCharacterModel
            );
            return npc;
        }

        static Color ColorFor(NpcRole role)
        {
            return role switch
            {
                NpcRole.LoudTalking => new Color(0.9f, 0.35f, 0.3f),
                NpcRole.PrioritySeatAbuse => new Color(0.95f, 0.55f, 0.2f),
                NpcRole.PhoneVolume => new Color(0.4f, 0.55f, 0.95f),
                NpcRole.HarassmentHint => new Color(0.7f, 0.2f, 0.35f),
                NpcRole.Fighting => new Color(0.85f, 0.15f, 0.15f),
                NpcRole.Pregnant => new Color(0.85f, 0.7f, 0.9f),
                NpcRole.CarryingChild => new Color(0.55f, 0.8f, 0.55f),
                NpcRole.Disability => new Color(0.45f, 0.75f, 0.85f),
                NpcRole.Elderly => new Color(0.8f, 0.75f, 0.55f),
                NpcRole.Security => new Color(0.2f, 0.35f, 0.55f),
                NpcRole.TicketOfficer => new Color(0.25f, 0.45f, 0.35f),
                NpcRole.DriverAssistant => new Color(0.45f, 0.35f, 0.2f),
                _ => new Color(0.65f, 0.68f, 0.7f)
            };
        }

        GameObject CreateBox(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_root.transform, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
            return go;
        }

        public NpcPassenger FindByRole(NpcRole role)
        {
            foreach (var n in _npcs)
            {
                if (n != null && n.Role == role && !n.IsExiting)
                    return n;
            }

            return null;
        }

        public IEnumerator BoardPassengerRoutine(NpcRole role, bool preferSit)
        {
            if (_door == null)
                yield break;

            yield return _door.Open();
            PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.DoorOpen);
            PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.NpcBoard);

            var npc = CreateNpc(role, _door.OutsidePoint, false);
            _npcs.Add(npc);
            yield return npc.WalkToRoutine(_door.InsidePoint);

            SeatSlot seat = preferSit ? FindAnyFreeSeat(NpcPassenger.IsPriorityRole(role)) : null;
            if (seat != null)
            {
                yield return npc.WalkToRoutine(seat.SitWorldPosition);
                npc.AssignSeat(seat);
                // Biarkan 1 frame pose duduk settle.
                yield return null;
            }
            else
            {
                Vector3 stand = _standSpots.Count > 0
                    ? _root.transform.TransformPoint(_standSpots[Random.Range(0, _standSpots.Count)])
                    : _door.InsidePoint + new Vector3(-1.5f, 0f, 0f);
                yield return npc.WalkToRoutine(stand);
                npc.StandAt(stand, Quaternion.identity);
            }

            yield return _door.Close();
            PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.DoorClose);
        }

        public IEnumerator ResponderResolveRoutine(NpcPassenger culprit, bool escortOff)
        {
            if (_responder == null || culprit == null)
                yield break;

            if (_door != null && !_door.IsOpen)
            {
                yield return _door.Open();
                PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.DoorOpen);
            }

            Vector3 approach = culprit.transform.position + (culprit.transform.position - _responder.transform.position).normalized * -0.9f;
            approach.y = 0f;
            yield return _responder.WalkToRoutine(approach);
            PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.ResponderArrive);
            yield return new WaitForSeconds(0.6f);

            if (escortOff && _door != null)
            {
                culprit.MarkExiting();
                culprit.Highlight(true);
                yield return culprit.WalkToRoutine(_door.InsidePoint);
                var escortTarget = culprit.transform.position;
                yield return _responder.WalkToRoutine(escortTarget);
                yield return culprit.WalkToRoutine(_door.OutsidePoint);
                yield return _responder.WalkToRoutine(_door.OutsidePoint + Vector3.right * 0.6f);
                culprit.gameObject.SetActive(false);
                PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.EscortResolve);
                yield return _responder.WalkToRoutine(_door.InsidePoint);
                yield return _door.Close();
                PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.DoorClose);

                Vector3 stationLocal = _mode == TransportMode.AngkutanUmum
                    ? new Vector3(-5.8f, 0f, 0.9f)
                    : new Vector3(-5.5f, 0f, 0f);
                Vector3 station = _root.transform.TransformPoint(stationLocal);
                yield return _responder.WalkToRoutine(station);
            }
            else
            {

                if (culprit.AssignedSeat != null && culprit.AssignedSeat.IsPriority)
                {
                    var freed = culprit.AssignedSeat;
                    culprit.VacateSeat();
                    Vector3 standLocal = _standSpots.Count > 0 ? _standSpots[0] : new Vector3(0f, 0f, 0.6f);
                    Vector3 stand = _root.transform.TransformPoint(standLocal);
                    yield return culprit.WalkToRoutine(stand);
                    culprit.StandAt(stand, Quaternion.identity);

                    TrySeatNearestPriorityEligible(freed);
                }

                Vector3 stationLocal2 = _mode == TransportMode.AngkutanUmum
                    ? new Vector3(-5.8f, 0f, 0.9f)
                    : new Vector3(-5.5f, 0f, 0f);
                Vector3 station2 = _root.transform.TransformPoint(stationLocal2);
                yield return _responder.WalkToRoutine(station2);
                if (_door != null && _door.IsOpen)
                    yield return _door.Close();
            }
        }

        public void TrySeatNearestPriorityEligible(SeatSlot seat)
        {
            if (seat == null || seat.IsOccupied)
                return;

            NpcPassenger best = null;
            float bestDist = float.MaxValue;
            foreach (var n in _npcs)
            {
                if (n == null || n.IsSitting || !n.IsPriorityEligible || n.IsExiting)
                    continue;
                float d = Vector3.Distance(n.transform.position, seat.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = n;
                }
            }

            if (best != null)
                best.AssignSeat(seat);
        }

        public IEnumerator GiveSeatTo(NpcRole role)
        {
            var needy = FindByRole(role);
            if (needy == null)
                yield break;

            if (needy.IsSitting)
                yield break;

            var seat = FindAnyFreeSeat(preferPriority: true);
            if (seat == null)
            {

                foreach (var s in _seats)
                {
                    if (s.IsOccupied && s.Occupant != null && s.Occupant.Role == NpcRole.Normal)
                    {
                        var donor = s.Occupant;
                        donor.VacateSeat();
                        Vector3 standLocal = _standSpots.Count > 0
                            ? _standSpots[Random.Range(0, _standSpots.Count)]
                            : new Vector3(0f, 0f, 0.6f);
                        Vector3 stand = _root.transform.TransformPoint(standLocal);
                        donor.StandAt(stand, Quaternion.identity);
                        seat = s;
                        break;
                    }
                }
            }

            if (seat == null)
                yield break;

            yield return needy.WalkToRoutine(seat.SitWorldPosition);
            needy.AssignSeat(seat);
            PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.SeatGive);
        }

        public IEnumerator PlayerNegurPriorityRoutine(NpcPassenger abuser)
        {
            if (abuser == null)
                yield break;

            abuser.Highlight(true);
            yield return new WaitForSeconds(0.4f);
            var seat = abuser.AssignedSeat;
            abuser.VacateSeat();
            Vector3 standLocal = _standSpots.Count > 0
                ? _standSpots[1 % _standSpots.Count]
                : new Vector3(-1f, 0f, 0.5f);
            Vector3 stand = _root.transform.TransformPoint(standLocal);
            yield return abuser.WalkToRoutine(stand);
            abuser.StandAt(stand, Quaternion.identity);
            TrySeatNearestPriorityEligible(seat);
            PeduliTransit.Audio.AudioManager.Instance?.PlaySfx(PeduliTransit.Audio.SfxId.NegurSuccess);
        }

        public IEnumerator IntroBoardDemo()
        {
            yield return BoardPassengerRoutine(NpcRole.Normal, preferSit: true);
            yield return BoardPassengerRoutine(NpcRole.Normal, preferSit: true);
            yield return BoardPassengerRoutine(NpcRole.Elderly, preferSit: true);
        }

        public void Clear()
        {
            _npcs.Clear();
            _seats.Clear();
            _standSpots.Clear();
            _door = null;
            _responder = null;
            if (_root != null)
                UnityEngine.Object.Destroy(_root);
            _root = null;
        }
    }
}
