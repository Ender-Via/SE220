using System;
using System.Collections;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Quản lý máu, nhận sát thương, hiệu ứng trúng đòn và cơ chế chết của quái theo SRS 5.2 & 6.1.
    /// Hỗ trợ:
    /// - Quái hệ băng nhận x2 sát thương từ rìu lửa (FR-ABL-01).
    /// - Giáp băng có khiên phía trước: chặn đòn thường từ phía trước, chỉ phá bằng đòn nặng hoặc rìu lửa (FR-CMB-02 & SRS 6.1).
    /// - Hiệu ứng trúng đòn: HitFlash (nháy trắng), HitStop (khựng hình 0.05s), knockback (giật lùi).
    /// - Hồi sinh khi người chơi rời phòng rồi quay lại.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Stats Reference")]
        [SerializeField] private EnemyStatsSO stats;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private HitFlash hitFlash;

        [Header("Shield Configuration (SRS 6.1 Giáp băng)")]
        [Tooltip("Bật khiên phía trước (chỉ đòn nặng hoặc lửa mới xuyên qua)")]
        [SerializeField] private bool hasFrontShield;

        [Header("Death Handling")]
        [Tooltip("Thời gian chờ trước khi deactive GameObject sau animation chết")]
        [SerializeField] private float deathDeactivateDelay = 1.0f;

        public int CurrentHp { get; private set; }
        public int MaxHp => stats != null ? stats.maxHp : 3;
        public bool IsDead => CurrentHp <= 0;
        public bool HasFrontShield => hasFrontShield;

        public event Action<DamageData, int> OnDamaged; // data, actualDamage
        public event Action OnDied;
        public event Action<DamageData> OnShieldBlocked;

        private Collider2D[] colliders;
        private Coroutine deathRoutine;
        private Vector3 initialSpawnPosition;
        private Quaternion initialSpawnRotation;

        private void Awake()
        {
            if (stats == null)
            {
                var enemyBase = GetComponent<EnemyBase>();
                if (enemyBase != null) stats = enemyBase.BaseStats;
            }

            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (hitFlash == null) hitFlash = GetComponentInChildren<HitFlash>();

            colliders = GetComponentsInChildren<Collider2D>();

            initialSpawnPosition = transform.position;
            initialSpawnRotation = transform.rotation;

            ResetHealth();
        }

        public void SetStats(EnemyStatsSO newStats)
        {
            stats = newStats;
            if (stats is PatrolEnemyStatsSO patrolStats)
            {
                hasFrontShield = patrolStats.hasFrontShield;
            }
            ResetHealth();
        }

        public void ResetHealth()
        {
            CurrentHp = MaxHp;
            if (deathRoutine != null)
            {
                StopCoroutine(deathRoutine);
                deathRoutine = null;
            }

            foreach (var col in colliders)
            {
                if (col != null) col.enabled = true;
            }

            if (rb != null)
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero;
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }

        /// <summary>
        /// Thực thi nhận sát thương từ giao diện IDamageable.
        /// </summary>
        public bool TakeDamage(DamageData damageData)
        {
            if (IsDead) return false;

            // 1. Kiểm tra cơ chế Khiên chắn phía trước (SRS 6.1 Giáp băng)
            if (hasFrontShield)
            {
                bool hitFromFront = CheckHitFromFront(damageData.HitPoint, damageData.KnockbackDirection);
                if (hitFromFront)
                {
                    // Đòn thường bị khiên chặn 100%
                    if (damageData.DamageType == DamageType.Normal)
                    {
                        OnShieldBlocked?.Invoke(damageData);
                        ApplyHitStop(0.04f);
                        return false;
                    }
                    // Đòn nặng (Heavy) hoặc Lửa (Fire) sẽ phá khiên theo SRS 6.1
                }
            }

            // 2. Tính toán lượng sát thương thực tế
            int finalDamage = damageData.Amount;
            bool isIce = stats != null && stats.isIceEnemy;

            if (isIce && damageData.DamageType == DamageType.Fire)
            {
                float multiplier = stats != null ? stats.iceDamageMultiplier : 2.0f;
                finalDamage = Mathf.RoundToInt(finalDamage * multiplier);
            }

            CurrentHp = Mathf.Max(0, CurrentHp - finalDamage);

            // 3. Hiệu ứng Game Feel (Hit-stop, HitFlash, Knockback)
            ApplyHitStop(damageData.DamageType == DamageType.Heavy ? 0.08f : 0.05f);

            if (hitFlash != null)
            {
                hitFlash.TriggerFlash();
            }

            // Knockback vật lý
            ApplyKnockback(damageData);

            OnDamaged?.Invoke(damageData, finalDamage);

            // 4. Xử lý khi cạn máu
            if (CurrentHp <= 0)
            {
                Die();
            }

            return true;
        }

        private bool CheckHitFromFront(Vector2 hitPoint, Vector2 knockbackDir)
        {
            // Kiểm tra hướng mặt của quái (scale.x > 0 hoặc sprite facing)
            float facingDir = transform.localScale.x >= 0 ? 1f : -1f;
            var enemyBase = GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                facingDir = enemyBase.IsFacingRight ? 1f : -1f;
            }

            // Nếu đòn đánh tới từ phía trước mặt quái
            float attackDir = hitPoint.x - transform.position.x;
            if (Mathf.Abs(attackDir) > 0.01f)
            {
                return Mathf.Sign(attackDir) == Mathf.Sign(facingDir);
            }

            // Fallback dựa trên hướng knockback
            return Mathf.Sign(-knockbackDir.x) == Mathf.Sign(facingDir);
        }

        private void ApplyKnockback(DamageData damageData)
        {
            if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic) return;

            float resistance = stats != null ? stats.knockbackResistance : 0f;
            if (resistance >= 1f) return;

            float effectiveForce = damageData.KnockbackForce * (1f - resistance);
            if (effectiveForce > 0.01f)
            {
                Vector2 dir = damageData.KnockbackDirection;
                if (dir.sqrMagnitude < 0.001f)
                {
                    dir = (Vector2)(transform.position - (Vector3)damageData.HitPoint).normalized;
                }
                rb.linearVelocity = dir * effectiveForce;
            }
        }

        private void ApplyHitStop(float duration)
        {
            // Gọi tiện ích HitStop dùng chung trong CharacterContext
            Player.CharacterContext.HitStop.Request(duration);
        }

        private void Die()
        {
            OnDied?.Invoke();

            foreach (var col in colliders)
            {
                if (col != null) col.enabled = false;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }

            deathRoutine = StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Thử phát animation Dead nếu Animator có clip
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.Play("Dead", 0, 0f);
            }

            yield return new WaitForSeconds(deathDeactivateDelay);

            // Mờ dần nếu chưa bị deactive
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                for (float t = 0f; t < 0.3f; t += Time.deltaTime)
                {
                    c.a = Mathf.Lerp(1f, 0f, t / 0.3f);
                    spriteRenderer.color = c;
                    yield return null;
                }
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hồi sinh quái về vị trí xuất phát ban đầu khi người chơi rời phòng và quay lại.
        /// </summary>
        public void RespawnAtOriginalSpawn()
        {
            transform.position = initialSpawnPosition;
            transform.rotation = initialSpawnRotation;
            gameObject.SetActive(true);
            ResetHealth();
        }
    }
}
