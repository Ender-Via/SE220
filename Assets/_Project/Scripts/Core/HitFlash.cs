using System.Collections;
using UnityEngine;

namespace Woodcutter.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [Tooltip("Material màu trắng để nháy khi trúng đòn")]
        public Material flashMaterial;

        [Tooltip("Thời gian nháy trắng (thường là 0.1s)")]
        public float flashDuration = 0.1f;

        private SpriteRenderer spriteRenderer;
        private Material originalMaterial;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // QUAN TRỌNG: Dùng sharedMaterial thay vì material để tránh rò rỉ bộ nhớ (memory leak)
            if (spriteRenderer != null)
            {
                originalMaterial = spriteRenderer.sharedMaterial;
            }
        }

        // Hàm này sẽ được hệ thống Health gọi khi nhân vật/quái vật bị trừ máu
        public void TriggerFlash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer != null && flashMaterial != null)
            {
                spriteRenderer.sharedMaterial = flashMaterial;

                // Dùng WaitForSecondsRealtime vì game có thể đang bị Hit-stop (TimeScale = 0)
                yield return new WaitForSecondsRealtime(flashDuration);

                spriteRenderer.sharedMaterial = originalMaterial;
            }
        }

        private void OnDisable()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            if (spriteRenderer != null && originalMaterial != null)
            {
                spriteRenderer.sharedMaterial = originalMaterial;
            }
        }
    }
}