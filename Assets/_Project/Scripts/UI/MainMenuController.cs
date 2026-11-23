using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Player;
using Woodcutter.World;

namespace Woodcutter.UI
{
    /// <summary>
    /// Menu chính (SRS FR-SYS-01, UC-01): Chơi mới, Tiếp tục, Tùy chọn, Thoát.
    /// - "Tiếp tục" mờ khi chưa có file lưu.
    /// - "Chơi mới" khi đã có file lưu thì hỏi xác nhận ghi đè.
    /// - File lưu hỏng thì thông báo và cho chơi mới.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Thông số")]
        [Tooltip("Dùng chung tên file lưu với GameProgress.")]
        [SerializeField] private SaveRespawnSettingsSO saveSettings;

        [Tooltip("Scene nền chứa Player + camera + RoomManager + GameProgress. " +
                 "Tạm thời là Sandbox_RoomTransition cho đến khi các phòng R1-xx có component Room.")]
        [SerializeField] private string gameSceneName = RoomManager.DefaultPersistentSceneName;

        [Header("Nút")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Header("Hộp thoại")]
        [SerializeField] private ConfirmDialog dialog;

        private bool isStarting;

        private void Awake()
        {
            if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            // Tùy chọn (FR-SYS-04) là P2: nút khóa cho đến khi có màn hình tùy chọn
            if (optionsButton != null) optionsButton.interactable = false;
        }

        private void Start()
        {
            // Về từ menu tạm dừng: đảm bảo thời gian và âm thanh chạy lại
            Time.timeScale = 1f;
            CharacterContext.HitStop.IsPaused = false;
            AudioListener.pause = false;

            SaveSystem.Configure(saveSettings != null ? saveSettings.saveFileName : SaveSystem.DefaultFileName);
            RefreshButtons();
            SelectDefault();
        }

        private void RefreshButtons()
        {
            if (continueButton != null) continueButton.interactable = SaveSystem.HasSave();
        }

        private void SelectDefault()
        {
            Button first = continueButton != null && continueButton.interactable ? continueButton : newGameButton;
            if (EventSystem.current != null && first != null)
                EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        private void OnNewGame()
        {
            if (isStarting) return;

            if (SaveSystem.HasSave() && dialog != null)
                dialog.Show("menu.confirm_overwrite", () => StartGame(GameStartMode.NewGame));
            else
                StartGame(GameStartMode.NewGame);
        }

        private void OnContinue()
        {
            if (isStarting) return;

            switch (SaveSystem.TryLoad(out _))
            {
                case SaveLoadResult.Ok:
                    StartGame(GameStartMode.Continue);
                    break;
                case SaveLoadResult.Corrupt:
                    if (dialog != null)
                        dialog.Show("menu.save_corrupt", () => StartGame(GameStartMode.NewGame));
                    else
                        StartGame(GameStartMode.NewGame);
                    break;
                default:
                    RefreshButtons();
                    SelectDefault();
                    break;
            }
        }

        private void StartGame(GameStartMode mode)
        {
            if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                Debug.LogError($"[MainMenu] Không nạp được scene '{gameSceneName}'. Thêm vào Build Profiles > Scene List.", this);
                return;
            }

            isStarting = true;
            SetButtonsInteractable(false);
            GameProgress.RequestStart(mode);
            SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
        }

        private void SetButtonsInteractable(bool value)
        {
            if (newGameButton != null) newGameButton.interactable = value;
            if (continueButton != null) continueButton.interactable = value;
            if (quitButton != null) quitButton.interactable = value;
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
