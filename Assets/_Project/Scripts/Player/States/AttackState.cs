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
        private float comboWindowTimer;
        private float chargeTimer;

        // Trạng thái đòn đánh
        private int currentComboStep; // 1 hoặc 2
        private bool isCharging;
        private bool hasFiredHeavyAttack;
        private bool nextComboBuffered;

        public AttackState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            // Khởi tạo mặc định là Combo 1
            currentComboStep = 1;
            nextComboBuffered = false;
            isCharging = true; // Bắt đầu sạc đòn nặng
            hasFiredHeavyAttack = false;
            chargeTimer = 0f;

            // Setup thời gian cho đòn thường (sẽ bị ghi đè nếu tung đòn nặng)
            attackTimer = context.CombatStats.attackDuration;
            comboWindowTimer = context.CombatStats.comboWindow;
                
            context.SetVisualState("Attack", new Color(1f, 0.25f, 0.25f)); // Đỏ rực

            isAirAttack = !context.CheckGrounded();

            if (isAirAttack)
            {
                // Khóa trọng lực và đứng im tuyệt đối trên không trung (không trôi, không recoil)
                context.Rigidbody.gravityScale = 0f;
                context.Rigidbody.linearVelocity = Vector2.zero;
                isCharging = false; // Không cho sạc đòn nặng trên không
            }
            else
            {
                // Trên mặt đất: dừng vận tốc ngang
                context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocityY);
            }
        }

        public override void LogicUpdate()
        {
            float dt = Time.deltaTime;

            // Xử lý sạc đòn nặng (FR-CMB-02)
            if (isCharging)
            {
                chargeTimer += dt;

                // Hiệu ứng nhấp nháy báo hiệu đang sạc
                float pulse = Mathf.PingPong(Time.time * 5f, 1f);
                context.SetVisualState("Charging", Color.Lerp(Color.yellow, Color.red, pulse));

                // Tự động tung đòn nặng nếu sạc đủ lâu
                if (chargeTimer >= context.CombatStats.heavyChargeTime && !hasFiredHeavyAttack)
                {
                    FireHeavyAttack();
                }

                // Đang sạc thì không trừ thời gian đòn thường
                return;
            }

            attackTimer -= Time.deltaTime;

            // Khi hết thời gian thi triển đòn
            if (attackTimer <= 0f)
            {
                // Nếu người chơi đã bấm tiếp phím đánh và vẫn trong cửa sổ combo
                if (nextComboBuffered && currentComboStep == 1 && comboWindowTimer > 0f)
                {
                    FireComboStep2();
                }
                else
                {
                    // Kết thúc chuỗi đòn
                    FinishAttackSequence();
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
            context.RegisterJumpBuffer();
        }

        public override void HandleAttackPressed()
        {
            // Cho phép buffer Combo 2 nếu đang chém nhịp 1
            if (!isCharging && !hasFiredHeavyAttack && currentComboStep == 1)
            {
                nextComboBuffered = true;
                Debug.Log("-> Đã nhận lệnh bấm bồi (Buffer) cho Combo 2!");
            }
        }

        private void FireComboStep2()
        {
            currentComboStep = 2;
            nextComboBuffered = false;

            // Cài đặt thời gian cho đòn 2
            attackTimer = context.CombatStats.attackDuration;

            context.SetVisualState("Attack2", new Color(1f, 0f, 0f)); // Đỏ đậm cho Combo 2

            // TODO: Xử lý Hitbox và sát thương (context.CombatStats.combo2Damage)
            Debug.Log("Tung đòn Combo 2!");
        }

        private void FireHeavyAttack()
        {
            isCharging = false;
            hasFiredHeavyAttack = true;

            // Tùy chỉnh thời gian thi triển đòn nặng (tạm dùng attackDuration gốc)
            attackTimer = context.CombatStats.attackDuration * 1.5f;

            context.SetVisualState("HeavyAttack", new Color(0.5f, 0f, 0.5f)); // Tím cho đòn nặng

            // TODO: Xử lý Hitbox phá khiên và hất lùi (context.CombatStats.heavyDamage)
            Debug.Log("Tung đòn Nặng (Phá khiên)!");
        }

        private void FinishAttackSequence()
        {
            if (context.CheckGrounded())
            {
                context.HandleLandingTransition();
            }
            else
            {
                context.FallState.SetCoyoteTime(false);
                stateMachine.ChangeState(context.FallState);
            }
        }

        // Người chơi thả nút chém
        public override void HandleAttackReleased()
        {
            if (isCharging && !hasFiredHeavyAttack)
            {
                isCharging = false;

                // Nếu nhả phím sớm trước khi sạc đủ -> Tung Combo 1 bình thường
                if (chargeTimer < context.CombatStats.heavyChargeTime)
                {
                    context.SetVisualState("Attack1", new Color(1f, 0.5f, 0.5f));
                    // Cấp lại thời gian đánh vì lúc nãy bị đóng băng bởi isCharging
                    attackTimer = context.CombatStats.attackDuration;

                    // TODO: Xử lý Hitbox và sát thương (context.CombatStats.combo1Damage)
                    Debug.Log("Tung đòn Combo 1!");
                }
            }
        }

        public override void Exit()
        {
            // Trả lại trọng lực rơi chuẩn khi kết thúc đòn tấn công trên không
            if (isAirAttack)
            {
                context.Rigidbody.gravityScale = context.Stats.fallingGravity;
            }

            // Đảm bảo cờ sạc đòn được tắt khi thoát state
            isCharging = false;
        }
    }
}
