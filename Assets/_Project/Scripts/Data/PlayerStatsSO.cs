using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Lưu trữ toàn bộ thông số chuyển động và chiến đấu của người chơi theo SRS Mục 5.1 & 5.2.
    /// Giúp Designer có thể tinh chỉnh trực tiếp trong Inspector mà không sửa code (NFR-14).
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Woodcutter/Player/PlayerStats")]
    public class PlayerStatsSO : ScriptableObject
    {
        [Header("Movement (SRS 5.1)")]
        [Tooltip("Tốc độ chạy tối đa (SRS: 8 ô/giây)")]
        public float maxRunSpeed = 8f;

        [Tooltip("Thời gian tăng tốc đạt tốc độ tối đa (SRS: 0.08 s)")]
        public float accelerationTime = 0.08f;

        [Tooltip("Thời gian giảm tốc dừng hẳn (SRS: 0.06 s)")]
        public float decelerationTime = 0.06f;

        [Header("Jump & Gravity (SRS 5.1)")]
        [Tooltip("Lực bật nhảy tức thì (phù hợp độ cao 1.5 - 4 ô)")]
        public float jumpForce = 14f;

        [Tooltip("Trọng lực khi bay lên")]
        public float ascendingGravity = 3.5f;

        [Tooltip("Trọng lực khi rơi xuống để cảm giác dứt khoát")]
        public float fallingGravity = 8f;

        [Tooltip("Tốc độ rơi tối đa (SRS: 18 ô/giây)")]
        public float maxFallSpeed = 18f;

        [Tooltip("Thời gian Coyote time (SRS: 0.1 s)")]
        public float coyoteTime = 0.1f;

        [Tooltip("Thời gian Jump buffer (SRS: 0.1 s)")]
        public float jumpBufferTime = 0.1f;

        [Header("Combat & Roll (SRS 5.1 & 5.2)")]
        [Tooltip("Thời gian khóa hành động của đòn chém thường (s)")]
        public float attackDuration = 0.35f;

        [Tooltip("Quãng đường lăn né (SRS: 4 ô)")]
        public float rollDistance = 4f;

        [Tooltip("Thời gian thực hiện lăn né (SRS: 0.35 s)")]
        public float rollDuration = 0.35f;

        [Tooltip("Thời gian bất tử i-frame khi lăn (SRS: 0.25 s)")]
        public float rollIframeDuration = 0.25f;

        [Tooltip("Thời gian hồi lăn né (SRS: 0.5 s)")]
        public float rollCooldown = 0.5f;
    }
}
