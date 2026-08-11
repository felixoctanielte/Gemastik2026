using UnityEngine;

namespace PeduliTransit.World
{
    public static class InteriorColliderUtility
    {
        /// <summary>
        /// Ensures imported interior meshes have colliders so the camera / player cannot pass through walls.
        /// </summary>
        public static void EnsureColliders(GameObject root)
        {
            if (root == null)
                return;

            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf == null || mf.sharedMesh == null)
                    continue;
                if (mf.GetComponent<Collider>() != null)
                    continue;

                var renderer = mf.GetComponent<MeshRenderer>();
                if (renderer == null || !renderer.enabled)
                    continue;

                Vector3 size = mf.sharedMesh.bounds.size;
                float maxDim = Mathf.Max(size.x, size.y, size.z) * MaxAbsScale(mf.transform);
                if (maxDim < 0.4f)
                    continue;

                // Prefer boxes for medium pieces; mesh for big shell.
                if (maxDim < 2.5f)
                {
                    var box = mf.gameObject.AddComponent<BoxCollider>();
                    box.center = mf.sharedMesh.bounds.center;
                    box.size = mf.sharedMesh.bounds.size;
                }
                else
                {
                    var meshCol = mf.gameObject.AddComponent<MeshCollider>();
                    meshCol.sharedMesh = mf.sharedMesh;
                    meshCol.convex = false;
                }
            }

            // Do NOT add SkinnedMesh colliders — they often become huge convex hulls and fling the player.
        }

        /// <summary>
        /// Solid invisible floor so CharacterController never falls through imperfect GLB mesh colliders.
        /// </summary>
        public static GameObject EnsurePlayableFloor(Transform parent, Bounds worldBounds)
        {
            if (parent == null)
                return null;

            var existing = parent.Find("GameplayFloor");
            if (existing != null)
                Object.Destroy(existing.gameObject);

            float width = Mathf.Max(worldBounds.size.x, 8f) + 4f;
            float depth = Mathf.Max(worldBounds.size.z, 4f) + 4f;
            float y = worldBounds.min.y;

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "GameplayFloor";
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(worldBounds.center.x, y - 0.05f, worldBounds.center.z);
            floor.transform.localScale = new Vector3(width, 0.1f, depth);

            var rend = floor.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false; // invisible but solid

            var col = floor.GetComponent<BoxCollider>();
            if (col != null)
                col.enabled = true;

            // Soft invisible side rails so player doesn't drop off aisle edges.
            CreateRail(parent, "Rail_L",
                new Vector3(worldBounds.center.x, y + 1.0f, worldBounds.max.z + 0.2f),
                new Vector3(width, 2f, 0.2f));
            CreateRail(parent, "Rail_R",
                new Vector3(worldBounds.center.x, y + 1.0f, worldBounds.min.z - 0.2f),
                new Vector3(width, 2f, 0.2f));

            return floor;
        }

        static void CreateRail(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = name;
            rail.transform.SetParent(parent, false);
            rail.transform.position = pos;
            rail.transform.localScale = scale;
            var rend = rail.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;
        }

        public static Bounds ComputeRendererBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            bool has = false;
            Bounds b = new Bounds();
            foreach (var r in renderers)
            {
                if (r == null || !r.enabled)
                    continue;
                if (!has)
                {
                    b = r.bounds;
                    has = true;
                }
                else b.Encapsulate(r.bounds);
            }

            if (!has)
                b = new Bounds(root.transform.position, new Vector3(8f, 3f, 4f));
            return b;
        }

        static float MaxAbsScale(Transform t)
        {
            Vector3 s = t.lossyScale;
            return Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        }
    }
}
