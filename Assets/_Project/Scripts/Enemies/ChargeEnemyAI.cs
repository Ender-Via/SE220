using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Khung AI 2: Lao tới (Charge / Dash AI Framework) theo SRS mục 6.1.
    /// Áp dụng cho:
    /// - Sói tuyết (SkullWolf - Khu 1): Gầm gừ (báo hiệu >= 0.4s) rồi vồ thẳng; người chơi lăn né qua rồi chém sau lưng.
    /// - Dơi hang (Cave Bat - Khu 2): Ngủ trong bóng tối, thức giấc khi có lửa/tiếp cận, lao sà tới.
    /// </summary>
    public class ChargeEnemyAI : EnemyBase
    {
        [Header("Charge Specific Configuration")]
        [SerializeField] private ChargeEnemyStatsSO chargeStats;

        [Header("Variant: Cave Bat Flying Charge")]
        [Tooltip("Cho phép quái bay lượn tự do (không bị trọng lực kéo xuống đất, ví dụ Dơi hang)")]
        [SerializeField] private bool isFlyingCharger = false;

        public ChargeEnemyStatsSO ChargeStats => chargeStats != null ? chargeStats : baseStats as ChargeEnemyStatsSO;

        // FSM States
        private ChargeIdleState idleState;
        private ChargeTelegraphState telegraphState;
        private ChargeLungeState lungeState;
        private ChargeRecoverState recoverState;

        private float nextChargeTime;

        protected override void Awake()
        {
            if (baseStats == null && chargeStats != null)
            {
                baseStats = chargeStats;
            }
            base.Awake();

            if (isFlyingCharger && rb != null)
            {
                rb.gravityScale = 0f;
            }

            idleState = new ChargeIdleState(this, StateMachine);
            telegraphState = new ChargeTelegraphState(this, StateMachine);
            lungeState = new ChargeLungeState(this, StateMachine);
            recoverState = new ChargeRecoverState(this, StateMachine);
        }

        protected override void Start()
        {
            base.Start();
            StateMachine.Initialize(idleState);
        }

        public override void Respawn()
        {
            base.Respawn();
            nextChargeTime = 0f;
            StateMachine.ChangeState(idleState);
        }

        public bool CanCharge()
        {
            return Time.time >= nextChargeTime;
        }

        public void MarkChargeUsed()
        {
            float cd = ChargeStats != null ? ChargeStats.chargeCooldown : 2.0f;
            nextChargeTime = Time.time + cd;
        }

        // ================================================================
        // FSM STATES
        // ================================================================

        /// <summary>
        /// State 1: Đứng quan sát / Rình mồi / Ngủ (Dơi hang)
        /// </summary>
        private class ChargeIdleState : IState
        {
            private readonly ChargeEnemyAI ai;
            private readonly StateMachine fsm;

            public ChargeIdleState(ChargeEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                ai.PlayAnimation("Idle");
                if (!ai.isFlyingCharger)
                {
                    ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
                }
                else
                {
                    ai.rb.linearVelocity = Vector2.zero;
                }
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                if (ai.Target == null) return;

                // Nếu là dơi hang đang ngủ: kiểm tra bán kính thức giấc
                if (ai.ChargeStats != null && ai.ChargeStats.isSleepingByDefault)
                {
                    if (ai.DistanceToTarget() > ai.ChargeStats.wakeUpDistance)
                        return;
                }

                // Kiểm tra người chơi vào tầm phát hiện và đã hồi chiêu lao
                if (ai.IsTargetInDetectRange() && ai.CanCharge())
                {
                    float dist = ai.DistanceToTarget();
                    float minDist = ai.ChargeStats != null ? ai.ChargeStats.minChargeDistance : 1.0f;

                    if (dist >= minDist)
                    {
                        fsm.ChangeState(ai.telegraphState);
                        return;
                    }
                }

                ai.FaceTarget();
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State 2: Báo hiệu trước khi vồ (Telegraph >= 0.4s theo SRS 6.1)
        /// Sói tuyết: Gầm gừ, rung mình lấy đà, khóa hướng vồ
        /// </summary>
        private class ChargeTelegraphState : IState
        {
            private readonly ChargeEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public ChargeTelegraphState(ChargeEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                float duration = ai.ChargeStats != null ? ai.ChargeStats.telegraphDuration : 0.6f;
                timer = duration;

                // Khựng lại tại chỗ lấy đà
                ai.rb.linearVelocity = ai.isFlyingCharger ? Vector2.zero : new Vector2(0f, ai.rb.linearVelocity.y);

                ai.FaceTarget();
                ai.PlayAnimation("Idle");
                ai.StartTelegraph(duration);
            }

            public void Exit()
            {
                ai.StopTelegraph();
            }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    fsm.ChangeState(ai.lungeState);
                }
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State 3: Vồ thẳng / Lao tới với tốc độ cao
        /// Người chơi có thể bấm Lăn né (K) để lướt xuyên qua cú vồ nhờ I-frame
        /// </summary>
        private class ChargeLungeState : IState
        {
            private readonly ChargeEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;
            private Vector2 lungeDirection;
            private bool hitTargetDuringLunge;

            public ChargeLungeState(ChargeEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                timer = ai.ChargeStats != null ? ai.ChargeStats.chargeDuration : 0.45f;
                hitTargetDuringLunge = false;

                // Xác định vector hướng lao
                if (ai.isFlyingCharger && ai.Target != null)
                {
                    lungeDirection = ai.DirectionToTarget();
                }
                else
                {
                    float dirX = ai.IsFacingRight ? 1f : -1f;
                    lungeDirection = new Vector2(dirX, 0f);
                }

                ai.PlayAnimation("Attack");
                ai.MarkChargeUsed();
            }

            public void Exit()
            {
                ai.rb.linearVelocity = ai.isFlyingCharger ? Vector2.zero : new Vector2(0f, ai.rb.linearVelocity.y);
            }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;

                // Kiểm tra chạm người chơi trong quá trình lao
                if (!hitTargetDuringLunge && ai.DistanceToTarget() <= 1.4f)
                {
                    ai.ExecuteChargeHit();
                    hitTargetDuringLunge = true;
                }

                // Dừng lại nếu va vào tường hoặc hết thời gian lao
                if (timer <= 0f || (!ai.isFlyingCharger && ai.CheckWallAhead()))
                {
                    fsm.ChangeState(ai.recoverState);
                }
            }

            public void PhysicsUpdate()
            {
                float speed = ai.ChargeStats != null ? ai.ChargeStats.chargeSpeed : 10f;
                if (ai.isFlyingCharger)
                {
                    ai.rb.linearVelocity = lungeDirection * speed;
                }
                else
                {
                    ai.rb.linearVelocity = new Vector2(lungeDirection.x * speed, ai.rb.linearVelocity.y);
                }
            }
        }

        /// <summary>
        /// State 4: Khựng lại hồi phục (SRS: Cửa sổ mở để người chơi lăn ra sau lưng rồi chém)
        /// Không quay đầu ngay lập tức!
        /// </summary>
        private class ChargeRecoverState : IState
        {
            private readonly ChargeEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public ChargeRecoverState(ChargeEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                timer = ai.ChargeStats != null ? ai.ChargeStats.recoverDuration : 0.9f;
                ai.rb.linearVelocity = ai.isFlyingCharger ? Vector2.zero : new Vector2(0f, ai.rb.linearVelocity.y);
                ai.PlayAnimation("Idle");
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    fsm.ChangeState(ai.idleState);
                }
            }

            public void PhysicsUpdate() { }
        }

        public void ExecuteChargeHit()
        {
            if (Target == null) return;
            int dmg = ChargeStats != null ? ChargeStats.contactDamage : 1;
            Vector2 knockDir = (IsFacingRight ? Vector2.right : Vector2.left) + Vector2.up * 0.5f;

            var damageable = Target.GetComponent<IDamageable>() ?? Target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                var dmgData = new DamageData(dmg, DamageType.Normal, transform.position, knockDir.normalized, 7f, gameObject);
                damageable.TakeDamage(dmgData);
                return;
            }

            var demoHealth = Target.GetComponent<Demo.DemoPlayerHealth>() ?? Target.GetComponentInParent<Demo.DemoPlayerHealth>();
            if (demoHealth != null)
            {
                demoHealth.TakeDamage(dmg, transform.position);
            }
        }
    }
}
