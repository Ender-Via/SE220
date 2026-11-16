using UnityEngine;

namespace Woodcutter.Data
{
    [CreateAssetMenu(fileName = "AbilitiesStats", menuName = "Woodcutter/Player/AbilitiesStats")]
    public class AbilitiesStatsSO : ScriptableObject
    {
        [Header("Red Stone - Fire Axe (SRS 5.2)")]
        [Tooltip("Sát thương rìu lửa")]
        public int fireAxeDamage = 2;

        [Tooltip("Thời gian hồi chiêu rìu lửa (giây)")]
        public float fireAxeCooldown = 1.5f;

        [Tooltip("Hệ số nhân sát thương với quái băng")]
        public float iceEnemyDamageMultiplier = 2.0f;

        [Header("Yellow Stone - Dash (SRS 5.2)")]
        [Tooltip("Quãng đường lướt (ô)")]
        public float dashDistance = 5f;

        [Tooltip("Thời gian lướt (giây)")]
        public float dashDuration = 0.2f;

        [Header("Green Stone - Plant Seed (SRS 5.2)")]
        [Tooltip("Thời gian cây mọc lên (giây)")]
        public float seedGrowDuration = 0.5f;

        [Tooltip("Độ cao của bệ cây (ô)")]
        public float platformHeight = 3f;
    }
}