using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Player;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho tất cả quái vật (Enemies) trong game.
    /// Quản lý vòng đời FSM, phát hiện người chơi, lật mặt sprite, kiểm tra mép vực/tường,
    /// va chạm gây sát thương và hồi sinh theo phòng.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Stats Reference")]
        [SerializeField] protected EnemyStatsSO baseStats;

        [Header("Components & Visuals")]
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected Collider2D col;
        [SerializeField] protected SpriteRenderer sr;
        [SerializeField] protected Animator anim;
        [SerializeField] protected EnemyHealth health;
        [SerializeField] protected EnemyTelegraphIndicator telegraphIndicator;

        [Header("Orientation Settings")]
        [Tooltip("Bật nếu hình vẽ gốc trong sprite sheet quay mặt sang trái (ví dụ SkullWolf, GreyCrow)")]
        [SerializeField] protected bool spriteFacesLeft = true;

        [Header("Physics & Ledge Detection")]
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] protected Transform wallCheckPoint;
        [SerializeField] protected Transform ledgeCheckPoint;
        [SerializeField] protected float wallCheckDistance = 0.4f;
        [SerializeField] protected float ledgeCheckDistance = 0.6f;

        [Header("Player Detection & Contact")]
        [Tooltip("Layer hoặc Tag của người chơi")]
        [SerializeField] protected LayerMask playerLayer;
        [SerializeField] protected float contactDamageCooldown = 1.0f;

        // FSM Cỗ máy trạng thái
        public StateMachine StateMachine { get; protected set; }
        public EnemyStatsSO BaseStats => baseStats;
        public EnemyHealth Health => health;
        public Rigidbody2D Rigidbody => rb;
        public SpriteRenderer SpriteRenderer => sr;
        public Animator Animator => anim;
        public bool IsFacingRight { get; protected set; } = true;
        public Transform Target { get; protected set; }

        protected Vector3 initialPosition;
        protected Quaternion initialRotation;
        protected float nextContactDamageTime;

        protected virtual void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (col == null) col = GetComponent<Collider2D>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            if (anim == null) anim = GetComponentInChildren<Animator>();
            if (health == null) health = GetComponent<EnemyHealth>();
            if (telegraphIndicator == null) telegraphIndicator = GetComponentInChildren<EnemyTelegraphIndicator>();

            if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Ground", "Default");
            if (playerLayer.value == 0) playerLayer = LayerMask.GetMask("Player", "Default");

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            StateMachine = new StateMachine();

            if (health != null)
            {
                health.OnDied += HandleDeath;
            }
        }

        protected virtual void Start()
        {
            FindPlayerTarget();
        }

        protected virtual void Update()
        {
            if (health != null && health.IsDead) return;

            // Định kỳ kiểm tra tìm Player nếu mất dấu
            if (Target == null || !Target.gameObject.activeInHierarchy)
            {
                FindPlayerTarget();
            }

            StateMachine.LogicUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (health != null && health.IsDead) return;

            StateMachine.PhysicsUpdate();
        }

        public void FindPlayerTarget()
        {
            // Tìm CharacterContext trong scene
            var playerCtx = FindAnyObjectByType<CharacterContext>();
            if (playerCtx != null)
            {
                Target = playerCtx.transform;
                return;
            }

            // Fallback tìm theo Tag
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                Target = playerObj.transform;
            }
        }

        public float DistanceToTarget()
        {
            if (Target == null) return float.MaxValue;
            return Vector2.Distance(transform.position, Target.position);
        }

        public Vector2 DirectionToTarget()
        {
            if (Target == null) return Vector2.zero;
            return ((Vector2)(Target.position - transform.position)).normalized;
        }

        public bool IsTargetInDetectRange()
        {
            if (Target == null || baseStats == null) return false;
            return DistanceToTarget() <= baseStats.detectRange;
        }

        // ================================================================
        // HƯỚNG MẶT & VISUALS
        // ================================================================
        public virtual void SetFacingDirection(bool faceRight)
        {
            IsFacingRight = faceRight;

            if (sr != null)
            {
                sr.flipX = spriteFacesLeft ? faceRight : !faceRight;
            }
            else
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * (faceRight ? 1f : -1f);
                transform.localScale = s;
            }
        }

        public void FaceTarget()
        {
            if (Target == null) return;
            float dx = Target.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                SetFacingDirection(dx > 0);
            }
        }

        public void PlayAnimation(string stateName)
        {
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                anim.Play(stateName, 0, 0f);
            }
        }

        // ================================================================
        // BÁO HIỆU ĐÒN ĐÁNH (TELEGRAPH >= 0.4S THEO SRS 6.1)
        // ================================================================
        public void StartTelegraph(float duration)
        {
            if (telegraphIndicator != null)
            {
                telegraphIndicator.ShowTelegraph(duration);
            }
        }

        public void StopTelegraph()
        {
            if (telegraphIndicator != null)
            {
                telegraphIndicator.StopTelegraph();
            }
        }

        // ================================================================
        // KIỂM TRA MẶT ĐẤT, TƯỜNG, MÉP VỰC (LEDGE & WALL SENSORS)
        // ================================================================
        public virtual bool CheckGrounded()
        {
            Vector2 checkPos = (Vector2)transform.position + Vector2.down * 0.1f;
            return Physics2D.OverlapCircle(checkPos, 0.25f, groundLayer) != null;
        }

        public virtual bool CheckWallAhead()
        {
            Vector2 origin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position;
            Vector2 dir = IsFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, groundLayer);
            return hit.collider != null && !hit.collider.isTrigger;
        }

        public virtual bool CheckLedgeAhead()
        {
            // Chiếu tia xuống phía trước mặt: nếu không chạm đất -> sắp rơi khỏi mép bục
            Vector2 origin = ledgeCheckPoint != null ? (Vector2)ledgeCheckPoint.position : (Vector2)transform.position + (IsFacingRight ? Vector2.right : Vector2.left) * 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDistance, groundLayer);
            return hit.collider == null;
        }

        // ================================================================
        // GÂY SÁT THƯƠNG CHẠM LÊN NGƯỜI CHƠI (CONTACT DAMAGE)
        // ================================================================
        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            TryDealContactDamage(collision.collider);
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            TryDealContactDamage(other);
        }

        protected virtual void TryDealContactDamage(Collider2D playerCollider)
        {
            if (health != null && health.IsDead) return;
            if (Time.time < nextContactDamageTime) return;

            // Kiểm tra xem đối tượng có phải là người chơi không
            if (!playerCollider.CompareTag("Player") && (playerLayer.value & (1 << playerCollider.gameObject.layer)) == 0)
                return;

            int damage = baseStats != null ? baseStats.contactDamage : 1;
            Vector2 knockDir = ((Vector2)(playerCollider.transform.position - transform.position)).normalized;
            if (knockDir.sqrMagnitude < 0.01f) knockDir = IsFacingRight ? Vector2.right : Vector2.left;
            knockDir = (knockDir + Vector2.up * 0.5f).normalized;

            // 1. Thử gửi qua IDamageable
            var damageable = playerCollider.GetComponent<IDamageable>() ?? playerCollider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                var dmgData = new DamageData(damage, DamageType.Normal, transform.position, knockDir, 5f, gameObject);
                if (damageable.TakeDamage(dmgData))
                {
                    nextContactDamageTime = Time.time + contactDamageCooldown;
                    return;
                }
            }

            // 2. Thử gửi qua DemoPlayerHealth (dành cho scene Demo / Sandbox)
            var demoHealth = playerCollider.GetComponent<Demo.DemoPlayerHealth>() ?? playerCollider.GetComponentInParent<Demo.DemoPlayerHealth>();
            if (demoHealth != null)
            {
                if (demoHealth.TakeDamage(damage, transform.position))
                {
                    nextContactDamageTime = Time.time + contactDamageCooldown;
                }
            }
        }

        protected virtual void HandleDeath()
        {
            StopTelegraph();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Hồi sinh quái về vị trí xuất phát khi người chơi rời phòng rồi quay lại (SRS 6.1).
        /// </summary>
        public virtual void Respawn()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            gameObject.SetActive(true);

            if (health != null)
            {
                health.ResetHealth();
            }

            nextContactDamageTime = 0f;
            FindPlayerTarget();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Vẽ bán kính phát hiện người chơi
            if (baseStats != null)
            {
                Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.35f);
                Gizmos.DrawWireSphere(transform.position, baseStats.detectRange);
            }

            // Vẽ raycast kiểm tra tường
            Vector2 wallOrigin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position;
            Vector2 wallDir = (IsFacingRight ? Vector2.right : Vector2.left) * wallCheckDistance;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallOrigin, wallOrigin + wallDir);

            // Vẽ raycast kiểm tra mép vực
            Vector2 ledgeOrigin = ledgeCheckPoint != null ? (Vector2)ledgeCheckPoint.position : (Vector2)transform.position + (IsFacingRight ? Vector2.right : Vector2.left) * 0.5f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeCheckDistance);
        }
    }
}
