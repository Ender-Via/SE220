using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Woodcutter.Player;
using Woodcutter.World;

namespace Woodcutter.UI
{
    /// <summary>
    /// Menu tạm dừng (SRS FR-SYS-02): Tiếp tục, Mẩu giấy, Tùy chọn, Về menu. Game dừng hoàn toàn khi mở:
    /// Time.timeScale = 0, khóa hit-stop, dừng âm thanh, tắt input nhân vật, không cộng giờ chơi.
    /// Mở/đóng bằng action Pause (Esc / Start) — dùng bản sao của action nên vẫn nghe được khi input nhân vật đã tắt.
    /// Không mở được khi đang chuyển phòng, đang chết hoặc hồi sinh.
    /// Đặt trong scene nền (prefab PF_PauseMenu).
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Giao diện")]
        [SerializeField] private GameObject root;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button notesButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private ConfirmDialog dialog;

        [Header("Scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Tham chiếu (để trống = tự tìm trong scene)")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private RespawnController respawnController;

        private InputAction pauseToggle;
        private bool isLeaving;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (playerInput == null) playerInput = FindAnyObjectByType<PlayerInput>();
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (respawnController == null) respawnController = FindAnyObjectByType<RespawnController>();

            InputAction source = playerInput != null && playerInput.actions != null
                ? playerInput.actions.FindAction("Pause", false)
                : null;
            if (source != null)
            {
                pauseToggle = source.Clone();
            }
            else
            {
                Debug.LogWarning("[PauseMenu] Không tìm thấy action Pause trong PlayerInput, dùng Esc / Start mặc định.", this);
                pauseToggle = new InputAction("PauseToggle", InputActionType.Button);
                pauseToggle.AddBinding("<Keyboard>/escape");
                pauseToggle.AddBinding("<Gamepad>/start");
            }

            if (resumeButton != null) resumeButton.onClick.AddListener(Close);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
            // Mẩu giấy (FR-NAR-01) và Tùy chọn (FR-SYS-04) chưa có: khóa nút
            if (notesButton != null) notesButton.interactable = false;
            if (optionsButton != null) optionsButton.interactable = false;

            if (root != null) root.SetActive(false);
        }

        private void OnEnable()
        {
            pauseToggle.performed += HandlePauseToggle;
            pauseToggle.Enable();
        }

        private void OnDisable()
        {
            pauseToggle.performed -= HandlePauseToggle;
            pauseToggle.Disable();
            if (IsOpen) RestoreGameState();
        }

        private void OnDestroy()
        {
            pauseToggle?.Dispose();
        }

        private void Update()
        {
            // Hit-stop hoặc hệ thống khác có thể đặt lại timeScale: giữ game dừng hẳn khi đang mở
            if (IsOpen && Time.timeScale != 0f)
                Time.timeScale = 0f;
        }

        private void HandlePauseToggle(InputAction.CallbackContext ctx)
        {
            if (isLeaving) return;

            if (!IsOpen)
            {
                Open();
            }
            else if (dialog != null && dialog.IsOpen)
            {
                dialog.Cancel();
            }
            else
            {
                Close();
            }
        }

        private bool CanPause()
        {
            RoomManager manager = RoomManager.Instance;
            if (manager != null && manager.IsTransitioning) return false;
            if (respawnController != null && respawnController.IsRespawning) return false;
            if (playerHealth != null && playerHealth.IsDead) return false;
            return true;
        }

        public void Open()
        {
            if (IsOpen || !CanPause()) return;

            IsOpen = true;
            Time.timeScale = 0f;
            CharacterContext.HitStop.IsPaused = true;
            AudioListener.pause = true;
            if (GameProgress.Instance != null) GameProgress.Instance.IsPaused = true;
            if (playerInput != null) playerInput.DeactivateInput();

            if (root != null) root.SetActive(true);
            if (EventSystem.current != null && resumeButton != null)
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (dialog != null && dialog.IsOpen) dialog.Cancel();
            if (root != null) root.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            RestoreGameState();
        }

        private void RestoreGameState()
        {
            IsOpen = false;
            CharacterContext.HitStop.IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (GameProgress.Instance != null) GameProgress.Instance.IsPaused = false;
            if (playerInput != null && !isLeaving) playerInput.ActivateInput();
        }

        private void OnMainMenu()
        {
            if (dialog != null)
                dialog.Show("pause.confirm_main_menu", GoToMainMenu);
            else
                GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogError($"[PauseMenu] Không nạp được scene '{mainMenuSceneName}'. Thêm vào Build Profiles > Scene List.", this);
                return;
            }

            isLeaving = true;
            RestoreGameState();
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }
    }
}
