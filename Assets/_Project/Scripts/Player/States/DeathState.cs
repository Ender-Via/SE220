using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái chết của nhân vật (FR-CMB-06).
    /// Khóa toàn bộ input, phát animation Death và chờ RespawnController (SYS) đưa về trạng thái sống.
    /// </summary>
    public class DeathState : CharacterBaseState
    {
        public DeathState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            context.DisableIFrames();

            if (context.Rigidbody != null)
            {
                context.Rigidbody.gravityScale = context.Stats.fallingGravity;
                context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocity.y);
            }

            context.SetVisualState("Die", Color.white);
        }

        public override void PhysicsUpdate()
        {
            // Giữ vận tốc ngang bằng 0, chỉ cho rơi xuống đất nếu chết trên không
            if (context.Rigidbody != null)
            {
                float clampedY = Mathf.Max(context.Rigidbody.linearVelocity.y, -context.Stats.maxFallSpeed);
                context.Rigidbody.linearVelocity = new Vector2(0f, clampedY);
            }
        }

        // Khóa hoàn toàn mọi input khi đã chết
        public override void HandleJumpPressed() { }
        public override void HandleJumpReleased() { }
        public override void HandleAttackPressed() { }
        public override void HandleAttackReleased() { }
        public override void HandleDodgePressed() { }
    }
}