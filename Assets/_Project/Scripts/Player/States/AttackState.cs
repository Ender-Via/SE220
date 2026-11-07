using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái tấn công (Attack).
    /// Minh họa tính năng Khóa hành động (Mutual Exclusion):
    /// Khi đang chém, các sự kiện Jump/Move không được override, đảm bảo đòn đánh thực hiện trọn vẹn mà không cần cờ boolean.
    /// </summary>
    public class AttackState : CharacterBaseState
    {
        private float attackTimer;

        public AttackState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            attackTimer = context.Stats.attackDuration;
            context.SetVisualState("Attack", new Color(1f, 0.25f, 0.25f)); // Đỏ rực
            // Khóa di chuyển ngang khi đang ra đòn
            context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocityY);
        }

        public override void LogicUpdate()
        {
            attackTimer -= Time.deltaTime;

            // Khi hết thời gian vung đòn -> Tự động quay về State thích hợp
            if (attackTimer <= 0f)
            {
                if (context.CheckGrounded())
                {
                    stateMachine.ChangeState(context.IdleState);
                }
                else
                {
                    context.FallState.SetCoyoteTime(false);
                    stateMachine.ChangeState(context.FallState);
                }
            }
        }

        // KHÔNG OVERRIDE HandleJumpPressed() hay HandleAttackPressed()!
        // Mọi input spam đều bị bỏ qua tuyệt đối trong trạng thái này.
    }
}
