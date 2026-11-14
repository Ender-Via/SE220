using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Woodcutter.Data;
using Woodcutter.UI;
using Debug = UnityEngine.Debug;

namespace Woodcutter.World
{
    /// <summary>
    /// Điều phối chuyển phòng (SRS FR-LVL-01, NFR-03: ≤ 1 giây).
    /// Nằm trong scene nền cùng Player + camera; mỗi phòng là một scene nạp kiểu Additive.
    /// Gỡ phòng cũ khi rời nên quái tự hồi sinh khi quay lại (SRS mục 6).
    /// Lưu/tải và hồi sinh ở checkpoint dùng lại <see cref="LoadRoom"/>.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [Header("Thông số")]
        [SerializeField] private CameraRoomSettingsSO settings;

        [Header("Tham chiếu trong scene nền")]
        [SerializeField] private RoomCamera roomCamera;
        [SerializeField] private ScreenFader fader;
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private PlayerInput playerInput;

        [Header("Phòng bắt đầu (khi không có phòng nào đang mở)")]
        [Tooltip("Tên scene phòng nạp khi vào game, ví dụ R1-01.")]
        [SerializeField] private string startRoomId;
        [Tooltip("Cửa xuất hiện ở phòng bắt đầu. Để trống = Default Spawn của phòng.")]
        [SerializeField] private string startDoorId;

        private static bool isBootstrapping;

        private float doorCooldownUntil;
        private Vector2 pendingArrivalVelocity;
        private readonly Stopwatch transitionTimer = new Stopwatch();

        public Room CurrentRoom { get; private set; }
        public bool IsTransitioning { get; private set; }

        /// <summary>Bắn ra mỗi khi nhân vật vào một phòng (dùng cho lưu game, nhạc, hồi màu...).</summary>
        public event Action<Room> OnRoomEntered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[RoomManager] Có hơn một RoomManager đang chạy. Phòng không được chứa Player/camera/RoomManager.", this);
                enabled = false;
                return;
            }

            Instance = this;
            isBootstrapping = false;

            if (settings == null && roomCamera != null) settings = roomCamera.Settings;
            if (settings == null) settings = ScriptableObject.CreateInstance<CameraRoomSettingsSO>();
            if (playerInput == null && playerBody != null) playerInput = playerBody.GetComponent<PlayerInput>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (roomCamera != null && playerBody != null)
                roomCamera.SetTarget(playerBody.transform);

            // Đã có phòng mở sẵn (mở thẳng scene phòng để thử) -> nhận phòng đó, không nạp phòng bắt đầu.
            if (Room.LoadedRooms.Count > 0)
            {
                Room existing = Room.LoadedRooms[0];
                SceneManager.SetActiveScene(existing.Scene);
                EnterRoom(existing, null);
                CompleteArrival();
                if (fader != null) fader.SetAlpha(0f);
                return;
            }

            if (!string.IsNullOrEmpty(startRoomId))
                LoadRoom(startRoomId, startDoorId);
            else
                Debug.LogWarning("[RoomManager] Chưa đặt Start Room Id và không có phòng nào đang mở.", this);
        }

        // ================================================================
        // API CÔNG KHAI
        // ================================================================

        public bool IsPlayer(Collider2D other)
        {
            return playerBody != null && other != null && other.attachedRigidbody == playerBody;
        }

        /// <summary>Gọi bởi <see cref="RoomDoor"/> khi nhân vật chạm cửa.</summary>
        public void RequestTransition(RoomDoor door)
        {
            if (IsTransitioning || Time.unscaledTime < doorCooldownUntil)
                return;

            LoadRoom(door.TargetRoomId, door.TargetDoorId);
        }

        /// <summary>
        /// Nạp phòng <paramref name="roomId"/> và đặt nhân vật ở cửa <paramref name="doorId"/>
        /// (để trống = Default Spawn). Dùng cho cửa, checkpoint, tải game.
        /// </summary>
        public bool LoadRoom(string roomId, string doorId)
        {
            if (IsTransitioning)
                return false;

            if (!Application.CanStreamedLevelBeLoaded(roomId))
            {
                Debug.LogError($"[RoomManager] Không nạp được scene '{roomId}'. Kiểm tra tên và thêm scene vào Build Profiles > Scene List.", this);
                return false;
            }

            StartCoroutine(TransitionRoutine(roomId, doorId));
            return true;
        }

        /// <summary>
        /// Mở thẳng một scene phòng trong Editor để thử: tự nạp thêm scene nền chứa Player + camera.
        /// </summary>
        public static void BootstrapFromRoom(Room room)
        {
            if (isBootstrapping || Instance != null || string.IsNullOrEmpty(room.PersistentSceneName))
                return;

            if (!Application.CanStreamedLevelBeLoaded(room.PersistentSceneName))
            {
                Debug.LogWarning($"[RoomManager] Không tìm thấy scene nền '{room.PersistentSceneName}' trong Build Profiles; phòng chạy không có Player.", room);
                return;
            }

            isBootstrapping = true;
            SceneManager.LoadScene(room.PersistentSceneName, LoadSceneMode.Additive);
        }

        // ================================================================
        // QUY TRÌNH CHUYỂN PHÒNG
        // ================================================================

        private IEnumerator TransitionRoutine(string roomId, string doorId)
        {
            IsTransitioning = true;
            transitionTimer.Restart();
            string fromRoom = CurrentRoom != null ? CurrentRoom.RoomId : "-";

            SetPlayerFrozen(true);
            ThreadPriority previousPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.High;

            // 1. Tối màn hình
            if (fader != null)
                yield return fader.FadeTo(1f, settings.fadeOutDuration);

            // 2. Gỡ phòng cũ (gỡ trước khi nạp để hai phòng không chồng lên nhau)
            Room oldRoom = CurrentRoom;
            CurrentRoom = null;
            if (oldRoom != null)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(oldRoom.Scene);
                if (unload != null)
                    yield return unload;
            }

            // 3. Nạp phòng mới
            AsyncOperation load = SceneManager.LoadSceneAsync(roomId, LoadSceneMode.Additive);
            yield return load;

            Scene scene = SceneManager.GetSceneByName(roomId);
            Room newRoom = scene.IsValid() ? Room.FindInScene(scene) : null;
            Application.backgroundLoadingPriority = previousPriority;

            if (newRoom == null)
            {
                Debug.LogError($"[RoomManager] Scene '{roomId}' không có component Room.", this);
                if (fader != null) fader.SetAlpha(0f);
                SetPlayerFrozen(false);
                IsTransitioning = false;
                yield break;
            }

            SceneManager.SetActiveScene(scene); // Object sinh ra lúc chạy (quái, hiệu ứng) thuộc phòng, gỡ theo phòng

            // 4. Đặt nhân vật đúng cửa, đổi biên camera, cho camera nhảy thẳng tới
            EnterRoom(newRoom, doorId);

            // Chờ 1 khung hình để Cinemachine cập nhật vị trí mới trước khi sáng màn hình
            yield return null;

            // 5. Thả nhân vật ngay khi bắt đầu sáng để không tăng độ trễ input
            CompleteArrival();
            if (fader != null)
                yield return fader.FadeTo(0f, settings.fadeInDuration);

            transitionTimer.Stop();
            IsTransitioning = false;

            float seconds = (float)transitionTimer.Elapsed.TotalSeconds;
            if (seconds > settings.transitionWarningThreshold)
                Debug.LogWarning($"[RoomManager] Chuyển {fromRoom} -> {roomId} mất {seconds:0.000}s, vượt ngưỡng {settings.transitionWarningThreshold}s (FR-LVL-01).", this);
            else
                Debug.Log($"[RoomManager] Chuyển {fromRoom} -> {roomId} ({doorId}) mất {seconds:0.000}s.", this);
        }

        private void EnterRoom(Room room, string doorId)
        {
            Vector2 position = room.DefaultSpawnPosition;
            pendingArrivalVelocity = Vector2.zero;

            if (!string.IsNullOrEmpty(doorId))
            {
                if (room.TryGetDoor(doorId, out RoomDoor door))
                {
                    position = door.SpawnPosition;
                    pendingArrivalVelocity = door.ArrivalVelocity;
                }
                else
                {
                    Debug.LogError($"[RoomManager] Phòng {room.RoomId} không có cửa '{doorId}'. Đặt nhân vật ở Default Spawn.", room);
                }
            }

            if (playerBody != null)
            {
                playerBody.position = position;
                playerBody.transform.position = new Vector3(position.x, position.y, playerBody.transform.position.z);
            }

            if (roomCamera != null)
            {
                roomCamera.SetBounds(room.CameraBounds);
                roomCamera.SnapToTarget();
            }

            CurrentRoom = room;
            OnRoomEntered?.Invoke(room);
        }

        private void CompleteArrival()
        {
            SetPlayerFrozen(false);
            if (playerBody != null)
                playerBody.linearVelocity = pendingArrivalVelocity;

            doorCooldownUntil = Time.unscaledTime + settings.doorCooldownAfterArrival;
        }

        private void SetPlayerFrozen(bool frozen)
        {
            if (playerBody != null)
            {
                if (frozen) playerBody.linearVelocity = Vector2.zero;
                playerBody.simulated = !frozen;
            }

            if (playerInput != null)
            {
                if (frozen) playerInput.DeactivateInput();
                else playerInput.ActivateInput();
            }
        }
    }
}
