using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Đống lửa trại: lúc đầu tắt (củi xám, không lửa).
    /// Khi người chơi có Đá Đỏ thì tự bùng cháy (trễ theo khoảng cách, như màu loang ra),
    /// hoặc chém Rìu lửa (L) vào cũng thắp được. Lửa lập lòe + tàn lửa bay lên + ánh sáng 2D.
    /// </summary>
    public class DemoCampfire : MonoBehaviour
    {
        public Transform[] flames;
        public Light2D glow;
        public float lightDelay = 0.6f;
        public Sprite emberSprite;

        bool lit;
        float litTime = -1f;
        Vector3[] baseScales;
        float nextEmber;

        void Awake()
        {
            baseScales = new Vector3[flames.Length];
            for (int i = 0; i < flames.Length; i++)
            {
                baseScales[i] = flames[i].localScale;
                flames[i].gameObject.SetActive(false);
            }
            if (glow) glow.enabled = false;
        }

        public void Light()
        {
            if (lit) return;
            lit = true;
            litTime = Time.time;
            foreach (var f in flames) f.gameObject.SetActive(true);
            if (glow) glow.enabled = true;
            DemoUI.Popup(transform.position + Vector3.up * 2f, "Lửa trại bùng cháy!", new Color(1f, 0.6f, 0.2f), 1.5f);
        }

        void Update()
        {
            if (!lit)
            {
                if (DemoUI.HasRedStone)
                {
                    if (litTime < 0f) litTime = Time.time + lightDelay;
                    else if (Time.time >= litTime) Light();
                }
                return;
            }

            float grow = Mathf.Clamp01((Time.time - litTime) / 0.4f);
            for (int i = 0; i < flames.Length; i++)
            {
                float n = Mathf.PerlinNoise(Time.time * 6f + i * 3.1f, i);
                float sy = 0.8f + n * 0.45f;
                float sx = 0.9f + Mathf.PerlinNoise(i, Time.time * 5f) * 0.2f;
                flames[i].localScale = new Vector3(baseScales[i].x * sx, baseScales[i].y * sy, 1f) * grow;
            }
            if (glow) glow.intensity = (0.9f + Mathf.PerlinNoise(Time.time * 8f, 0f) * 0.5f) * grow;

            if (Time.time >= nextEmber)
            {
                nextEmber = Time.time + Random.Range(0.08f, 0.2f);
                SpawnEmber();
            }
        }

        void SpawnEmber()
        {
            var go = new GameObject("Ember");
            go.transform.position = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0f);
            go.transform.localScale = Vector3.one * Random.Range(0.06f, 0.12f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = emberSprite;
            sr.sortingOrder = 6;
            sr.color = Random.value < 0.5f ? new Color(1f, 0.55f, 0.1f) : new Color(1f, 0.85f, 0.3f);
            go.AddComponent<Ember>();
        }

        class Ember : MonoBehaviour
        {
            float t, life = 1.1f, drift;
            SpriteRenderer sr;
            void Awake() { sr = GetComponent<SpriteRenderer>(); drift = Random.Range(-0.4f, 0.4f); life = Random.Range(0.8f, 1.4f); }
            void Update()
            {
                t += Time.deltaTime;
                transform.position += new Vector3(drift + Mathf.Sin(t * 6f) * 0.3f, 1.6f, 0f) * Time.deltaTime;
                var c = sr.color; c.a = 1f - t / life; sr.color = c;
                if (t >= life) Destroy(gameObject);
            }
        }
    }
}
