using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Enemies;
using Woodcutter.Player;

namespace Woodcutter.EditorTools
{
    /// <summary>
    /// Tiện ích Editor thiết lập Khung 3 AI Quái vật (T019 - FR-QUAI) và Scene thử nghiệm Sandbox_Enemies.
    /// Menu: Woodcutter > Enemies > Setup Enemy Framework & Sandbox.
    /// </summary>
    public static class EnemySetupUtility
    {
        private const string DataPath = "Assets/_Project/Data/Enemies";
        private const string PrefabPath = "Assets/_Project/Prefabs/Enemies";
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_Enemies.unity";
        private const string EnemyArtPath = "Assets/_Project/Art/Characters/Enemies/";

        [MenuItem("Woodcutter/Enemies/Setup Enemy Framework & Sandbox")]
        public static void SetupAll()
        {
            EnsureDirectories();
            SetupStatsAssets();
            SetupPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildSandboxScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemySetupUtility] Hoàn thành thiết lập Khung 3 AI quái và Scene Sandbox_Enemies!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Enemies"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Enemies");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Enemies"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Enemies");
            }
        }

        public static void SetupStatsAssets()
        {
            EnsureDirectories();

            // 1. Sói tuyết (Khung Lao tới - SRS 6.1)
            string wolfPath = $"{DataPath}/SO_SkullWolfStats.asset";
            var wolfStats = AssetDatabase.LoadAssetAtPath<ChargeEnemyStatsSO>(wolfPath);
            if (wolfStats == null)
            {
                wolfStats = ScriptableObject.CreateInstance<ChargeEnemyStatsSO>();
                wolfStats.maxHp = 3;
                wolfStats.contactDamage = 1;
                wolfStats.moveSpeed = 3f;
                wolfStats.detectRange = 7f;
                wolfStats.telegraphDuration = 0.6f; // SRS >= 0.4s
                wolfStats.recoverDuration = 0.9f;
                wolfStats.chargeSpeed = 10f;
                wolfStats.chargeDuration = 0.45f;
                wolfStats.chargeCooldown = 2.0f;
                wolfStats.isIceEnemy = true;
                wolfStats.iceDamageMultiplier = 2.0f;
                AssetDatabase.CreateAsset(wolfStats, wolfPath);
            }

            // 2. Nhện băng (Khung Đi tuần - SRS 6.1)
            string spiderPath = $"{DataPath}/SO_PixelSpiderStats.asset";
            var spiderStats = AssetDatabase.LoadAssetAtPath<PatrolEnemyStatsSO>(spiderPath);
            if (spiderStats == null)
            {
                spiderStats = ScriptableObject.CreateInstance<PatrolEnemyStatsSO>();
                spiderStats.maxHp = 2;
                spiderStats.contactDamage = 1;
                spiderStats.moveSpeed = 2f;
                spiderStats.patrolSpeed = 2f;
                spiderStats.patrolDistance = 4f;
                spiderStats.detectRange = 5f;
                spiderStats.attackRange = 1.2f;
                spiderStats.telegraphDuration = 0.5f; // SRS >= 0.4s
                spiderStats.recoverDuration = 0.8f;
                spiderStats.canAmbushFromCeiling = true;
                spiderStats.ceilingDropSpeed = 8f;
                spiderStats.isIceEnemy = true;
                spiderStats.iceDamageMultiplier = 2.0f;
                AssetDatabase.CreateAsset(spiderStats, spiderPath);
            }

            // 3. Quạ xám (Khung Bắn xa - SRS 6.1)
            string crowPath = $"{DataPath}/SO_GreyCrowStats.asset";
            var crowStats = AssetDatabase.LoadAssetAtPath<RangedEnemyStatsSO>(crowPath);
            if (crowStats == null)
            {
                crowStats = ScriptableObject.CreateInstance<RangedEnemyStatsSO>();
                crowStats.maxHp = 2;
                crowStats.contactDamage = 1;
                crowStats.moveSpeed = 3.5f;
                crowStats.detectRange = 8f;
                crowStats.telegraphDuration = 0.6f; // SRS >= 0.4s
                crowStats.recoverDuration = 0.8f;
                crowStats.projectileSpeed = 7f;
                crowStats.projectileDamage = 1;
                crowStats.projectileLifetime = 4f;
                crowStats.burstCount = 1;
                crowStats.attackCooldown = 2.5f;
                crowStats.hoverAltitude = 2.5f;
                crowStats.kiteDistance = 3.5f;
                crowStats.swoopDownOnAttack = true; // SRS: chém lên khi nó hạ thấp
                crowStats.swoopDistanceY = 1.2f;
                crowStats.isIceEnemy = true;
                crowStats.iceDamageMultiplier = 2.0f;
                AssetDatabase.CreateAsset(crowStats, crowPath);
            }

            // 4. Giáp băng (Biến thể Khung Đi tuần - SRS 6.1)
            string armorPath = $"{DataPath}/SO_IceArmorStats.asset";
            var armorStats = AssetDatabase.LoadAssetAtPath<PatrolEnemyStatsSO>(armorPath);
            if (armorStats == null)
            {
                armorStats = ScriptableObject.CreateInstance<PatrolEnemyStatsSO>();
                armorStats.maxHp = 5;
                armorStats.contactDamage = 1;
                armorStats.patrolSpeed = 1.5f;
                armorStats.chaseSpeed = 2.5f;
                armorStats.patrolDistance = 3.5f;
                armorStats.detectRange = 5f;
                armorStats.attackRange = 1.2f;
                armorStats.telegraphDuration = 0.5f;
                armorStats.hasFrontShield = true; // Chặn đòn thường từ phía trước
                armorStats.isIceEnemy = true;
                armorStats.iceDamageMultiplier = 2.0f;
                AssetDatabase.CreateAsset(armorStats, armorPath);
            }

            // 5. Dơi hang (Biến thể Khung Lao tới - SRS 6.1)
            string batPath = $"{DataPath}/SO_CaveBatStats.asset";
            var batStats = AssetDatabase.LoadAssetAtPath<ChargeEnemyStatsSO>(batPath);
            if (batStats == null)
            {
                batStats = ScriptableObject.CreateInstance<ChargeEnemyStatsSO>();
                batStats.maxHp = 2;
                batStats.contactDamage = 1;
                batStats.moveSpeed = 3f;
                batStats.detectRange = 5f;
                batStats.telegraphDuration = 0.5f;
                batStats.chargeSpeed = 8f;
                batStats.chargeDuration = 0.4f;
                batStats.isSleepingByDefault = true;
                batStats.wakeUpDistance = 4f;
                batStats.isIceEnemy = true;
                batStats.iceDamageMultiplier = 2.0f;
                AssetDatabase.CreateAsset(batStats, batPath);
            }

            // 6. Tinh linh sương (Biến thể Khung Bắn xa - SRS 6.1)
            string spiritPath = $"{DataPath}/SO_FrostSpiritStats.asset";
            var spiritStats = AssetDatabase.LoadAssetAtPath<RangedEnemyStatsSO>(spiritPath);
            if (spiritStats == null)
            {
                spiritStats = ScriptableObject.CreateInstance<RangedEnemyStatsSO>();
                spiritStats.maxHp = 4;
                spiritStats.contactDamage = 1;
                spiritStats.moveSpeed = 4f;
                spiritStats.detectRange = 8f;
                spiritStats.telegraphDuration = 0.6f;
                spiritStats.projectileSpeed = 8f;
                spiritStats.projectileDamage = 1;
                spiritStats.burstCount = 3; // Loạt 3 viên
                spiritStats.burstInterval = 0.2f;
                spiritStats.attackCooldown = 3.0f;
                spiritStats.isIceEnemy = true;
                spiritStats.iceDamageMultiplier = 2.0f;
                AssetDatabase.CreateAsset(spiritStats, spiritPath);
            }
        }

        public static void SetupPrefabs()
        {
            EnsureDirectories();

            Sprite squareSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Characters/Square.png")
                .OfType<Sprite>().FirstOrDefault();

            // 0. Prefab Đạn Băng (PF_EnemyProjectile)
            string projPrefabPath = $"{PrefabPath}/PF_EnemyProjectile.prefab";
            var projGo = new GameObject("PF_EnemyProjectile");
            var projSr = projGo.AddComponent<SpriteRenderer>();
            projSr.sprite = squareSprite;
            projSr.color = new Color(0.6f, 0.9f, 1f, 1f); // Xanh băng
            projGo.transform.localScale = new Vector3(0.35f, 0.2f, 1f);

            var projCol = projGo.AddComponent<CircleCollider2D>();
            projCol.radius = 0.25f;
            projCol.isTrigger = true;

            projGo.AddComponent<EnemyProjectile>();
            PrefabUtility.SaveAsPrefabAsset(projGo, projPrefabPath);
            Object.DestroyImmediate(projGo);

            var projPrefabRef = AssetDatabase.LoadAssetAtPath<GameObject>(projPrefabPath);

            // 1. Sói tuyết (PF_SkullWolf) - Khung Lao tới
            string wolfPrefabPath = $"{PrefabPath}/PF_SkullWolf.prefab";
            var wolfGo = new GameObject("PF_SkullWolf");
            var wolfRb = wolfGo.AddComponent<Rigidbody2D>();
            wolfRb.freezeRotation = true;
            wolfRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var wolfCol = wolfGo.AddComponent<BoxCollider2D>();
            wolfCol.size = new Vector2(2.0f, 1.1f);
            wolfCol.offset = new Vector2(0f, 0.55f);

            var wolfSr = wolfGo.AddComponent<SpriteRenderer>();
            var wolfSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/AtariBoy_SkullWolf/SkullWolf_Sheet.png").OfType<Sprite>().FirstOrDefault();
            if (wolfSprite != null) wolfSr.sprite = wolfSprite;

            var wolfAnim = wolfGo.AddComponent<Animator>();
            wolfAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                $"{EnemyArtPath}SkullWolf/SkullWolf_Animator.controller");

            wolfGo.AddComponent<HitFlash>();
            wolfGo.AddComponent<EnemyTelegraphIndicator>();
            var wolfHealth = wolfGo.AddComponent<EnemyHealth>();
            var wolfAI = wolfGo.AddComponent<ChargeEnemyAI>();

            var wolfStats = AssetDatabase.LoadAssetAtPath<ChargeEnemyStatsSO>($"{DataPath}/SO_SkullWolfStats.asset");
            var wolfSerialized = new SerializedObject(wolfAI);
            wolfSerialized.FindProperty("baseStats").objectReferenceValue = wolfStats;
            wolfSerialized.FindProperty("chargeStats").objectReferenceValue = wolfStats;
            wolfSerialized.FindProperty("spriteFacesLeft").boolValue = true;
            wolfSerialized.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(wolfGo, wolfPrefabPath);
            Object.DestroyImmediate(wolfGo);

            // 2. Nhện băng (PF_PixelSpider) - Khung Đi tuần
            string spiderPrefabPath = $"{PrefabPath}/PF_PixelSpider.prefab";
            var spiderGo = new GameObject("PF_PixelSpider");
            var spiderRb = spiderGo.AddComponent<Rigidbody2D>();
            spiderRb.freezeRotation = true;
            spiderRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var spiderCol = spiderGo.AddComponent<BoxCollider2D>();
            spiderCol.size = new Vector2(1.2f, 0.7f);
            spiderCol.offset = new Vector2(0f, 0.35f);

            var spiderSr = spiderGo.AddComponent<SpriteRenderer>();
            var spiderSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/Spider_v2/Spider_Sheet.png").OfType<Sprite>().FirstOrDefault();
            if (spiderSprite != null) spiderSr.sprite = spiderSprite;

            var spiderAnim = spiderGo.AddComponent<Animator>();
            spiderAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                $"{EnemyArtPath}PixelSpider/PixelSpider_Animator.controller");

            spiderGo.AddComponent<HitFlash>();
            spiderGo.AddComponent<EnemyTelegraphIndicator>();
            var spiderHealth = spiderGo.AddComponent<EnemyHealth>();
            var spiderAI = spiderGo.AddComponent<PatrolEnemyAI>();

            var spiderStats = AssetDatabase.LoadAssetAtPath<PatrolEnemyStatsSO>($"{DataPath}/SO_PixelSpiderStats.asset");
            var spiderSerialized = new SerializedObject(spiderAI);
            spiderSerialized.FindProperty("baseStats").objectReferenceValue = spiderStats;
            spiderSerialized.FindProperty("patrolStats").objectReferenceValue = spiderStats;
            spiderSerialized.FindProperty("spriteFacesLeft").boolValue = false;
            spiderSerialized.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(spiderGo, spiderPrefabPath);
            Object.DestroyImmediate(spiderGo);

            // 3. Quạ xám (PF_GreyCrow) - Khung Bắn xa
            string crowPrefabPath = $"{PrefabPath}/PF_GreyCrow.prefab";
            var crowGo = new GameObject("PF_GreyCrow");
            var crowRb = crowGo.AddComponent<Rigidbody2D>();
            crowRb.freezeRotation = true;
            crowRb.gravityScale = 0f;

            var crowCol = crowGo.AddComponent<CircleCollider2D>();
            crowCol.radius = 0.5f;
            crowCol.offset = new Vector2(0f, 0.5f);

            var crowSr = crowGo.AddComponent<SpriteRenderer>();
            var crowSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/SmithyGames_Crow/Crow.png").OfType<Sprite>().FirstOrDefault();
            if (crowSprite != null) crowSr.sprite = crowSprite;

            var crowAnim = crowGo.AddComponent<Animator>();
            crowAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                $"{EnemyArtPath}GreyCrow/GreyCrow_Animator.controller");

            crowGo.AddComponent<HitFlash>();
            crowGo.AddComponent<EnemyTelegraphIndicator>();
            var crowHealth = crowGo.AddComponent<EnemyHealth>();
            var crowAI = crowGo.AddComponent<RangedEnemyAI>();

            var crowStats = AssetDatabase.LoadAssetAtPath<RangedEnemyStatsSO>($"{DataPath}/SO_GreyCrowStats.asset");
            var crowSerialized = new SerializedObject(crowAI);
            crowSerialized.FindProperty("baseStats").objectReferenceValue = crowStats;
            crowSerialized.FindProperty("rangedStats").objectReferenceValue = crowStats;
            crowSerialized.FindProperty("projectilePrefab").objectReferenceValue = projPrefabRef;
            crowSerialized.FindProperty("spriteFacesLeft").boolValue = true;
            crowSerialized.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(crowGo, crowPrefabPath);
            Object.DestroyImmediate(crowGo);
        }

        public static void BuildSandboxScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Sprite square = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Characters/Square.png")
                .OfType<Sprite>().FirstOrDefault();
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 0;

            // Camera
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.22f, 0.28f);
            camGo.transform.position = new Vector3(0, 2f, -10);

            // Light 2D
            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            // Ground & Platforms
            var level = new GameObject("Level").transform;
            CreateBox("Ground", level, square, new Vector2(0f, -1.5f), new Vector2(50f, 3f), new Color(0.35f, 0.38f, 0.42f), groundLayer);
            CreateBox("PlatformLeft", level, square, new Vector2(-8f, 1.5f), new Vector2(6f, 0.5f), new Color(0.45f, 0.48f, 0.55f), groundLayer);
            CreateBox("CeilingSpider", level, square, new Vector2(0f, 5.5f), new Vector2(8f, 0.5f), new Color(0.45f, 0.48f, 0.55f), groundLayer);
            CreateBox("WallLeft", level, square, new Vector2(-25f, 4f), new Vector2(2f, 10f), new Color(0.3f, 0.3f, 0.35f), groundLayer);
            CreateBox("WallRight", level, square, new Vector2(25f, 4f), new Vector2(2f, 10f), new Color(0.3f, 0.3f, 0.35f), groundLayer);

            // Player
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/PF_PlayerPrototype.prefab");
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(-12f, 0.5f, 0f);
            }

            // Spawn the 3 Enemies
            var wolfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/PF_SkullWolf.prefab");
            if (wolfPrefab != null)
            {
                var wolf = (GameObject)PrefabUtility.InstantiatePrefab(wolfPrefab);
                wolf.name = "Enemy_SkullWolf (Lao Toi)";
                wolf.transform.position = new Vector3(8f, 0f, 0f);
            }

            var spiderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/PF_PixelSpider.prefab");
            if (spiderPrefab != null)
            {
                var spider = (GameObject)PrefabUtility.InstantiatePrefab(spiderPrefab);
                spider.name = "Enemy_PixelSpider (Di Tuan)";
                spider.transform.position = new Vector3(0f, 0f, 0f);
            }

            var crowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/PF_GreyCrow.prefab");
            if (crowPrefab != null)
            {
                var crow = (GameObject)PrefabUtility.InstantiatePrefab(crowPrefab);
                crow.name = "Enemy_GreyCrow (Ban Xa)";
                crow.transform.position = new Vector3(-3f, 3.5f, 0f);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject CreateBox(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size, Color color, int layer)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.layer = layer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            return go;
        }
    }
}
