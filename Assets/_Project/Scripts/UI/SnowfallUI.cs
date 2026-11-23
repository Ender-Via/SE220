using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Woodcutter.UI
{
    /// <summary>
    /// Tuyết rơi trắng đen cho nền menu chính (SRS mục 8.2). Vẽ bằng Image trong Canvas,
    /// chạy theo thời gian thực nên vẫn rơi khi game tạm dừng. Bản gray-box; ARTE thay sau.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SnowfallUI : MonoBehaviour
    {
        [SerializeField, Min(1)] private int flakeCount = 80;
        [SerializeField] private Vector2 sizeRange = new Vector2(2f, 6f);
        [SerializeField] private Vector2 fallSpeedRange = new Vector2(20f, 60f);
        [SerializeField] private float swayAmplitude = 12f;
        [SerializeField] private Color flakeColor = new Color(1f, 1f, 1f, 0.8f);

        private struct Flake
        {
            public RectTransform rect;
            public float speed;
            public float phase;
            public float baseX;
        }

        private readonly List<Flake> flakes = new List<Flake>();
        private RectTransform area;

        private void Start()
        {
            area = (RectTransform)transform;
            Rect r = area.rect;
            for (int i = 0; i < flakeCount; i++)
            {
                var go = new GameObject("Flake", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var image = go.GetComponent<Image>();
                image.color = flakeColor;
                image.raycastTarget = false;

                var rect = (RectTransform)go.transform;
                float size = Random.Range(sizeRange.x, sizeRange.y);
                rect.sizeDelta = new Vector2(size, size);
                float x = Random.Range(r.xMin, r.xMax);
                rect.anchoredPosition = new Vector2(x, Random.Range(r.yMin, r.yMax));

                flakes.Add(new Flake
                {
                    rect = rect,
                    speed = Random.Range(fallSpeedRange.x, fallSpeedRange.y),
                    phase = Random.Range(0f, Mathf.PI * 2f),
                    baseX = x
                });
            }
        }

        private void Update()
        {
            if (area == null) return;
            Rect r = area.rect;
            float dt = Time.unscaledDeltaTime;
            float t = Time.unscaledTime;

            for (int i = 0; i < flakes.Count; i++)
            {
                Flake f = flakes[i];
                Vector2 p = f.rect.anchoredPosition;
                p.y -= f.speed * dt;
                if (p.y < r.yMin - 10f)
                {
                    p.y = r.yMax + 10f;
                    f.baseX = Random.Range(r.xMin, r.xMax);
                }
                p.x = f.baseX + Mathf.Sin(t * 0.8f + f.phase) * swayAmplitude;
                f.rect.anchoredPosition = p;
                flakes[i] = f;
            }
        }
    }
}
