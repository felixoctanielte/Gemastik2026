using PeduliTransit.Bootstrap;
using PeduliTransit.Player;
using PeduliTransit.UI;
using PeduliTransit.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PeduliTransit.EditorTools
{
    public static class PrototypeSetup
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string ControlsHint =
            "Kamera di MC: tahan KLIK KANAN + geser mouse = lihat sekitar | WASD = jalan | Scroll = zoom";

        [MenuItem("Peduli Transit/Setup Hierarchy Di Scene")]
        public static void SetupHierarchy()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyHierarchy(scene);
            EditorUtility.DisplayDialog(
                "Peduli Transit",
                "Hierarchy sudah dibuat di SampleScene!\n\n" +
                "PeduliTransit\n" +
                " ├ Systems / GameBootstrap (MC Casual1 + interior + phone auto-wire)\n" +
                " ├ UI / CameraControlsHint\n" +
                " └ World / Level / Player / FreeLookCamera\n\n" +
                ControlsHint + "\n\n" +
                "Tekan Play.",
                "OK");
        }

        [MenuItem("Peduli Transit/Update Camera Controls Di Scene")]
        public static void UpdateCameraControlsInScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyHierarchy(scene);
            EditorUtility.DisplayDialog(
                "Peduli Transit",
                "SampleScene di-update:\n\n• FreeLookCamera di Level/Player\n• Hint kontrol di UI\n• Main Camera scene dinonaktifkan\n\n" +
                ControlsHint,
                "OK");
        }

        [MenuItem("Peduli Transit/Fix Assets & Scene (Gemastik Ready)")]
        public static void FixAllForGemastik()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyHierarchy(scene);

            // Mirror phone + Casual1 into Resources for runtime fallback / build.
            EnsureResourcesCopy("Assets/Prefabs/Casual1.prefab", "Assets/Resources/Casual1.prefab");
            var phone = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/cell_phone.glb");
            if (phone != null)
            {
                var folder = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder("Assets", "Resources");
                var dest = "Assets/Resources/CellPhone.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(dest) == null)
                {
                    var instance = PrefabUtility.InstantiatePrefab(phone) as GameObject;
                    PrefabUtility.SaveAsPrefabAsset(instance, dest);
                    Object.DestroyImmediate(instance);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Peduli Transit",
                "Scene + assets sudah di-wire untuk testing Gemastik.\n\n" +
                "• MC Casual1 (cewek)\n" +
                "• Interior KRL / Bus / Angkot\n" +
                "• Phone cell_phone → Resources/CellPhone\n" +
                "• Kamera follow MC + anti tembus tembok\n" +
                "• Kasus sequential + jeda 7–10 detik\n\n" +
                "Buka SampleScene → Play.",
                "OK");
        }

        public static void BatchUpdateScene()
        {
            FixAllForGemastik();
        }

        static void EnsureResourcesCopy(string src, string dest)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(src) == null)
                return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(dest) != null)
                return;
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CopyAsset(src, dest);
        }

        static void ApplyHierarchy(Scene scene)
        {
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
            var playerFolder = GetOrCreate(level, "Player");

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

            WireBootstrapAssets(bootstrapGo.GetComponent<GameBootstrap>(), slots);
            SetupFreeLookCamera(playerFolder);
            SetupControlsHint(ui);
            RetireDefaultMainCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            Selection.activeGameObject = rootGo;
            EditorGUIUtility.PingObject(rootGo);
        }

        static void WireBootstrapAssets(GameBootstrap bootstrap, InteriorAssetSlots slots)
        {
            if (bootstrap == null)
                return;

            var so = new SerializedObject(bootstrap);

            AssignPrefab(so, "playerVisualPrefab",
                "Assets/Prefabs/Casual1.prefab",
                "Assets/AnimeGirls/Casual1/Casual1.prefab");
            AssignAnimator(so, "playerAnimatorController", "Assets/Controller/PlayerController.controller");
            AssignAnimator(so, "npcAnimatorController", "Assets/Controller/PlayerController.controller");
            AssignPrefab(so, "phonePrefab", "Assets/UI/cell_phone.glb");

            var npcProp = so.FindProperty("npcCharacterPrefabs");
            if (npcProp != null && npcProp.arraySize == 0)
            {
                string[] npcPaths =
                {
                    "Assets/Prefabs/npc_csl_00_character_01f_01.prefab",
                    "Assets/Prefabs/npc_csl_00_character_01f_03.prefab",
                    "Assets/Prefabs/npc_csl_00_character_02f_01.prefab",
                    "Assets/Prefabs/npc_csl_00_character_02f_02.prefab",
                    "Assets/Prefabs/npc_csl_00_character_02f_03.prefab",
                };
                npcProp.arraySize = npcPaths.Length;
                for (int i = 0; i < npcPaths.Length; i++)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(npcPaths[i]);
                    npcProp.GetArrayElementAtIndex(i).objectReferenceValue = go;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            if (slots != null)
            {
                var slotSo = new SerializedObject(slots);
                AssignToProperty(slotSo, "krlInteriorPrefab", "Assets/Prefabs/KRLWrapper.prefab");
                AssignToProperty(slotSo, "busInteriorPrefab", "Assets/Prefabs/BusWrapper.prefab");
                AssignToProperty(slotSo, "angkutanInteriorPrefab", "Assets/Prefabs/AngkotWrapper.prefab");
                slotSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void AssignPrefab(SerializedObject so, string propName, params string[] paths)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null)
                return;
            foreach (var path in paths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                prop.objectReferenceValue = go;
                return;
            }
        }

        static void AssignAnimator(SerializedObject so, string propName, string path)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null)
                return;
            prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        }

        static void AssignToProperty(SerializedObject so, string propName, string path)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null)
                return;
            prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        static void SetupFreeLookCamera(Transform playerFolder)
        {
            var existing = playerFolder.Find("FreeLookCamera");
            GameObject camGo;
            if (existing == null)
            {
                camGo = new GameObject("FreeLookCamera", typeof(Camera), typeof(AudioListener), typeof(FreeLookCamera));
                camGo.transform.SetParent(playerFolder, false);
            }
            else
            {
                camGo = existing.gameObject;
                if (camGo.GetComponent<Camera>() == null)
                    camGo.AddComponent<Camera>();
                if (camGo.GetComponent<AudioListener>() == null)
                    camGo.AddComponent<AudioListener>();
                if (camGo.GetComponent<FreeLookCamera>() == null)
                    camGo.AddComponent<FreeLookCamera>();
            }

            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 60f;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.tag = "MainCamera";
            cam.enabled = true;

            var freelook = camGo.GetComponent<FreeLookCamera>();
            freelook.Init(new Vector3(0f, 1.25f, 0f), yawDegrees: 200f, distance: 4.5f);
            freelook.LookEnabled = true;

            camGo.name = "FreeLookCamera";
        }

        static void SetupControlsHint(Transform uiRoot)
        {
            var hintTf = uiRoot.Find("CameraControlsHint");
            GameObject hintGo;
            if (hintTf == null)
            {
                hintGo = new GameObject("CameraControlsHint");
                hintGo.transform.SetParent(uiRoot, false);
            }
            else
            {
                hintGo = hintTf.gameObject;
            }

            var note = hintGo.GetComponent<CameraControlsSceneHint>();
            if (note == null)
                note = hintGo.AddComponent<CameraControlsSceneHint>();

            note.controls =
                "Kamera mengikuti MC cewek:\n" +
                "• Tahan KLIK KANAN + geser mouse = lihat sekitar\n" +
                "• WASD = jalan\n" +
                "• Scroll = zoom\n" +
                "• Kasus muncul satu per satu + jeda 7–10 detik";

            hintGo.name = "CameraControlsHint";
            hintGo.SetActive(true);

            var overlayTf = uiRoot.Find("CameraControlsOverlay");
            GameObject overlayGo;
            if (overlayTf == null)
            {
                overlayGo = new GameObject("CameraControlsOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                overlayGo.transform.SetParent(uiRoot, false);
            }
            else
            {
                overlayGo = overlayTf.gameObject;
            }

            var canvas = overlayGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = overlayGo.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = overlayGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (overlayGo.GetComponent<GraphicRaycaster>() == null)
                overlayGo.AddComponent<GraphicRaycaster>();

            var barTf = overlayGo.transform.Find("Bar");
            Image bar;
            if (barTf == null)
            {
                var barGo = new GameObject("Bar", typeof(RectTransform), typeof(Image));
                barGo.transform.SetParent(overlayGo.transform, false);
                bar = barGo.GetComponent<Image>();
            }
            else
            {
                bar = barTf.GetComponent<Image>();
            }

            bar.color = new Color(0.05f, 0.1f, 0.12f, 0.82f);
            bar.raycastTarget = false;
            var barRt = bar.rectTransform;
            barRt.anchorMin = new Vector2(0.08f, 0f);
            barRt.anchorMax = new Vector2(0.92f, 0f);
            barRt.pivot = new Vector2(0.5f, 0f);
            barRt.anchoredPosition = new Vector2(0f, 12f);
            barRt.sizeDelta = new Vector2(0f, 42f);

            var labelTf = bar.transform.Find("Label");
            Text label;
            if (labelTf == null)
            {
                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(bar.transform, false);
                label = labelGo.GetComponent<Text>();
            }
            else
            {
                label = labelTf.GetComponent<Text>();
            }

            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (label.font == null)
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 16;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.95f, 0.78f, 0.35f, 1f);
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.text = ControlsHint;

            var labelRt = label.rectTransform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10f, 4f);
            labelRt.offsetMax = new Vector2(-10f, -4f);

            overlayGo.SetActive(true);
        }

        static void RetireDefaultMainCamera()
        {
            var mainCam = GameObject.Find("Main Camera");
            if (mainCam == null)
                mainCam = GameObject.Find("Main Camera (Scene Default)");
            if (mainCam == null)
                mainCam = GameObject.Find("Main Camera (Scene Default — diganti FreeLook saat Play)");

            if (mainCam == null)
                return;

            mainCam.name = "Main Camera (Scene Default — diganti FreeLook saat Play)";
            mainCam.tag = "Untagged";

            var cam = mainCam.GetComponent<Camera>();
            if (cam != null)
                cam.enabled = false;

            var listener = mainCam.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = false;
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
