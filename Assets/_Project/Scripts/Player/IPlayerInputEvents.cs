using System;
using UnityEngine;

namespace Woodcutter.Player
{
    /// <summary>
    /// Giao diện trừu tượng hóa toàn bộ sự kiện đầu vào theo SRS Mục 8.1.
    /// Cho phép Player Controller độc lập hoàn toàn với phần cứng và Input System.
    /// </summary>
    public interface IPlayerInputEvents
    {
        // Di chuyển (A/D, Mũi tên | Cần trái, D-pad)
        event Action<Vector2> OnMoveInput;

        // Nhìn lên/xuống - phục vụ chém lên/xuống (W/S | Cần trái lên/xuống)
        event Action<Vector2> OnLookInput;

        // Nhảy (Space | A)
        event Action OnJumpPressed;
        event Action OnJumpReleased;

        // Chém / đòn nặng (J | X)
        event Action OnAttackPressed;
        event Action OnAttackReleased;

        // Lăn né (K | B)
        event Action OnDodgePressed;

        // Rìu lửa - đá đỏ (L | Y)
        event Action OnFireAxePressed;

        // Dash - đá vàng (Shift | RB)
        event Action OnDashPressed;

        // Gieo hạt - đá xanh (I | LB)
        event Action OnPlantSeedPressed;

        // Tương tác (E | D-pad lên)
        event Action OnInteractPressed;

        // Tạm dừng (Esc | Start)
        event Action OnPausePressed;
    }
}
