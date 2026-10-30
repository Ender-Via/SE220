# Hướng dẫn đóng góp

Áp dụng cho cả nhóm và Claude Code. Ngữ cảnh dự án: [CLAUDE.md](CLAUDE.md), yêu cầu: [Docs/SRS.md](Docs/SRS.md).

## 1. Cài đặt lần đầu

1. Cài **Unity 6.6 (6000.6.7f1)** (đúng số trong `ProjectSettings/ProjectVersion.txt`) và Git LFS.
2. Clone repo, rồi chạy một lần:
   ```bash
   git lfs install
   ```
3. Unity Hub → **Add → Add project from disk** → chọn thư mục repo.
4. Không đổi phiên bản Unity, không thêm/gỡ/nâng package khi chưa thống nhất với nhóm.

## 2. Nhánh

- `main`: luôn mở được và chạy được. Chỉ nhận thay đổi qua Pull Request, cần ≥ 1 người duyệt.
- Nhánh tính năng: `<vai>/<mã-SRS>-<mô-tả-ngắn>`
  - Vai: `lead`, `sys`, `lvl`, `artc`, `arte`
  - Ví dụ: `lead/MOV-03-coyote-time`, `sys/SYS-05-save-load`, `lvl/LVL-02-greybox-khu1`
- Mã yêu cầu lấy theo SRS mục 3 (MOV, CMB, ABL, LVL, NAR, AUD, SYS). Việc không có mã thì bỏ phần mã.

## 3. Commit

Dạng: `<loại>(<phạm vi>): <mô tả> [<mã-SRS>]`

- Loại: `feat`, `fix`, `refactor`, `art`, `audio`, `level`, `docs`, `chore`
- Ví dụ: `feat(player): thêm coyote time và jump buffer [MOV-03]`
- Commit nhỏ, mỗi commit một ý. Luôn commit kèm file `.meta`.

## 4. Pull Request

Mỗi PR một tính năng. Mô tả PR gồm: mã SRS, đã làm gì, cách test, ảnh/GIF nếu có thay đổi nhìn thấy được.

Checklist trước khi tạo PR:

- [ ] Project mở không lỗi Console, game chạy được.
- [ ] Không có thay đổi trong `ProjectSettings/ProjectVersion.txt` hay `Packages/` (trừ khi nhóm đã đồng ý).
- [ ] Thông số gameplay đọc từ ScriptableObject, không hardcode.
- [ ] Không sửa scene/prefab của người khác khi chưa báo.
- [ ] Chuỗi chữ hiển thị trong game nằm trong file ngôn ngữ (`Assets/_Project/Localization`).
- [ ] Đã kèm đủ file `.meta`, không có file thừa trong `Library/`, `Temp/`, `Logs/`.

## 5. Cấu trúc thư mục

```
Assets/
├─ _Project/          Toàn bộ nội dung của nhóm
│  ├─ Scripts/        Core, Player, Abilities, Enemies, Bosses, World, UI, Data, Editor
│  ├─ Data/           File .asset ScriptableObject (thông số chỉnh trong Inspector)
│  ├─ Prefabs/        Player, Enemies, Bosses, Interactables, UI
│  ├─ Scenes/         Boot, MainMenu; Rooms/ (R1-01 … R3-05); Sandbox/ (thử riêng)
│  ├─ Art/            Characters, Environment, VFX, UI
│  ├─ Audio/          Music, SFX
│  ├─ Shaders/        Shader Graph (hồi màu)
│  ├─ Input/          Input Actions
│  └─ Localization/   vi.json (en.json là P3)
├─ Settings/          Cài đặt URP do template tạo
└─ ThirdParty/        Asset tải về, không sửa trực tiếp
Docs/                 SRS, GDD, style guide
```

## 6. Quy ước đặt tên

- Tên file, class, thư mục bằng **tiếng Anh**, PascalCase: `PlayerMovement.cs`, `PlayerStats.asset`.
- Namespace: `Woodcutter.<Thư mục>` (ví dụ `Woodcutter.Player`).
- Scene phòng đặt theo mã phòng: `R1-01.unity`. Scene thử riêng: `Sandbox_<Tên>.unity`.
- Prefab: `PF_<Tên>`, ScriptableObject dữ liệu: `<Tên>Stats` / `<Tên>Data`.
- Sprite: `SPR_<Đối tượng>_<Hành động>`, âm thanh: `SFX_<Tên>`, `MUS_<Tên>`.

## 7. Tránh xung đột trong Unity

- **Mỗi người làm trên scene/prefab riêng.** Muốn sửa scene/prefab dùng chung thì báo nhóm trước, hoặc khóa file:
  ```bash
  git lfs lock "Assets/_Project/Scenes/Rooms/R1-01.unity"
  ```
  Sửa xong thì `git lfs unlock` cùng đường dẫn.
- Thử nghiệm trong `Scenes/Sandbox/` của mình, không thử trong scene phòng.
- Pull `main` thường xuyên trước khi bắt đầu việc mới.
- Cấu hình Smart Merge (UnityYAMLMerge) cho Git để merge scene/prefab, xem tài liệu Unity “Smart merge”.

## 8. Nhịp làm việc

- Đầu tuần: nhận việc theo bảng phân công và lịch 8 tuần trong CLAUDE.md mục 12.
- Cuối tuần: merge các PR đã duyệt vào `main`, cả nhóm chơi thử bản build.
- Vướng quá nửa ngày thì báo nhóm.

## 9. Làm việc với Claude Code

- Mỗi phiên giao một mã yêu cầu SRS cụ thể, làm trên nhánh riêng.
- Claude không tự commit/push và không đẩy lên `main` khi chưa được cho phép.
- Không thêm tính năng ngoài phạm vi SRS khi chưa hỏi nhóm.
