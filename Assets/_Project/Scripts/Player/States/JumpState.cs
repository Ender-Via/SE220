using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái nhảy (Jump).
    /// Chỉ được kích hoạt khi nhân vật thực sự bật nhảy từ mặt đất.
    /// Khóa nhảy liên tiếp (chỉ nhảy 1 lần, không double jump).
    /// </summary>
    public class JumpState : CharacterBaseState
    {
        public JumpState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            context.SetVisualState("Jump", new Color(1f, 0.85f, 0.2f)); // Vàng
            // Cung cấp lực đẩy vọt tức thì
            context.Rigidbody.linearVelocity = new Vector2(context.Rigidbody.linearVelocityX, context.Stats.jumpForce);
            context.Rigidbody.gravityScale = context.Stats.ascendingGravity;
        }

        public override void LogicUpdate()
        {
            // Khi vận tốc Y giảm về <= 0, nhân vật đạt đỉnh nhảy -> chuyển sang FallState
            if (context.Rigidbody.linearVelocityY <= 0f)
            {
                context.FallState.SetCoyoteTime(false);
                stateMachine.ChangeState(context.FallState);
            }
        }

        public override void PhysicsUpdate()
        {
            // Điều hướng trên không
            float targetSpeed = context.CurrentInputVector.x * context.Stats.maxRunSpeed;
            context.Rigidbody.linearVelocity = new Vector2(targetSpeed, context.Rigidbody.linearVelocityY);
        }

        // BẮT SỰ KIỆN NHẢ NÚT (SRS FR-MOV-02: Variable Jump Height):
        public override void HandleJumpReleased()
        {
            // Nếu người chơi nhả phím khi nhân vật đang bay lên:
            // Cắt 90% vận tốc Y ngay lập tức -> Triệt tiêu đà nhảy, ép chuyển sang rơi
            if (context.Rigidbody.linearVelocityY > 0f)
            {
                context.Rigidbody.linearVelocity = new Vector2(context.Rigidbody.linearVelocityX, context.Rigidbody.linearVelocityY * 0.1f);
                context.FallState.SetCoyoteTime(false);
                stateMachine.ChangeState(context.FallState);
            }
        }

        // KHÓA NHẢY LẦN 2: Khi đang trên không, tuyệt đối không cho phép nhảy nữa
        public override void HandleJumpPressed()
        {
            // Không làm gì cả - Chống infinite jump
        }

        public override void HandleAttackPressed()
        {
            stateMachine.ChangeState(context.AttackState);
        }
    }
}
