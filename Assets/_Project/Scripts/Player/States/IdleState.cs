using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái đứng yên (Idle).
    /// Áp dụng giảm tốc về 0 theo decelerationTime 0.06s (SRS 5.1).
    /// </summary>
    public class IdleState : CharacterBaseState
    {
        public IdleState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            context.SetVisualState("Idle", Color.white);

            if (context.HasBufferedJump())
            {
                context.ConsumeJumpBuffer();
                stateMachine.ChangeState(context.JumpState);
            }
        }

        public override void LogicUpdate()
        {
            // Nếu không còn chạm đất (bước hụt hoặc bệ di chuyển) -> rơi xuống kèm Coyote time
            if (!context.CheckGrounded())
            {
                context.FallState.SetCoyoteTime(true);
                stateMachine.ChangeState(context.FallState);
                return;
            }

            // Nếu có tín hiệu di chuyển ngang -> chuyển sang MoveState
            if (Mathf.Abs(context.CurrentInputVector.x) > 0.05f)
            {
                stateMachine.ChangeState(context.MoveState);
            }
        }

        public override void PhysicsUpdate()
        {
            // Giảm tốc mượt mà về 0 theo decelerationTime (SRS 5.1)
            float newX = context.ComputeHorizontalVelocity(0f);
            context.Rigidbody.linearVelocity = new Vector2(newX, context.Rigidbody.linearVelocityY);
        }

        public override void HandleJumpPressed()
        {
            // CHỈ CHO PHÉP NHẢY KHI THẬT SỰ CHẠM ĐẤT
            if (context.CheckGrounded())
            {
                stateMachine.ChangeState(context.JumpState);
            }
        }

        public override void HandleAttackPressed()
        {
            stateMachine.ChangeState(context.AttackState);
        }
    }
}
