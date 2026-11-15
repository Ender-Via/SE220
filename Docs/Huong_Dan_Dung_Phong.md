# Hướng dẫn dựng phòng và nối cửa (T014 / FR-LVL-01)

Dành cho LVL (T009 gray-box khu 1 và các khu sau). Phần code camera + chuyển phòng đã có sẵn, phòng chỉ cần dựng đúng quy tắc dưới đây là chạy.

## 1. Cách hệ thống hoạt động

- **Scene nền** (hiện là `Scenes/Sandbox/Sandbox_RoomTransition.unity`) chứa Player, Main Camera + Cinemachine, màn fade và `RoomManager`. Scene này luôn mở.
- **Mỗi phòng là một scene riêng** (`Scenes/Rooms/R1-01.unity`...), nạp thêm (Additive). Khi đi qua cửa: tối màn hình → gỡ phòng cũ → nạp phòng mới → đặt nhân vật ở cửa tương ứng → sáng màn hình. Mục tiêu ≤ 1 giây, thời gian thực tế in ra Console mỗi lần chuyển.
- Phòng cũ bị gỡ nên quái tự hồi sinh khi quay lại (SRS mục 6).
- Thông số camera và thời gian fade nằm trong `Data/CameraRoomSettings.asset`, chỉnh trong Inspector.

## 2. Dựng một phòng mới

1. Tạo scene trống, lưu tên đúng mã phòng: `Assets/_Project/Scenes/Rooms/R1-02.unity`.
2. Kéo `Prefabs/World/PF_RoomTemplate` vào scene, đặt ở (0, 0). Mỗi phòng có hệ tọa độ riêng, góc dưới trái là (0, 0).
3. Chỉnh `CameraBounds` (BoxCollider2D, Is Trigger) cho bao đúng khu vực camera được phép nhìn.
4. Dựng tilemap/khối trong `Geometry`, layer **Ground**.
5. Đặt `DefaultSpawn` ở chỗ đứng an toàn (dùng khi mở thẳng scene để thử, hoặc khi tải game).
6. Thêm scene vào **File > Build Profiles > Scene List** (bắt buộc, nếu không sẽ không nạp được).
7. **Không** đặt Player, Main Camera, Cinemachine Camera, RoomManager hay Global Light trong scene phòng.

**Thử nhanh:** mở thẳng scene phòng rồi bấm Play. Phòng tự nạp scene nền (Player + camera) và đặt nhân vật ở `DefaultSpawn`. Scene nền cũng phải có trong Build Profiles.

## 3. Nối cửa

Mỗi cửa là một `RoomDoor` (prefab `PF_RoomDoor`) gồm trigger + `SpawnPoint` con.

| Trường | Ý nghĩa |
|---|---|
| Door Id | Tên cửa, duy nhất trong phòng: `Left`, `Right`, `Top`, `Bottom`, `Left_Upper`... |
| Target Room Id | Tên scene phòng đích, ví dụ `R1-03` |
| Target Door Id | Door Id của cửa bên phòng đích |
| Spawn Point | Chỗ nhân vật xuất hiện khi **đi vào** phòng này qua cửa này |
| Arrival Velocity | Vận tốc lúc xuất hiện (ô/giây), thường để 0 |

Cửa luôn khai báo **hai chiều**. Ví dụ R1-01 ↔ R1-02:

- R1-01 / `Right` → Target `R1-02`, Door `Left`
- R1-02 / `Left` → Target `R1-01`, Door `Right`

Trong phòng có lối tắt một chiều (R2-07 → R1-01) vẫn khai báo cửa ở cả hai phòng; chặn chiều ngược bằng vật cản (FR-LVL-03), không xóa cửa.

## 4. Quy tắc bắt buộc

- **Phòng tối thiểu bằng 1 màn hình: 20 × 11,25 ô** (ortho size 5,625 với PPU 32, màn hình tham chiếu 640×360). Phòng nhỏ hơn thì camera không bám được theo biên. Phòng hẹp chiều ngang nhưng cao (giếng) vẫn phải rộng ≥ 20 ô cho phần CameraBounds.
- **Spawn Point đặt ngoài vùng trigger của cửa**, cách mép 1–2 ô, có đất để đứng, nếu không nhân vật sẽ bị chuyển ngược lại.
- **Cửa ngang:** trigger dày 1 ô đặt sát mép phòng, cao bằng khe đi qua (thường 4 ô).
- **Cửa dọc, đi xuống (rơi vào lỗ):** trigger nằm dưới lỗ sàn. Spawn phía phòng dưới đặt ngay dưới miệng lỗ, nhân vật rơi tiếp xuống.
- **Cửa dọc, đi lên:** spawn phía phòng trên đặt **trên gờ cạnh lỗ**, có nền đất vững để đứng, không dựa vào lực đẩy ngang và không đặt ngay trên lỗ (sẽ rơi ngược lại → kẹt vòng lặp, vi phạm NFR-06). Phía phòng dưới phải có bậc để nhảy tới miệng lỗ (khoảng cách bậc theo quy tắc nhảy ở SRS mục 5).
- Tên scene và Door Id phân biệt hoa thường.

## 5. Kiểm tra trước khi tạo PR

- Đi qua mỗi cửa cả hai chiều, nhân vật ra đúng cửa.
- Console không có lỗi `[RoomManager]` / `[Room]`, thời gian chuyển in ra đều < 1 giây.
- Camera không lộ ra ngoài phòng.

## 6. Ví dụ có sẵn

Chạy menu **Woodcutter > Build Room Transition Sandbox** để dựng lại 3 phòng mẫu trong `Scenes/Sandbox/`:

- `Sandbox_RoomA` ↔ `Sandbox_RoomB`: cửa ngang.
- `Sandbox_RoomB` ↔ `Sandbox_RoomC`: lỗ sàn rơi xuống giếng, nhảy bậc lên lại.

## 7. Ghi chú cấu hình và kế hoạch tiếp theo

- **Quy ước hiển thị:** 1 ô = 1 unit = 32 px (2×2 tile 16 px). Ortho size 5,625 = 11,25 ô chiều cao, ngang 20 ô. Pixel Perfect Camera (URP 2D) thiết lập Assets PPU 32, Reference Resolution 640×360, Grid Snapping = Upscale Render Texture kết hợp CinemachinePixelPerfect trên CinemachineCamera.
- **Scene nền tập trung:** Scene nền chính thức (Boot/Game) sẽ thay `Sandbox_RoomTransition` khi làm menu + lưu/tải (tuần 3). Khi đó chỉ cần cập nhật tại một nơi duy nhất (`RoomManager.DefaultPersistentSceneName` hoặc trong `CameraRoomSettingsSO`), toàn bộ các phòng tự động kế thừa.
- **Build List:** Scene `SampleScene` được tắt trong EditorBuildSettings. Trước khi tạo bản build phát hành tuần 8, cần rà soát và bỏ các scene sandbox khỏi danh sách build.
