using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Woodcutter.UI
{
    /// <summary>
    /// Hộp thoại Có/Không dùng chung cho menu chính và menu tạm dừng
    /// (hỏi ghi đè file lưu UC-01, báo file lưu hỏng, xác nhận về menu).
    /// Mặc định chọn sẵn "Không" để bấm nhầm không mất tiến trình.
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private LocalizedText message;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action onYes;
        private Action onNo;
        private GameObject selectionBeforeOpen;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (root == null) root = gameObject;
            if (yesButton != null) yesButton.onClick.AddListener(HandleYes);
            if (noButton != null) noButton.onClick.AddListener(HandleNo);
            root.SetActive(false);
        }

        /// <summary>Mở hộp thoại với chuỗi theo khóa <paramref name="messageKey"/>.</summary>
        public void Show(string messageKey, Action yes, Action no = null)
        {
            onYes = yes;
            onNo = no;
            if (message != null) message.SetKey(messageKey);

            selectionBeforeOpen = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            root.SetActive(true);
            Select(noButton != null ? noButton.gameObject : yesButton != null ? yesButton.gameObject : null);
        }

        /// <summary>Đóng như bấm "Không" (dùng cho phím quay lại / tạm dừng).</summary>
        public void Cancel()
        {
            if (IsOpen) HandleNo();
        }

        private void HandleYes()
        {
            Action callback = onYes;
            Close();
            callback?.Invoke();
        }

        private void HandleNo()
        {
            Action callback = onNo;
            Close();
            Select(selectionBeforeOpen);
            callback?.Invoke();
        }

        private void Close()
        {
            onYes = null;
            onNo = null;
            root.SetActive(false);
        }

        private static void Select(GameObject target)
        {
            if (EventSystem.current != null && target != null && target.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(target);
        }
    }
}
