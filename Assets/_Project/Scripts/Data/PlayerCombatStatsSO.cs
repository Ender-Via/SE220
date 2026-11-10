using UnityEngine;

namespace Woodcutter.Data
{
    [CreateAssetMenu(fileName = "PlayerCombatStats", menuName = "Woodcutter/Player/CombatStats")]
    public class PlayerCombatStatsSO : ScriptableObject
    {
        [Header("Basic Attack (SRS 5.2)")]
        [Tooltip("Sát thương chém nhịp 1")]
        public int combo1Damage = 1;

        [Tooltip("Sát thương chém nhịp 2")]
        public int combo2Damage = 1;

        [Tooltip("Cửa sổ thời gian bấm chuỗi combo (giây)")]
        public float comboWindow = 0.4f;

        [Tooltip("Kích thước Hitbox chém (ngang x dọc)")]
        public Vector2 hitboxSize = new Vector2(1.5f, 1.5f);

        [Header("Heavy Attack (SRS 5.2)")]
        [Tooltip("Thời gian nạp đòn nặng (giây)")]
        public float heavyChargeTime = 0.6f;

        [Tooltip("Sát thương đòn nặng (phá khiên)")]
        public int heavyDamage = 3;

        [Header("Game Feel")]
        [Tooltip("Thời gian khựng (Hit-stop) đòn thường (giây)")]
        public float normalHitStop = 0.05f;

        [Tooltip("Thời gian khựng (Hit-stop) đòn nặng (giây)")]
        public float heavyHitStop = 0.1f;
    }
}