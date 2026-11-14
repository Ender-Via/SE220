using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Thông số camera và chuyển phòng (SRS FR-LVL-01, NFR-03, NFR-14).
    /// Chỉnh trong Inspector, không hardcode trong code hay scene.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraRoomSettings", menuName = "Woodcutter/World/CameraRoomSettings")]
    public class CameraRoomSettingsSO : ScriptableObject
    {
        [Header("Camera - khung hình")]
        [Tooltip("Nửa chiều cao khung nhìn tính bằng ô (1 ô = 1 unit = 16 px). " +
                 "11.25 = 22.5 ô chiều cao, pixel-perfect ở 720p (x2) và 1080p (x3).")]
        public float orthographicSize = 11.25f;

        [Tooltip("Vị trí nhân vật trên màn hình, (0,0) là chính giữa, đơn vị nửa màn hình.")]
        public Vector2 screenPosition = new Vector2(0f, -0.1f);

        [Header("Camera - bám theo")]
        [Tooltip("Độ trễ bám theo trục X/Y (giây). 0 = bám cứng.")]
        public Vector2 followDamping = new Vector2(0.3f, 0.5f);

        [Tooltip("Vùng chết (tỉ lệ màn hình) mà nhân vật di chuyển trong đó camera không theo.")]
        public Vector2 deadZoneSize = new Vector2(0.05f, 0.15f);

        [Tooltip("Nhìn trước theo hướng chạy (giây). 0 = tắt.")]
        [Min(0f)] public float lookaheadTime = 0.25f;

        [Tooltip("Làm mượt nhìn trước.")]
        [Range(0f, 30f)] public float lookaheadSmoothing = 8f;

        [Header("Camera - giới hạn trong phòng")]
        [Tooltip("Độ trễ khi camera chạm biên phòng (giây).")]
        [Min(0f)] public float confinerDamping = 0.2f;

        [Header("Chuyển phòng (SRS: tổng ≤ 1 giây)")]
        [Tooltip("Thời gian tối màn hình trước khi đổi phòng (giây, thời gian thực).")]
        [Min(0f)] public float fadeOutDuration = 0.15f;

        [Tooltip("Thời gian sáng màn hình sau khi đổi phòng (giây, thời gian thực).")]
        [Min(0f)] public float fadeInDuration = 0.2f;

        [Tooltip("Sau khi tới phòng mới, cửa bị bỏ qua trong khoảng này để tránh chuyển ngược ngay.")]
        [Min(0f)] public float doorCooldownAfterArrival = 0.3f;

        [Tooltip("Ngưỡng cảnh báo trong Console nếu một lần chuyển phòng mất lâu hơn (giây).")]
        [Min(0.1f)] public float transitionWarningThreshold = 1f;
    }
}
