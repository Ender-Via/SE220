using System;
using System.Collections;
using UnityEngine;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Vật/quái nhận sát thương từ rìu.
    /// - requiresFire: chỉ rìu lửa mới gây sát thương (tường băng).
    /// - isIce: nhận x2 sát thương từ rìu lửa.
    /// - respawnDelay > 0: hồi lại sau khi bị phá (khúc gỗ tập chém).
    /// </summary>
    public class DemoDamageable : MonoBehaviour
    {
        public int maxHp = 3;
        public bool requiresFire;
        public bool isIce;
        [Tooltip("> 0: tự hồi lại sau bấy nhiêu giây (khúc gỗ tập chém). 0: biến mất.")]
        public float respawnDelay;
        [Tooltip("Câu hiện khi chém thường vào vật chỉ rìu lửa phá được.")]
        public string needFireMessage = "Rìu thường không phá được băng – dùng Rìu lửa (L)";

        public int Hp { get; private set; }
        public bool IsDead => Hp <= 0;

        public event Action<int, bool> OnDamaged;   // (sát thương, là lửa)
        public event Action OnDied;

        SpriteRenderer[] renderers;
        Color[] baseColors;
        Collider2D[] colliders;

        void Awake()
        {
            Hp = maxHp;
            renderers = GetComponentsInChildren<SpriteRenderer>();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].color;
            colliders = GetComponentsInChildren<Collider2D>();
        }

        /// <returns>true nếu đòn có tác dụng.</returns>
        /// <param name="iceMultiplier">Hệ số rìu lửa với vật/quái băng (AbilitiesStatsSO.iceEnemyDamageMultiplier).</param>
        public bool TakeHit(int damage, bool isFire, float iceMultiplier = 1f)
        {
            if (IsDead) return false;
            if (requiresFire && !isFire)
            {
                DemoUI.Popup(transform.position + Vector3.up * 2f, "Không ăn thua!", new Color(0.7f, 0.85f, 1f));
                DemoUI.Banner(needFireMessage, 2f);
                return false;
            }

            bool iceBonus = isFire && isIce && iceMultiplier > 1f;
            if (iceBonus) damage = Mathf.RoundToInt(damage * iceMultiplier);
            Hp = Mathf.Max(0, Hp - damage);
            DemoUI.Popup(transform.position + Vector3.up * 1.8f, "-" + damage + (iceBonus ? " (x" + iceMultiplier.ToString("0.#") + " băng)" : ""),
                isFire ? new Color(1f, 0.55f, 0.2f) : Color.white);
            OnDamaged?.Invoke(damage, isFire);
            StopCoroutine(nameof(Flash));
            StartCoroutine(nameof(Flash));

            if (Hp <= 0)
            {
                OnDied?.Invoke();
                StartCoroutine(DieRoutine());
            }
            return true;
        }

        IEnumerator Flash()
        {
            for (int i = 0; i < renderers.Length; i++) renderers[i].color = Color.white * 3f;
            yield return new WaitForSeconds(0.08f);
            RestoreColors();
        }

        void RestoreColors()
        {
            for (int i = 0; i < renderers.Length; i++) renderers[i].color = baseColors[i];
        }

        IEnumerator DieRoutine()
        {
            // Quái tự lo animation chết (DemoWolf). Vật thường: mờ dần.
            bool selfManaged = GetComponent<DemoWolf>() != null;
            if (!selfManaged)
            {
                foreach (var c in colliders) c.enabled = false;
                for (float t = 0; t < 0.4f; t += Time.deltaTime)
                {
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        var c = baseColors[i]; c.a = 1f - t / 0.4f; renderers[i].color = c;
                    }
                    yield return null;
                }
                foreach (var r in renderers) r.enabled = false;
            }

            if (respawnDelay <= 0f) yield break;
            yield return new WaitForSeconds(respawnDelay);
            Hp = maxHp;
            foreach (var r in renderers) r.enabled = true;
            foreach (var c in colliders) c.enabled = true;
            RestoreColors();
        }
    }
}
