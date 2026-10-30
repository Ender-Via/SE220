# CLAUDE.md – Tiều phu và những viên đá màu (tên tạm)

Tài liệu ngữ cảnh cho Claude Code. Đặt file này ở thư mục gốc của project Unity.
Tài liệu đầy đủ: SRS v1.0 (`Docs/SRS.md`), Game Design Overview (`Docs/GDD.md`), bảng phân công `Phân công.xlsx` (thư mục `Info/` ngoài repo).

## 1. Tổng quan dự án

- **Thể loại:** 2D Metroidvania ngắn, side-scroll, cận chiến bằng rìu.
- **Thời lượng chơi:** 30–40 phút. **Thời gian làm:** 8 tuần. **Nhóm:** 5 người.
- **Engine:** Unity 6.6 (6000.6.7f1), bản Recommended trên Hub, không phải LTS; dự án 2D với URP.
- **Nền tảng:** PC Windows 10/11, bàn phím + tay cầm XInput, 16:9 từ 1280×720.
- **Ngôn ngữ trong game:** tiếng Việt, mọi chuỗi chữ để trong file ngôn ngữ (tiếng Anh là P3).

**Pitch:** Thế giới chỉ còn trắng, đen và xanh băng nhạt. Một ông tiều phu nhặt được viên đá đỏ và mẩu giấy cháy dở, lần theo dấu những người phiêu lưu đã chết để thu 3 viên đá màu và trả màu cho thế giới.

## 2. Cốt truyện (tóm tắt)

- Cây cội nguồn có sức mạnh sinh ra muôn loài. Một nhóm phiêu lưu (có ông tiều phu) đến được cây, phá cây để lấy các viên đá năng lượng, rồi phản bội nhau. Phần lớn chết, vài người cầm đá chạy trốn.
- Ông tiều phu đứng ngoài cuộc, chỉ mang về **một khúc cây dưới gốc cây cội nguồn để làm cán rìu mới**. Từ đó thế giới mất màu, quái biến dị xuất hiện.
- Ông tìm thấy viên đá đỏ và mẩu giấy cháy dở trong một ngách nhỏ, rồi lên đường.
- Mẩu giấy là của những người đã chạy trốn và chết trên đường.
- **Ending:** đặt 3 viên đá vào phiến đá → cây chưa hồi sinh → tháo cán rìu trả về gốc cây → hồi màu toàn cảnh → credits.
- Trong game dùng từ "phản bội nhau", giữ không khí nghiêm túc.

## 3. Core loop

Chặt chém → Khám phá (theo mẩu giấy) → Nâng cấp (nhận viên đá = năng lực mới + hồi màu vùng đó) → Backtrack (mở lối cũ).
Không có cây kỹ năng, tiền tệ, nâng cấp rìu. Nguyên tắc: **màu sắc = tiến trình = năng lực**.

## 4. Cơ chế

**Di chuyển:** chạy, nhảy theo thời gian giữ phím, coyote time, jump buffer, lăn né có i-frame.
**Chiến đấu:** combo rìu 2 nhịp, đòn nặng (giữ phím, phá khiên), chém lên/xuống (P2). Game feel: hit-stop, rung màn hình (Cinemachine), nháy trắng.

| Viên đá | Năng lực | Mở đường | Vị trí |
|---|---|---|---|
| Đỏ (lửa) | Rìu lửa | Tan băng, đốt mạng nhện/dây leo, soi hang tối, thắp đống lửa | R1-06 |
| Vàng (ánh sáng) | Dash trên không, 1 lần mỗi lần rời đất | Khe xa 7–9 ô | Sau boss 1 (R2-06) |
| Xanh lá (sự sống) | Gieo hạt ở ô đất đánh dấu, mọc bệ cố định | Vách ≥ 5 ô | R3-02 |

Nếu năng lực xanh không kịp tuần 6: viên xanh thành phần thưởng ending, không phải năng lực.

**Không làm:** ném rìu, đỡ đòn, trượt tường, nâng cấp rìu, bản đồ, fast travel, chơi mạng.

## 5. Thông số khởi điểm (đặt trong ScriptableObject, chỉnh trong Inspector)

1 ô = 16 px, nhân vật cao 2 ô, máu tính theo vạch.

| Thông số | Giá trị |
|---|---|
| Tốc độ chạy | 8 ô/s, tăng tốc 0,08 s, giảm tốc 0,06 s |
| Nhảy | thấp 1,5 ô, cao 4 ô; rơi tối đa 18 ô/s |
| Coyote / buffer | 0,1 s / 0,1 s |
| Lăn | 4 ô trong 0,35 s; i-frame 0,25 s; hồi 0,5 s |
| Máu | 5 vạch, +1 mỗi viên đá (tối đa 8); i-frame khi bị đánh 1,0 s |
| Rìu | nhịp 1/2: 1 sát thương; cửa sổ combo 0,4 s; tầm 1,5×1,5 ô |
| Đòn nặng | nạp 0,6 s, 3 sát thương |
| Hit-stop | 0,05 s (thường) / 0,1 s (nặng) |
| Rìu lửa | 2 sát thương, hồi 1,5 s, x2 với quái băng |
| Dash | 5 ô trong 0,2 s |
| Gieo hạt | mọc 0,5 s, bệ cao 3 ô |

Quy tắc màn: khe nhảy thường ≤ 5 ô, bệ ≤ 3,5 ô, checkpoint cách nhau ≤ 3 phút chơi.

## 6. Quái và boss

Mọi đòn có báo hiệu ≥ 0,4 s. Quái hồi sinh khi rời phòng rồi quay lại.

| Quái | Khung AI | Khu | Máu | Ghi chú |
|---|---|---|---|---|
| Sói tuyết | Lao tới | 1 (+3) | 3 | Gầm rồi vồ |
| Nhện băng | Đi tuần | 1 | 2 | Rơi từ trần |
| Quạ xám | Bắn xa | 1 | 2 | Ném đá băng |
| Giáp băng | Đi tuần | 2 | 5 | Khiên trước, phá bằng đòn nặng/lửa |
| Dơi hang | Lao tới | 2 | 2 | Ngủ trong tối, tỉnh khi có lửa |
| Tinh linh sương | Bắn xa | 2–3 | 4 | Loạt 3 viên |

**Boss 1 – Người gác vách băng** (40 máu): đập tay, quét ngang, mưa băng; giai đoạn 2 (<50%) khoác giáp băng, chỉ rìu lửa phá được, nhanh hơn 20%.

**Boss cuối – Bóng của nhóm phiêu lưu** (30 + 30 máu), tái sử dụng code/VFX năng lực người chơi:
- Giai đoạn 1: khối bóng 3 cánh tay. Tay đỏ ném 3 cầu lửa; tay vàng dash xuyên màn hình; tay xanh rễ đâm lên; 2 tay giằng co nhau 2 s tạo cửa sổ sát thương.
- Giai đoạn 2: bóng ông tiều phu cầm rìu cán cây cội nguồn: combo 2 nhịp, đòn nặng cắm đất 1,5 s, lăn né ngẫu nhiên 30%. Bị hạ thì quỳ xuống, đặt rìu, tan biến → ending.

## 7. Màn chơi (18 phòng, bố cục chữ L ngược)

Khu 1 đi ngang sang phải, khu 2 đi xuống, khu 3 đi lên. Thế giới trắng đen + xanh băng; vùng hồi màu loang ra như mực thấm giấy.

- **Khu 1:** R1-01 Lều (CP) → R1-02 Bìa rừng → R1-03 Bãi săn → R1-04 Rừng chết (lối băng xuống hầm, cần đỏ) → R1-05 Thung lũng tuyết (phòng bí mật bằng dash, P2) → R1-06 Xác thám hiểm (đá đỏ).
- **Khu 2:** R2-01 Cửa hầm (CP) → R2-02 Đường hầm tối → R2-03 Hồ băng → R2-04 Giếng sâu → R2-05 Phòng chờ (CP) → R2-06 Boss 1 → R2-07 Vách đá (lối tắt về R1-01, lên khu 3 cần vàng).
- **Khu 3:** R3-01 Sườn núi → R3-02 Vườn đá (CP, đá xanh) → R3-03 Vách rễ (cần xanh) → R3-04 Chân cây (CP) → R3-05 Ngọn cây (boss cuối, ending).

## 8. Điều khiển

| Hành động | Bàn phím | Tay cầm |
|---|---|---|
| Di chuyển / nhìn lên-xuống | A/D, W/S | Cần trái |
| Nhảy | Space | A |
| Chém / giữ đòn nặng | J | X |
| Lăn | K | B |
| Rìu lửa | L | Y |
| Dash | Shift | RB |
| Gieo hạt | I | LB |
| Tương tác | E | D-pad lên |
| Tạm dừng | Esc | Start |

Dùng **Input System mới**.

## 9. Lưu game

1 slot, JSON qua `JsonUtility`, tại `Application.persistentDataPath`. Tự lưu khi chạm checkpoint, nhận đá, hạ boss. Ghi file tạm rồi đổi tên (an toàn khi tắt ngang). Settings lưu riêng `settings.json`. Trạng thái hồi màu suy ra từ `stones`.

```json
{
  "version": 1,
  "checkpoint_id": "R2-05",
  "max_hp": 6,
  "stones": ["red"],
  "notes": [1, 2],
  "opened_obstacles": ["R1-04_ice"],
  "shortcuts": [],
  "bosses_defeated": [],
  "secrets": [],
  "play_time": 812,
  "game_completed": false
}
```

## 10. Kỹ thuật và quy ước

- **URP 2D:** hồi màu bằng Shader Graph (mask grayscale theo bán kính từ tâm, 2–3 s); hang tối dùng 2D Lights.
- **Camera:** Cinemachine. Chuyển phòng ≤ 1 s.
- **Thông số gameplay:** luôn đọc từ ScriptableObject, không hardcode.
- **Yêu cầu phi chức năng chính:** 60 FPS, input < 100 ms, RAM ≤ 1 GB, không softlock, không phụ thuộc màu để truyền thông tin.
- **Git:** Git + Git LFS, `.gitignore` chuẩn Unity. Mỗi người làm trên scene/prefab riêng. Không commit thay đổi tự sinh trong `ProjectSettings/ProjectVersion.txt` hoặc `Packages/` do khác bản vá Unity.
- **Phiên bản:** cả nhóm dùng **Unity 6.6 (6000.6.7f1)** (số ghi trong `ProjectVersion.txt`). Build chính thức bằng máy có bản chuẩn. Không tự ý nâng cấp package.

## 11. Phân vai

| Mã | Vai trò | Phụ trách |
|---|---|---|
| LEAD | Trưởng nhóm + lập trình gameplay | Di chuyển, chiến đấu, năng lực, game feel, Git |
| SYS | Lập trình hệ thống + AI | Quái, boss, lưu/tải, UI, chuyển phòng, shader hồi màu |
| LVL | Thiết kế màn chơi + cốt truyện | 18 phòng, mẩu giấy, cân bằng, chơi thử |
| ARTC | Họa sĩ nhân vật | Tiều phu, quái, boss, animation, VFX |
| ARTE | Họa sĩ môi trường + âm thanh | Tileset, nền, UI, nhạc xếp lớp, SFX |

## 12. Lịch 8 tuần

| Tuần | Trọng tâm |
|---|---|
| 1 | Setup project/Git, style guide, di chuyển, ScriptableObject thông số, gray-box khu 1 |
| 2 | Lăn, combo rìu, game feel, camera + chuyển phòng |
| 3 | Máu/chết, 3 khung AI quái, checkpoint + lưu/tải, menu |
| 4 | Đá đỏ, vật cản theo năng lực, shader hồi màu, gray-box khu 2 |
| 5 | Dash, boss 1, HUD, lối tắt → **vertical slice gray-box** |
| 6 | Gieo hạt, boss cuối, ending, khu 3 → **feature freeze** |
| 7 | Art thật, nhạc xếp lớp, SFX, VFX, tùy chọn |
| 8 | Chơi thử 3–5 người, sửa lỗi, tối ưu, credits → **build phát hành** |

## 13. Gợi ý cho Claude Code khi làm việc

- Ưu tiên code đơn giản, dễ chỉnh trong Inspector; tách component nhỏ (PlayerMovement, PlayerCombat, AbilityController, Health, EnemyAI, SaveSystem...).
- Năng lực viên đá nên dùng chung interface để boss cuối tái sử dụng hiệu ứng.
- Không thêm tính năng ngoài phạm vi ở mục 4 mà chưa hỏi nhóm.
- Câu hỏi còn mở: tên game chính thức, nền tảng phát hành (itch.io/Steam), nguồn nhạc. Đã chốt Unity 6.6 (6000.6.7f1).

## 14. Lưu ý khi làm việc

- Không tự ý commit khi chưa cho phép.
- Không đẩy trực tiếp lên main khi chưa cho phép.
- Đọc các PR trước đó để làm việc nếu cần.
- Làm theo yêu cầu của CONTRIBUTING.md.
- Nếu làm lâu quá hãy báo mình vấn đề.
- Bám sát file SRS, nếu có vấn đề gì liên quan đến nội dung file đó hãy báo mình.
