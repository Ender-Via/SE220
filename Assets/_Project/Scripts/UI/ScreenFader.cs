using System.Collections;
using UnityEngine;

namespace Woodcutter.UI
{
    /// <summary>
    /// Màn đen phủ toàn màn hình, dùng khi chuyển phòng / hồi sinh.
    /// Chạy theo thời gian thực (unscaled) để không bị ảnh hưởng bởi hit-stop hay pause.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public bool IsOpaque => canvasGroup != null && canvasGroup.alpha >= 0.999f;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
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
                elapsed += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(start, targetAlpha, elapsed / duration));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }
    }
}
