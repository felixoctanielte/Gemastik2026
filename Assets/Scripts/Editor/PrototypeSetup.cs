using PeduliTransit.Bootstrap;
using PeduliTransit.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeduliTransit.EditorTools
{
    public static class PrototypeSetup
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Peduli Transit/Setup Hierarchy Di Scene")]
        public static void SetupHierarchy()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var rootGo = GameObject.Find("PeduliTransit");
            if (rootGo == null)
                rootGo = new GameObject("PeduliTransit");

            var systems = GetOrCreate(rootGo.transform, "Systems");
            var ui = GetOrCreate(rootGo.transform, "UI");
            var world = GetOrCreate(rootGo.transform, "World");
            var hub = GetOrCreate(world, "Hub");
            var level = GetOrCreate(world, "Level");
            var spawns = GetOrCreate(world, "Spawns");
            var interiorAnchor = GetOrCreate(spawns, "InteriorAnchor");
            var playerSpawn = GetOrCreate(spawns, "PlayerSpawn");
            playerSpawn.position = new Vector3(0f, 0.1f, 0f);

            GetOrCreate(level, "Interior_KRL_PlacePrefabHere");
            GetOrCreate(level, "Interior_Bus_PlacePrefabHere");
            GetOrCreate(level, "Interior_Angkutan_PlacePrefabHere");
            GetOrCreate(level, "NPCs");
            GetOrCreate(level, "Player");

            var slotTf = GetOrCreate(level, "InteriorSlot");
            var slots = slotTf.GetComponent<InteriorAssetSlots>();
            if (slots == null)
                slots = slotTf.gameObject.AddComponent<InteriorAssetSlots>();
            slots.interiorAnchor = interiorAnchor;
            slots.playerSpawn = playerSpawn;

            var bootstrapTf = systems.Find("GameBootstrap");
            GameObject bootstrapGo;
            if (bootstrapTf == null)
            {
                bootstrapGo = new GameObject("GameBootstrap");
                bootstrapGo.transform.SetParent(systems, false);
            }
            else
            {
                bootstrapGo = bootstrapTf.gameObject;
            }

            if (bootstrapGo.GetComponent<GameBootstrap>() == null)
                bootstrapGo.AddComponent<GameBootstrap>();

            // Soft-disable default scene camera name clarity
            var mainCam = GameObject.Find("Main Camera");
            if (mainCam != null)
                mainCam.name = "Main Camera (Scene Default)";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            Selection.activeGameObject = rootGo;
            EditorGUIUtility.PingObject(rootGo);

            EditorUtility.DisplayDialog(
                "Peduli Transit",
                "Hierarchy sudah dibuat di SampleScene!\n\n" +
                "PeduliTransit\n" +
                " ├ Systems / GameBootstrap\n" +
                " ├ UI\n" +
                " └ World / Level / Spawns / InteriorSlot\n\n" +
                "Teman nanti drag prefab Sketchfab ke InteriorAssetSlots (KRL/Bus/Angkutan).\n" +
                "Kalau kosong, game pakai interior primitif.\n\n" +
                "Tekan Play.",
                "OK");
        }

        static Transform GetOrCreate(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null)
                return t;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}
