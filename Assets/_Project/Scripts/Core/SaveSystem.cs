using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Woodcutter.Core
{
    public enum SaveLoadResult
    {
        /// <summary>Đọc file lưu thành công.</summary>
        Ok,
        /// <summary>Chưa có file lưu.</summary>
        NoSave,
        /// <summary>File lưu hỏng hoặc không đọc được (UC-01: thông báo và cho chơi mới).</summary>
        Corrupt
    }

    /// <summary>
    /// Đọc/ghi file lưu 1 slot bằng JsonUtility tại Application.persistentDataPath (SRS FR-SYS-06, mục 9).
    /// Ghi vào file tạm rồi đổi tên để tắt game giữa chừng không làm hỏng file (NFR-07).
    /// Không giữ trạng thái: dữ liệu đang chơi nằm trong <c>GameProgress</c>.
    /// </summary>
    public static class SaveSystem
    {
        public const string DefaultFileName = "save.json";
        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        private static string fileName = DefaultFileName;

        /// <summary>Đổi tên file lưu (đọc từ SaveRespawnSettings). Chỉ nhận tên file, không nhận đường dẫn.</summary>
        public static void Configure(string saveFileName)
        {
            fileName = string.IsNullOrWhiteSpace(saveFileName) ? DefaultFileName : Path.GetFileName(saveFileName.Trim());
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, fileName);
        private static string TempPath => SavePath + TempSuffix;
        private static string BackupPath => SavePath + BackupSuffix;

        public static bool HasSave() => File.Exists(SavePath);

        /// <summary>Đọc file lưu. Trả về <see cref="SaveLoadResult.Corrupt"/> thay vì ném lỗi.</summary>
        public static SaveLoadResult TryLoad(out SaveData data)
        {
            data = null;
            if (!File.Exists(SavePath))
                return SaveLoadResult.NoSave;

            try
            {
                string json = File.ReadAllText(SavePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning($"[SaveSystem] File lưu rỗng: {SavePath}");
                    return SaveLoadResult.Corrupt;
                }

                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                if (loaded == null || loaded.version < 1 || loaded.version > SaveData.CurrentVersion)
                {
                    Debug.LogWarning($"[SaveSystem] File lưu không hợp lệ hoặc sai phiên bản ({loaded?.version}): {SavePath}");
                    return SaveLoadResult.Corrupt;
                }

                loaded.EnsureLists();
                if (loaded.play_time < 0f) loaded.play_time = 0f;
                data = loaded;
                return SaveLoadResult.Ok;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Không đọc được file lưu {SavePath}: {e.Message}");
                return SaveLoadResult.Corrupt;
            }
        }

        /// <summary>Ghi file lưu: ghi file tạm, đẩy xuống đĩa, rồi thay file chính.</summary>
        public static bool Write(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[SaveSystem] Không ghi được: dữ liệu null.");
                return false;
            }

            data.version = SaveData.CurrentVersion;
            data.EnsureLists();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(data, true));

                using (var stream = new FileStream(TempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(SavePath))
                {
                    // Thay nguyên tử trên cùng ổ đĩa; giữ bản cũ làm .bak rồi xóa
                    File.Replace(TempPath, SavePath, BackupPath, true);
                    TryDelete(BackupPath);
                }
                else
                {
                    File.Move(TempPath, SavePath);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ghi file lưu thất bại ({SavePath}): {e.Message}");
                TryDelete(TempPath);
                return false;
            }
        }

        /// <summary>Xóa file lưu (chơi mới ghi đè, công cụ debug).</summary>
        public static void Delete()
        {
            TryDelete(SavePath);
            TryDelete(TempPath);
            TryDelete(BackupPath);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Không xóa được {path}: {e.Message}");
            }
        }
    }
}
