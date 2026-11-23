using System;
using System.Collections.Generic;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Player;

namespace Woodcutter.World
{
    /// <summary>Mã viên đá dùng trong file lưu (SRS mục 9).</summary>
    public static class StoneId
    {
        public const string Red = "red";
        public const string Yellow = "yellow";
        public const string Green = "green";
    }

    /// <summary>Cách vào game, do menu chính đặt trước khi nạp scene nền (FR-SYS-01, UC-01).</summary>
    public enum GameStartMode
    {
        /// <summary>Mở thẳng scene trong Editor: đọc file lưu nếu bật Load Existing Save On Start.</summary>
        Default,
        /// <summary>Chơi mới: tạo file lưu mới.</summary>
        NewGame,
        /// <summary>Tiếp tục: đọc file lưu, vào checkpoint cuối.</summary>
        Continue
    }

    /// <summary>
    /// Giữ tiến trình đang chơi (SaveData) trong scene nền, tự lưu tại checkpoint / nhận đá / hạ boss
    /// (SRS FR-SYS-06, FR-LVL-04, mục 9). Các hệ thống khác (đá, vật cản, mẩu giấy, boss) gọi API ở đây
    /// thay vì đụng thẳng vào file lưu.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Đọc file lưu trước RoomManager.Start chọn phòng bắt đầu
    public class GameProgress : MonoBehaviour
    {
        public static GameProgress Instance { get; private set; }

        private static GameStartMode pendingStartMode = GameStartMode.Default;

        [Header("Thông số")]
        [SerializeField] private SaveRespawnSettingsSO settings;

        [Header("Tham chiếu trong scene nền")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Thử trong Editor")]
        [Tooltip("Khi mở thẳng scene (không qua menu chính): đọc file lưu có sẵn và vào checkpoint cuối.")]
        [SerializeField] private bool loadExistingSaveOnStart = true;

        public SaveData Data { get; private set; } = new SaveData();
        public SaveRespawnSettingsSO Settings => settings;

        /// <summary>Kết quả đọc file lưu lúc vào game (menu dùng để báo file hỏng).</summary>
        public SaveLoadResult LastLoadResult { get; private set; } = SaveLoadResult.NoSave;

        /// <summary>Menu tạm dừng đặt true để không cộng thời gian chơi.</summary>
        public bool IsPaused { get; set; }

        /// <summary>Phòng checkpoint để vào game / hồi sinh. Rỗng = chưa có checkpoint.</summary>
        public string CheckpointRoomId => Data.checkpoint_id;

        /// <summary>Bắn ra sau mỗi lần ghi file (true = thành công).</summary>
        public event Action<bool> OnSaved;

        /// <summary>Bắn ra khi tiến trình đổi (đá, vật cản, mẩu giấy...). HUD/hồi màu nghe sự kiện này.</summary>
        public event Action OnProgressChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            pendingStartMode = GameStartMode.Default;
        }

        /// <summary>Menu chính gọi trước khi nạp scene nền.</summary>
        public static void RequestStart(GameStartMode mode)
        {
            pendingStartMode = mode;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[GameProgress] Có hơn một GameProgress đang chạy.", this);
                enabled = false;
                return;
            }

            Instance = this;
            if (settings == null) settings = ScriptableObject.CreateInstance<SaveRespawnSettingsSO>();
            SaveSystem.Configure(settings.saveFileName);
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();

            GameStartMode mode = pendingStartMode;
            pendingStartMode = GameStartMode.Default;
            InitializeData(mode);
        }

        private void Start()
        {
            ApplyToPlayer();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsPaused)
                Data.play_time += Time.unscaledDeltaTime;
        }

        private void InitializeData(GameStartMode mode)
        {
            if (mode == GameStartMode.NewGame)
            {
                // UC-01: chơi mới tạo file lưu mới (menu đã hỏi xác nhận ghi đè)
                Data = new SaveData();
                LastLoadResult = SaveLoadResult.NoSave;
                SaveSystem.Write(Data);
                return;
            }

            if (mode == GameStartMode.Default && !loadExistingSaveOnStart)
            {
                Data = new SaveData();
                LastLoadResult = SaveLoadResult.NoSave;
                return;
            }

            LastLoadResult = SaveSystem.TryLoad(out SaveData loaded);
            if (LastLoadResult == SaveLoadResult.Ok)
            {
                Data = loaded;
                Debug.Log($"[GameProgress] Đã tải file lưu: checkpoint '{Data.checkpoint_id}', đá [{string.Join(", ", Data.stones)}].", this);
            }
            else
            {
                if (LastLoadResult == SaveLoadResult.Corrupt)
                    Debug.LogWarning("[GameProgress] File lưu hỏng, bắt đầu tiến trình mới (file cũ giữ nguyên tới lần lưu kế tiếp).", this);
                Data = new SaveData();
            }
        }

        private void ApplyToPlayer()
        {
            if (playerHealth != null && Data.max_hp > 0)
                playerHealth.SetMaxHealth(Data.max_hp, true);
        }

        // ================================================================
        // LƯU
        // ================================================================

        /// <summary>Đặt checkpoint hồi sinh (mã phòng) và tự lưu (FR-LVL-04).</summary>
        public bool SetCheckpointAndSave(string roomId)
        {
            if (!string.IsNullOrEmpty(roomId))
                Data.checkpoint_id = roomId;
            return SaveGame();
        }

        /// <summary>Ghi toàn bộ tiến trình hiện tại ra file.</summary>
        public bool SaveGame()
        {
            if (playerHealth != null && playerHealth.MaxHp > 0)
                Data.max_hp = playerHealth.MaxHp;

            bool ok = SaveSystem.Write(Data);
            if (ok) Debug.Log($"[GameProgress] Đã lưu game tại checkpoint '{Data.checkpoint_id}'.", this);
            OnSaved?.Invoke(ok);
            return ok;
        }

        // ================================================================
        // TIẾN TRÌNH (các task sau gọi)
        // ================================================================

        /// <summary>Nhận viên đá: tự lưu (SRS mục 9). Trả về false nếu đã có.</summary>
        public bool AddStone(string stoneId)
        {
            if (!AddUnique(Data.stones, stoneId)) return false;
            SaveGame();
            return true;
        }

        public bool HasStone(string stoneId) => Data.stones.Contains(stoneId);

        /// <summary>Hạ boss: tự lưu (SRS mục 9).</summary>
        public bool MarkBossDefeated(string bossId)
        {
            if (!AddUnique(Data.bosses_defeated, bossId)) return false;
            SaveGame();
            return true;
        }

        public bool IsBossDefeated(string bossId) => Data.bosses_defeated.Contains(bossId);

        /// <summary>Vật cản đã phá (FR-LVL-02). Ghi vào file ở lần lưu kế tiếp, giữ nguyên khi chết.</summary>
        public bool MarkObstacleOpened(string obstacleId) => AddUnique(Data.opened_obstacles, obstacleId);
        public bool IsObstacleOpened(string obstacleId) => Data.opened_obstacles.Contains(obstacleId);

        public bool MarkShortcutOpened(string shortcutId) => AddUnique(Data.shortcuts, shortcutId);
        public bool IsShortcutOpened(string shortcutId) => Data.shortcuts.Contains(shortcutId);

        public bool MarkSecretTaken(string secretId) => AddUnique(Data.secrets, secretId);
        public bool IsSecretTaken(string secretId) => Data.secrets.Contains(secretId);

        public bool AddNote(int noteNumber)
        {
            if (Data.notes.Contains(noteNumber)) return false;
            Data.notes.Add(noteNumber);
            Data.notes.Sort();
            OnProgressChanged?.Invoke();
            return true;
        }

        public bool HasNote(int noteNumber) => Data.notes.Contains(noteNumber);

        /// <summary>Đã xem ending: tự lưu.</summary>
        public void MarkGameCompleted()
        {
            if (Data.game_completed) return;
            Data.game_completed = true;
            OnProgressChanged?.Invoke();
            SaveGame();
        }

        private bool AddUnique(List<string> list, string id)
        {
            if (string.IsNullOrEmpty(id) || list.Contains(id)) return false;
            list.Add(id);
            OnProgressChanged?.Invoke();
            return true;
        }
    }
}
