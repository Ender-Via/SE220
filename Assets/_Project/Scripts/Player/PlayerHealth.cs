using System;
using System.Collections;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;

namespace Woodcutter.Player
{
    /// <summary>
    /// Quản lý hệ thống máu, nhận sát thương và khung bất tử (I-frame) của nhân vật người chơi
    /// theo SRS mục 5.1 (5 vạch máu khởi điểm, +1 mỗi viên đá tối đa 8 vạch; i-frame khi bị đánh 1.0s).
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private CharacterContext context;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Knockback Settings")]
        [SerializeField] private Vector2 defaultKnockback = new Vector2(6f, 5f);
        [SerializeField] private float knockbackControlLockTime = 0.2f;

        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        public event Action<int, int> OnHealthChanged; // current, max
        public event Action<DamageData> OnDamaged;
        public event Action OnDied;

        private float invulnerableUntil;
        private Coroutine blinkRoutine;
        private Vector3 respawnPosition;

        private void Awake()
        {
            if (context == null) context = GetComponent<CharacterContext>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            respawnPosition = transform.position;
        }

        private void Start()
        {
            InitializeHealth();
        }

        public void InitializeHealth()
        {
            int startingHp = (context != null && context.Stats != null) ? context.Stats.initialHealth : 5;
            MaxHp = startingHp;
            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public void SetRespawnPosition(Vector3 newRespawnPos)
        {
            respawnPosition = newRespawnPos;
        }

        public void IncreaseMaxHealth(int amount)
        {
            int maxLimit = (context != null && context.Stats != null) ? context.Stats.maxHealthLimit : 8;
            MaxHp = Mathf.Min(maxLimit, MaxHp + amount);
            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        /// <summary>
        /// Thực thi nhận sát thương từ kẻ địch hoặc cạm bẫy.
        /// </summary>
        public bool TakeDamage(DamageData damageData)
        {
            if (IsDead) return false;

            // 1. Kiểm tra I-frame từ cú lăn (Roll I-frame)
            if (context != null && context.IsInvulnerable)
            {
                return false;
            }

            // 2. Kiểm tra I-frame sau khi bị dính đòn (Hurt I-frame 1.0s)
            if (Time.time < invulnerableUntil)
            {
                return false;
            }

            // 3. Trừ máu
            CurrentHp = Mathf.Max(0, CurrentHp - damageData.Amount);
            float hurtDuration = (context != null && context.Stats != null) ? context.Stats.hurtIframeDuration : 1.0f;
            invulnerableUntil = Time.time + hurtDuration;

            // 4. Knockback phản lực
            float dir = Mathf.Sign(transform.position.x - damageData.HitPoint.x);
            if (Mathf.Abs(dir) < 0.01f) dir = (context != null && context.IsFacingRight) ? -1f : 1f;

            float kForce = damageData.KnockbackForce > 0f ? damageData.KnockbackForce : defaultKnockback.x;
            Vector2 knockVelocity = new Vector2(kForce * dir, defaultKnockback.y);
            if (context != null)
            {
                context.OverrideHorizontalVelocity(knockVelocity, knockbackControlLockTime);
                context.ApplyHitFeedback(0.08f, 1f);
            }

            // 5. Hiệu ứng nhấp nháy sprite
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = StartCoroutine(BlinkRoutine());

            OnDamaged?.Invoke(damageData);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            // 6. Xử lý khi cạn máu
            if (CurrentHp <= 0)
            {
                Die();
            }

            return true;
        }

        private IEnumerator BlinkRoutine()
        {
            while (Time.time < invulnerableUntil)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(0.08f);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            blinkRoutine = null;
        }

        private void Die()
        {
            OnDied?.Invoke();
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(1.0f);

            if (context != null && context.Rigidbody != null)
            {
                context.Rigidbody.linearVelocity = Vector2.zero;
            }

            transform.position = respawnPosition;
            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        private void OnDisable()
        {
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }
    }
}
