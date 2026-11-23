using System.Collections;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Khung AI 1: Đi tuần (Patrol AI Framework) theo SRS mục 6.1.
    /// Áp dụng cho:
    /// - Nhện băng (PixelSpider - Khu 1): Đi tuần trên sàn/trần, rơi xuống phục kích khi người chơi đi qua.
    /// - Giáp băng (Ice Armor - Khu 2): Đi tuần có khiên trước mặt, chỉ đòn nặng hoặc rìu lửa mới xuyên qua.
    /// </summary>
    public class PatrolEnemyAI : EnemyBase
    {
        [Header("Patrol Specific Configuration")]
        [SerializeField] private PatrolEnemyStatsSO patrolStats;

        [Header("Ceiling Ambush (Nhện băng)")]
        [Tooltip("Bắt đầu treo bám trên trần nhà và rơi xuống khi người chơi đi qua (SRS 6.1)")]
        [SerializeField] private bool startOnCeiling = false;
        [SerializeField] private float ambushDetectWidth = 2.0f;
        [SerializeField] private float ambushDetectHeight = 6.0f;

        public PatrolEnemyStatsSO PatrolStats => patrolStats != null ? patrolStats : baseStats as PatrolEnemyStatsSO;

        // Các State của FSM
        private PatrolWalkingState walkingState;
        private PatrolIdleState idleState;
        private PatrolChaseState chaseState;
        private PatrolTelegraphState telegraphState;
        private PatrolAttackState attackState;
        private PatrolRecoverState recoverState;
        private PatrolCeilingState ceilingState;

        private float patrolStartX;

        protected override void Awake()
        {
            if (baseStats == null && patrolStats != null)
            {
                baseStats = patrolStats;
            }
            base.Awake();

            patrolStartX = transform.position.x;

            // Khởi tạo các State cho FSM
            walkingState = new PatrolWalkingState(this, StateMachine);
            idleState = new PatrolIdleState(this, StateMachine);
            chaseState = new PatrolChaseState(this, StateMachine);
            telegraphState = new PatrolTelegraphState(this, StateMachine);
            attackState = new PatrolAttackState(this, StateMachine);
            recoverState = new PatrolRecoverState(this, StateMachine);
            ceilingState = new PatrolCeilingState(this, StateMachine);
        }

        protected override void Start()
        {
            base.Start();

            if (startOnCeiling && (PatrolStats != null && PatrolStats.canAmbushFromCeiling))
            {
                StateMachine.Initialize(ceilingState);
            }
            else
            {
                StateMachine.Initialize(walkingState);
            }
        }

        public override void Respawn()
        {
            base.Respawn();
            patrolStartX = transform.position.x;
            if (startOnCeiling && (PatrolStats != null && PatrolStats.canAmbushFromCeiling))
            {
                StateMachine.ChangeState(ceilingState);
            }
            else
            {
                StateMachine.ChangeState(walkingState);
            }
        }

        // ================================================================
        // FSM STATES
        // ================================================================

        /// <summary>
        /// State 1: Bám trần nhà phục kích (dành cho Nhện băng)
        /// </summary>
        private class PatrolCeilingState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;

            public PatrolCeilingState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                ai.rb.gravityScale = 0f;
                ai.rb.linearVelocity = Vector2.zero;
                ai.PlayAnimation("Idle");
                if (ai.sr != null) ai.sr.flipY = true; // Lộn ngược khi bám trần
            }

            public void Exit()
            {
                ai.rb.gravityScale = 3f;
                if (ai.sr != null) ai.sr.flipY = false;
            }

            public void LogicUpdate()
            {
                if (ai.Target == null) return;

                // Kiểm tra người chơi đi qua ngay bên dưới
                Vector2 targetPos = ai.Target.position;
                Vector2 myPos = ai.transform.position;

                bool inXRange = Mathf.Abs(targetPos.x - myPos.x) <= ai.ambushDetectWidth;
                bool belowMe = targetPos.y < myPos.y && (myPos.y - targetPos.y) <= ai.ambushDetectHeight;

                if (inXRange && belowMe)
                {
                    // Rơi xuống phục kích!
                    fsm.ChangeState(new PatrolDropState(ai, fsm));
                }
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State rơi từ trần xuống đất
        /// </summary>
        private class PatrolDropState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;

            public PatrolDropState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                ai.PlayAnimation("Fall");
                float dropSpeed = ai.PatrolStats != null ? ai.PatrolStats.ceilingDropSpeed : 8f;
                ai.rb.linearVelocity = new Vector2(0f, -dropSpeed);
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                if (ai.CheckGrounded())
                {
                    ai.PlayAnimation("Idle");
                    fsm.ChangeState(ai.idleState);
                }
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State 2: Đi tuần tra bình thường
        /// </summary>
        private class PatrolWalkingState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;

            public PatrolWalkingState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                ai.PlayAnimation("Walk");
            }

            public void Exit()
            {
                ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
            }

            public void LogicUpdate()
            {
                // Kiểm tra nếu phát hiện người chơi
                if (ai.IsTargetInDetectRange())
                {
                    fsm.ChangeState(ai.chaseState);
                    return;
                }

                // Kiểm tra chạm tường hoặc sắp rơi mép vực hoặc vượt quá phạm vi tuần tra
                float patrolDist = ai.PatrolStats != null ? ai.PatrolStats.patrolDistance : 4f;
                bool exceededDist = Mathf.Abs(ai.transform.position.x - ai.patrolStartX) > patrolDist;
                bool movingAway = (ai.transform.position.x - ai.patrolStartX) * (ai.IsFacingRight ? 1 : -1) > 0;

                if (ai.CheckWallAhead() || ai.CheckLedgeAhead() || (exceededDist && movingAway))
                {
                    fsm.ChangeState(ai.idleState);
                }
            }

            public void PhysicsUpdate()
            {
                float speed = ai.PatrolStats != null ? ai.PatrolStats.patrolSpeed : 2f;
                float dirX = ai.IsFacingRight ? 1f : -1f;
                ai.rb.linearVelocity = new Vector2(dirX * speed, ai.rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// State 3: Đứng nghỉ khi đến điểm quay đầu
        /// </summary>
        private class PatrolIdleState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public PatrolIdleState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                timer = ai.PatrolStats != null ? ai.PatrolStats.idleWaitTime : 1.2f;
                ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
                ai.PlayAnimation("Idle");
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                if (ai.IsTargetInDetectRange())
                {
                    fsm.ChangeState(ai.chaseState);
                    return;
                }

                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    // Quay đầu và tiếp tục đi tuần
                    ai.SetFacingDirection(!ai.IsFacingRight);
                    fsm.ChangeState(ai.walkingState);
                }
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State 4: Đuổi theo người chơi
        /// </summary>
        private class PatrolChaseState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;

            public PatrolChaseState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                ai.PlayAnimation("Walk");
            }

            public void Exit()
            {
                ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
            }

            public void LogicUpdate()
            {
                if (!ai.IsTargetInDetectRange())
                {
                    // Mất dấu người chơi -> quay lại đi tuần
                    fsm.ChangeState(ai.walkingState);
                    return;
                }

                ai.FaceTarget();

                float attackRange = ai.PatrolStats != null ? ai.PatrolStats.attackRange : 1.2f;
                if (ai.DistanceToTarget() <= attackRange)
                {
                    // Vào tầm đánh -> Báo hiệu đòn đánh (Telegraph >= 0.4s)
                    fsm.ChangeState(ai.telegraphState);
                    return;
                }

                // Không đuổi tiếp nếu gặp tường hoặc mép vực
                if (ai.CheckWallAhead() || ai.CheckLedgeAhead())
                {
                    ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
                    ai.PlayAnimation("Idle");
                }
                else
                {
                    ai.PlayAnimation("Walk");
                }
            }

            public void PhysicsUpdate()
            {
                if (ai.CheckWallAhead() || ai.CheckLedgeAhead())
                {
                    ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
                    return;
                }

                float speed = ai.PatrolStats != null ? ai.PatrolStats.chaseSpeed : 3.5f;
                float dirX = ai.IsFacingRight ? 1f : -1f;
                ai.rb.linearVelocity = new Vector2(dirX * speed, ai.rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// State 5: Báo hiệu trước khi ra đòn (Telegraph >= 0.4s theo SRS 6.1)
        /// </summary>
        private class PatrolTelegraphState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public PatrolTelegraphState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                float duration = ai.PatrolStats != null ? ai.PatrolStats.telegraphDuration : 0.5f;
                timer = duration;
                ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
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
                    fsm.ChangeState(ai.attackState);
                }
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State 6: Tung đòn tấn công
        /// </summary>
        private class PatrolAttackState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public PatrolAttackState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                timer = 0.4f; // Thời gian vung đòn
                ai.PlayAnimation("Attack");

                // Gây sát thương cận chiến nếu người chơi còn trong tầm
                float attackRange = ai.PatrolStats != null ? ai.PatrolStats.attackRange + 0.3f : 1.5f;
                if (ai.DistanceToTarget() <= attackRange)
                {
                    ai.ExecuteMeleeDamage();
                }
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    fsm.ChangeState(ai.recoverState);
                }
            }

            public void PhysicsUpdate() { }
        }

        /// <summary>
        /// State 7: Hồi phục / khựng sau đòn đánh
        /// </summary>
        private class PatrolRecoverState : IState
        {
            private readonly PatrolEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public PatrolRecoverState(PatrolEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                timer = ai.PatrolStats != null ? ai.PatrolStats.recoverDuration : 0.8f;
                ai.rb.linearVelocity = new Vector2(0f, ai.rb.linearVelocity.y);
                ai.PlayAnimation("Idle");
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    fsm.ChangeState(ai.walkingState);
                }
            }

            public void PhysicsUpdate() { }
        }

        public void ExecuteMeleeDamage()
        {
            if (Target == null) return;
            int dmg = PatrolStats != null ? PatrolStats.contactDamage : 1;
            Vector2 knockDir = (IsFacingRight ? Vector2.right : Vector2.left) + Vector2.up * 0.4f;

            var damageable = Target.GetComponent<IDamageable>() ?? Target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                var dmgData = new DamageData(dmg, DamageType.Normal, transform.position, knockDir.normalized, 6f, gameObject);
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
