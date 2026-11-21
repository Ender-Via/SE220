namespace Woodcutter.Core
{
    /// <summary>
    /// Các loại sát thương trong game (SRS 5.2 & 6.1).
    /// </summary>
    public enum DamageType
    {
        /// <summary>Đòn chém thường (nhịp 1 và nhịp 2 của combo rìu, 1 sát thương).</summary>
        Normal,

        /// <summary>Đòn nặng (nạp đòn 0.5s, 3 sát thương, phá khiên của quái giáp băng).</summary>
        Heavy,

        /// <summary>Rìu lửa (đá đỏ, 2 sát thương, x2 sát thương với quái hệ băng, làm tan băng).</summary>
        Fire
    }
}
