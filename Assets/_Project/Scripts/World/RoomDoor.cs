using UnityEngine;

namespace Woodcutter.World
{
    /// <summary>
    /// Cửa/mép phòng. Nhân vật chạm trigger -> chuyển sang cửa <see cref="targetDoorId"/> của phòng <see cref="targetRoomId"/>.
    /// Mỗi cửa có điểm xuất hiện riêng, dùng khi nhân vật đi VÀO phòng này qua cửa này (FR-LVL-01).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RoomDoor : MonoBehaviour
    {
        [Tooltip("Mã cửa, duy nhất trong phòng. Ví dụ: Left, Right, Top, Bottom, Left_Upper.")]
        [SerializeField] private string doorId = "Left";

        [Tooltip("Tên scene phòng đích (ví dụ R1-02). Scene phải có trong Build Profiles.")]
        [SerializeField] private string targetRoomId;

        [Tooltip("Mã cửa ở phòng đích mà nhân vật sẽ xuất hiện.")]
        [SerializeField] private string targetDoorId = "Right";

        [Tooltip("Nơi nhân vật đứng khi đi vào phòng này qua cửa này. Đặt NGOÀI vùng trigger, cách mép 1–2 ô.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("Vận tốc đặt cho nhân vật khi xuất hiện ở cửa này (ô/giây). " +
                 "Dùng cho cửa ở sàn khi đi lên: đẩy nhân vật lên gờ bên cạnh để không rơi ngược lại.")]
        [SerializeField] private Vector2 arrivalVelocity = Vector2.zero;

        public string DoorId => doorId;
        public string TargetRoomId => targetRoomId;
        public string TargetDoorId => targetDoorId;
        public Vector2 ArrivalVelocity => arrivalVelocity;
        public Vector2 SpawnPosition => spawnPoint != null ? (Vector2)spawnPoint.position : (Vector2)transform.position;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            RoomManager manager = RoomManager.Instance;
            if (manager == null || !manager.IsPlayer(other))
                return;

            if (string.IsNullOrEmpty(targetRoomId))
            {
                Debug.LogWarning($"[RoomDoor] Cửa '{doorId}' ({gameObject.scene.name}) chưa đặt Target Room Id.", this);
                return;
            }

            manager.RequestTransition(this);
        }

        private void OnDrawGizmos()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.35f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }

            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.4f);
                Gizmos.DrawLine(transform.position, spawnPoint.position);
            }
        }
    }
}
