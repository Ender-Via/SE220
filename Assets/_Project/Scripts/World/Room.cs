using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Woodcutter.World
{
    /// <summary>
    /// Gốc của một phòng. Mỗi scene phòng (R1-01, R1-02...) có đúng một Room.
    /// Giữ biên camera và danh sách cửa để RoomManager đặt nhân vật đúng cửa (FR-LVL-01).
    /// </summary>
    public class Room : MonoBehaviour
    {
        [Tooltip("Mã phòng, phải trùng tên scene (ví dụ R1-01). Để trống = lấy tên scene.")]
        [SerializeField] private string roomId;

        [Tooltip("Collider giới hạn camera (BoxCollider2D hoặc PolygonCollider2D, đặt Is Trigger). " +
                 "Nên rộng/cao ít nhất bằng một màn hình.")]
        [SerializeField] private Collider2D cameraBounds;

        [Tooltip("Điểm xuất hiện khi vào phòng không qua cửa (bắt đầu game, mở thẳng scene để thử).")]
        [SerializeField] private Transform defaultSpawn;

        [Tooltip("Scene nền chứa Player + camera + RoomManager. Tự nạp thêm khi mở thẳng scene phòng này để thử.")]
        [SerializeField] private string persistentSceneName = "Sandbox_RoomTransition";

        private static readonly List<Room> loadedRooms = new List<Room>();
        private readonly Dictionary<string, RoomDoor> doorLookup = new Dictionary<string, RoomDoor>();

        /// <summary>Các phòng đang được nạp (thường chỉ có 1).</summary>
        public static IReadOnlyList<Room> LoadedRooms => loadedRooms;

        public string RoomId => string.IsNullOrEmpty(roomId) ? gameObject.scene.name : roomId;
        public Collider2D CameraBounds => cameraBounds;
        public Scene Scene => gameObject.scene;
        public string PersistentSceneName => persistentSceneName;

        public Vector2 DefaultSpawnPosition => defaultSpawn != null ? (Vector2)defaultSpawn.position : (Vector2)transform.position;

        private void Awake()
        {
            CacheDoors();
        }

        private void OnEnable()
        {
            if (!loadedRooms.Contains(this))
                loadedRooms.Add(this);
        }

        private void OnDisable()
        {
            loadedRooms.Remove(this);
        }

        private void Start()
        {
            // Mở thẳng scene phòng để thử (chưa có RoomManager) -> tự nạp scene nền có Player + camera.
            if (RoomManager.Instance == null)
                RoomManager.BootstrapFromRoom(this);
        }

        private void CacheDoors()
        {
            doorLookup.Clear();
            foreach (RoomDoor door in GetComponentsInChildren<RoomDoor>(true))
            {
                if (string.IsNullOrEmpty(door.DoorId))
                {
                    Debug.LogWarning($"[Room] Cửa '{door.name}' trong phòng {RoomId} chưa đặt Door Id.", door);
                    continue;
                }

                if (doorLookup.ContainsKey(door.DoorId))
                {
                    Debug.LogError($"[Room] Trùng Door Id '{door.DoorId}' trong phòng {RoomId}.", door);
                    continue;
                }

                doorLookup.Add(door.DoorId, door);
            }
        }

        public bool TryGetDoor(string doorId, out RoomDoor door)
        {
            if (doorLookup.Count == 0) CacheDoors();
            return doorLookup.TryGetValue(doorId ?? string.Empty, out door);
        }

        public static Room FindInScene(Scene scene)
        {
            for (int i = 0; i < loadedRooms.Count; i++)
            {
                if (loadedRooms[i] != null && loadedRooms[i].gameObject.scene == scene)
                    return loadedRooms[i];
            }
            return null;
        }

        private void OnValidate()
        {
            if (cameraBounds != null && !cameraBounds.isTrigger)
                Debug.LogWarning($"[Room] Camera Bounds của {name} nên bật Is Trigger để không chặn nhân vật.", this);
        }

        private void OnDrawGizmos()
        {
            if (cameraBounds == null) return;
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.6f);
            Bounds b = cameraBounds.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}
