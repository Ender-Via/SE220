using System.Collections;
using UnityEngine;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Hiển thị tín hiệu cảnh báo trước đòn đánh (Telegraph >= 0.4s) theo SRS mục 6.
    /// Giúp người chơi có đủ thời gian phản xạ (lăn né, nhảy hoặc phản đòn).
    /// </summary>
    public class EnemyTelegraphIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Màu sắc báo hiệu")]
        [SerializeField] private Color telegraphColor = new Color(1f, 0.3f, 0.2f, 1f);

        [Tooltip("Tần số rung lắc trong thời gian báo hiệu")]
        [SerializeField] private float shakeFrequency = 50f;

        [Tooltip("Biên độ rung lắc")]
        [SerializeField] private float shakeMagnitude = 0.06f;

        [Header("References")]
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private GameObject warningIcon;

        private Coroutine activeRoutine;
        private Color originalColor = Color.white;
        private Vector3 originalLocalPos = Vector3.zero;

        private void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteTransform == null && targetRenderer != null) spriteTransform = targetRenderer.transform;
            if (targetRenderer != null) originalColor = targetRenderer.color;
            if (spriteTransform != null) originalLocalPos = spriteTransform.localPosition;

            if (warningIcon == null)
            {
                CreateDefaultWarningIcon();
            }
            if (warningIcon != null) warningIcon.SetActive(false);
        }

        private void CreateDefaultWarningIcon()
        {
            // Tự sinh icon cảnh báo "!" nhỏ phía trên đầu nếu chưa có trong prefab
            var iconGo = new GameObject("TelegraphWarningIcon");
            iconGo.transform.SetParent(transform, false);
            iconGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var textMesh = iconGo.AddComponent<TextMesh>();
            textMesh.text = "!";
            textMesh.fontSize = 28;
            textMesh.characterSize = 0.12f;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = telegraphColor;

            var meshRenderer = iconGo.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 100; // Hiển thị trên đầu quái
            }

            warningIcon = iconGo;
            warningIcon.SetActive(false);
        }

        public void ShowTelegraph(float duration)
        {
            StopTelegraph();
            activeRoutine = StartCoroutine(TelegraphRoutine(duration));
        }

        public void StopTelegraph()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            if (warningIcon != null) warningIcon.SetActive(false);
            if (targetRenderer != null) targetRenderer.color = originalColor;
            if (spriteTransform != null) spriteTransform.localPosition = originalLocalPos;
        }

        private IEnumerator TelegraphRoutine(float duration)
        {
            if (warningIcon != null) warningIcon.SetActive(true);

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = timer / duration;

                // 1. Nhấp nháy màu đỏ cảnh báo
                if (targetRenderer != null)
                {
                    targetRenderer.color = Color.Lerp(originalColor, telegraphColor, Mathf.PingPong(timer * 8f, 1f));
                }

                // 2. Rung giật sprite lấy đà
                if (spriteTransform != null)
                {
                    float offsetX = Mathf.Sin(timer * shakeFrequency) * shakeMagnitude;
                    spriteTransform.localPosition = originalLocalPos + new Vector3(offsetX, 0f, 0f);
                }

                yield return null;
            }

            StopTelegraph();
        }

        private void OnDisable()
        {
            StopTelegraph();
        }
    }
}
