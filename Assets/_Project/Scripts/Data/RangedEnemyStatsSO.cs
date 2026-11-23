using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Thông số cho khung AI Bắn xa (Ranged AI) theo SRS 6.1 (Quạ xám, Tinh linh sương).
    /// </summary>
    [CreateAssetMenu(fileName = "RangedEnemyStats", menuName = "Woodcutter/Enemies/RangedStats")]
    public class RangedEnemyStatsSO : EnemyStatsSO
    {
        [Header("Ranged Attack & Projectile (SRS 6.1)")]
        [Tooltip("Tốc độ bay của viên đạn băng / đá tuyết (ô/giây)")]
        public float projectileSpeed = 7f;

        [Tooltip("Sát thương của mỗi viên đạn")]
        public int projectileDamage = 1;

        [Tooltip("Thời gian tồn tại tối đa của viên đạn trước khi tự hủy (giây)")]
        public float projectileLifetime = 4.0f;

        [Tooltip("Số lượng đạn bắn trong mỗi lượt (Quạ: 1, Tinh linh sương: 3 theo SRS 6.1)")]
        public int burstCount = 1;

        [Tooltip("Khoảng cách thời gian giữa các viên trong loạt đạn 3 viên (giây)")]
        public float burstInterval = 0.2f;

        [Tooltip("Thời gian hồi giữa các lượt bắn (giây)")]
        public float attackCooldown = 2.5f;

        [Header("Flight & Hovering")]
        [Tooltip("Độ cao bay lơ lửng mong muốn so với mặt đất (ô)")]
        public float hoverAltitude = 2.5f;

        [Tooltip("Biên độ dao động lượn sóng khi bay")]
        public float hoverBobbingAmplitude = 0.3f;

        [Tooltip("Tần số dao động lượn sóng")]
        public float hoverBobbingFrequency = 2f;

        [Tooltip("Khoảng cách tối thiểu muốn giữ với người chơi (Kiting: nếu lại gần thì lùi ra)")]
        public float kiteDistance = 3.5f;

        [Header("Variant: Grey Crow Swoop (Quạ xám)")]
        [Tooltip("Hạ thấp độ cao khi tấn công ném đá (SRS: người chơi chém lên khi nó hạ thấp)")]
        public bool swoopDownOnAttack = true;

        [Tooltip("Độ hạ thấp trục Y khi chuẩn bị tấn công (ô)")]
        public float swoopDistanceY = 1.2f;
    }
}
