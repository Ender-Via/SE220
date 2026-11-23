using UnityEngine;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Bảng ghi chú tiếng Việt treo tại một điểm trong thế giới
    /// (DemoUI vẽ). Có thể kiêm luôn checkpoint: người chơi đi qua thì cập nhật điểm hồi sinh.
    /// </summary>
    public class DemoSign : MonoBehaviour
    {
        public string title;
        [TextArea(3, 12)] public string body;
        public float width = 420f;
        [Tooltip("Đi qua vị trí này (theo trục X) thì lưu làm điểm hồi sinh.")]
        public bool isCheckpoint;

        DemoPlayerHealth player;
        bool reached;

        void Start()
        {
            DemoUI.Register(this);
            player = FindAnyObjectByType<DemoPlayerHealth>();
        }

        void Update()
        {
            if (!isCheckpoint || reached || player == null) return;
            if (player.transform.position.x >= transform.position.x - 1f)
            {
                reached = true;
                player.RespawnPoint = new Vector3(transform.position.x, player.transform.position.y + 0.5f, 0f);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = isCheckpoint ? Color.cyan : Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
