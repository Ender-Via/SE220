using System.Collections;
using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Gắn sát thương vào các đòn rìu đã có trong AttackState
    /// (main chưa có hitbox – xem TODO trong AttackState) và thêm Rìu lửa (L) sau khi nhặt Đá Đỏ.
    /// Không sửa code nhân vật: chỉ nghe sự kiện input từ PlayerInputBroker.
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class DemoPlayerCombat : MonoBehaviour
    {
        [Header("Hitbox rìu (ô)")]
        public Vector2 hitboxSize = new Vector2(1.5f, 1.5f);
        public Vector2 hitboxOffset = new Vector2(0.9f, 0.9f);

        [Header("Rìu lửa")]
        public bool fireAxeUnlocked;
        public int fireDamage = 2;
        public float fireCooldown = 1.5f;
        public Sprite fireSprite;

        public float FireCooldownLeft => Mathf.Max(0f, nextFireTime - Time.time);

        CharacterContext ctx;
        PlayerInputBroker input;
        float pressTime;
        bool holding, heavyFired;
        float lastLightHitTime = -10f;
        int comboStep;
        float nextFireTime;
        readonly Collider2D[] buffer = new Collider2D[16];

        void Awake()
        {
            ctx = GetComponent<CharacterContext>();
            input = GetComponent<PlayerInputBroker>();
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

        float Charge => ctx.CombatStats != null ? ctx.CombatStats.heavyChargeTime : 0.6f;
        float AttackDur => ctx.CombatStats != null ? ctx.CombatStats.attackDuration : 0.25f;
        float ComboWindow => ctx.CombatStats != null ? ctx.CombatStats.comboWindow : 0.4f;

        void AttackPressed()
        {
            // Bấm lần 2 trong cửa sổ combo -> nhịp 2
            if (comboStep == 1 && Time.time - lastLightHitTime <= AttackDur + ComboWindow)
            {
                comboStep = 2;
                StartCoroutine(DelayedHit(AttackDur, 1, false, 0.05f, "Nhịp 2"));
                return;
            }
            holding = true;
            heavyFired = false;
            pressTime = Time.time;
        }

        void AttackReleased()
        {
            if (!holding) return;
            holding = false;
            if (!heavyFired && Time.time - pressTime < Charge)
            {
                comboStep = 1;
                lastLightHitTime = Time.time;
                StartCoroutine(DelayedHit(0.08f, 1, false, 0.05f, null));
            }
        }

        void Update()
        {
            if (holding && !heavyFired && Time.time - pressTime >= Charge && ctx.StateMachine?.CurrentState == ctx.AttackState)
            {
                heavyFired = true;
                comboStep = 0;
                StartCoroutine(DelayedHit(0.1f, 3, false, 0.1f, "Đòn nặng!"));
            }
            if (comboStep == 2 && Time.time - lastLightHitTime > 1f) comboStep = 0;
        }

        IEnumerator DelayedHit(float delay, int damage, bool fire, float hitStop, string label)
        {
            yield return new WaitForSeconds(delay);
            if (label != null) DemoUI.Popup(transform.position + Vector3.up * 2.4f, label, new Color(1f, 0.95f, 0.6f));
            DoHit(damage, fire, hitStop);
        }

        void FireAxe()
        {
            if (!fireAxeUnlocked)
            {
                DemoUI.Banner("Chưa có Đá Đỏ – hãy nhặt viên đá ở đoạn 2", 2f);
                return;
            }
            if (Time.time < nextFireTime)
            {
                DemoUI.Banner("Rìu lửa đang hồi: " + FireCooldownLeft.ToString("0.0") + " s", 1f);
                return;
            }
            nextFireTime = Time.time + fireCooldown;
            StartCoroutine(FireVfx());
            DoHit(fireDamage, true, 0.1f);
        }

        IEnumerator FireVfx()
        {
            var go = new GameObject("FX_FireAxe");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = fireSprite;
            sr.sortingOrder = 50;
            Vector3 c = HitboxCenter();
            go.transform.position = c;
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                float k = t / 0.3f;
                go.transform.localScale = new Vector3(hitboxSize.x * (0.6f + k * 0.8f), hitboxSize.y * (0.6f + k * 0.8f), 1f);
                go.transform.rotation = Quaternion.Euler(0, 0, 45f + k * 90f * (ctx.IsFacingRight ? -1 : 1));
                sr.color = Color.Lerp(new Color(1f, 0.85f, 0.2f, 0.95f), new Color(1f, 0.25f, 0.05f, 0f), k);
                yield return null;
            }
            Destroy(go);
        }

        Vector3 HitboxCenter()
        {
            float dir = ctx.IsFacingRight ? 1f : -1f;
            return transform.position + new Vector3(hitboxOffset.x * dir, hitboxOffset.y, 0f);
        }

        void DoHit(int damage, bool fire, float hitStop)
        {
            int n = Physics2D.OverlapBox(HitboxCenter(), hitboxSize, 0f, ContactFilter2D.noFilter, buffer);
            bool any = false;
            var hitThisSwing = new System.Collections.Generic.HashSet<DemoDamageable>();
            for (int i = 0; i < n; i++)
            {
                var col = buffer[i];
                if (col == null || col.transform.IsChildOf(transform)) continue;
                if (fire) { var fireplace = col.GetComponentInParent<DemoCampfire>(); if (fireplace) fireplace.Light(); }
                var d = col.GetComponentInParent<DemoDamageable>();
                if (d == null || !hitThisSwing.Add(d)) continue;
                if (d.TakeHit(damage, fire)) any = true;
            }
            if (any) ctx.ApplyHitFeedback(hitStop, fire || damage >= 3 ? 1f : 0.5f);
        }

        void OnDrawGizmosSelected()
        {
            if (ctx == null) ctx = GetComponent<CharacterContext>();
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Application.isPlaying ? HitboxCenter() : transform.position + (Vector3)hitboxOffset, hitboxSize);
        }
    }
}
