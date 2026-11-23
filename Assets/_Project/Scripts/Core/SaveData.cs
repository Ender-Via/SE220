using System;
using System.Collections.Generic;

namespace Woodcutter.Core
{
    /// <summary>
    /// Cấu trúc file lưu game, đúng bảng SRS mục 9.
    /// Tên trường viết snake_case vì <see cref="UnityEngine.JsonUtility"/> ghi đúng tên trường ra JSON.
    /// Trạng thái hồi màu không lưu riêng: suy ra từ <see cref="stones"/>.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Phiên bản cấu trúc hiện tại. Tăng khi đổi cấu trúc và thêm bước nâng cấp trong SaveSystem.</summary>
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        /// <summary>Mã phòng có checkpoint hồi sinh, ví dụ "R2-05". Rỗng = chưa chạm checkpoint nào.</summary>
        public string checkpoint_id = string.Empty;

        /// <summary>Máu tối đa hiện tại. 0 = dùng máu khởi điểm trong PlayerStats.</summary>
        public int max_hp;

        /// <summary>Các viên đá đã có: "red", "yellow", "green".</summary>
        public List<string> stones = new List<string>();

        /// <summary>Số thứ tự các mẩu giấy đã nhặt.</summary>
        public List<int> notes = new List<int>();

        /// <summary>ID vật cản đã phá, ví dụ "R1-04_ice".</summary>
        public List<string> opened_obstacles = new List<string>();

        public List<string> shortcuts = new List<string>();
        public List<string> bosses_defeated = new List<string>();
        public List<string> secrets = new List<string>();

        /// <summary>Tổng thời gian chơi (giây).</summary>
        public float play_time;

        public bool game_completed;

        public bool HasCheckpoint => !string.IsNullOrEmpty(checkpoint_id);

        /// <summary>Đảm bảo không có danh sách null (file lưu thiếu trường vẫn đọc được).</summary>
        public void EnsureLists()
        {
            if (checkpoint_id == null) checkpoint_id = string.Empty;
            if (stones == null) stones = new List<string>();
            if (notes == null) notes = new List<int>();
            if (opened_obstacles == null) opened_obstacles = new List<string>();
            if (shortcuts == null) shortcuts = new List<string>();
            if (bosses_defeated == null) bosses_defeated = new List<string>();
            if (secrets == null) secrets = new List<string>();
        }
    }
}
