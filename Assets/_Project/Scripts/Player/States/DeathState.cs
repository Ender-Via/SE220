using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Trạng thái chết của nhân vật (FR-CMB-06).
    /// Khóa toàn bộ input, phát animation Death và đếm ngược thời gian hồi sinh.
    /// </summary>
    public class DeathState : CharacterBaseState
    {
        private float respawnTimer;
        private bool hasTriggeredRespawn;

        public DeathState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            hasTriggeredRespawn = false;
            respawnTimer = context.RespawnComponent != null ? context.RespawnComponent.RespawnDelay : 1.5f;

            // Tắt I-frame lăn né nếu lỡ đang bật và dừng vận tốc ngang
            context.DisableIFrames();
            if (context.Rigidbody != null)
            {
                context.Rigidbody.gravityScale = context.Stats.fallingGravity;
                context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocity.y);
            }

            // Phát animation Death
            context.SetVisualState("Death", Color.white);
        }

        public override void LogicUpdate()
        {
            if (hasTriggeredRespawn) return;

            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                hasTriggeredRespawn = true;
                if (context.RespawnComponent != null)
                {
                    context.RespawnComponent.Respawn();
                }
                else
                {
                    // Fallback an toàn nếu quên gắn PlayerRespawn
                    context.HandleLandingTransition();
                }
            }
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