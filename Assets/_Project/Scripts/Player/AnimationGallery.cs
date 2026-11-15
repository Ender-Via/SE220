using System.Collections.Generic;
using UnityEngine;

namespace Woodcutter.Player
{
    /// <summary>
    /// Công cụ xem thử (chỉ dùng trong Sandbox_AnimationGallery, không dùng trong game):
    /// phát mọi state của Animator cạnh nhau, lặp lại cả các clip không lặp (đòn đánh, chết...).
    /// Nút trên màn hình để chỉnh tốc độ phát và phát lại từ đầu; tên state hiện dưới mỗi nhân vật.
    /// </summary>
    public class AnimationGallery : MonoBehaviour
    {
        [System.Serializable]
        public class Item
        {
            public string stateName;
            public Animator animator;
            public bool loop = true;
        }

        [Tooltip("Mỗi ô: một Animator và state nó phát.")]
        public List<Item> items = new List<Item>();

        [Tooltip("Tốc độ phát chung. 0.25 = chậm 4 lần, để soi từng frame.")]
        [Range(0.05f, 2f)] public float speed = 1f;

        [Tooltip("Clip không lặp: giữ frame cuối bao lâu (giây thực) rồi phát lại.")]
        [Min(0f)] public float holdBeforeReplay = 0.8f;

        static readonly float[] SpeedOptions = { 0.25f, 0.5f, 1f };
        readonly Dictionary<Item, float> holdTimers = new Dictionary<Item, float>();
        GUIStyle labelStyle;

        void Start() => ReplayAll();

        public void ReplayAll()
        {
            holdTimers.Clear();
            foreach (var item in items)
                if (item.animator != null) item.animator.Play(item.stateName, 0, 0f);
        }

        void Update()
        {
            foreach (var item in items)
            {
                if (item.animator == null) continue;
                item.animator.speed = speed;
                if (item.loop) continue;

                // Clip không lặp: hết clip -> giữ frame cuối một lúc -> phát lại.
                var info = item.animator.GetCurrentAnimatorStateInfo(0);
                if (info.normalizedTime < 1f) continue;
                holdTimers.TryGetValue(item, out float held);
                held += Time.unscaledDeltaTime;
                if (held >= holdBeforeReplay)
                {
                    item.animator.Play(item.stateName, 0, 0f);
                    held = 0f;
                }
                holdTimers[item] = held;
            }
        }

        void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, fontSize = 14 };
                labelStyle.normal.textColor = Color.white;
            }

            // Nút điều khiển góc trên trái
            GUILayout.BeginArea(new Rect(10, 10, 420, 30));
            GUILayout.BeginHorizontal();
            foreach (float s in SpeedOptions)
                if (GUILayout.Button(Mathf.Approximately(speed, s) ? $"[{s}x]" : $"{s}x", GUILayout.Width(60))) speed = s;
            if (GUILayout.Button("Phát lại", GUILayout.Width(90))) ReplayAll();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Tên state dưới chân mỗi nhân vật
            var cam = Camera.main;
            if (cam == null) return;
            foreach (var item in items)
            {
                if (item.animator == null) continue;
                Vector3 p = cam.WorldToScreenPoint(item.animator.transform.position);
                GUI.Label(new Rect(p.x - 90, Screen.height - p.y + 6, 180, 22),
                    item.stateName + (item.loop ? "" : " (1 lần)"), labelStyle);
            }
        }
    }
}
