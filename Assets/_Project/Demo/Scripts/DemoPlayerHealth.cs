using System.Collections;
using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Máu theo vạch (5, +1 mỗi viên đá), i-frame 1 s khi bị đánh,
    /// tôn trọng i-frame của cú lăn (CharacterContext.IsInvulnerable). Hết máu -> về checkpoint.
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class DemoPlayerHealth : MonoBehaviour
    {
        public int maxHp = 5;
        public float hurtInvulnerability = 1f;
        public Vector2 knockback = new Vector2(6f, 5f);

        public int Hp { get; private set; }
        public Vector3 RespawnPoint { get; set; }

        CharacterContext ctx;
        SpriteRenderer sr;
        float invulnUntil;

        void Awake()
        {
            ctx = GetComponent<CharacterContext>();
            sr = GetComponent<SpriteRenderer>();
            Hp = maxHp;
            RespawnPoint = transform.position;
        }

        public void AddMaxHp(int amount)
        {
            maxHp += amount;
            Hp = maxHp;
        }

        /// <returns>true nếu trúng đòn.</returns>
        public bool TakeDamage(int damage, Vector3 sourcePos)
        {
            if (ctx.IsInvulnerable)
            {
                DemoUI.Popup(transform.position + Vector3.up * 2.2f, "Né!", new Color(0.6f, 1f, 0.7f));
                return false;
            }
            if (Time.time < invulnUntil) return false;

            Hp = Mathf.Max(0, Hp - damage);
            invulnUntil = Time.time + hurtInvulnerability;
            float dir = Mathf.Sign(transform.position.x - sourcePos.x);
            if (dir == 0) dir = 1;
            ctx.OverrideHorizontalVelocity(new Vector2(knockback.x * dir, knockback.y), 0.2f);
            ctx.ApplyHitFeedback(0.08f, 1f);
            StartCoroutine(Blink());

            if (Hp <= 0) StartCoroutine(Respawn());
            return true;
        }

        IEnumerator Blink()
        {
            while (Time.time < invulnUntil)
            {
                if (sr) sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(0.08f);
            }
            if (sr) sr.enabled = true;
        }

        IEnumerator Respawn()
        {
            DemoUI.Banner("Ngã xuống… quay về điểm lưu", 1.5f);
            yield return new WaitForSeconds(0.6f);
            ctx.Rigidbody.linearVelocity = Vector2.zero;
            transform.position = RespawnPoint;
            Hp = maxHp;
        }
    }
}
