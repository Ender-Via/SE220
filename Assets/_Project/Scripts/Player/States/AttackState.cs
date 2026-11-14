using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái tấn công (Attack).
    /// Hỗ trợ cả tấn công trên mặt đất và trên không:
    /// - Trên mặt đất: Phanh dừng di chuyển ngang, khóa hành động trong attackDuration.
    /// - Trên không: Khóa hoàn toàn trọng lực (gravityScale = 0), đứng im hoàn toàn tại chỗ (linearVelocity = 0, không recoil),
    ///   chỉ được tấn công MỘT lần duy nhất trong không trung. Tấn công xong mới cho rơi tiếp.
    /// </summary>
    public class AttackState : CharacterBaseState
    {
        private float attackTimer;
        private bool isAirAttack;

        public AttackState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            attackTimer = context.Stats.attackDuration;
            context.SetVisualState("Attack", new Color(1f, 0.25f, 0.25f)); // Đỏ rực

            isAirAttack = !context.CheckGrounded();

            if (isAirAttack)
            {
                // Khóa trọng lực và đứng im tuyệt đối trên không trung (không trôi, không recoil)
                context.Rigidbody.gravityScale = 0f;
                context.Rigidbody.linearVelocity = Vector2.zero;
            }
            else
            {
                // Trên mặt đất: dừng vận tốc ngang
                context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocityY);
            }
        }

        public override void LogicUpdate()
        {
            attackTimer -= Time.deltaTime;

            // Khi hết thời gian vung đòn -> Tự động quay về State thích hợp
            if (attackTimer <= 0f)
            {
                if (context.CheckGrounded())
                {
                    // Chạm đất: Nếu có Jump Buffer trước đó thì thực thi nhảy ngay lập tức
                    context.HandleLandingTransition();
                }
                else
                {
                    // Vẫn đang ở trên không -> Cho rơi tiếp sau khi chém xong
                    context.FallState.SetCoyoteTime(false);
                    stateMachine.ChangeState(context.FallState);
                }
            }
        }

        public override void PhysicsUpdate()
        {
            if (isAirAttack)
            {
                // Giữ vị trí đóng băng tuyệt đối trong suốt thời gian tấn công trên không
                context.Rigidbody.linearVelocity = Vector2.zero;
            }
        }

        public override void HandleJumpPressed()
        {
            // Bấm nhảy khi đang chém trên không -> Ghi nhận vào Jump Buffer
            if (isAirAttack)
            {
                context.RegisterJumpBuffer();
            }
        }

        public override void Exit()
        {
            // Trả lại trọng lực rơi chuẩn khi kết thúc đòn tấn công trên không
            if (isAirAttack)
            {
                context.Rigidbody.gravityScale = context.Stats.fallingGravity;
            }
        }
    }
}
