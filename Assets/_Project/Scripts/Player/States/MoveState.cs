using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái di chuyển trên mặt đất (Move).
    /// </summary>
    public class MoveState : CharacterBaseState
    {
        public MoveState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            context.SetVisualState("Move", new Color(0.35f, 0.75f, 1f)); // Xanh dương nhạt
        }

        public override void LogicUpdate()
        {
            // Nếu bước ra khỏi mép bệ -> rơi xuống kèm Coyote time
            if (!context.CheckGrounded())
            {
                context.FallState.SetCoyoteTime(true);
                stateMachine.ChangeState(context.FallState);
                return;
            }

            // Nếu người chơi nhả phím di chuyển -> quay về Idle
            if (Mathf.Abs(context.CurrentInputVector.x) <= 0.05f)
            {
                stateMachine.ChangeState(context.IdleState);
            }
        }

        public override void PhysicsUpdate()
        {
            float targetSpeed = context.CurrentInputVector.x * context.Stats.maxRunSpeed;
            context.Rigidbody.linearVelocity = new Vector2(targetSpeed, context.Rigidbody.linearVelocityY);
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
