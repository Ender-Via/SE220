using System;
using UnityEngine;

namespace Woodcutter.Player
{
    /// <summary>
    /// Quản lý luồng Chết & Hồi sinh tại điểm hồi sinh gần nhất (FR-CMB-06).
    /// Không tải lại Scene để giữ nguyên viên đá đã nhặt và các lối đi đã mở.
    /// </summary>
    [RequireComponent(typeof(CharacterContext), typeof(PlayerHealth))]
    public class PlayerRespawn : MonoBehaviour
    {
        [Header("Respawn Settings (SRS FR-CMB-06)")]
        [Tooltip("Thời gian chờ trước khi hồi sinh (SRS yêu cầu <= 2 giây)")]
        [Range(0.1f, 2f)]
        [SerializeField] private float respawnDelay = 1.5f;

        public float RespawnDelay => respawnDelay;
        public Vector3 RespawnPosition { get; private set; }

        private CharacterContext context;
        private PlayerHealth playerHealth;

        // Event cho Camera hoặc hiệu ứng chuyển cảnh (ScreenFader) đăng ký sau này nếu cần
        public event Action OnRespawned;

        private void Awake()
        {
            context = GetComponent<CharacterContext>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            // Lưu tạm vị trí xuất phát ban đầu làm điểm hồi sinh mặc định khi chưa có Checkpoint
            RespawnPosition = transform.position;
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDied += HandlePlayerDied;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDied -= HandlePlayerDied;
            }
        }

        private void HandlePlayerDied()
        {
            if (context != null && context.StateMachine != null && context.DeathState != null)
            {
                context.StateMachine.ChangeState(context.DeathState);
            }
        }

        /// <summary>
        /// Hàm chờ nối với hệ thống Checkpoint.
        /// Gọi hàm này khi người chơi kích hoạt một Checkpoint mới.
        /// </summary>
        public void SetRespawnPoint(Vector3 newPosition)
        {
            RespawnPosition = newPosition;
            Debug.Log($"[PlayerRespawn] Đã cập nhật điểm hồi sinh mới: {RespawnPosition}");
        }

        /// <summary>
        /// Hồi sinh nhân vật về điểm hồi sinh gần nhất với đầy máu, không reload Scene.
        /// </summary>
        public void Respawn()
        {
            // 1. Đưa về vị trí hồi sinh và xóa vận tốc cũ
            transform.position = RespawnPosition;
            if (context.Rigidbody != null)
            {
                context.Rigidbody.linearVelocity = Vector2.zero;
                context.Rigidbody.gravityScale = context.Stats.fallingGravity;
            }

            // 2. Khôi phục trạng thái chiến đấu & hồi đầy máu
            context.CanAirAttack = true;
            playerHealth.RestoreFullHealth();

            // 3. Đưa FSM trở lại trạng thái hoạt động bình thường
            if (context.CheckGrounded())
                context.StateMachine.ChangeState(context.IdleState);
            else
                context.StateMachine.ChangeState(context.FallState);

            OnRespawned?.Invoke();
            Debug.Log($"[PlayerRespawn] Đã hồi sinh đầy máu tại {RespawnPosition}!");
        }

        [ContextMenu("Test: Đặt vị trí hiện tại làm Checkpoint tạm")]
        private void DebugSetCurrentPositionAsCheckpoint()
        {
            if (!Application.isPlaying) return;
            SetRespawnPoint(transform.position);
        }
    }
}