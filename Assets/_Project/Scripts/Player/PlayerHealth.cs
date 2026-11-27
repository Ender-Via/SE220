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

        /// <summary>Bắn ra sau khi hồi sinh đầy máu (checkpoint hoặc dịch chuyển dự phòng).</summary>
        public event Action OnRevived;

        /// <summary>
        /// Bật bởi RespawnController (scene nền có lưu game): khi chết chỉ bắn <see cref="OnDied"/>,
        /// việc đưa về checkpoint do RespawnController làm (FR-CMB-06). Tắt = dịch chuyển về respawnPosition như cũ.
        /// </summary>
        public bool UseExternalRespawn { get; set; }

        private float invulnerableUntil;
        private Coroutine blinkRoutine;
        private SpriteRenderer[] blinkRenderers;
        private bool blinkVisible = true;
        private Vector3 respawnPosition;

        private void Awake()
        {
            if (context == null) context = GetComponent<CharacterContext>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            // Nhân vật rig nhiều mảnh: nháy tất cả SpriteRenderer con (đầu, thân, tay, chân, khăn, rìu...)
            blinkRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            respawnPosition = transform.position;
        }

        private void Start()
        {
            // GameProgress có thể đã đặt máu tối đa từ file lưu trước Start này
            if (MaxHp <= 0) InitializeHealth();
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

        /// <summary>Đặt máu tối đa (tải game). Bị giới hạn bởi maxHealthLimit trong PlayerStats.</summary>
        public void SetMaxHealth(int maxHp, bool refill)
        {
            int maxLimit = (context != null && context.Stats != null) ? context.Stats.maxHealthLimit : 8;
            MaxHp = Mathf.Clamp(maxHp, 1, maxLimit);
            CurrentHp = refill ? MaxHp : Mathf.Min(CurrentHp, MaxHp);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        /// <summary>Hồi sinh: đầy máu, xóa i-frame bị đánh, dừng nhấp nháy.</summary>
        public void ReviveFull()
        {
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }
            SetBlinkVisible(true);

            invulnerableUntil = 0f;
            if (context != null && context.Rigidbody != null)
                context.Rigidbody.linearVelocity = Vector2.zero;

            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
            OnRevived?.Invoke();
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
                SetBlinkVisible(!blinkVisible);
                yield return new WaitForSeconds(0.08f);
            }

            SetBlinkVisible(true);
            blinkRoutine = null;
        }

        // Ẩn/hiện mọi mảnh của nhân vật khi nháy i-frame
        private void SetBlinkVisible(bool visible)
        {
            blinkVisible = visible;
            if (blinkRenderers == null || blinkRenderers.Length == 0)
            {
                if (spriteRenderer != null) spriteRenderer.enabled = visible;
                return;
            }
            for (int i = 0; i < blinkRenderers.Length; i++)
                if (blinkRenderers[i] != null) blinkRenderers[i].enabled = visible;
        }

        private void Die()
        {
            OnDied?.Invoke();
            if (!UseExternalRespawn)
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
            ReviveFull();
        }

        private void OnDisable()
        {
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }
            SetBlinkVisible(true);
        }
    }
}
