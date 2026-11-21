using System.Collections;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Khung AI 3: Bắn xa (Ranged AI Framework) theo SRS mục 6.1.
    /// Áp dụng cho:
    /// - Quạ xám (GreyCrow - Khu 1): Bay lượn, hạ thấp ném đá tuyết (người chơi chém lên khi nó hạ thấp).
    /// - Tinh linh sương (Frost Spirit - Khu 2–3): Bay lơ lửng, bắn loạt 3 viên đạn băng, giữ khoảng cách (kiting).
    /// </summary>
    public class RangedEnemyAI : EnemyBase
    {
        [Header("Ranged Specific Configuration")]
        [SerializeField] private RangedEnemyStatsSO rangedStats;

        [Header("Projectile Setup")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        public RangedEnemyStatsSO RangedStats => rangedStats != null ? rangedStats : baseStats as RangedEnemyStatsSO;

        // FSM States
        private RangedHoverState hoverState;
        private RangedTelegraphState telegraphState;
        private RangedFireState fireState;
        private RangedCooldownState cooldownState;

        private Vector2 baseFlightPosition;
        private float nextFireTime;

        protected override void Awake()
        {
            if (baseStats == null && rangedStats != null)
            {
                baseStats = rangedStats;
            }
            base.Awake();

            if (rb != null)
            {
                rb.gravityScale = 0f; // Quái bay lượn không bị trọng lực kéo
            }

            baseFlightPosition = transform.position;

            hoverState = new RangedHoverState(this, StateMachine);
            telegraphState = new RangedTelegraphState(this, StateMachine);
            fireState = new RangedFireState(this, StateMachine);
            cooldownState = new RangedCooldownState(this, StateMachine);
        }

        protected override void Start()
        {
            base.Start();
            StateMachine.Initialize(hoverState);
        }

        public override void Respawn()
        {
            base.Respawn();
            baseFlightPosition = initialPosition;
            nextFireTime = 0f;
            StateMachine.ChangeState(hoverState);
        }

        public bool CanFire()
        {
            return Time.time >= nextFireTime;
        }

        public void MarkFired()
        {
            float cd = RangedStats != null ? RangedStats.attackCooldown : 2.5f;
            nextFireTime = Time.time + cd;
        }

        public void SpawnProjectile(Vector2 targetPos)
        {
            Vector2 spawnPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            Vector2 dir = (targetPos - spawnPos).normalized;
            if (dir.sqrMagnitude < 0.001f) dir = IsFacingRight ? Vector2.right : Vector2.left;

            GameObject projObj;
            if (projectilePrefab != null)
            {
                projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Tự động tạo viên đạn tạm thời nếu chưa gán prefab
                projObj = CreateDefaultProjectile(spawnPos);
            }

            var proj = projObj.GetComponent<EnemyProjectile>();
            if (proj != null)
            {
                float pSpeed = RangedStats != null ? RangedStats.projectileSpeed : 7f;
                int pDamage = RangedStats != null ? RangedStats.projectileDamage : 1;
                float pLifetime = RangedStats != null ? RangedStats.projectileLifetime : 4f;
                proj.Initialize(dir, pSpeed, pDamage, pLifetime);
            }
        }

        private GameObject CreateDefaultProjectile(Vector2 spawnPos)
        {
            var go = new GameObject("DefaultIceRockProjectile");
            go.transform.position = spawnPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.6f, 0.9f, 1f, 1f); // Xanh băng

            // Tạo collider trigger
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;

            go.AddComponent<EnemyProjectile>();
            return go;
        }

        // ================================================================
        // FSM STATES
        // ================================================================

        /// <summary>
        /// State 1: Bay lượn sóng và giữ khoảng cách với người chơi (Hover & Kite)
        /// </summary>
        private class RangedHoverState : IState
        {
            private readonly RangedEnemyAI ai;
            private readonly StateMachine fsm;
            private float hoverTimer;

            public RangedHoverState(RangedEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                ai.PlayAnimation("Fly");
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                hoverTimer += Time.deltaTime;

                if (ai.Target != null)
                {
                    ai.FaceTarget();

                    // Nếu trong tầm phát hiện và đã hồi chiêu bắn -> Chuyển sang báo hiệu
                    if (ai.IsTargetInDetectRange() && ai.CanFire())
                    {
                        fsm.ChangeState(ai.telegraphState);
                        return;
                    }
                }
            }

            public void PhysicsUpdate()
            {
                // Dao động lượn sóng trục Y (Bobbing)
                float amp = ai.RangedStats != null ? ai.RangedStats.hoverBobbingAmplitude : 0.3f;
                float freq = ai.RangedStats != null ? ai.RangedStats.hoverBobbingFrequency : 2f;
                float bobbingY = Mathf.Sin(hoverTimer * freq) * amp;

                // Kiting: nếu người chơi tới quá gần, bay lùi ra giữ khoảng cách
                float kiteDist = ai.RangedStats != null ? ai.RangedStats.kiteDistance : 3.5f;
                Vector2 targetVel = Vector2.zero;

                if (ai.Target != null)
                {
                    float currentDist = ai.DistanceToTarget();
                    if (currentDist < kiteDist)
                    {
                        // Lùi ra xa
                        Vector2 retreatDir = -ai.DirectionToTarget();
                        targetVel = retreatDir * (ai.RangedStats != null ? ai.RangedStats.moveSpeed : 3f);
                    }
                }

                targetVel.y += bobbingY;
                ai.rb.linearVelocity = targetVel;
            }
        }

        /// <summary>
        /// State 2: Báo hiệu trước khi bắn (Telegraph >= 0.4s)
        /// Quạ xám: Hạ thấp độ cao theo SRS "chém lên khi nó hạ thấp"
        /// </summary>
        private class RangedTelegraphState : IState
        {
            private readonly RangedEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;
            private Vector3 startPos;
            private Vector3 swoopTargetPos;

            public RangedTelegraphState(RangedEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                float duration = ai.RangedStats != null ? ai.RangedStats.telegraphDuration : 0.6f;
                timer = duration;
                startPos = ai.transform.position;

                ai.FaceTarget();
                ai.StartTelegraph(duration);

                // Nếu có đặc tính hạ thấp độ cao (Quạ xám)
                if (ai.RangedStats != null && ai.RangedStats.swoopDownOnAttack)
                {
                    float swoopY = ai.RangedStats.swoopDistanceY;
                    swoopTargetPos = startPos + Vector3.down * swoopY;
                }
                else
                {
                    swoopTargetPos = startPos;
                }
            }

            public void Exit()
            {
                ai.StopTelegraph();
            }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;
                float duration = ai.RangedStats != null ? ai.RangedStats.telegraphDuration : 0.6f;
                float progress = 1f - Mathf.Clamp01(timer / duration);

                // Hạ dần độ cao xuống vị trí dễ chém
                ai.transform.position = Vector3.Lerp(startPos, swoopTargetPos, progress);

                if (timer <= 0f)
                {
                    fsm.ChangeState(ai.fireState);
                }
            }

            public void PhysicsUpdate()
            {
                ai.rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// State 3: Khai hỏa (Bắn đơn hoặc bắn loạt 3 viên)
        /// </summary>
        private class RangedFireState : IState
        {
            private readonly RangedEnemyAI ai;
            private readonly StateMachine fsm;
            private Coroutine burstRoutine;
            private bool finishedBurst;

            public RangedFireState(RangedEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                finishedBurst = false;
                ai.PlayAnimation("Fly");
                ai.MarkFired();
                burstRoutine = ai.StartCoroutine(FireSequenceRoutine());
            }

            public void Exit()
            {
                if (burstRoutine != null)
                {
                    ai.StopCoroutine(burstRoutine);
                    burstRoutine = null;
                }
            }

            public void LogicUpdate()
            {
                if (finishedBurst)
                {
                    fsm.ChangeState(ai.cooldownState);
                }
            }

            public void PhysicsUpdate()
            {
                ai.rb.linearVelocity = Vector2.zero;
            }

            private IEnumerator FireSequenceRoutine()
            {
                int count = ai.RangedStats != null ? ai.RangedStats.burstCount : 1;
                float interval = ai.RangedStats != null ? ai.RangedStats.burstInterval : 0.2f;

                for (int i = 0; i < count; i++)
                {
                    if (ai.Target != null)
                    {
                        ai.FaceTarget();
                        ai.SpawnProjectile(ai.Target.position);
                    }
                    if (count > 1 && i < count - 1)
                    {
                        yield return new WaitForSeconds(interval);
                    }
                }

                yield return new WaitForSeconds(0.2f);
                finishedBurst = true;
            }
        }

        /// <summary>
        /// State 4: Hồi chiêu sau khi bắn, bay về lại độ cao an toàn
        /// </summary>
        private class RangedCooldownState : IState
        {
            private readonly RangedEnemyAI ai;
            private readonly StateMachine fsm;
            private float timer;

            public RangedCooldownState(RangedEnemyAI ai, StateMachine fsm) { this.ai = ai; this.fsm = fsm; }

            public void Enter()
            {
                timer = 0.8f;
                ai.PlayAnimation("Fly");
            }

            public void Exit() { }

            public void LogicUpdate()
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    fsm.ChangeState(ai.hoverState);
                }
            }

            public void PhysicsUpdate()
            {
                // Bay trở lại độ cao gốc
                float speed = ai.RangedStats != null ? ai.RangedStats.moveSpeed : 3f;
                Vector2 targetPos = new Vector2(ai.transform.position.x, ai.baseFlightPosition.y);
                ai.transform.position = Vector2.MoveTowards(ai.transform.position, targetPos, speed * Time.fixedDeltaTime);
            }
        }
    }
}
