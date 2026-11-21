namespace Woodcutter.Core
{
    /// <summary>
    /// Giao diện chuẩn cho tất cả thực thể có thể nhận sát thương (Player, Enemies, Bosses, Vật cản phá được).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Nhận sát thương theo dữ liệu truyền vào.
        /// </summary>
        /// <param name="damageData">Gói dữ liệu sát thương.</param>
        /// <returns>True nếu đòn đánh gây sát thương thành công (không bị khiên/i-frame chặn hoàn toàn).</returns>
        bool TakeDamage(DamageData damageData);
    }
}
