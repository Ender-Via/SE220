using UnityEngine;

namespace Woodcutter.Data
{
    /// <summary>
    /// Thông số cơ sở cho tất cả các loại quái thường và quái đặc biệt theo SRS mục 6.1.
    /// Cho phép Game Designer chỉnh sửa thông số trong Inspector mà không sửa code (NFR-14).
    /// </summary>
    public abstract class EnemyStatsSO : ScriptableObject
    {
        [Header("Health & Combat (SRS 6.1)")]
        [Tooltip("Máu tối đa của quái (Sói: 3, Nhện: 2, Quạ: 2, Giáp băng: 5, Dơi: 2, Tinh linh: 4)")]
        public int maxHp = 3;

        [Tooltip("Sát thương gây ra khi va chạm hoặc trúng đòn (SRS 6.1: mặc định 1)")]
        public int contactDamage = 1;

        [Tooltip("Quái thuộc hệ băng (nhận x2 sát thương từ rìu lửa theo SRS 5.2 & FR-ABL-01)")]
        public bool isIceEnemy = true;

        [Tooltip("Hệ số nhân sát thương từ rìu lửa đối với quái băng (SRS: 2.0x)")]
        public float iceDamageMultiplier = 2.0f;

        [Tooltip("Hệ số kháng đẩy lùi (0 = bị đẩy bình thường, 1 = không bị giật lùi)")]
        [Range(0f, 1f)]
        public float knockbackResistance = 0f;

        [Header("Movement & Detection")]
        [Tooltip("Tốc độ di chuyển cơ bản (ô/giây)")]
        public float moveSpeed = 3f;

        [Tooltip("Khoảng cách phát hiện người chơi (ô)")]
        public float detectRange = 6f;

        [Header("Telegraph & Attack Timing (SRS 6.1)")]
        [Tooltip("Thời gian báo hiệu tối thiểu trước khi ra đòn (SRS: >= 0.4s)")]
        public float telegraphDuration = 0.5f;

        [Tooltip("Thời gian khựng lại / nghỉ sau khi ra đòn (cửa sổ để người chơi phản đòn)")]
        public float recoverDuration = 0.8f;
    }
}
