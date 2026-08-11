using PeduliTransit.Core;
using PeduliTransit.Events;
using PeduliTransit.Managers;
using PeduliTransit.Player;
using PeduliTransit.UI;
using PeduliTransit.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeduliTransit.Bootstrap
{

    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("Hierarchy roots (auto-filled jika kosong)")]
        [SerializeField] Transform systemsRoot;
        [SerializeField] Transform uiRoot;
        [SerializeField] Transform worldRoot;
        [SerializeField] InteriorAssetSlots interiorSlots;

        [Header("NPC")]
        [Tooltip("Drag prefab npc_csl_00_character_01f, 02f, dst dari Project window. Kosong = fallback capsule.")]
        [SerializeField] GameObject[] npcCharacterPrefabs;
        [SerializeField] RuntimeAnimatorController npcAnimatorController;

        [Header("Player Visual")]
        [Tooltip("Drag prefab Casual1 (Assets/Prefabs/Casual1) — MC cewek")]
        [SerializeField] GameObject playerVisualPrefab;
        [SerializeField] Vector3 playerVisualOffset = Vector3.zero;
        [SerializeField] RuntimeAnimatorController playerAnimatorController;
        [SerializeField] GameObject phonePrefab;

        Canvas _canvas;
        LoginUI _login;
        HubUI _hub;
        GameplayUI _gameplayUi;
        EventDirector _director;
        VehicleInteriorBuilder _worldBuilder;
        FreeLookCamera _camera;
        PlayerMotor _playerMotor;
        GameObject _levelRoot;
        GameObject _hubRoot;
        GameObject _playerRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (FindObjectOfType<GameBootstrap>() != null)
                return;

            var root = new GameObject("PeduliTransit");
            var systems = new GameObject("Systems");
            systems.transform.SetParent(root.transform, false);
            var bootstrap = systems.AddComponent<GameBootstrap>();
            bootstrap.EnsureHierarchy(root.transform);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var peduliRoot = transform.root.name == "PeduliTransit"
                ? transform.root
                : null;
            if (peduliRoot != null)
                EnsureHierarchy(peduliRoot);
            else
                EnsureHierarchy(null);

            if (FindObjectOfType<GameManager>() == null)
            {
                var gmGo = new GameObject("GameManager");
                gmGo.transform.SetParent(systemsRoot != null ? systemsRoot : transform, false);
                gmGo.AddComponent<GameManager>();
            }

            DisableExtraSceneCameras();
            EnsureEventSystem();
            EnsureLighting();
            BuildCanvas();

            AutoWireMissingAssets();

            _worldBuilder = new VehicleInteriorBuilder();
            _worldBuilder.CharacterPrefabs = npcCharacterPrefabs;
            _worldBuilder.NpcAnimatorController = npcAnimatorController;

            if (GetComponent<EventDirector>() == null)
                _director = gameObject.AddComponent<EventDirector>();
            else
                _director = GetComponent<EventDirector>();

            if (GetComponent<GameplayUI>() == null)
                _gameplayUi = gameObject.AddComponent<GameplayUI>();
            else
                _gameplayUi = GetComponent<GameplayUI>();

            _gameplayUi.Init(_canvas.transform);
            _gameplayUi.ConfigurePhoneProp(phonePrefab != null ? phonePrefab : PhonePropPresenter.TryLoadPhonePrefab());
            ShowLogin();
        }

        void AutoWireMissingAssets()
        {
            if (playerVisualPrefab == null)
                playerVisualPrefab = LoadFirstPrefab(
                    "Assets/Prefabs/Casual1.prefab",
                    "Assets/AnimeGirls/Casual1/Casual1.prefab");

            if (playerAnimatorController == null)
                playerAnimatorController = LoadAnimator("Assets/Controller/PlayerController.controller");

            if (npcAnimatorController == null)
                npcAnimatorController = playerAnimatorController;

            if (phonePrefab == null)
                phonePrefab = PhonePropPresenter.TryLoadPhonePrefab();

            if ((npcCharacterPrefabs == null || npcCharacterPrefabs.Length == 0))
            {
                npcCharacterPrefabs = new[]
                {
                    LoadFirstPrefab("Assets/Prefabs/npc_csl_00_character_01f_01.prefab"),
                    LoadFirstPrefab("Assets/Prefabs/npc_csl_00_character_01f_03.prefab"),
                    LoadFirstPrefab("Assets/Prefabs/npc_csl_00_character_02f_01.prefab"),
                    LoadFirstPrefab("Assets/Prefabs/npc_csl_00_character_02f_02.prefab"),
                    LoadFirstPrefab("Assets/Prefabs/npc_csl_00_character_02f_03.prefab"),
                };
            }

            if (interiorSlots != null)
            {
                if (interiorSlots.krlInteriorPrefab == null)
                    interiorSlots.krlInteriorPrefab = LoadFirstPrefab("Assets/Prefabs/KRLWrapper.prefab");
                if (interiorSlots.busInteriorPrefab == null)
                    interiorSlots.busInteriorPrefab = LoadFirstPrefab("Assets/Prefabs/BusWrapper.prefab")
                        ?? Resources.Load<GameObject>("BusWrapper");
                if (interiorSlots.angkutanInteriorPrefab == null)
                    interiorSlots.angkutanInteriorPrefab = LoadFirstPrefab("Assets/Prefabs/AngkotWrapper.prefab");
            }
        }

        static GameObject LoadFirstPrefab(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;
#if UNITY_EDITOR
                var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                    return go;
#endif
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var fromRes = Resources.Load<GameObject>(fileName);
                if (fromRes != null)
                    return fromRes;
            }

            return null;
        }

        static RuntimeAnimatorController LoadAnimator(string path)
        {
#if UNITY_EDITOR
            var ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
            if (ctrl != null)
                return ctrl;
#endif
            return Resources.Load<RuntimeAnimatorController>("PlayerController");
        }

        public void EnsureHierarchy(Transform peduliRoot)
        {
            if (peduliRoot == null)
            {
                var existing = GameObject.Find("PeduliTransit");
                peduliRoot = existing != null
                    ? existing.transform
                    : new GameObject("PeduliTransit").transform;
            }

            systemsRoot = FindOrCreateChild(peduliRoot, "Systems");
            uiRoot = FindOrCreateChild(peduliRoot, "UI");
            worldRoot = FindOrCreateChild(peduliRoot, "World");

            var hubFolder = FindOrCreateChild(worldRoot, "Hub");
            var levelFolder = FindOrCreateChild(worldRoot, "Level");
            var spawns = FindOrCreateChild(worldRoot, "Spawns");

            var interiorAnchor = FindOrCreateChild(spawns, "InteriorAnchor");
            var playerSpawn = FindOrCreateChild(spawns, "PlayerSpawn");
            if (playerSpawn.position == Vector3.zero)
                playerSpawn.position = new Vector3(0f, 0.1f, 0f);

            if (transform.parent != systemsRoot)
                transform.SetParent(systemsRoot, false);
            gameObject.name = "GameBootstrap";

            interiorSlots = peduliRoot.GetComponentInChildren<InteriorAssetSlots>();
            if (interiorSlots == null)
            {
                var slotGo = FindOrCreateChild(levelFolder, "InteriorSlot").gameObject;
                interiorSlots = slotGo.AddComponent<InteriorAssetSlots>();
            }

            interiorSlots.interiorAnchor = interiorAnchor;
            interiorSlots.playerSpawn = playerSpawn;

            FindOrCreateChild(levelFolder, "Interior_KRL_PlacePrefabHere");
            FindOrCreateChild(levelFolder, "Interior_Bus_PlacePrefabHere");
            FindOrCreateChild(levelFolder, "Interior_Angkutan_PlacePrefabHere");
            FindOrCreateChild(levelFolder, "NPCs");
            FindOrCreateChild(levelFolder, "Player");
            _ = hubFolder;
        }

        static Transform FindOrCreateChild(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null)
                return t;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        void DisableExtraSceneCameras()
        {
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if (worldRoot != null && cam.transform.IsChildOf(worldRoot))
                    continue;

                cam.enabled = false;
                var listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false;
            }
        }

        void ClearUiChildren()
        {
            if (_canvas == null)
                return;

            for (int i = _canvas.transform.childCount - 1; i >= 0; i--)
            {
                var child = _canvas.transform.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(systemsRoot != null ? systemsRoot : transform, false);
        }

        void EnsureLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.55f, 0.58f);

            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                lightGo.transform.SetParent(transform.root, false);
            }
        }

        void BuildCanvas()
        {
            var existing = uiRoot != null ? uiRoot.GetComponentInChildren<Canvas>() : null;
            if (existing != null)
            {
                _canvas = existing;
                ResponsiveUI.ApplyCanvasScaler(_canvas);
                return;
            }

            var canvasGo = new GameObject("UICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(uiRoot != null ? uiRoot : transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            ResponsiveUI.ApplyCanvasScaler(_canvas);
        }

        void ShowLogin()
        {
            ClearLevel();
            ClearUiChildren();
            _gameplayUi.Init(_canvas.transform);
            _gameplayUi.HideAll();
            DestroyUiBehaviours();

            GameManager.Instance.SetState(GameState.Login);
            _login = gameObject.AddComponent<LoginUI>();
            _login.Build(_canvas.transform, ShowHub);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            BuildHubAtmosphere();
        }

        public void ShowHub()
        {
            ClearLevel();
            ClearUiChildren();
            _gameplayUi.Init(_canvas.transform);
            _gameplayUi.HideAll();
            DestroyUiBehaviours();

            GameManager.Instance.ReturnToHub();
            _hub = gameObject.AddComponent<HubUI>();
            _hub.Build(_canvas.transform, StartMode);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            BuildHubAtmosphere();
        }

        void DestroyUiBehaviours()
        {
            if (_login != null)
            {
                Destroy(_login);
                _login = null;
            }

            if (_hub != null)
            {
                Destroy(_hub);
                _hub = null;
            }
        }

        public void ReturnToHubFromResult()
        {
            _director.StopDirector();
            ShowHub();
        }

        void StartMode(TransportMode mode)
        {
            DestroyUiBehaviours();
            ClearUiChildren();
            _gameplayUi.Init(_canvas.transform);

            ClearHubAtmosphere();
            GameManager.Instance.BeginSession(mode);
            SpawnLevel(mode);

            _director.StopDirector();
            _director.Begin(mode, _gameplayUi, _worldBuilder, _camera, null);
        }

        void SpawnLevel(TransportMode mode)
        {
            ClearLevel();
            AutoWireMissingAssets();

            var levelFolder = worldRoot != null ? worldRoot.Find("Level") : null;
            _levelRoot = new GameObject($"LevelSession_{mode}");
            _levelRoot.transform.SetParent(levelFolder != null ? levelFolder : worldRoot, false);

            var interiorParent = new GameObject("Interior");
            interiorParent.transform.SetParent(_levelRoot.transform, false);

            var prefab = interiorSlots != null ? interiorSlots.GetPrefab(mode) : null;
            var anchor = interiorSlots != null && interiorSlots.interiorAnchor != null
                ? interiorSlots.interiorAnchor
                : interiorParent.transform;

            _worldBuilder.CharacterPrefabs = npcCharacterPrefabs;
            _worldBuilder.NpcAnimatorController = npcAnimatorController;

            GameObject interiorInstance = null;
            if (prefab != null)
            {
                interiorInstance = Instantiate(prefab, interiorParent.transform);
                interiorInstance.name = $"InteriorAsset_{mode}";
                interiorInstance.transform.position = anchor.position;
                interiorInstance.transform.rotation = anchor.rotation;
                InteriorColliderUtility.EnsureColliders(interiorInstance);

                _worldBuilder.BuildNpcsOnly(mode, interiorParent.transform);
            }
            else
            {
                _worldBuilder.Build(mode, interiorParent.transform);
                if (_worldBuilder.Root != null)
                    InteriorColliderUtility.EnsureColliders(_worldBuilder.Root.gameObject);
            }

            Bounds interiorBounds = InteriorColliderUtility.ComputeRendererBounds(interiorParent);
            InteriorColliderUtility.EnsurePlayableFloor(interiorParent.transform, interiorBounds);

            Vector3 spawnPos = ResolvePlayerSpawn(interiorBounds);

            var playerParent = new GameObject("Player");
            playerParent.transform.SetParent(_levelRoot.transform, false);
            _playerRoot = playerParent;

            var mc = SpawnMainCharacter(playerParent.transform, spawnPos);

            // Place feet on gameplay floor before motor runs.
            float floorY = interiorBounds.min.y;
            var cc = mc.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                mc.transform.position = new Vector3(spawnPos.x, floorY + 0.05f, spawnPos.z);
                cc.enabled = true;
            }

            var camGo = ResolveFreeLookCamera(playerParent.transform);
            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 58f;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.tag = "MainCamera";
            cam.enabled = true;

            foreach (var c in FindObjectsOfType<Camera>())
            {
                if (c == cam)
                    continue;
                c.enabled = false;
            }

            foreach (var listener in FindObjectsOfType<AudioListener>())
            {
                if (listener.gameObject != camGo)
                    listener.enabled = false;
            }

            _camera = camGo.GetComponent<FreeLookCamera>();
            var ignore = mc.GetComponentsInChildren<Collider>();
            _camera.SetFollowTarget(mc.transform, ignore);
            _camera.Init(mc.transform.position + Vector3.up * 1.45f, yawDegrees: mc.transform.eulerAngles.y, distance: 3.2f);
            _camera.LookEnabled = true;

            _playerMotor = mc.GetComponent<PlayerMotor>();
            if (_playerMotor == null)
                _playerMotor = mc.AddComponent<PlayerMotor>();
            _playerMotor.Init(_camera);
            _playerMotor.SnapToGround(5f);
            _playerMotor.MoveEnabled = true;

            _gameplayUi?.ConfigurePhoneProp(phonePrefab != null ? phonePrefab : PhonePropPresenter.TryLoadPhonePrefab());
        }

        Vector3 ResolvePlayerSpawn(Bounds interiorBounds)
        {
            Vector3 spawnPos = interiorSlots != null && interiorSlots.playerSpawn != null
                ? interiorSlots.playerSpawn.position
                : new Vector3(0f, 0.05f, 0f);

            if (interiorBounds.size.magnitude > 1f)
            {
                Vector3 centerAisle = new Vector3(interiorBounds.center.x, interiorBounds.min.y + 0.05f, interiorBounds.center.z);
                if (spawnPos.sqrMagnitude < 0.01f || !interiorBounds.Contains(spawnPos + Vector3.up))
                    spawnPos = centerAisle;
            }

            if (_worldBuilder != null && _worldBuilder.Root != null)
            {
                var fallback = _worldBuilder.Root.TransformPoint(new Vector3(2.2f, 0.05f, 0f));
                if (interiorBounds.size.magnitude <= 1f)
                    spawnPos = fallback;
            }

            return spawnPos;
        }

        GameObject SpawnMainCharacter(Transform parent, Vector3 spawnPos)
        {
            GameObject mc;
            var visualPrefab = playerVisualPrefab;
            if (visualPrefab == null)
                visualPrefab = LoadFirstPrefab("Assets/Prefabs/Casual1.prefab", "Assets/AnimeGirls/Casual1/Casual1.prefab");

            if (visualPrefab != null)
            {
                mc = Instantiate(visualPrefab, parent);
                mc.name = "MC_Casual1";
                mc.transform.position = spawnPos + playerVisualOffset;
                mc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                mc.transform.localScale = Vector3.one;

                foreach (var col in mc.GetComponentsInChildren<Collider>())
                    Destroy(col);

                if (playerAnimatorController != null)
                {
                    var anim = mc.GetComponentInChildren<Animator>();
                    if (anim != null)
                    {
                        anim.runtimeAnimatorController = playerAnimatorController;
                        anim.applyRootMotion = false;
                        anim.enabled = true;
                        anim.speed = 1f;
                        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                }
            }
            else
            {
                mc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                mc.name = "MC_Fallback";
                mc.transform.SetParent(parent, false);
                mc.transform.position = spawnPos + Vector3.up * 0.9f;
                mc.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
                Destroy(mc.GetComponent<Collider>());
                var rend = mc.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(Shader.Find("Standard"));
                    rend.material.color = new Color(0.85f, 0.55f, 0.7f);
                }
            }

            var controller = mc.GetComponent<CharacterController>();
            if (controller == null)
                controller = mc.AddComponent<CharacterController>();
            controller.height = 1.6f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.85f, 0f);
            controller.skinWidth = 0.05f;
            controller.enabled = true;

            return mc;
        }

        GameObject ResolveFreeLookCamera(Transform fallbackParent)
        {
            FreeLookCamera existing = null;
            if (worldRoot != null)
            {
                var level = worldRoot.Find("Level");
                var player = level != null ? level.Find("Player") : null;
                var sceneCam = player != null ? player.Find("FreeLookCamera") : null;
                if (sceneCam != null)
                    existing = sceneCam.GetComponent<FreeLookCamera>();
            }

            if (existing == null)
                existing = FindObjectOfType<FreeLookCamera>();

            GameObject camGo;
            if (existing != null)
            {
                camGo = existing.gameObject;
                if (camGo.GetComponent<Camera>() == null)
                    camGo.AddComponent<Camera>();
                if (camGo.GetComponent<AudioListener>() == null)
                    camGo.AddComponent<AudioListener>();
            }
            else
            {
                camGo = new GameObject("FreeLookCamera", typeof(Camera), typeof(AudioListener), typeof(FreeLookCamera));
                camGo.transform.SetParent(fallbackParent, false);
            }

            return camGo;
        }

        void BuildHubAtmosphere()
        {
            if (_hubRoot != null)
                return;

            var hubFolder = worldRoot != null ? worldRoot.Find("Hub") : null;
            _hubRoot = new GameObject("HubAtmosphere");
            _hubRoot.transform.SetParent(hubFolder != null ? hubFolder : worldRoot, false);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "HubFloor";
            floor.transform.SetParent(_hubRoot.transform, false);
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            floor.GetComponent<Renderer>().material.color = new Color(0.12f, 0.22f, 0.24f);

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "HubPillar";
            pillar.transform.SetParent(_hubRoot.transform, false);
            pillar.transform.position = new Vector3(0f, 1.5f, 4f);
            pillar.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
            pillar.GetComponent<Renderer>().material.color = new Color(0.18f, 0.55f, 0.52f);

            var camGo = new GameObject("HubCamera", typeof(Camera), typeof(AudioListener));
            camGo.transform.SetParent(_hubRoot.transform, false);
            camGo.transform.position = new Vector3(0f, 2.2f, -6f);
            camGo.transform.LookAt(new Vector3(0f, 1.2f, 2f));
            camGo.tag = "MainCamera";

            foreach (var c in FindObjectsOfType<Camera>())
            {
                if (c != camGo.GetComponent<Camera>())
                    c.enabled = false;
            }
        }

        void ClearHubAtmosphere()
        {
            if (_hubRoot != null)
                Destroy(_hubRoot);
            _hubRoot = null;
        }

        void ClearLevel()
        {
            _director?.StopDirector();
            _worldBuilder?.Clear();
            if (_levelRoot != null)
                Destroy(_levelRoot);
            _levelRoot = null;
            _camera = null;
            _playerMotor = null;
            _playerRoot = null;
        }

        public void SetPlayerControl(bool enabled)
        {
            if (_camera != null)
                _camera.LookEnabled = enabled;
            if (_playerMotor != null)
                _playerMotor.MoveEnabled = enabled;
        }
    }
}