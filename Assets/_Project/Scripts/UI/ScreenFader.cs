using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Woodcutter.UI
{
    /// <summary>
    /// Màn phủ toàn màn hình, dùng khi chuyển phòng (đen) / hồi sinh (trắng, UC-05).
    /// Chạy theo thời gian thực (unscaled) để không bị ảnh hưởng bởi hit-stop hay pause.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Ảnh phủ màn hình. Để trống = tự tìm Graphic con đầu tiên.")]
        [SerializeField] private Graphic overlay;

        private Color defaultColor = Color.black;

        public bool IsOpaque => canvasGroup != null && canvasGroup.alpha >= 0.999f;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            if (overlay == null) overlay = GetComponentInChildren<Graphic>(true);
            if (overlay != null) defaultColor = overlay.color;
        }

        /// <summary>Đổi màu màn phủ (độ mờ vẫn do CanvasGroup quyết định).</summary>
        public void SetColor(Color color)
        {
            if (overlay != null) overlay.color = new Color(color.r, color.g, color.b, 1f);
        }

        /// <summary>Trả về màu ban đầu trong scene (đen).</summary>
        public void ResetColor()
        {
            if (overlay != null) overlay.color = defaultColor;
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float start = canvasGroup.alpha;
            if (duration <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
                SetAlpha(Mathf.Lerp(start, targetAlpha, elapsed / duration));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }
    }
}
