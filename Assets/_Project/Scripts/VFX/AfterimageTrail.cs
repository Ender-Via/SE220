using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Woodcutter.VFX
{
    /// <summary>
    /// Tàn ảnh khi né/dash: trong lúc bật, cứ mỗi khoảng thời gian chụp lại tư thế hiện tại của nhân vật
    /// (bản sao đứng yên tại chỗ), rồi cho mờ dần và tự hủy.
    /// Màu tàn ảnh = màu nhân vật tại thời điểm sinh ra × ghostTint, nên khi nhân vật đổi màu (dash đá vàng)
    /// thì tàn ảnh cũng mang đúng màu lúc đó.
    /// Đặt component này ở gốc hình nhân vật (cùng chỗ với Animator).
    /// </summary>
    public class AfterimageTrail : MonoBehaviour
    {
        [Header("Sinh tàn ảnh")]
        [Tooltip("Khoảng cách thời gian giữa hai tàn ảnh (giây)")]
        [SerializeField] float spawnInterval = 0.06f;
        [Tooltip("Số tàn ảnh tối đa mỗi lần né (0 = không giới hạn)")]
        [SerializeField] int maxPerBurst = 4;
        [Tooltip("Sinh tàn ảnh đầu tiên ngay khi bắt đầu")]
        [SerializeField] bool spawnOnStart = true;

        [Header("Hình dạng")]
        [Tooltip("Thời gian một tàn ảnh mờ hẳn (giây)")]
        [SerializeField] float lifetime = 0.25f;
        [Tooltip("Màu nhân với màu nhân vật; alpha = độ đậm lúc mới sinh")]
        [SerializeField] Color ghostTint = new Color(1f, 1f, 1f, 0.55f);
        [Tooltip("Tàn ảnh vẽ sau nhân vật bao nhiêu bậc Order in Layer")]
        [SerializeField] int sortingOffset = -40;
        [Tooltip("Tên các đối tượng con KHÔNG chép vào tàn ảnh (vệt chém...)")]
        [SerializeField] string[] excludeNames = { "VFX_Slash", "VFX_Heavy" };

        bool emitting;
        float timer;
        int spawnedThisBurst;

        readonly List<SpriteRenderer> sourceRenderers = new List<SpriteRenderer>();
        static readonly List<SpriteRenderer> tmpRenderers = new List<SpriteRenderer>();

        /// <summary>Bật/tắt sinh tàn ảnh. Bật lại sau khi tắt sẽ bắt đầu lượt mới.</summary>
        public void SetEmitting(bool on)
        {
            if (on == emitting) return;
            emitting = on;
            if (on)
            {
                spawnedThisBurst = 0;
                timer = spawnOnStart ? 0f : spawnInterval;
            }
        }

        void LateUpdate()
        {
            if (!emitting) return;
            if (maxPerBurst > 0 && spawnedThisBurst >= maxPerBurst) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer += spawnInterval;
            SpawnGhost();
            spawnedThisBurst++;
        }

        void SpawnGhost()
        {
            // Bản sao giữ nguyên tư thế xương ngay lúc này (LateUpdate chạy sau Animator)
            GameObject ghost = Instantiate(gameObject, transform.position, transform.rotation);
            ghost.name = name + "_Afterimage";
            ghost.transform.localScale = transform.lossyScale; // giữ hướng quay mặt (scale âm)

            // Bỏ mọi thứ làm bản sao chuyển động hoặc sinh thêm tàn ảnh
            foreach (var anim in ghost.GetComponentsInChildren<Animator>(true)) anim.enabled = false;
            foreach (var trail in ghost.GetComponentsInChildren<AfterimageTrail>(true)) Destroy(trail);
            foreach (var t in ghost.GetComponentsInChildren<Transform>(true))
                for (int i = 0; i < excludeNames.Length; i++)
                    if (t != null && t.name == excludeNames[i]) Destroy(t.gameObject);

            // Màu = màu hiện tại của từng bộ phận × tint
            sourceRenderers.Clear();
            GetComponentsInChildren(true, sourceRenderers);
            tmpRenderers.Clear();
            ghost.GetComponentsInChildren(true, tmpRenderers);
            var fade = ghost.AddComponent<AfterimageFade>();
            fade.Init(lifetime, ghostTint.a);
            for (int i = 0; i < tmpRenderers.Count; i++)
            {
                var r = tmpRenderers[i];
                Color baseColor = i < sourceRenderers.Count ? sourceRenderers[i].color : r.color;
                Color c = baseColor * ghostTint;
                c.a = baseColor.a;
                r.color = c;
                r.sortingOrder += sortingOffset;
                fade.Add(r, c);
            }
        }
    }

    /// <summary>Cho tàn ảnh mờ dần rồi tự hủy. Do AfterimageTrail tự gắn vào.</summary>
    public class AfterimageFade : MonoBehaviour
    {
        readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        readonly List<Color> colors = new List<Color>();
        float lifetime, startAlpha, age;

        public void Init(float life, float alpha)
        {
            lifetime = Mathf.Max(0.01f, life);
            startAlpha = alpha;
        }

        public void Add(SpriteRenderer r, Color c)
        {
            renderers.Add(r);
            colors.Add(c);
        }

        void Update()
        {
            age += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(age / lifetime);
            float a = startAlpha * k * k; // mờ nhanh dần về cuối
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null) continue;
                Color c = colors[i];
                c.a *= a;
                renderers[i].color = c;
            }
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
