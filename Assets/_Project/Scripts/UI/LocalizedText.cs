using UnityEngine;
using UnityEngine.UI;

namespace Woodcutter.UI
{
    /// <summary>
    /// Gắn vào UI Text: hiển thị chuỗi theo khóa trong file ngôn ngữ (NFR-15).
    /// Nếu bảng chữ chưa nạp, tự nạp từ <see cref="languageFile"/>.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("Khóa trong file ngôn ngữ, ví dụ menu.new_game.")]
        [SerializeField] private string key;

        [Tooltip("File ngôn ngữ mặc định (Assets/_Project/Localization/vi.json), dùng khi chưa nạp.")]
        [SerializeField] private TextAsset languageFile;

        private Text label;

        public string Key => key;

        private void Awake()
        {
            label = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (!Localization.IsLoaded && languageFile != null)
                Localization.Load(languageFile);

            Localization.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Localization.OnLanguageChanged -= Refresh;
        }

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        public void Refresh()
        {
            if (label == null) label = GetComponent<Text>();
            if (!string.IsNullOrEmpty(key))
                label.text = Localization.Get(key);
        }
    }
}
