using UnityEngine;

namespace Woodcutter.Core
{
    /// <summary>
    /// Giao diện chung cho mọi thực thể có thể nhận sát thương (Player, Enemy, Boss, vật thể phá hủy).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Nhận sát thương. Trả về true nếu sát thương thực sự được áp dụng (không bị chặn bởi I-frame hoặc đã chết).
        /// </summary>
        bool TakeDamage(int amount, Vector2 hitDirection = default);
    }
}