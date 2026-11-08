using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Woodcutter.Data;
using Woodcutter.Player;

namespace Woodcutter.EditorTools
{
    public static class SetupSandboxScene
    {
        [MenuItem("Woodcutter/Build Sandbox Scene")]
        public static void BuildScene()
        {
            // 1. Tạo sprite hình vuông cơ bản nếu chưa có
            Sprite squareSprite = EnsureSquareSprite();

            // 2. Tạo hoặc nạp PlayerStats.asset
            PlayerStatsSO stats = EnsurePlayerStatsAsset();

            // 3. Tạo hoặc nạp Physics Material 2D (Zero friction cho Player)
            PhysicsMaterial2D playerPhysicMat = EnsurePlayerPhysicsMaterial();

            // 4. Nạp Input Actions asset
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Input/Woodcutter_InputActions.inputactions");
            if (inputActions == null)
            {
                Debug.LogError("Không tìm thấy Woodcutter_InputActions.inputactions!");
                return;
            }

            // Layer IDs
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) playerLayer = 6;
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 7;

            // 5. Tạo Scene mới
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 5.1 Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f); // Màu nền tối tuyết
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 1f, -10f);
            camGo.AddComponent<UniversalAdditionalCameraData>();

            // 5.2 Global Light 2D (URP 2D)
            GameObject lightGo = new GameObject("Global Light 2D");
            Light2D light2d = lightGo.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Global;
            light2d.intensity = 1.0f;
            light2d.color = Color.white;

            // 5.3 Mặt đất (Ground)
            GameObject groundGo = new GameObject("Ground");
            groundGo.layer = groundLayer;
            groundGo.transform.position = new Vector3(0f, -2.5f, 0f);
            groundGo.transform.localScale = new Vector3(20f, 1f, 1f);
            var groundSr = groundGo.AddComponent<SpriteRenderer>();
            groundSr.sprite = squareSprite;
            groundSr.color = new Color(0.25f, 0.3f, 0.35f);
            var groundCol = groundGo.AddComponent<BoxCollider2D>();
            groundCol.size = Vector2.one;

            // 5.4 Tường hai bên (Walls)
            GameObject leftWall = new GameObject("Wall_Left");
            leftWall.layer = groundLayer;
            leftWall.transform.position = new Vector3(-10.5f, 1.5f, 0f);
            leftWall.transform.localScale = new Vector3(1f, 8f, 1f);
            var leftWallSr = leftWall.AddComponent<SpriteRenderer>();
            leftWallSr.sprite = squareSprite;
            leftWallSr.color = new Color(0.2f, 0.22f, 0.26f);
            var leftWallCol = leftWall.AddComponent<BoxCollider2D>();
            leftWallCol.size = Vector2.one;

            GameObject rightWall = new GameObject("Wall_Right");
            rightWall.layer = groundLayer;
            rightWall.transform.position = new Vector3(10.5f, 1.5f, 0f);
            rightWall.transform.localScale = new Vector3(1f, 8f, 1f);
            var rightWallSr = rightWall.AddComponent<SpriteRenderer>();
            rightWallSr.sprite = squareSprite;
            rightWallSr.color = new Color(0.2f, 0.22f, 0.26f);
            var rightWallCol = rightWall.AddComponent<BoxCollider2D>();
            rightWallCol.size = Vector2.one;

            // 5.5 Bệ nhảy thử nghiệm (Platforms)
            GameObject plat1 = new GameObject("Platform_1");
            plat1.layer = groundLayer;
            plat1.transform.position = new Vector3(-4f, -0.5f, 0f);
            plat1.transform.localScale = new Vector3(4f, 0.5f, 1f);
            var plat1Sr = plat1.AddComponent<SpriteRenderer>();
            plat1Sr.sprite = squareSprite;
            plat1Sr.color = new Color(0.3f, 0.4f, 0.45f);
            var plat1Col = plat1.AddComponent<BoxCollider2D>();
            plat1Col.size = Vector2.one;

            GameObject plat2 = new GameObject("Platform_2");
            plat2.layer = groundLayer;
            plat2.transform.position = new Vector3(4f, 1.2f, 0f);
            plat2.transform.localScale = new Vector3(4f, 0.5f, 1f);
            var plat2Sr = plat2.AddComponent<SpriteRenderer>();
            plat2Sr.sprite = squareSprite;
            plat2Sr.color = new Color(0.3f, 0.4f, 0.45f);
            var plat2Col = plat2.AddComponent<BoxCollider2D>();
            plat2Col.size = Vector2.one;

            // 5.6 Player Prototype Square
            GameObject playerGo = new GameObject("PlayerSquare");
            playerGo.layer = playerLayer;
            playerGo.transform.position = new Vector3(0f, 0f, 0f);

            var playerSr = playerGo.AddComponent<SpriteRenderer>();
            playerSr.sprite = squareSprite;
            playerSr.color = Color.white;

            var playerCol = playerGo.AddComponent<BoxCollider2D>();
            playerCol.size = new Vector2(1f, 1f);
            playerCol.sharedMaterial = playerPhysicMat;

            var playerRb = playerGo.AddComponent<Rigidbody2D>();
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.freezeRotation = true;
            playerRb.gravityScale = stats.ascendingGravity;
            playerRb.sharedMaterial = playerPhysicMat;

            var playerInput = playerGo.AddComponent<PlayerInput>();
            playerInput.actions = inputActions;
            playerInput.defaultActionMap = "Player";
            playerInput.defaultControlScheme = "Keyboard&Mouse";

            var inputBroker = playerGo.AddComponent<PlayerInputBroker>();

            // GroundCheck child: Đặt ở đáy nhân vật
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.layer = playerLayer;
            groundCheck.transform.SetParent(playerGo.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            var context = playerGo.AddComponent<CharacterContext>();
            context.Rigidbody = playerRb;
            context.SpriteRenderer = playerSr;
            context.GroundCheckPoint = groundCheck.transform;
            context.GroundCheckDimensions = new Vector2(0.8f, 0.15f);
            context.GroundLayerMask = (1 << groundLayer) | (1 << 0); // Ground layer + Default

            // Gán stats qua SerializedObject
            SerializedObject contextSo = new SerializedObject(context);
            contextSo.FindProperty("stats").objectReferenceValue = stats;
            contextSo.FindProperty("inputBroker").objectReferenceValue = inputBroker;
            contextSo.ApplyModifiedProperties();

            // 6. Lưu Prefab (Đảm bảo thư mục tồn tại)
            string prefabPath = "Assets/_Project/Prefabs/Player/PF_PlayerPrototype.prefab";
            EnsureDirectoryExists(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(playerGo, prefabPath);
            Debug.Log($"Đã tạo và lưu Prefab tại: {prefabPath}");

            // 7. Lưu Scene (Đảm bảo thư mục tồn tại)
            string scenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_InputFSM.unity";
            EnsureDirectoryExists(scenePath);
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"Đã tạo và lưu Scene thành công tại: {scenePath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Sprite EnsureSquareSprite()
        {
            string path = "Assets/_Project/Art/Characters/Square.png";
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            EnsureDirectoryExists(path);

            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static PlayerStatsSO EnsurePlayerStatsAsset()
        {
            string path = "Assets/_Project/Data/PlayerStats.asset";
            PlayerStatsSO existing = AssetDatabase.LoadAssetAtPath<PlayerStatsSO>(path);
            if (existing != null)
                return existing;

            EnsureDirectoryExists(path);

            PlayerStatsSO stats = ScriptableObject.CreateInstance<PlayerStatsSO>();
            stats.maxRunSpeed = 8f;
            stats.accelerationTime = 0.08f;
            stats.decelerationTime = 0.06f;
            stats.jumpForce = 14f;
            stats.ascendingGravity = 3.5f;
            stats.fallingGravity = 8f;
            stats.maxFallSpeed = 18f;
            stats.coyoteTime = 0.1f;
            stats.jumpBufferTime = 0.1f;
            stats.attackDuration = 0.35f;
            stats.rollDistance = 4f;
            stats.rollDuration = 0.35f;
            stats.rollIframeDuration = 0.25f;
            stats.rollCooldown = 0.5f;

            AssetDatabase.CreateAsset(stats, path);
            AssetDatabase.SaveAssets();
            return stats;
        }

        private static PhysicsMaterial2D EnsurePlayerPhysicsMaterial()
        {
            string path = "Assets/_Project/Materials/PM_Player.physicsMaterial2D";
            PhysicsMaterial2D existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (existing != null)
                return existing;

            EnsureDirectoryExists(path);

            PhysicsMaterial2D mat = new PhysicsMaterial2D("PM_Player");
            mat.friction = 0f; // Zero friction để không bị dính vách tường khi nhảy ép vào tường
            mat.bounciness = 0f;

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static void EnsureDirectoryExists(string assetPath)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
