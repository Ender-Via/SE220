using System;
using System.Collections.Generic;
using UnityEngine;

namespace Woodcutter.UI
{
    /// <summary>
    /// Bảng chữ hiển thị theo ngôn ngữ (NFR-15): mọi chuỗi trong game lấy từ file
    /// <c>Assets/_Project/Localization/vi.json</c> theo khóa. Tiếng Anh (en.json) là P3.
    /// File dạng <c>{"entries":[{"key":"...","value":"..."}]}</c> vì JsonUtility không đọc được dictionary.
    /// </summary>
    public static class Localization
    {
        [Serializable]
        private class Entry
        {
            public string key;
            public string value;
        }

        [Serializable]
        private class Table
        {
            public List<Entry> entries = new List<Entry>();
        }

        private static Dictionary<string, string> strings;
        private static readonly HashSet<string> reportedMissing = new HashSet<string>();

        public static bool IsLoaded => strings != null;

        /// <summary>Bắn ra khi nạp file ngôn ngữ mới; LocalizedText tự cập nhật.</summary>
        public static event Action OnLanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            strings = null;
            reportedMissing.Clear();
            OnLanguageChanged = null;
        }

        public static void Load(TextAsset languageFile)
        {
            if (languageFile == null)
            {
                Debug.LogError("[Localization] Chưa gán file ngôn ngữ.");
                return;
            }

            var map = new Dictionary<string, string>();
            try
            {
                Table table = JsonUtility.FromJson<Table>(languageFile.text);
                if (table?.entries != null)
                {
                    foreach (Entry e in table.entries)
                    {
                        if (string.IsNullOrEmpty(e?.key)) continue;
                        if (map.ContainsKey(e.key))
                            Debug.LogWarning($"[Localization] Trùng khóa '{e.key}' trong {languageFile.name}.");
                        map[e.key] = e.value ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Localization] File {languageFile.name} sai định dạng: {ex.Message}");
            }

            strings = map;
            reportedMissing.Clear();
            OnLanguageChanged?.Invoke();
        }

        /// <summary>Lấy chuỗi theo khóa. Thiếu khóa thì trả về "#khóa" để dễ thấy khi chơi thử.</summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (strings != null && strings.TryGetValue(key, out string value)) return value;

            if (reportedMissing.Add(key))
                Debug.LogWarning($"[Localization] Thiếu khóa '{key}'{(strings == null ? " (chưa nạp file ngôn ngữ)" : string.Empty)}.");
            return "#" + key;
        }
    }
}
