using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Player;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Gắn sát thương vào các đòn rìu của AttackState (main chưa có hitbox – xem TODO
    /// trong AttackState) và thêm Rìu lửa (L) sau khi nhặt Đá Đỏ. Không sửa code nhân vật.
    ///
    /// Bám đúng luồng AttackState (chỉ gây sát thương khi CurrentState == AttackState):
    /// - Vào AttackState trên không  -> nhịp 1 ngay, không có nhịp 2 / đòn nặng.
    /// - Dưới đất: đang nạp; thả J trước heavyChargeTime -> nhịp 1; giữ đủ -> đòn nặng.
    /// - Bấm J sau nhịp 1 -> nhịp 2 bắn tại max(lúc bấm, nhịp 1 + attackDuration), giống buffer trong AttackState.
    /// Mọi thông số đọc từ PlayerCombatStatsSO / AbilitiesStatsSO qua CharacterContext.
    /// TODO (LEAD): AttackState nên bắn event khi tung đòn để demo/hitbox thật nghe theo thay vì mô phỏng lại.
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class DemoPlayerCombat : MonoBehaviour
    {
        public enum AttackKind { Combo1, Combo2, Heavy, FireAxe }

        [Tooltip("Tâm hitbox so với chân nhân vật: X = nửa hitbox + khoảng này, Y = độ cao ngang thân.")]
        public float hitboxForwardGap = 0.15f;
        public float hitboxHeight = 0.9f;
        public bool fireAxeUnlocked;
        public Sprite fireSprite;

        /// <summary>Bắn khi một đòn được tung ra (dùng cho DemoPlayerVisuals).</summary>
        public event Action<AttackKind> OnAttackFired;
        public event Action OnChargeStarted;

        public float FireCooldownLeft => Mathf.Max(0f, nextFireTime - Time.time);

        CharacterContext ctx;
        PlayerInputBroker input;
        readonly Collider2D[] buffer = new Collider2D[16];
        readonly HashSet<DemoDamageable> hitThisSwing = new HashSet<DemoDamageable>();

        // Trạng thái mô phỏng AttackState
        bool inAttack, isAir, charging, heavyFired, comboBuffered;
        int comboStep;
        float enterTime, combo1Time, bufferPressTime;
        float nextFireTime;

        PlayerCombatStatsSO Combat => ctx.CombatStats;
        AbilitiesStatsSO Abilities => ctx.AbilitiesStats;
        bool InAttackState => ctx.StateMachine != null && ctx.StateMachine.CurrentState == ctx.AttackState;

        void Awake()
        {
            ctx = GetComponent<CharacterContext>();
            input = GetComponent<PlayerInputBroker>();
        }

        void Start()
        {
            if (ctx.StateMachine != null) ctx.StateMachine.OnStateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            if (ctx != null && ctx.StateMachine != null) ctx.StateMachine.OnStateChanged -= OnStateChanged;
        }

        void OnEnable()
        {
            if (input == null) return;
            input.OnAttackPressed += AttackPressed;
            input.OnAttackReleased += AttackReleased;
            input.OnFireAxePressed += FireAxe;
        }

        void OnDisable()
        {
            if (input == null) return;
            input.OnAttackPressed -= AttackPressed;
            input.OnAttackReleased -= AttackReleased;
            input.OnFireAxePressed -= FireAxe;
        }

        void OnStateChanged(IState s)
        {
            if (s == ctx.AttackState)
            {
                inAttack = true;
                enterTime = Time.time;
                comboStep = 0; comboBuffered = false; heavyFired = false;
                isAir = !ctx.CheckGrounded();
                if (isAir)
                {
                    charging = false;
                    Fire(AttackKind.Combo1);
                }
                else
                {
                    charging = true;
                    OnChargeStarted?.Invoke();
                }
            }
            else
            {
                inAttack = charging = comboBuffered = false;
                comboStep = 0;
            }
        }

        void AttackPressed()
        {
            // Giống AttackState.HandleAttackPressed: chỉ buffer nhịp 2 khi đang ở nhịp 1 dưới đất
            if (!InAttackState || isAir || charging || heavyFired || comboStep != 1 || comboBuffered) return;
            comboBuffered = true;
            bufferPressTime = Time.time;
        }

        void AttackReleased()
        {
            // Giống AttackState.HandleAttackReleased: thả trước khi nạp đủ -> nhịp 1
            if (!InAttackState || !charging || heavyFired) return;
            charging = false;
            if (Time.time - enterTime < Combat.heavyChargeTime) Fire(AttackKind.Combo1);
        }

        void Update()
        {
            if (!inAttack || !InAttackState) return;

            if (charging && !heavyFired && Time.time - enterTime >= Combat.heavyChargeTime)
            {
                charging = false;
                Fire(AttackKind.Heavy);
            }

            if (comboStep == 1 && comboBuffered && Time.time >= Mathf.Max(bufferPressTime, combo1Time + Combat.attackDuration))
                Fire(AttackKind.Combo2);
        }

        void Fire(AttackKind kind)
        {
            switch (kind)
            {
                case AttackKind.Combo1:
                    comboStep = 1; combo1Time = Time.time;
                    DoHit(Combat.combo1Damage, false, Combat.normalHitStop, 0.5f);
                    break;
                case AttackKind.Combo2:
                    comboStep = 2; comboBuffered = false;
                    DemoUI.Popup(transform.position + Vector3.up * 2.4f, "Nhịp 2", new Color(1f, 0.95f, 0.6f));
                    DoHit(Combat.combo2Damage, false, Combat.normalHitStop, 0.5f);
                    break;
                case AttackKind.Heavy:
                    heavyFired = true;
                    DemoUI.Popup(transform.position + Vector3.up * 2.4f, "Đòn nặng!", new Color(1f, 0.95f, 0.6f));
                    DoHit(Combat.heavyDamage, false, Combat.heavyHitStop, 1f);
                    break;
            }
            OnAttackFired?.Invoke(kind);
        }

        void FireAxe()
        {
            if (!fireAxeUnlocked)
            {
                DemoUI.Banner("Chưa có Đá Đỏ – hãy nhặt viên đá ở đoạn 2", 2f);
                return;
            }
            var st = ctx.StateMachine?.CurrentState;
            if (st == ctx.RollState || st == ctx.AttackState) return; // không chen vào lúc lăn / đang chém
            if (Time.time < nextFireTime)
            {
                DemoUI.Banner("Rìu lửa đang hồi: " + FireCooldownLeft.ToString("0.0") + " s", 1f);
                return;
            }
            nextFireTime = Time.time + Abilities.fireAxeCooldown;
            StartCoroutine(FireVfx());
            DoHit(Abilities.fireAxeDamage, true, Combat.heavyHitStop, 1f);
            OnAttackFired?.Invoke(AttackKind.FireAxe);
        }

        IEnumerator FireVfx()
        {
            var go = new GameObject("FX_FireAxe");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = fireSprite;
            sr.sortingOrder = 50;
            go.transform.position = HitboxCenter();
            Vector2 size = Combat.hitboxSize;
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                float k = t / 0.3f;
                go.transform.localScale = new Vector3(size.x * (0.6f + k * 0.8f), size.y * (0.6f + k * 0.8f), 1f);
                go.transform.rotation = Quaternion.Euler(0, 0, 45f + k * 90f * (ctx.IsFacingRight ? -1 : 1));
                sr.color = Color.Lerp(new Color(1f, 0.85f, 0.2f, 0.95f), new Color(1f, 0.25f, 0.05f, 0f), k);
                yield return null;
            }
            Destroy(go);
        }

        Vector3 HitboxCenter()
        {
            float dir = ctx.IsFacingRight ? 1f : -1f;
            float forward = Combat != null ? Combat.hitboxSize.x * 0.5f + hitboxForwardGap : 0.9f;
            return transform.position + new Vector3(forward * dir, hitboxHeight, 0f);
        }

        void DoHit(int damage, bool fire, float hitStop, float shake)
        {
            int n = Physics2D.OverlapBox(HitboxCenter(), Combat.hitboxSize, 0f, ContactFilter2D.noFilter, buffer);
            bool any = false;
            hitThisSwing.Clear();
            float iceMul = Abilities != null ? Abilities.iceEnemyDamageMultiplier : 1f;
            for (int i = 0; i < n; i++)
            {
                var col = buffer[i];
                if (col == null || col.transform.IsChildOf(transform)) continue;
                if (fire) { var fireplace = col.GetComponentInParent<DemoCampfire>(); if (fireplace) fireplace.Light(); }
                var d = col.GetComponentInParent<DemoDamageable>();
                if (d == null || !hitThisSwing.Add(d)) continue;
                if (d.TakeHit(damage, fire, iceMul)) any = true;
            }
            if (any) ctx.ApplyHitFeedback(hitStop, shake);
        }

        void OnDrawGizmosSelected()
        {
            if (ctx == null) ctx = GetComponent<CharacterContext>();
            if (ctx == null || ctx.CombatStats == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(HitboxCenter(), ctx.CombatStats.hitboxSize);
        }
    }
}
