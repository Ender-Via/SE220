using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Thông số cho khung AI Đi tuần (Patrol AI) theo SRS 6.1 (Nhện băng, Giáp băng).
    /// </summary>
    [CreateAssetMenu(fileName = "PatrolEnemyStats", menuName = "Woodcutter/Enemies/PatrolStats")]
    public class PatrolEnemyStatsSO : EnemyStatsSO
    {
        [Header("Patrol Behavior")]
        [Tooltip("Tốc độ khi đang đi tuần (ô/giây)")]
        public float patrolSpeed = 2f;

        [Tooltip("Bán kính tuần tra từ điểm gốc xuất phát (ô)")]
        public float patrolDistance = 4f;

        [Tooltip("Thời gian đứng yên trước khi quay đầu tuần tra (giây)")]
        public float idleWaitTime = 1.2f;

        [Header("Chase & Attack")]
        [Tooltip("Tốc độ bám đuổi khi phát hiện người chơi (ô/giây)")]
        public float chaseSpeed = 3.5f;

        [Tooltip("Khoảng cách kích hoạt đòn đánh cận chiến (ô)")]
        public float attackRange = 1.2f;

        [Header("Variant: Armored Enemy (Giáp băng)")]
        [Tooltip("Quái có khiên phía trước: chặn 100% đòn thường từ phía trước, chỉ phá bằng đòn nặng hoặc lửa (SRS 6.1)")]
        public bool hasFrontShield = false;

        [Header("Variant: Spider Ambush (Nhện băng)")]
        [Tooltip("Có khả năng bám trần nhà và rơi xuống khi người chơi đi qua (SRS 6.1)")]
        public bool canAmbushFromCeiling = false;

        [Tooltip("Tốc độ rơi khi phục kích từ trần nhà xuống")]
        public float ceilingDropSpeed = 8f;
    }
}
