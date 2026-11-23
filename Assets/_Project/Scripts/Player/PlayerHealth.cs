using System;
using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Player
{
    /// <summary>
    /// Quản lý máu và cơ chế bất tử tạm thời (Hurt I-frame) của người chơi (FR-CMB-05).
    /// Đọc toàn bộ thông số từ PlayerStatsSO thông qua CharacterContext.
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Visual Feedback (Hurt I-Frame)")]
        [Tooltip("Tốc độ nhấp nháy độ mờ (Alpha) trong thời gian bất tử sau khi bị đánh")]
        [SerializeField] private float blinkInterval = 0.08f;

        [Tooltip("Độ mờ tối thiểu khi nhấp nháy")]
        [Range(0.1f, 0.9f)]
        [SerializeField] private float blinkMinAlpha = 0.35f;

        private CharacterContext context;
        private HitFlash hitFlash;
        private SpriteRenderer spriteRenderer;

        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        // Quản lý thời gian bất tử sau khi bị đánh (đếm bằng timer, Zero GC)
        private float hurtIFrameTimer;
        private float blinkTimer;
        private bool isBlinkDimmed;

        /// <summary>
        /// Đang trong thời gian bất tử sau khi bị đánh.
        /// </summary>
        public bool IsHurtInvulnerable => hurtIFrameTimer > 0f;

        /// <summary>
        /// Tổng hợp trạng thái bất tử: bao gồm cả lúc lăn né (Roll) và lúc vừa bị đánh (Hurt).
        /// </summary>
        public bool IsInvulnerable => IsHurtInvulnerable || (context != null && context.IsInvulnerable);

        // Events để UI (HUD) và hệ thống Chết/Hồi sinh (FR-CMB-06) đăng ký lắng nghe
        public event Action<int, int> OnHealthChanged;
        public event Action<int, Vector2> OnDamaged;
        public event Action OnDied;

        private void Awake()
        {
            context = GetComponent<CharacterContext>();
            hitFlash = GetComponent<HitFlash>();
            spriteRenderer = context.SpriteRenderer != null ? context.SpriteRenderer : GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (context.Stats == null)
            {
                Debug.LogError($"[PlayerHealth] Thiếu PlayerStatsSO trên {gameObject.name}!", this);
                enabled = false;
                return;
            }

            MaxHealth = Mathf.Clamp(context.Stats.initialHealth, 1, context.Stats.maxHealthLimit);
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Update()
        {
            if (hurtIFrameTimer > 0f)
            {
                hurtIFrameTimer -= Time.deltaTime;
                UpdateBlinkVisual(Time.deltaTime);

                if (hurtIFrameTimer <= 0f)
                {
                    ResetSpriteAlpha();
                }
            }
        }

        private void OnDisable()
        {
            // Chốt an toàn: Đảm bảo sprite không bị kẹt trạng thái mờ khi object bị tắt/chuyển phòng
            ResetSpriteAlpha();
        }

        /// <summary>
        /// Gây sát thương lên người chơi (FR-CMB-05).
        /// </summary>
        public bool TakeDamage(int amount, Vector2 hitDirection = default)
        {
            if (amount <= 0 || IsDead || IsInvulnerable)
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnDamaged?.Invoke(amount, hitDirection);

            // Kích hoạt nháy trắng (HitFlash) khi trúng đòn
            if (hitFlash != null)
            {
                hitFlash.TriggerFlash();
            }

            Debug.Log($"[PlayerHealth] Bị đánh -{amount} HP. Máu còn lại: {CurrentHealth}/{MaxHealth}");

            if (CurrentHealth <= 0)
            {
                hurtIFrameTimer = 0f;
                ResetSpriteAlpha();
                OnDied?.Invoke();
                Debug.Log("[PlayerHealth] Nhân vật đã chết (Máu về 0)!");
            }
            else
            {
                // Kích hoạt I-frame sau khi bị đánh theo SRS (mặc định 1.0s)
                hurtIFrameTimer = context.Stats.hurtIframeDuration;
                blinkTimer = 0f;
                isBlinkDimmed = false;
            }

            return true;
        }

        // ================================================================
        // CÁC HÀM HỖ TRỢ CHO FR-CMB-06 (HỒI SINH) & NHẶT ĐÁ TĂNG MÁU
        // ================================================================
        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = MaxHealth;
            hurtIFrameTimer = 0f;
            ResetSpriteAlpha();
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void IncreaseMaxHealth(int amount)
        {
            if (amount <= 0 || context.Stats == null) return;

            MaxHealth = Mathf.Min(context.Stats.maxHealthLimit, MaxHealth + amount);
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        // ================================================================
        // HIỆU ỨNG NHẤP NHÁY I-FRAME
        // ================================================================
        private void UpdateBlinkVisual(float dt)
        {
            if (spriteRenderer == null) return;

            blinkTimer -= dt;
            if (blinkTimer <= 0f)
            {
                blinkTimer = blinkInterval;
                isBlinkDimmed = !isBlinkDimmed;
                SetSpriteAlpha(isBlinkDimmed ? blinkMinAlpha : 1f);
            }
        }

        private void ResetSpriteAlpha()
        {
            isBlinkDimmed = false;
            SetSpriteAlpha(1f);
        }

        private void SetSpriteAlpha(float alpha)
        {
            if (spriteRenderer == null) return;
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        // ================================================================
        // TIỆN ÍCH TEST TRONG EDITOR (KHÔNG CẦN QUÁI)
        // ================================================================
        [ContextMenu("Test: Nhận 1 sát thương")]
        private void DebugTake1Damage()
        {
            if (!Application.isPlaying) return;
            TakeDamage(1);
        }

        [ContextMenu("Test: Hồi đầy máu")]
        private void DebugRestoreFullHealth()
        {
            if (!Application.isPlaying) return;
            RestoreFullHealth();
        }
    }
}