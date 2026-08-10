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
        [Tooltip("Drag prefab Casual1 (Assets/AnimeGirls/Casual1/Casual1) ke sini")]
        [SerializeField] GameObject playerVisualPrefab;
        [SerializeField] Vector3 playerVisualOffset = Vector3.zero;
        [SerializeField] RuntimeAnimatorController playerAnimatorController;

        Canvas _canvas;
        LoginUI _login;
        HubUI _hub;
        GameplayUI _gameplayUi;
        EventDirector _director;
        VehicleInteriorBuilder _worldBuilder;
        FreeLookCamera _camera;
        GameObject _levelRoot;
        GameObject _hubRoot;

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
            ShowLogin();
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

            if (prefab != null)
            {
                var instance = Instantiate(prefab, interiorParent.transform);
                instance.name = $"InteriorAsset_{mode}";
                instance.transform.position = anchor.position;
                instance.transform.rotation = anchor.rotation;

                _worldBuilder.BuildNpcsOnly(mode, interiorParent.transform);
            }
            else
            {
                _worldBuilder.Build(mode, interiorParent.transform);
            }

            var playerParent = new GameObject("Player");
            playerParent.transform.SetParent(_levelRoot.transform, false);

            Vector3 spawnPos = interiorSlots != null && interiorSlots.playerSpawn != null
                ? interiorSlots.playerSpawn.position
                : new Vector3(0f, 0.1f, 0f);

            var avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "PlayerAvatar";
            avatar.transform.SetParent(playerParent.transform, false);
            avatar.transform.position = spawnPos + Vector3.up * 0.9f;
            avatar.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
            UnityEngine.Object.Destroy(avatar.GetComponent<Collider>());
            var avatarRend = avatar.GetComponent<Renderer>();
            if (avatarRend != null)
            {
                avatarRend.material = new Material(Shader.Find("Standard"));
                avatarRend.material.color = new Color(0.2f, 0.75f, 0.7f);
            }

            var camGo = ResolveFreeLookCamera(playerParent.transform);
            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 60f;
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

            Vector3 focus = new Vector3(0f, 1.25f, 0f);
            if (_worldBuilder != null && _worldBuilder.Root != null)
                focus = _worldBuilder.Root.TransformPoint(new Vector3(0f, 1.25f, 0f));

            if (focus.y < 0.5f)
                focus.y = 1.25f;

            _camera.Init(focus, yawDegrees: 200f, distance: 4.5f);
            _camera.LookEnabled = true;
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
        }
    }
}