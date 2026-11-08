using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái rơi tự do (Fall).
    /// Kích hoạt khi:
    /// 1. Nhảy đạt đỉnh vận tốc Y <= 0 (từ JumpState).
    /// 2. Nhả nút nhảy sớm (Variable Jump Height).
    /// 3. Bước hụt ra khỏi mép bệ (từ MoveState hoặc IdleState).
    /// 4. Kết thúc đòn tấn công trên không.
    /// </summary>
    public class FallState : CharacterBaseState
    {
        private float coyoteTimer;
        private bool canCoyoteJump;

        public FallState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        /// <summary>
        /// Kích hoạt cửa sổ Coyote time khi vừa bước hụt mép bệ (SRS FR-MOV-03).
        /// </summary>
        public void SetCoyoteTime(bool allowCoyote)
        {
            canCoyoteJump = allowCoyote;
            coyoteTimer = allowCoyote ? context.Stats.coyoteTime : 0f;
        }

        public override void Enter()
        {
            context.SetVisualState("Fall", new Color(0.9f, 0.6f, 0.2f)); // Cam đất
            context.Rigidbody.gravityScale = context.Stats.fallingGravity;
        }

        public override void LogicUpdate()
        {
            if (coyoteTimer > 0f)
            {
                coyoteTimer -= Time.deltaTime;
                if (coyoteTimer <= 0f) canCoyoteJump = false;
            }

            // Khi chạm đất: Kiểm tra Jump Buffer (SRS FR-MOV-03)
            if (context.CheckGrounded())
            {
                if (context.HasBufferedJump())
                {
                    context.ConsumeJumpBuffer();
                    stateMachine.ChangeState(context.JumpState);
                }
                else if (Mathf.Abs(context.CurrentInputVector.x) > 0.05f)
                {
                    stateMachine.ChangeState(context.MoveState);
                }
                else
                {
                    stateMachine.ChangeState(context.IdleState);
                }
            }
        }

        public override void PhysicsUpdate()
        {
            // Điều hướng trên không theo gia tốc / giảm tốc (SRS 5.1)
            float newX = context.ComputeHorizontalVelocity(context.CurrentInputVector.x);

            // Giới hạn tốc độ rơi tối đa (SRS FR-MOV-05: maxFallSpeed = 18 ô/s)
            float velY = context.Rigidbody.linearVelocityY;
            if (velY < -context.Stats.maxFallSpeed)
            {
                velY = -context.Stats.maxFallSpeed;
            }

            context.Rigidbody.linearVelocity = new Vector2(newX, velY);
        }

        public override void HandleJumpPressed()
        {
            // 1. Nhảy cứu vớt nếu còn trong Coyote time (SRS FR-MOV-03)
            if (canCoyoteJump && coyoteTimer > 0f)
            {
                canCoyoteJump = false;
                stateMachine.ChangeState(context.JumpState);
                return;
            }

            // 2. Nếu đã hết Coyote time, ghi nhận vào Jump Buffer (SRS FR-MOV-03: 0.1s)
            // Nhấn Space trước khi chạm đất 0.1s sẽ tự động nhảy ngay khi tiếp đất
            context.RegisterJumpBuffer();
        }

        public override void HandleAttackPressed()
        {
            // Chỉ được tấn công 1 lần duy nhất trong không trung
            if (context.CanAirAttack)
            {
                context.CanAirAttack = false;
                stateMachine.ChangeState(context.AttackState);
            }
        }

        public override void Exit()
        {
            canCoyoteJump = false;
            context.Rigidbody.gravityScale = context.Stats.ascendingGravity;
        }
    }
}
