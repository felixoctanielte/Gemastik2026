using System.Collections.Generic;
using UnityEngine;

namespace PeduliTransit.World
{
    /// <summary>
    /// Places / keeps visible sliding door leaves on the vehicle SIDE entrance (not inside the aisle).
    /// </summary>
    public static class InteriorDoorBinder
    {
        public static void PlaceSideEntrance(VehicleDoor door, Transform npcRoot, Bounds worldBounds)
        {
            if (door == null || npcRoot == null)
                return;

            EnsureProxyLeaves(door);

            // Long axis of carriage = door track; short axis = outward through the doorway.
            bool longIsX = worldBounds.size.x >= worldBounds.size.z;
            Vector3 outward = longIsX ? Vector3.forward : Vector3.right;
            Vector3 along = longIsX ? Vector3.right : Vector3.forward;

            // Prefer the + side wall with a slight inset so the door sits in the doorway, not the aisle centre.
            float halfSide = (longIsX ? worldBounds.size.z : worldBounds.size.x) * 0.5f;
            float floorY = worldBounds.min.y;
            Vector3 center = worldBounds.center;

            Vector3 doorPos = center + outward * (halfSide - 0.15f);
            doorPos.y = floorY + 1.15f;
            // Park door near mid-carriage (slightly toward one boarding bay).
            doorPos += along * (longIsX ? worldBounds.size.x * 0.08f : worldBounds.size.z * 0.08f);

            door.transform.position = doorPos;
            // Local +Z = along-track so leaves slide left/right along the wall; outward is -right.
            door.transform.rotation = Quaternion.LookRotation(along, Vector3.up);

            var left = door.LeftLeaf;
            var right = door.RightLeaf;
            if (left != null)
            {
                left.localPosition = new Vector3(0f, 0f, 0.7f);
                left.localRotation = Quaternion.identity;
                left.localScale = new Vector3(0.14f, 2.2f, 1.2f);
                left.gameObject.SetActive(true);
            }
            if (right != null)
            {
                right.localPosition = new Vector3(0f, 0f, -0.7f);
                right.localRotation = Quaternion.identity;
                right.localScale = new Vector3(0.14f, 2.2f, 1.2f);
                right.gameObject.SetActive(true);
            }

            Vector3 outside = doorPos + outward * 1.9f;
            Vector3 inside = doorPos - outward * 1.5f;
            outside.y = floorY;
            inside.y = floorY;

            door.Init(left, right, outside, inside, autoAxis: false);
            door.EnsurePassageCollidersDisabledWhileOpen = true;
            door.SetPassagePoints(outside, inside);

            // Parent stays under NPC root so it follows align.
            if (door.transform.parent != npcRoot)
                door.transform.SetParent(npcRoot, true);
        }

        public static void BindOrCreate(VehicleDoor door, GameObject interiorRoot, Transform npcRoot, Vector3 preferredInsideLocal)
        {
            if (door == null)
                return;

            Bounds b = interiorRoot != null
                ? InteriorColliderUtility.ComputeRendererBounds(interiorRoot)
                : new Bounds(npcRoot != null ? npcRoot.position : Vector3.zero, new Vector3(12f, 3f, 4f));

            if (npcRoot != null)
                PlaceSideEntrance(door, npcRoot, b);
            else
                EnsureProxyLeaves(door);
        }

        static void EnsureProxyLeaves(VehicleDoor door)
        {
            Transform holder = door.transform;
            var left = holder.Find("DoorLeft") ?? holder.Find("DoorLeaf_L");
            var right = holder.Find("DoorRight") ?? holder.Find("DoorLeaf_R");

            if (left == null)
                left = CreateLeaf(holder, "DoorLeft", new Vector3(0f, 0f, 0.7f)).transform;
            if (right == null)
                right = CreateLeaf(holder, "DoorRight", new Vector3(0f, 0f, -0.7f)).transform;

            StyleLeaf(left.gameObject);
            StyleLeaf(right.gameObject);
            door.LeftLeaf = left;
            door.RightLeaf = right;
        }

        static GameObject CreateLeaf(Transform parent, string name, Vector3 localPos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(0.14f, 2.2f, 1.2f);
            return go;
        }

        static void StyleLeaf(GameObject go)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend == null)
                return;
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = new Color(0.15f, 0.8f, 0.95f, 1f);
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", new Color(0.15f, 0.45f, 0.6f));
        }
    }
}
