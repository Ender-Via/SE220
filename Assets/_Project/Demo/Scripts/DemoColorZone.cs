using System.Collections;
using UnityEngine;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Giả lập "hồi màu loang ra như mực thấm giấy":
    /// mọi sprite con lúc đầu xám, khi Restore() thì màu thật loang ra từ một điểm trong vài giây.
    /// (Bản thật sẽ là Shader Graph mask theo bán kính – xem CLAUDE.md mục 10.)
    /// </summary>
    public class DemoColorZone : MonoBehaviour
    {
        public float duration = 2.5f;
        public float maxRadius = 25f;

        SpriteRenderer[] rs;
        Color[] trueColors;

        void Awake()
        {
            rs = GetComponentsInChildren<SpriteRenderer>(true);
            trueColors = new Color[rs.Length];
            for (int i = 0; i < rs.Length; i++)
            {
                trueColors[i] = rs[i].color;
                rs[i].color = Gray(trueColors[i]);
            }
        }

        static Color Gray(Color c)
        {
            float g = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
            return new Color(g, g, g, c.a);
        }

        public void Restore(Vector3 origin) => StartCoroutine(RestoreRoutine(origin));

        IEnumerator RestoreRoutine(Vector3 origin)
        {
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                float radius = maxRadius * (t / duration);
                for (int i = 0; i < rs.Length; i++)
                {
                    float d = Vector2.Distance(rs[i].transform.position, origin);
                    float k = Mathf.Clamp01((radius - d) / 3f);
                    rs[i].color = Color.Lerp(Gray(trueColors[i]), trueColors[i], k);
                }
                yield return null;
            }
            for (int i = 0; i < rs.Length; i++) rs[i].color = trueColors[i];
        }
    }
}
