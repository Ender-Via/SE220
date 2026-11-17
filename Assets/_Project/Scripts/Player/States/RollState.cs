using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    public class RollState : CharacterBaseState
    {
        private float rollTimer;
        private float iFrameTimer;
        private bool hasDisabledIFrames;

        public RollState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }
        public override void Enter()
        {
            base.Enter();

            // Khởi tạo các bộ đếm từ SRS
            rollTimer = context.Stats.rollDuration;
            iFrameTimer = context.Stats.rollIframeDuration;
            hasDisabledIFrames = false;

            // Ghi nhận cooldown và bật I-frame
            context.MarkRollUsed();
            context.EnableIFrames();

            // TÍNH TOÁN TỐC ĐỘ: V = S / t
            float calculatedRollSpeed = context.Stats.rollDistance / context.Stats.rollDuration;
            float faceDir = context.IsFacingRight ? 1f : -1f;

            // Áp dụng lực di chuyển (Sử dụng linearVelocity chuẩn Unity 6)
            context.Rigidbody.linearVelocity = new Vector2(faceDir * calculatedRollSpeed, context.Rigidbody.linearVelocity.y);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            float dt = Time.deltaTime;

            // 1. Quản lý thời gian I-frame
            if (!hasDisabledIFrames)
            {
                iFrameTimer -= dt;
                if (iFrameTimer <= 0f)
                {
                    context.DisableIFrames();
                    hasDisabledIFrames = true;
                }
            }

            // 2. Quản lý thời gian kết thúc State lăn
            rollTimer -= dt;
            if (rollTimer <= 0f)
            {
                // Dừng lực lăn thừa trước khi chuyển state (tránh nhân vật bị trượt thêm)
                context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocity.y);
                context.HandleLandingTransition();
            }
        }

        public override void Exit()
        {
            base.Exit();
            // Fail-safe: Chắc chắn tắt I-frame nếu State bị ngắt đột ngột (rớt vực, trúng đòn...)
            if (!hasDisabledIFrames)
            {
                context.DisableIFrames();
            }
        }
    }
}