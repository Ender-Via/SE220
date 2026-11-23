using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Thông số cho khung AI Lao tới (Charge AI) theo SRS 6.1 (Sói tuyết, Dơi hang).
    /// </summary>
    [CreateAssetMenu(fileName = "ChargeEnemyStats", menuName = "Woodcutter/Enemies/ChargeStats")]
    public class ChargeEnemyStatsSO : EnemyStatsSO
    {
        [Header("Charge / Dash Attack (SRS 6.1)")]
        [Tooltip("Tốc độ lao tới / vồ thẳng (ô/giây, SRS: Sói tuyết gầm rồi vồ thẳng)")]
        public float chargeSpeed = 10f;

        [Tooltip("Thời gian thực hiện cú lao vọt tới (giây)")]
        public float chargeDuration = 0.45f;

        [Tooltip("Thời gian hồi giữa các lần lao tới (giây)")]
        public float chargeCooldown = 2.0f;

        [Tooltip("Khoảng cách tối thiểu để kích hoạt cú vồ (ô)")]
        public float minChargeDistance = 1.0f;

        [Header("Variant: Cave Bat (Dơi hang)")]
        [Tooltip("Bắt đầu ở trạng thái ngủ treo trần (SRS: ngủ trong bóng tối, tỉnh khi có ánh lửa)")]
        public bool isSleepingByDefault = false;

        [Tooltip("Bán kính phát hiện ánh lửa làm thức giấc")]
        public float wakeUpDistance = 5f;
    }
}
