using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Woodcutter.Data;
using Woodcutter.Player;
using Woodcutter.UI;
using Debug = UnityEngine.Debug;

namespace Woodcutter.World
{
    /// <summary>
    /// Chết và hồi sinh (SRS FR-CMB-06, UC-05): khóa nhân vật → màn hình chuyển trắng →
    /// nạp lại phòng checkpoint (quái và boss hồi đầy vì phòng được nạp lại) → đầy máu.
    /// Viên đá, lối đã mở, mẩu giấy giữ nguyên vì nằm trong <see cref="GameProgress"/>.
    /// Đặt trong scene nền cùng RoomManager.
    /// </summary>
    public class RespawnController : MonoBehaviour
    {
        [Header("Tham chiếu trong scene nền")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ScreenFader fader;

        private SaveRespawnSettingsSO settings;
        private bool isRespawning;
        private readonly Stopwatch respawnTimer = new Stopwatch();

        public bool IsRespawning => isRespawning;

        private void Awake()
        {
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        private void OnEnable()
        {
            if (playerHealth == null) return;
            playerHealth.UseExternalRespawn = true;
            playerHealth.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (playerHealth == null) return;
            playerHealth.OnDied -= HandleDied;
            playerHealth.UseExternalRespawn = false;
        }

        private void Start()
        {
            GameProgress progress = GameProgress.Instance;
            settings = progress != null ? progress.Settings : null;
            if (settings == null) settings = ScriptableObject.CreateInstance<SaveRespawnSettingsSO>();
        }

        private void HandleDied()
        {
            if (isRespawning) return;
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            isRespawning = true;
            respawnTimer.Restart();

            RoomManager manager = RoomManager.Instance;
            if (manager != null) manager.SetPlayerFrozen(true);

            // 1. Đứng yên một nhịp (chỗ cho hoạt cảnh chết sau này)
            if (settings.deathDelay > 0f)
                yield return new WaitForSecondsRealtime(settings.deathDelay);

            // Đợi nếu đang chuyển phòng dở (chết đúng lúc chạm cửa)
            while (manager != null && manager.IsTransitioning)
                yield return null;

            // 2. Màn hình chuyển trắng (tuyết)
            if (fader != null)
            {
                fader.SetColor(settings.deathFadeColor);
                yield return fader.FadeTo(1f, settings.deathFadeDuration);
            }

            // 3. Đầy máu khi màn đã che kín
            playerHealth.ReviveFull();

            // 4. Nạp lại phòng checkpoint; chưa có checkpoint thì nạp lại phòng hiện tại ở Default Spawn
            GameProgress progress = GameProgress.Instance;
            string roomId = progress != null && progress.Data.HasCheckpoint ? progress.CheckpointRoomId : null;
            string spawnId = RoomManager.CheckpointSpawnId;
            if (string.IsNullOrEmpty(roomId) && manager != null && manager.CurrentRoom != null)
            {
                roomId = manager.CurrentRoom.RoomId;
                spawnId = null;
            }

            bool started = manager != null && !string.IsNullOrEmpty(roomId) && manager.LoadRoom(roomId, spawnId);
            if (started)
            {
                // LoadRoom tự sáng màn hình và thả nhân vật
                while (manager.IsTransitioning)
                    yield return null;
            }
            else
            {
                Debug.LogError($"[RespawnController] Không nạp được phòng hồi sinh '{roomId}'. Hồi sinh tại chỗ để tránh kẹt.", this);
                if (manager != null) manager.SetPlayerFrozen(false);
                if (fader != null) yield return fader.FadeTo(0f, settings.deathFadeDuration);
            }

            if (fader != null) fader.ResetColor();

            respawnTimer.Stop();
            float seconds = (float)respawnTimer.Elapsed.TotalSeconds;
            if (seconds > settings.respawnWarningThreshold)
                Debug.LogWarning($"[RespawnController] Hồi sinh mất {seconds:0.00}s, vượt ngưỡng {settings.respawnWarningThreshold}s (FR-CMB-06).", this);
            else
                Debug.Log($"[RespawnController] Hồi sinh tại '{roomId}' mất {seconds:0.00}s.", this);

            isRespawning = false;
        }
    }
}
