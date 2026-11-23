using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.World
{
    /// <summary>
    /// Checkpoint trong phòng (SRS FR-LVL-04): hồi đầy máu, đặt điểm hồi sinh, tự lưu game.
    /// Kích hoạt khi nhân vật chạm vào (SRS mục 9: "tự ghi khi chạm checkpoint"),
    /// và khi đứng trong vùng bấm Tương tác (FR-LVL-04: "tương tác tại checkpoint ghi file lưu").
    /// checkpoint_id = mã phòng, nên mỗi phòng tối đa 1 checkpoint.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip("Nơi nhân vật xuất hiện khi hồi sinh / tải game. Để trống = vị trí checkpoint.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("Biểu tượng hiện ngắn khi đã lưu (không dùng chữ, NFR-12: có hình dạng riêng).")]
        [SerializeField] private GameObject savedIndicator;

        [Tooltip("Đối tượng bật khi checkpoint này đang là điểm hồi sinh (ví dụ ngọn lửa). Có thể để trống.")]
        [SerializeField] private GameObject activeVisual;

        private PlayerHealth playerInside;
        private PlayerInputBroker inputInside;
        private float retriggerAllowedAt;
        private float hideIndicatorAt = -1f;

        public Vector2 SpawnPosition => spawnPoint != null ? (Vector2)spawnPoint.position : (Vector2)transform.position;
        public string CheckpointId => gameObject.scene.IsValid() ? gameObject.scene.name : name;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            if (savedIndicator != null) savedIndicator.SetActive(false);
            RefreshActiveVisual();
        }

        private void OnDisable()
        {
            UnbindInput();
            playerInside = null;
        }

        private void Update()
        {
            if (hideIndicatorAt >= 0f && Time.unscaledTime >= hideIndicatorAt)
            {
                hideIndicatorAt = -1f;
                if (savedIndicator != null) savedIndicator.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            RoomManager manager = RoomManager.Instance;
            if (manager == null || !manager.IsPlayer(other) || other.attachedRigidbody == null)
                return;

            playerInside = other.attachedRigidbody.GetComponent<PlayerHealth>();
            UnbindInput();
            inputInside = other.attachedRigidbody.GetComponent<PlayerInputBroker>();
            if (inputInside != null) inputInside.OnInteractPressed += HandleInteract;

            // Hồi sinh tại đây thì nhân vật xuất hiện trong vùng trigger: không lưu lại lần nữa
            if (manager.IsTransitioning || Time.unscaledTime < retriggerAllowedAt)
                return;

            Activate();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.attachedRigidbody == null || playerInside == null ||
                other.attachedRigidbody.gameObject != playerInside.gameObject)
                return;

            UnbindInput();
            playerInside = null;
        }

        private void HandleInteract()
        {
            if (playerInside != null && !playerInside.IsDead)
                Activate();
        }

        /// <summary>Hồi đầy máu, đặt checkpoint, tự lưu.</summary>
        public void Activate()
        {
            GameProgress progress = GameProgress.Instance;
            float cooldown = progress != null && progress.Settings != null ? progress.Settings.checkpointRetriggerCooldown : 1f;
            retriggerAllowedAt = Time.unscaledTime + cooldown;

            if (playerInside != null && !playerInside.IsDead)
                playerInside.Heal(playerInside.MaxHp);

            if (progress == null)
            {
                Debug.LogWarning("[Checkpoint] Scene nền chưa có GameProgress: không lưu được.", this);
                return;
            }

            bool ok = progress.SetCheckpointAndSave(CheckpointId);
            RefreshActiveVisual();

            if (ok && savedIndicator != null)
            {
                savedIndicator.SetActive(true);
                hideIndicatorAt = Time.unscaledTime + (progress.Settings != null ? progress.Settings.savedIndicatorDuration : 1.5f);
            }
        }

        private void RefreshActiveVisual()
        {
            if (activeVisual == null) return;
            GameProgress progress = GameProgress.Instance;
            activeVisual.SetActive(progress != null && progress.CheckpointRoomId == CheckpointId);
        }

        private void UnbindInput()
        {
            if (inputInside != null) inputInside.OnInteractPressed -= HandleInteract;
            inputInside = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.8f);
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 2f, 0f));
            Gizmos.DrawWireSphere(SpawnPosition, 0.3f);
        }
    }
}
