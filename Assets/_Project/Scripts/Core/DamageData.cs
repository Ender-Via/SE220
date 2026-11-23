using System;
using UnityEngine;

namespace Woodcutter.Core
{
    /// <summary>
    /// Gói dữ liệu sát thương truyền từ kẻ tấn công sang mục tiêu (SRS 5.1, 5.2, 6.1).
    /// </summary>
    [Serializable]
    public struct DamageData
    {
        /// <summary>Lượng sát thương cơ bản.</summary>
        public int Amount;

        /// <summary>Loại sát thương (Thường, Nặng - phá khiên, Lửa - x2 với quái băng).</summary>
        public DamageType DamageType;

        /// <summary>Điểm va chạm / vị trí trúng đòn.</summary>
        public Vector2 HitPoint;

        /// <summary>Hướng đẩy lùi (chuẩn hóa).</summary>
        public Vector2 KnockbackDirection;

        /// <summary>Lực đẩy lùi.</summary>
        public float KnockbackForce;

        /// <summary>Đối tượng gây sát thương (Player hoặc Enemy).</summary>
        public GameObject Source;

        public DamageData(int amount, DamageType damageType, Vector2 hitPoint, Vector2 knockbackDirection, float knockbackForce = 0f, GameObject source = null)
        {
            Amount = amount;
            DamageType = damageType;
            HitPoint = hitPoint;
            KnockbackDirection = knockbackDirection;
            KnockbackForce = knockbackForce;
            Source = source;
        }
    }
}
