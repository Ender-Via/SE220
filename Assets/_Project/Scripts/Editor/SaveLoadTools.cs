using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Player;
using Woodcutter.UI;
using Woodcutter.World;

namespace Woodcutter.EditorTools
{
    /// <summary>
    /// Công cụ Editor cho lưu/tải và checkpoint (T020: FR-SYS-06, FR-LVL-04, FR-CMB-06).
    /// - Woodcutter > Save > Setup Save-Load Sandbox: thêm GameProgress + RespawnController vào scene nền
    ///   Sandbox_RoomTransition và đặt checkpoint ở Sandbox_RoomA, Sandbox_RoomC.
    /// - Các mục debug: mở thư mục lưu, in/xóa file lưu, ghi file hỏng, giết nhân vật khi đang Play.
    /// </summary>
    public static class SaveLoadTools
    {
        private const string SandboxFolder = "Assets/_Project/Scenes/Sandbox";
        private const string PersistentScenePath = SandboxFolder + "/Sandbox_RoomTransition.unity";
        private const string RoomAPath = SandboxFolder + "/Sandbox_RoomA.unity";
        private const string RoomCPath = SandboxFolder + "/Sandbox_RoomC.unity";
        private const string SettingsPath = "Assets/_Project/Data/SaveRespawnSettings.asset";
        private const string SquareSpritePath = "Assets/_Project/Art/Characters/Square.png";

        // ================================================================
        // DỰNG SANDBOX
        // ================================================================

        [MenuItem("Woodcutter/Save/Setup Save-Load Sandbox")]
        public static void SetupSandbox()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SaveRespawnSettingsSO settings = EnsureSettingsAsset();
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);

            SetupPersistentScene(settings);
            AddCheckpointToRoom(RoomAPath, new Vector2(9f, 2f), square);
            AddCheckpointToRoom(RoomCPath, new Vector2(8f, 2f), square);

            EditorSceneManager.OpenScene(PersistentScenePath, OpenSceneMode.Single);
            Debug.Log("[SaveLoadTools] Xong. Mở Sandbox_RoomTransition rồi bấm Play. Checkpoint: Sandbox_RoomA, Sandbox_RoomC. " +
                      "Giết nhân vật: Woodcutter > Save > Kill Player (Play Mode).");
        }

        /// <summary>Gọi từ dòng lệnh: Unity -batchmode -executeMethod Woodcutter.EditorTools.SaveLoadTools.SetupSandboxBatch</summary>
        public static void SetupSandboxBatch()
        {
            SaveRespawnSettingsSO settings = EnsureSettingsAsset();
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            SetupPersistentScene(settings);
            AddCheckpointToRoom(RoomAPath, new Vector2(9f, 2f), square);
            AddCheckpointToRoom(RoomCPath, new Vector2(8f, 2f), square);
            AssetDatabase.SaveAssets();
        }

        private static SaveRespawnSettingsSO EnsureSettingsAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<SaveRespawnSettingsSO>(SettingsPath);
            if (existing != null) return existing;

            var created = ScriptableObject.CreateInstance<SaveRespawnSettingsSO>();
            AssetDatabase.CreateAsset(created, SettingsPath);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static void SetupPersistentScene(SaveRespawnSettingsSO settings)
        {
            Scene scene = EditorSceneManager.OpenScene(PersistentScenePath, OpenSceneMode.Single);

            PlayerHealth health = Object.FindAnyObjectByType<PlayerHealth>();
            if (health == null)
            {
                CharacterContext player = Object.FindAnyObjectByType<CharacterContext>();
                if (player == null)
                {
                    Debug.LogError("[SaveLoadTools] Scene nền không có Player (CharacterContext).");
                    return;
                }
                health = player.gameObject.AddComponent<PlayerHealth>(); // ghi đè trên instance, không sửa prefab
            }

            ScreenFader fader = Object.FindAnyObjectByType<ScreenFader>();

            GameProgress progress = Object.FindAnyObjectByType<GameProgress>();
            GameObject session = progress != null ? progress.gameObject : new GameObject("GameSession");
            if (progress == null) progress = session.AddComponent<GameProgress>();
            RespawnController respawn = session.GetComponent<RespawnController>();
            if (respawn == null) respawn = session.AddComponent<RespawnController>();

            SetRefs(progress, ("settings", settings), ("playerHealth", health));
            SetRefs(respawn, ("playerHealth", health), ("fader", fader));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddCheckpointToRoom(string scenePath, Vector2 localPosition, Sprite square)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Room room = Object.FindAnyObjectByType<Room>();
            if (room == null)
            {
                Debug.LogError($"[SaveLoadTools] {scenePath} không có Room.");
                return;
            }

            if (room.GetComponentInChildren<Checkpoint>(true) != null)
                return; // đã có

            var go = new GameObject("Checkpoint");
            go.transform.SetParent(room.transform, false);
            go.transform.localPosition = new Vector3(localPosition.x, localPosition.y + 1.5f, 0f);

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(2f, 3f);

            // Cột mốc (hình chữ nhật đứng) để nhận ra khi ảnh trắng đen (NFR-11/12)
            CreateSprite("Post", go.transform, square, new Vector3(0f, -0.5f, 0f),
                new Vector3(0.4f, 2f, 1f), new Color(0.75f, 0.75f, 0.75f), 0f);

            var activeVisual = CreateSprite("ActiveFlame", go.transform, square, new Vector3(0f, 0.75f, 0f),
                new Vector3(0.5f, 0.5f, 1f), new Color(0.85f, 0.95f, 1f), 45f);

            // Biểu tượng "đã lưu": hình thoi lớn phía trên, không dùng chữ
            var indicator = CreateSprite("SavedIndicator", go.transform, square, new Vector3(0f, 2f, 0f),
                new Vector3(0.6f, 0.6f, 1f), Color.white, 45f);
            indicator.SetActive(false);

            var spawn = new GameObject("SpawnPoint");
            spawn.transform.SetParent(go.transform, false);
            spawn.transform.localPosition = new Vector3(1.5f, -0.5f, 0f);

            var checkpoint = go.AddComponent<Checkpoint>();
            SetRefs(checkpoint, ("spawnPoint", spawn.transform), ("savedIndicator", indicator), ("activeVisual", activeVisual));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject CreateSprite(string name, Transform parent, Sprite sprite, Vector3 localPos,
            Vector3 scale, Color color, float rotationZ)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 5;
            return go;
        }

        private static void SetRefs(Object target, params (string field, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in refs)
            {
                SerializedProperty prop = so.FindProperty(field);
                if (prop == null)
                {
                    Debug.LogError($"[SaveLoadTools] {target.GetType().Name} không có trường '{field}'.");
                    continue;
                }
                prop.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ================================================================
        // DEBUG FILE LƯU
        // ================================================================

        [MenuItem("Woodcutter/Save/Open Save Folder")]
        private static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath + "/");
        }

        [MenuItem("Woodcutter/Save/Print Save File")]
        private static void PrintSave()
        {
            string path = SaveSystem.SavePath;
            Debug.Log(File.Exists(path) ? $"[SaveLoadTools] {path}\n{File.ReadAllText(path)}" : $"[SaveLoadTools] Chưa có file lưu: {path}");
        }

        [MenuItem("Woodcutter/Save/Delete Save File")]
        private static void DeleteSave()
        {
            if (!EditorUtility.DisplayDialog("Xóa file lưu", $"Xóa {SaveSystem.SavePath}?", "Xóa", "Hủy"))
                return;
            SaveSystem.Delete();
            Debug.Log("[SaveLoadTools] Đã xóa file lưu.");
        }

        [MenuItem("Woodcutter/Save/Write Corrupt Save (test)")]
        private static void WriteCorruptSave()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SaveSystem.SavePath, "{ \"version\": 1, \"checkpoint_id\": \"R1-0");
            Debug.Log($"[SaveLoadTools] Đã ghi file lưu hỏng để thử: {SaveSystem.SavePath}");
        }

        /// <summary>
        /// Tự kiểm tra SaveSystem trên file riêng (không đụng file lưu thật): ghi/đọc đủ trường, ghi đè,
        /// không để lại file tạm, phát hiện file hỏng / rỗng / sai phiên bản.
        /// Dòng lệnh: -executeMethod Woodcutter.EditorTools.SaveLoadTools.RunSelfTest
        /// </summary>
        [MenuItem("Woodcutter/Save/Run Save System Self-Test")]
        public static void RunSelfTest()
        {
            const string testFile = "save_selftest.json";
            int failed = 0;
            void Check(bool condition, string label)
            {
                if (condition) Debug.Log($"[SaveSelfTest] OK   {label}");
                else { failed++; Debug.LogError($"[SaveSelfTest] FAIL {label}"); }
            }

            SaveSystem.Configure(testFile);
            try
            {
                SaveSystem.Delete();
                Check(!SaveSystem.HasSave(), "chưa có file");
                Check(SaveSystem.TryLoad(out _) == SaveLoadResult.NoSave, "TryLoad = NoSave khi chưa có file");

                var data = new SaveData
                {
                    checkpoint_id = "R2-05", max_hp = 6, play_time = 812f, game_completed = false
                };
                data.stones.Add("red");
                data.notes.AddRange(new[] { 1, 2 });
                data.opened_obstacles.AddRange(new[] { "R1-04_ice", "R2-03_ice" });
                Check(SaveSystem.Write(data), "ghi lần đầu");
                Check(SaveSystem.TryLoad(out SaveData a) == SaveLoadResult.Ok, "đọc lại = Ok");
                Check(a != null && a.checkpoint_id == "R2-05" && a.max_hp == 6 && a.stones.Count == 1 && a.stones[0] == "red"
                      && a.notes.Count == 2 && a.opened_obstacles.Count == 2 && Mathf.Approximately(a.play_time, 812f)
                      && a.version == SaveData.CurrentVersion, "đủ trường đúng giá trị");

                string json = File.ReadAllText(SaveSystem.SavePath);
                Check(json.Contains("\"checkpoint_id\"") && json.Contains("\"opened_obstacles\"") && json.Contains("\"bosses_defeated\""),
                    "tên trường JSON đúng SRS mục 9");

                data.checkpoint_id = "R3-02";
                data.stones.Add("yellow");
                Check(SaveSystem.Write(data), "ghi đè");
                Check(SaveSystem.TryLoad(out SaveData b) == SaveLoadResult.Ok && b.checkpoint_id == "R3-02" && b.stones.Count == 2, "đọc bản ghi đè");
                Check(!File.Exists(SaveSystem.SavePath + ".tmp") && !File.Exists(SaveSystem.SavePath + ".bak"), "không còn file tạm/.bak");

                File.WriteAllText(SaveSystem.SavePath, "{ \"version\": 1, \"checkpoint_id\": \"R1-0");
                Check(SaveSystem.TryLoad(out _) == SaveLoadResult.Corrupt, "JSON cụt = Corrupt");
                File.WriteAllText(SaveSystem.SavePath, "");
                Check(SaveSystem.TryLoad(out _) == SaveLoadResult.Corrupt, "file rỗng = Corrupt");
                File.WriteAllText(SaveSystem.SavePath, "{ \"version\": 99 }");
                Check(SaveSystem.TryLoad(out _) == SaveLoadResult.Corrupt, "sai phiên bản = Corrupt");
                File.WriteAllText(SaveSystem.SavePath, "{ \"version\": 1, \"checkpoint_id\": \"R1-01\" }");
                Check(SaveSystem.TryLoad(out SaveData c) == SaveLoadResult.Ok && c.stones != null && c.secrets != null, "thiếu trường vẫn đọc được, danh sách không null");

                Check(SaveSystem.Write(data) && SaveSystem.TryLoad(out _) == SaveLoadResult.Ok, "ghi đè lên file hỏng");
            }
            finally
            {
                SaveSystem.Delete();
                SaveSystem.Configure(SaveSystem.DefaultFileName);
            }

            if (failed == 0) Debug.Log("[SaveSelfTest] TẤT CẢ ĐẠT");
            else Debug.LogError($"[SaveSelfTest] {failed} mục KHÔNG ĐẠT");
        }

        [MenuItem("Woodcutter/Save/Kill Player (Play Mode)")]
        private static void KillPlayer()
        {
            PlayerHealth health = Object.FindAnyObjectByType<PlayerHealth>();
            if (health == null) return;
            bool hit = health.TakeDamage(new DamageData(999, default, health.transform.position, Vector2.zero, 0f, null));
            if (!hit) Debug.Log("[SaveLoadTools] Nhân vật đang bất tử (i-frame), thử lại sau.");
        }

        [MenuItem("Woodcutter/Save/Kill Player (Play Mode)", true)]
        private static bool KillPlayerValidate() => Application.isPlaying;
    }
}
