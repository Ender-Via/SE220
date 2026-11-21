using System.Collections;
using UnityEngine;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. AI "lao tới" của Sói tuyết theo SRS:
    /// thấy người chơi -> gầm (rung + dấu "!", báo hiệu ≥ 0,4 s) -> vồ thẳng -> nghỉ -> lặp lại.
    /// Lăn (K) xuyên qua cú vồ rồi chém sau lưng.
    /// </summary>
    [RequireComponent(typeof(DemoDamageable))]
    public class DemoWolf : MonoBehaviour
    {
        public float detectRange = 7f;
        public float telegraphTime = 0.6f;
        public float lungeSpeed = 11f;
        public float lungeTime = 0.45f;
        public float recoverTime = 0.9f;
        public int contactDamage = 1;
        public Vector2 bodySize = new Vector2(2.2f, 1.1f);
        [Tooltip("Sprite gốc quay mặt sang trái.")]
        public bool spriteFacesLeft = true;
        public Transform sprite;

        DemoDamageable dmg;
        Animator anim;
        SpriteRenderer sr;
        DemoPlayerHealth player;
        float minX, maxX;
        bool busy, dead;
        Coroutine attackRoutine;

        void Awake()
        {
            dmg = GetComponent<DemoDamageable>();
            if (sprite == null) sprite = transform;
            anim = sprite.GetComponent<Animator>();
            sr = sprite.GetComponent<SpriteRenderer>();
            dmg.OnDamaged += (d, f) => { if (!dead) Play("Hit"); };
            dmg.OnDied += () => StartCoroutine(Die());
            minX = transform.position.x - 9f;
            maxX = transform.position.x + 9f;
        }

        void Start() => player = FindAnyObjectByType<DemoPlayerHealth>();

        void Update()
        {
            if (dead || busy || player == null) return;
            float dx = player.transform.position.x - transform.position.x;
            float dy = Mathf.Abs(player.transform.position.y - transform.position.y);
            Face(dx);
            if (Mathf.Abs(dx) < detectRange && dy < 3f) attackRoutine = StartCoroutine(Attack(Mathf.Sign(dx)));
        }

        void Face(float dx)
        {
            if (Mathf.Abs(dx) < 0.05f || sr == null) return;
            bool right = dx > 0;
            sr.flipX = spriteFacesLeft ? right : !right;
        }

        IEnumerator Attack(float dir)
        {
            busy = true;
            // 1. Gầm: rung tại chỗ + dấu "!"
            Play("Idle");
            DemoUI.Popup(transform.position + Vector3.up * 1.8f, "! Grrr", new Color(1f, 0.4f, 0.35f), telegraphTime);
            Vector3 basePos = sprite.localPosition;
            for (float t = 0; t < telegraphTime; t += Time.deltaTime)
            {
                if (dead) yield break;
                sprite.localPosition = basePos + new Vector3(Mathf.Sin(t * 70f) * 0.06f, 0, 0);
                yield return null;
            }
            sprite.localPosition = basePos;

            // 2. Vồ thẳng
            Play("Attack");
            bool hitPlayer = false;
            for (float t = 0; t < lungeTime; t += Time.deltaTime)
            {
                if (dead) yield break;
                var p = transform.position;
                p.x = Mathf.Clamp(p.x + dir * lungeSpeed * Time.deltaTime, minX, maxX);
                transform.position = p;
                if (!hitPlayer && TouchingPlayer()) hitPlayer = player.TakeDamage(contactDamage, transform.position);
                yield return null;
            }

            // 3. Nghỉ – cửa sổ để người chơi phản đòn
            Play("Idle");
            yield return new WaitForSeconds(recoverTime);
            busy = false;
        }

        bool TouchingPlayer()
        {
            var c = transform.position + Vector3.up * bodySize.y * 0.5f;
            var pp = player.transform.position + Vector3.up * 0.9f;
            return Mathf.Abs(pp.x - c.x) < bodySize.x * 0.5f + 0.4f && Mathf.Abs(pp.y - c.y) < bodySize.y * 0.5f + 0.9f;
        }

        IEnumerator Die()
        {
            dead = true;
            CancelAttack();
            Play("Dead");
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            DemoUI.Banner("Hạ được Sói tuyết!", 2f);
            yield return new WaitForSeconds(1.2f);
            gameObject.SetActive(false);
        }

        /// <summary>Dừng đợt tấn công đang chạy (gầm/vồ/nghỉ) và đưa sprite về vị trí gốc.</summary>
        void CancelAttack()
        {
            if (attackRoutine != null) { StopCoroutine(attackRoutine); attackRoutine = null; }
            busy = true;
            if (sprite != null) sprite.localPosition = Vector3.zero;
        }

        void Play(string s)
        {
            if (anim != null && anim.runtimeAnimatorController != null) anim.Play(s, 0, 0f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * bodySize.y * 0.5f, bodySize);
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(detectRange * 2, 6, 0));
        }
    }
}
