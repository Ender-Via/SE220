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

            // Khi chạm đất an toàn -> trở về đất
            if (context.CheckGrounded())
            {
                if (Mathf.Abs(context.CurrentInputVector.x) > 0.05f)
                    stateMachine.ChangeState(context.MoveState);
                else
                    stateMachine.ChangeState(context.IdleState);
            }
        }

        public override void PhysicsUpdate()
        {
            // Cho phép điều hướng trên không
            float targetSpeed = context.CurrentInputVector.x * context.Stats.maxRunSpeed;

            // Giới hạn tốc độ rơi tối đa (SRS FR-MOV-05: maxFallSpeed = 18 ô/s)
            float velY = context.Rigidbody.linearVelocityY;
            if (velY < -context.Stats.maxFallSpeed)
            {
                velY = -context.Stats.maxFallSpeed;
            }

            context.Rigidbody.linearVelocity = new Vector2(targetSpeed, velY);
        }

        public override void HandleJumpPressed()
        {
            // Chỉ cho phép nhảy nếu bước hụt mép và còn trong cửa sổ Coyote time (0.1s)
            if (canCoyoteJump && coyoteTimer > 0f)
            {
                canCoyoteJump = false;
                stateMachine.ChangeState(context.JumpState);
            }
            // Hết Coyote time -> Khóa hoàn toàn, KHÔNG CHO NHẢY LẦN 2 TRÊN KHÔNG
        }

        public override void HandleAttackPressed()
        {
            stateMachine.ChangeState(context.AttackState);
        }

        public override void Exit()
        {
            canCoyoteJump = false;
            context.Rigidbody.gravityScale = context.Stats.ascendingGravity;
        }
    }
}
