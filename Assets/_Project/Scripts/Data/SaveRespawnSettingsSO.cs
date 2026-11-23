using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Thông số lưu game, checkpoint và hồi sinh (SRS FR-LVL-04, FR-SYS-06, FR-CMB-06, NFR-14).
    /// Chỉnh trong Inspector, không hardcode.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveRespawnSettings", menuName = "Woodcutter/World/SaveRespawnSettings")]
    public class SaveRespawnSettingsSO : ScriptableObject
    {
        [Header("File lưu (SRS mục 9)")]
        [Tooltip("Tên file lưu trong Application.persistentDataPath.")]
        public string saveFileName = "save.json";

        [Header("Hồi sinh (FR-CMB-06: tổng ≤ 2 giây)")]
        [Tooltip("Thời gian đứng yên sau khi chết, trước khi màn hình chuyển trắng (giây, thời gian thực).")]
        [Min(0f)] public float deathDelay = 0.6f;

        [Tooltip("Thời gian màn hình chuyển trắng (tuyết) (giây, thời gian thực).")]
        [Min(0f)] public float deathFadeDuration = 0.4f;

        [Tooltip("Màu màn chuyển khi chết (UC-05: màn hình chuyển trắng).")]
        public Color deathFadeColor = Color.white;

        [Tooltip("Ngưỡng cảnh báo trong Console nếu từ lúc chết tới lúc điều khiển lại lâu hơn (giây).")]
        [Min(0.1f)] public float respawnWarningThreshold = 2f;

        [Header("Checkpoint (FR-LVL-04)")]
        [Tooltip("Thời gian hiện biểu tượng \"đã lưu\" ở checkpoint (giây).")]
        [Min(0f)] public float savedIndicatorDuration = 1.5f;

        [Tooltip("Sau khi kích hoạt checkpoint bằng cách chạm, bỏ qua lần chạm lại trong khoảng này (giây).")]
        [Min(0f)] public float checkpointRetriggerCooldown = 1f;
    }
}
