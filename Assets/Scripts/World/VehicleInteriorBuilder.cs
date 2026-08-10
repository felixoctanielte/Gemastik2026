using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.NPC;
using UnityEngine;

namespace PeduliTransit.World
{
    public class VehicleInteriorBuilder
    {
        readonly List<NpcPassenger> _npcs = new List<NpcPassenger>();
        GameObject _root;

        /// <summary>
        /// Prefab karakter (npc_csl_00_character_01f, 02f, dst). Di-assign dari GameBootstrap.
        /// Kalau kosong, fallback ke capsule lama.
        /// </summary>
        public GameObject[] CharacterPrefabs;

        public RuntimeAnimatorController NpcAnimatorController;

        public Transform Root => _root != null ? _root.transform : null;
        public IReadOnlyList<NpcPassenger> Npcs => _npcs;

        public void Build(TransportMode mode, Transform parent)
        {
            Clear();
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
            CreateBox("WallFront", new Vector3(7.05f, 1.6f, 0f), new Vector3(0.15f, 3.2f, 6f), floorColor * 0.9f);

            for (int i = -3; i <= 3; i++)
            {
                CreateSeat(new Vector3(i * 1.7f, 0.35f, 2.1f), true);
                CreateSeat(new Vector3(i * 1.7f, 0.35f, -2.1f), true);
            }

            SpawnCrowd();
        }

        /// <summary>
        /// Dipakai kalau teman sudah pasang prefab interior (mis. KRLWrapper) — hanya spawn NPC.
        /// </summary>
        public void BuildNpcsOnly(TransportMode mode, Transform parent)
        {
            Clear();
            _root = new GameObject($"NPCs_{mode}");
            _root.transform.SetParent(parent, false);
            SpawnCrowd();
        }

        void CreateSeat(Vector3 pos, bool priorityTint)
        {
            CreateBox("Seat", pos, new Vector3(0.9f, 0.45f, 0.7f),
                priorityTint ? new Color(0.75f, 0.35f, 0.2f) : new Color(0.35f, 0.4f, 0.45f));
            CreateBox("Backrest", pos + new Vector3(0f, 0.55f, pos.z > 0 ? 0.35f : -0.35f),
                new Vector3(0.9f, 0.7f, 0.12f), new Color(0.3f, 0.34f, 0.38f));
        }

        void SpawnCrowd()
        {
            var roles = new[]
            {
                NpcRole.LoudTalking,
                NpcRole.PrioritySeatAbuse,
                NpcRole.PhoneVolume,
                NpcRole.HarassmentHint,
                NpcRole.Pregnant,
                NpcRole.CarryingChild,
                NpcRole.Disability,
                NpcRole.Elderly,
                NpcRole.Normal,
                NpcRole.Normal
            };

            for (int i = 0; i < roles.Length; i++)
            {
                bool sitting = i % 3 != 0;
                float x = -5.5f + i * 1.15f;
                float z = sitting ? (i % 2 == 0 ? 2.1f : -2.1f) : (i % 2 == 0 ? 0.6f : -0.6f);
                float y = sitting ? 0.95f : 1.0f;
                var npc = CreateNpc(roles[i], new Vector3(x, y, z), sitting);
                _npcs.Add(npc);
            }
        }

        NpcPassenger CreateNpc(NpcRole role, Vector3 pos, bool sitting)
        {
            GameObject body;
            bool usingCharacterModel = CharacterPrefabs != null && CharacterPrefabs.Length > 0;

            if (usingCharacterModel)
            {
                var prefab = CharacterPrefabs[Random.Range(0, CharacterPrefabs.Length)];
                body = Object.Instantiate(prefab, _root.transform);
                body.name = $"NPC_{role}";

                // Model karakter pivot-nya di kaki (y=0), beda dari capsule yang pivot-nya di tengah.
                body.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
                // Hadapkan NPC ke tengah gerbong (baris seat kiri hadap kanan, kanan hadap kiri).
                body.transform.localRotation = Quaternion.Euler(0f, pos.z > 0 ? 180f : 0f, 0f);
            }
            else
            {
                // Fallback capsule kalau CharacterPrefabs belum di-assign di Inspector.
                body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = $"NPC_{role}";
                body.transform.SetParent(_root.transform, false);
                body.transform.position = pos;
                body.transform.localScale = sitting ? new Vector3(0.7f, 0.55f, 0.7f) : new Vector3(0.7f, 0.9f, 0.7f);
                Object.Destroy(body.GetComponent<Collider>());
            }

            var npc = body.GetComponent<NpcPassenger>();
            if (npc == null)
                npc = body.AddComponent<NpcPassenger>();

            npc.Setup(
                role,
                sitting,
                ColorFor(role),
                NpcAnimatorController,
                tintMaterial: !usingCharacterModel
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
                NpcRole.Pregnant => new Color(0.85f, 0.7f, 0.9f),
                NpcRole.CarryingChild => new Color(0.55f, 0.8f, 0.55f),
                NpcRole.Disability => new Color(0.45f, 0.75f, 0.85f),
                NpcRole.Elderly => new Color(0.8f, 0.75f, 0.55f),
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
                if (n.Role == role)
                    return n;
            }

            return _npcs.Count > 0 ? _npcs[0] : null;
        }

        public void Clear()
        {
            _npcs.Clear();
            if (_root != null)
                Object.Destroy(_root);
            _root = null;
        }
    }
}