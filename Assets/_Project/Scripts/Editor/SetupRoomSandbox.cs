using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Woodcutter.Data;
using Woodcutter.UI;
using Woodcutter.World;

namespace Woodcutter.EditorTools
{
    /// <summary>
    /// Dựng sandbox thử camera + chuyển phòng (T014, FR-LVL-01):
    /// - Scene nền Sandbox_RoomTransition (Player, camera Cinemachine, màn fade, RoomManager)
    /// - 3 phòng thử: A ↔ B (ngang), B ↔ C (dọc, rơi xuống giếng / nhảy lên)
    /// - Prefab mẫu PF_RoomTemplate + PF_RoomDoor cho LVL dựng phòng thật
    /// Đơn vị: 1 ô = 1 unit. Chạy lại menu sẽ ghi đè các file trên.
    /// </summary>
    public static class SetupRoomSandbox
    {
        private const string SandboxFolder = "Assets/_Project/Scenes/Sandbox";
        private const string PersistentSceneName = "Sandbox_RoomTransition";
        private const string RoomA = "Sandbox_RoomA";
        private const string RoomB = "Sandbox_RoomB";
        private const string RoomC = "Sandbox_RoomC";

        private const string SettingsPath = "Assets/_Project/Data/CameraRoomSettings.asset";
        private const string PlayerStatsPath = "Assets/_Project/Data/PlayerStats.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/PF_PlayerPrototype.prefab";
        private const string SquareSpritePath = "Assets/_Project/Art/Characters/Square.png";
        private const string RoomTemplatePath = "Assets/_Project/Prefabs/World/PF_RoomTemplate.prefab";
        private const string DoorPrefabPath = "Assets/_Project/Prefabs/World/PF_RoomDoor.prefab";

        private static readonly Color WallColor = new Color(0.22f, 0.25f, 0.3f);
        private static readonly Color PlatformColor = new Color(0.32f, 0.42f, 0.48f);

        private struct DoorSpec
        {
            public string Id;
            public string TargetRoom;
            public string TargetDoor;
            public Rect Trigger;        // xMin, yMin, width, height
            public Vector2 Spawn;
            public Vector2 ArrivalVelocity;
        }

        [MenuItem("Woodcutter/Build Room Transition Sandbox")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (square == null || playerPrefab == null)
            {
                Debug.LogError("[SetupRoomSandbox] Thiếu Square.png hoặc PF_PlayerPrototype. Chạy 'Woodcutter/Build Sandbox Scene' trước.");
                return;
            }

            CameraRoomSettingsSO settings = EnsureSettingsAsset();

            BuildRoomTemplatePrefabs();
            string roomAPath = BuildRoomA(square);
            string roomBPath = BuildRoomB(square);
            string roomCPath = BuildRoomC(square);
            string persistentPath = BuildPersistentScene(settings, playerPrefab);

            AddScenesToBuildList(persistentPath, roomAPath, roomBPath, roomCPath);

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(persistentPath, OpenSceneMode.Single);
            Debug.Log("[SetupRoomSandbox] Xong. Mở Sandbox_RoomTransition rồi bấm Play. Cửa phải phòng A -> B, lỗ sàn phòng B -> C.");
        }

        // ================================================================
        // SCENE NỀN
        // ================================================================

        private static string BuildPersistentScene(CameraRoomSettingsSO settings, GameObject playerPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Main Camera + Brain
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = settings.orthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<CinemachineBrain>();

            // Global Light 2D
            var lightGo = new GameObject("Global Light 2D");
            var light2d = lightGo.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Global;

            // Player (prefab instance, gán PlayerStats ở instance để không sửa prefab của LEAD)
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "Player";
            player.transform.position = Vector3.zero;
            var stats = AssetDatabase.LoadAssetAtPath<PlayerStatsSO>(PlayerStatsPath);
            var context = player.GetComponent<Woodcutter.Player.CharacterContext>();
            if (stats != null && context != null)
            {
                var so = new SerializedObject(context);
                so.FindProperty("stats").objectReferenceValue = stats;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.interpolation = RigidbodyInterpolation2D.Interpolate; // Camera bám mượt, không rung

            // Cinemachine Camera
            var vcamGo = new GameObject("CM_RoomCamera");
            vcamGo.transform.position = new Vector3(0f, 0f, -10f);
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            vcam.Follow = player.transform;
            var composer = vcamGo.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = 10f;
            var confiner = vcamGo.AddComponent<CinemachineConfiner2D>();
            vcamGo.AddComponent<CinemachineImpulseListener>(); // Cho T013 (rung màn hình)
            var roomCamera = vcamGo.AddComponent<RoomCamera>();
            SetRefs(roomCamera, ("settings", settings), ("virtualCamera", vcam),
                ("positionComposer", composer), ("confiner", confiner));

            // Màn fade
            ScreenFader fader = CreateFader();

            // RoomManager
            var managerGo = new GameObject("RoomManager");
            var manager = managerGo.AddComponent<RoomManager>();
            SetRefs(manager, ("settings", settings), ("roomCamera", roomCamera), ("fader", fader),
                ("playerBody", playerBody), ("playerInput", player.GetComponent<PlayerInput>()));
            SetString(manager, "startRoomId", RoomA);
            SetString(manager, "startDoorId", "");

            string path = $"{SandboxFolder}/{PersistentSceneName}.unity";
            EnsureDirectoryExists(path);
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static ScreenFader CreateFader()
        {
            var canvasGo = new GameObject("ScreenFader");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            var group = canvasGo.AddComponent<CanvasGroup>();
            group.alpha = 1f; // Bắt đầu tối, sáng dần sau khi nạp phòng đầu tiên
            group.blocksRaycasts = false;
            group.interactable = false;

            var imageGo = new GameObject("Black");
            imageGo.transform.SetParent(canvasGo.transform, false);
            var rect = imageGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;

            var fader = canvasGo.AddComponent<ScreenFader>();
            SetRefs(fader, ("canvasGroup", group));
            return fader;
        }

        // ================================================================
        // 3 PHÒNG THỬ (tọa độ cục bộ, mỗi phòng bắt đầu từ (0,0))
        // ================================================================

        // Phòng A 48x24: cửa phải (khe cao 4 ô) sang B.
        private static string BuildRoomA(Sprite sq)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transform geo = CreateRoomRoot(RoomA, new Vector2(48f, 24f), new Vector2(5f, 3f), new[]
            {
                new DoorSpec { Id = "Right", TargetRoom = RoomB, TargetDoor = "Left",
                    Trigger = new Rect(47.5f, 2f, 1f, 4f), Spawn = new Vector2(45f, 3f) },
            });

            Block(geo, sq, "Floor", 0, 0, 48, 2, WallColor);
            Block(geo, sq, "Ceiling", 0, 23, 48, 24, WallColor);
            Block(geo, sq, "Wall_Left", 0, 2, 1, 23, WallColor);
            Block(geo, sq, "Wall_Right_Upper", 47, 6, 48, 23, WallColor);
            Block(geo, sq, "Platform_1", 12, 4.5f, 17, 5, PlatformColor);
            Block(geo, sq, "Platform_2", 20, 7f, 25, 7.5f, PlatformColor);
            Block(geo, sq, "Platform_3", 28, 9.5f, 33, 10, PlatformColor);

            return SaveRoomScene(scene, RoomA);
        }

        // Phòng B 64x24: cửa trái về A, lỗ sàn (x 56..60) xuống C.
        private static string BuildRoomB(Sprite sq)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transform geo = CreateRoomRoot(RoomB, new Vector2(64f, 24f), new Vector2(4f, 3f), new[]
            {
                new DoorSpec { Id = "Left", TargetRoom = RoomA, TargetDoor = "Right",
                    Trigger = new Rect(-0.5f, 2f, 1f, 4f), Spawn = new Vector2(3f, 3f) },
                // Đi lên từ C: xuất hiện trên gờ bên phải lỗ, không rơi ngược lại
                new DoorSpec { Id = "Bottom", TargetRoom = RoomC, TargetDoor = "Top",
                    Trigger = new Rect(56f, -1f, 4f, 1.5f), Spawn = new Vector2(61.5f, 3f) },
            });

            Block(geo, sq, "Floor_Left", 0, 0, 56, 2, WallColor);
            Block(geo, sq, "Floor_Right", 60, 0, 64, 2, WallColor);
            Block(geo, sq, "Ceiling", 0, 23, 64, 24, WallColor);
            Block(geo, sq, "Wall_Left_Upper", 0, 6, 1, 23, WallColor);
            Block(geo, sq, "Wall_Right", 63, 2, 64, 23, WallColor);
            Block(geo, sq, "Platform_1", 18, 4.5f, 24, 5, PlatformColor);
            Block(geo, sq, "Platform_2", 30, 7f, 36, 7.5f, PlatformColor);

            return SaveRoomScene(scene, RoomB);
        }

        // Phòng C 40x32: giếng dọc, cửa trên (x 16..20) về B. Bậc zíc zắc cách nhau 2.5 ô để nhảy lên lại.
        private static string BuildRoomC(Sprite sq)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transform geo = CreateRoomRoot(RoomC, new Vector2(40f, 32f), new Vector2(18f, 3f), new[]
            {
                // Rơi từ B xuống: xuất hiện ngay dưới miệng giếng, rơi tự do xuống đáy
                new DoorSpec { Id = "Top", TargetRoom = RoomB, TargetDoor = "Bottom",
                    Trigger = new Rect(16f, 32f, 4f, 2f), Spawn = new Vector2(18f, 30f) },
            });

            Block(geo, sq, "Floor", 0, 0, 40, 2, WallColor);
            Block(geo, sq, "Wall_Left", 0, 2, 1, 32, WallColor);
            Block(geo, sq, "Wall_Right", 39, 2, 40, 32, WallColor);
            Block(geo, sq, "Ceiling_Left", 0, 32, 16, 33, WallColor);
            Block(geo, sq, "Ceiling_Right", 20, 32, 40, 33, WallColor);

            // Bậc lên: trái [11,17] / phải [19,25] xen kẽ, mặt trên cách nhau 2.5 ô, khe giữa 17..19 để rơi xuống.
            // Bậc cuối (trái, mặt 29.5) có mép phải nằm dưới miệng giếng: nhảy thẳng lên là vào cửa Top.
            float top = 4.5f;
            bool left = true;
            int index = 1;
            while (top <= 29.5f)
            {
                float xMin = left ? 11f : 19f;
                Block(geo, sq, $"Step_{index:00}", xMin, top - 0.5f, xMin + 6f, top, PlatformColor);
                top += 2.5f;
                left = !left;
                index++;
            }

            return SaveRoomScene(scene, RoomC);
        }

        // ================================================================
        // PREFAB MẪU CHO LVL
        // ================================================================

        private static void BuildRoomTemplatePrefabs()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cửa đơn lẻ
            GameObject door = CreateDoor(null, new DoorSpec
            {
                Id = "Door", TargetRoom = "", TargetDoor = "",
                Trigger = new Rect(-0.5f, 0f, 1f, 4f), Spawn = new Vector2(-2f, 1f),
            });
            door.transform.position = Vector3.zero;
            EnsureDirectoryExists(DoorPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(door, DoorPrefabPath);
            Object.DestroyImmediate(door);

            // Phòng mẫu 48x27 (vừa 1 màn hình + lề), cửa trái/phải sát sàn
            Transform geometry = CreateRoomRoot("RoomTemplate", new Vector2(48f, 27f), new Vector2(4f, 3f), new[]
            {
                new DoorSpec { Id = "Left", Trigger = new Rect(-0.5f, 2f, 1f, 4f), Spawn = new Vector2(3f, 3f) },
                new DoorSpec { Id = "Right", Trigger = new Rect(47.5f, 2f, 1f, 4f), Spawn = new Vector2(45f, 3f) },
            });
            GameObject root = geometry.parent.gameObject;
            SetString(root.GetComponent<Room>(), "roomId", ""); // Để trống = lấy tên scene
            EnsureDirectoryExists(RoomTemplatePath);
            PrefabUtility.SaveAsPrefabAsset(root, RoomTemplatePath);
            Object.DestroyImmediate(root);
        }

        // ================================================================
        // TIỆN ÍCH DỰNG PHÒNG
        // ================================================================

        /// <summary>Tạo Room + biên camera + điểm xuất hiện + cửa. Trả về object chứa hình khối.</summary>
        private static Transform CreateRoomRoot(string roomId, Vector2 size, Vector2 defaultSpawn, DoorSpec[] doors)
        {
            var root = new GameObject($"Room_{roomId}");
            var room = root.AddComponent<Room>();

            var boundsGo = new GameObject("CameraBounds");
            boundsGo.transform.SetParent(root.transform, false);
            boundsGo.layer = LayerMask.NameToLayer("Ignore Raycast");
            var bounds = boundsGo.AddComponent<BoxCollider2D>();
            bounds.isTrigger = true;
            bounds.offset = size * 0.5f;
            bounds.size = size;

            var spawnGo = new GameObject("DefaultSpawn");
            spawnGo.transform.SetParent(root.transform, false);
            spawnGo.transform.localPosition = defaultSpawn;

            var doorsRoot = new GameObject("Doors");
            doorsRoot.transform.SetParent(root.transform, false);
            foreach (DoorSpec spec in doors)
                CreateDoor(doorsRoot.transform, spec);

            var geometry = new GameObject("Geometry");
            geometry.transform.SetParent(root.transform, false);

            SetRefs(room, ("cameraBounds", bounds), ("defaultSpawn", spawnGo.transform));
            SetString(room, "roomId", roomId);
            SetString(room, "persistentSceneName", PersistentSceneName);
            return geometry.transform;
        }

        private static GameObject CreateDoor(Transform parent, DoorSpec spec)
        {
            var doorGo = new GameObject($"Door_{spec.Id}");
            if (parent != null) doorGo.transform.SetParent(parent, false);
            doorGo.layer = LayerMask.NameToLayer("Ignore Raycast");
            doorGo.transform.localPosition = spec.Trigger.center;

            var col = doorGo.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = spec.Trigger.size;

            var spawnGo = new GameObject("SpawnPoint");
            spawnGo.transform.SetParent(doorGo.transform, false);
            spawnGo.transform.localPosition = spec.Spawn - spec.Trigger.center;

            var door = doorGo.AddComponent<RoomDoor>();
            SetString(door, "doorId", spec.Id);
            SetString(door, "targetRoomId", spec.TargetRoom ?? "");
            SetString(door, "targetDoorId", spec.TargetDoor ?? "");
            var so = new SerializedObject(door);
            so.FindProperty("spawnPoint").objectReferenceValue = spawnGo.transform;
            so.FindProperty("arrivalVelocity").vector2Value = spec.ArrivalVelocity;
            so.ApplyModifiedPropertiesWithoutUndo();
            return doorGo;
        }

        private static void Block(Transform parent, Sprite sprite, string name,
            float xMin, float yMin, float xMax, float yMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            int groundLayer = LayerMask.NameToLayer("Ground");
            go.layer = groundLayer >= 0 ? groundLayer : 0;
            go.transform.localPosition = new Vector3((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f, 0f);
            go.transform.localScale = new Vector3(xMax - xMin, yMax - yMin, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private static string SaveRoomScene(Scene scene, string roomId)
        {
            // Đèn riêng cho phòng không cần: scene nền đã có Global Light 2D
            string path = $"{SandboxFolder}/{roomId}.unity";
            EnsureDirectoryExists(path);
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        // ================================================================
        // TIỆN ÍCH CHUNG
        // ================================================================

        private static CameraRoomSettingsSO EnsureSettingsAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CameraRoomSettingsSO>(SettingsPath);
            if (existing != null) return existing;

            EnsureDirectoryExists(SettingsPath);
            var created = ScriptableObject.CreateInstance<CameraRoomSettingsSO>();
            AssetDatabase.CreateAsset(created, SettingsPath);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static void AddScenesToBuildList(params string[] paths)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (string path in paths)
            {
                if (scenes.Exists(s => s.path == path)) continue;
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SetRefs(Object target, params (string field, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in refs)
            {
                SerializedProperty prop = so.FindProperty(field);
                if (prop == null)
                {
                    Debug.LogError($"[SetupRoomSandbox] Không tìm thấy field '{field}' trên {target.GetType().Name}.");
                    continue;
                }
                prop.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectoryExists(string assetPath)
        {
            string dir = Path.GetDirectoryName(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }
    }
}
