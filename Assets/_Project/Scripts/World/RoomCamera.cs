using Unity.Cinemachine;
using UnityEngine;
using Woodcutter.Data;

namespace Woodcutter.World
{
    /// <summary>
    /// Rig camera Cinemachine 3: bám theo nhân vật (PositionComposer) và giới hạn trong phòng (Confiner2D).
    /// Mọi thông số lấy từ <see cref="CameraRoomSettingsSO"/> (NFR-14).
    /// Rung màn hình (FR-CMB-04, T013) dùng CinemachineImpulseListener gắn sẵn trên cùng object.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class RoomCamera : MonoBehaviour
    {
        [SerializeField] private CameraRoomSettingsSO settings;
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private CinemachinePositionComposer positionComposer;
        [SerializeField] private CinemachineConfiner2D confiner;

        public CameraRoomSettingsSO Settings => settings;

        private void Awake()
        {
            if (virtualCamera == null) virtualCamera = GetComponent<CinemachineCamera>();
            if (positionComposer == null) positionComposer = GetComponent<CinemachinePositionComposer>();
            if (confiner == null) confiner = GetComponent<CinemachineConfiner2D>();
            if (settings == null) settings = ScriptableObject.CreateInstance<CameraRoomSettingsSO>();

            ApplySettings();
        }

        /// <summary>Đọc lại thông số từ ScriptableObject (gọi được lúc chạy để chỉnh thử trong Play Mode).</summary>
        [ContextMenu("Apply Settings")]
        public void ApplySettings()
        {
            if (virtualCamera != null)
            {
                LensSettings lens = virtualCamera.Lens;
                lens.OrthographicSize = settings.orthographicSize;
                virtualCamera.Lens = lens;
            }

            if (positionComposer != null)
            {
                positionComposer.Damping = new Vector3(settings.followDamping.x, settings.followDamping.y, 0f);

                ScreenComposerSettings composition = positionComposer.Composition;
                composition.ScreenPosition = settings.screenPosition;
                composition.DeadZone.Enabled = settings.deadZoneSize.sqrMagnitude > 0f;
                composition.DeadZone.Size = settings.deadZoneSize;
                positionComposer.Composition = composition;

                LookaheadSettings lookahead = positionComposer.Lookahead;
                lookahead.Enabled = settings.lookaheadTime > 0f;
                lookahead.Time = settings.lookaheadTime;
                lookahead.Smoothing = settings.lookaheadSmoothing;
                lookahead.IgnoreY = true; // Nhảy liên tục làm camera giật nếu nhìn trước theo trục Y
                positionComposer.Lookahead = lookahead;
            }

            if (confiner != null)
            {
                confiner.Damping = settings.confinerDamping;
                confiner.InvalidateLensCache();
            }
        }

        public void SetTarget(Transform target)
        {
            virtualCamera.Follow = target;
        }

        /// <summary>Đổi biên camera sang phòng mới. Biên nằm ở scene khác nên phải gán lúc chạy.</summary>
        public void SetBounds(Collider2D bounds)
        {
            if (confiner == null) return;

            confiner.BoundingShape2D = bounds;
            confiner.InvalidateBoundingShapeCache();
        }

        /// <summary>Bỏ qua damping ở khung hình tới: camera nhảy thẳng tới nhân vật thay vì trượt qua.</summary>
        public void SnapToTarget()
        {
            virtualCamera.PreviousStateIsValid = false;
        }
    }
}
