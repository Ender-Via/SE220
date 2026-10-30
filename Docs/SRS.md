# SRS – Tiều phu và những viên đá màu (v1.0)

Oct 30, 2026 · @Kim

## 1. Giới thiệu

### 1.1 Mục đích

Tài liệu này đặc tả đầy đủ yêu cầu chức năng và phi chức năng của game *Tiều phu và những viên đá màu* (tên tạm). Đối tượng đọc: lập trình viên, họa sĩ, người thiết kế màn chơi, người kiểm thử và giảng viên/người đánh giá.

### 1.2 Phạm vi sản phẩm

Game 2D Metroidvania ngắn cho PC, chơi đơn, thời lượng 30–40 phút. Người chơi điều khiển một tiều phu dùng rìu chiến đấu, thu thập 3 viên đá màu để mở năng lực mới và hồi phục màu sắc cho thế giới.

**Trong phạm vi:** 3 khu vực, khoảng 18 phòng, 3 năng lực, 6 loại quái, 2 boss, hệ thống lưu game, hồi màu, nhạc xếp lớp, ending.

**Ngoài phạm vi:** chơi mạng, ném rìu, đỡ đòn, trượt tường, nâng cấp rìu, bản đồ, fast travel, phiên bản di động/console.

### 1.3 Thuật ngữ

| Thuật ngữ | Định nghĩa |
| --- | --- |
| Metroidvania | Thể loại khám phá bản đồ liên thông, mở đường bằng năng lực mới |
| Backtrack | Quay lại khu cũ để dùng năng lực mới mở lối |
| Viên đá | Vật phẩm chính, mỗi viên mở 1 năng lực và hồi màu 1 vùng |
| Hồi màu | Hiệu ứng chuyển vùng từ trắng đen sang có màu |
| Mẩu giấy | Vật phẩm cốt truyện chỉ vị trí viên đá tiếp theo |
| Checkpoint | Điểm lưu game và hồi sinh |
| Hit-stop | Khoảnh khắc dừng hình rất ngắn khi đòn đánh trúng |
| I-frame | Khung hình bất tử, nhân vật không nhận sát thương |
| Gray-box | Màn chơi dựng bằng khối xám, chưa có art |
| Feature freeze | Mốc ngừng thêm tính năng mới |
| P1 / P2 / P3 | Mức ưu tiên: bắt buộc / nên có / nếu dư thời gian |

### 1.4 Tài liệu tham chiếu

- Game Design Overview & SRS (bản tóm tắt) của dự án.
- Cấu trúc tài liệu theo IEEE 830-1998 (Recommended Practice for Software Requirements Specifications).

## 2. Mô tả tổng thể

### 2.1 Góc nhìn sản phẩm

Sản phẩm độc lập, không phụ thuộc hệ thống khác, chạy offline. Gồm các phân hệ: điều khiển nhân vật, chiến đấu, AI quái/boss, năng lực viên đá, màn chơi và chuyển phòng, hồi màu (shader), âm thanh xếp lớp, giao diện, lưu/tải game.

### 2.2 Chức năng chính

- Điều khiển tiều phu di chuyển và chiến đấu bằng rìu.
- Thu thập 3 viên đá, mỗi viên mở một năng lực.
- Mở đường bằng năng lực và quay lại khu cũ.
- Đánh 2 boss và hoàn thành ending.
- Đọc mẩu giấy để biết cốt truyện và hướng đi.
- Lưu và tiếp tục tiến trình.

### 2.3 Đặc điểm người dùng

| Nhóm | Mô tả | Nhu cầu |
| --- | --- | --- |
| Người chơi chính | 16–30 tuổi, từng chơi game hành động 2D | Chiến đấu chắc tay, khám phá, câu chuyện cảm xúc |
| Người chơi phổ thông | Ít chơi thể loại này | Độ khó vừa phải, hướng đi rõ, checkpoint dày |
| Người đánh giá | Giảng viên, ban giám khảo, người chơi thử | Trải nghiệm trọn vẹn trong một lần chơi |

### 2.4 Môi trường hoạt động

- Hệ điều hành: Windows 10/11 64-bit.
- Cấu hình tối thiểu: CPU 2 nhân 2.0 GHz, RAM 4 GB, GPU hỗ trợ OpenGL 3.3 / DirectX 11, 500 MB ổ cứng.
- Thiết bị nhập: bàn phím, tay cầm XInput.

### 2.5 Ràng buộc

- Thời gian phát triển: 8 tuần, feature freeze cuối tuần 6.
- Nhân lực: nhóm 5 người, phân vai theo bảng dưới.
- Engine: Unity 6.6 (6000.6.7f1) (cả nhóm dùng cùng một bản vá, ghi trong ProjectSettings/ProjectVersion.txt), dự án 2D với Universal Render Pipeline (URP).
- Bảng màu cố định: đen, trắng, xám, 2 tông xanh băng; màu ấm chỉ xuất hiện qua hồi màu.
- Asset bên ngoài phải có giấy phép cho phép sử dụng.

**Phân vai đề xuất (5 người):**

| Vai trò | Phụ trách chính | Yêu cầu liên quan |
| --- | --- | --- |
| Trưởng nhóm + lập trình gameplay | Di chuyển, chiến đấu, năng lực, game feel, quản lý tiến độ và Git | FR-MOV, FR-CMB, FR-ABL |
| Lập trình hệ thống + AI | Quái, boss, lưu/tải, UI, chuyển phòng, shader hồi màu | FR-SYS, FR-LVL, FR-NAR-02, mục 6 |
| Thiết kế màn chơi + cốt truyện | Dựng 18 phòng (gray-box → hoàn thiện), mẩu giấy, cân bằng thông số, tổ chức chơi thử | Mục 5, 7, FR-NAR |
| Họa sĩ nhân vật | Nhân vật chính, quái, boss, animation, VFX đòn đánh | Mục 6, FR-CMB-04 |
| Họa sĩ môi trường + âm thanh | Tileset, nền, UI, bảng màu; nhạc xếp lớp và hiệu ứng âm (tự làm hoặc nguồn có giấy phép) | Mục 8, FR-AUD |

Quy ước làm việc: họp ngắn 2 lần/tuần, mỗi người làm trên scene/prefab riêng để tránh xung đột merge, build chơi thử cuối mỗi tuần.

### 2.6 Giả định và phụ thuộc

- Người chơi quen điều khiển game 2D cơ bản.
- Hiệu ứng hồi màu làm bằng Shader Graph của URP (mask grayscale theo bán kính); ánh sáng hang tối dùng 2D Lights.
- Nếu năng lực xanh lá không kịp tuần 6, viên xanh trở thành phần thưởng ending (xem FR-ABL-03).

## 3. Yêu cầu chức năng

Mỗi yêu cầu có mã, mức ưu tiên và tiêu chí nghiệm thu kiểm tra được. Thông số cụ thể nằm ở mục 5.

### 3.1 Di chuyển (MOV)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-MOV-01 | Chạy trái/phải với gia tốc và giảm tốc | P1 | Đạt tốc độ tối đa trong ≤ 0,1 giây; dừng trong ≤ 0,08 giây |
| FR-MOV-02 | Nhảy có độ cao thay đổi theo thời gian giữ phím | P1 | Nhấn nhanh nhảy thấp, giữ nhảy cao đúng thông số |
| FR-MOV-03 | Coyote time và jump buffer | P1 | Nhảy vẫn nhận trong 0,1 giây sau khi rời mép hoặc trước khi chạm đất |
| FR-MOV-04 | Lăn né có i-frame và thời gian hồi | P1 | Không nhận sát thương trong i-frame; không lăn được khi đang hồi |
| FR-MOV-05 | Rơi với tốc độ tối đa giới hạn | P1 | Tốc độ rơi không vượt thông số |

### 3.2 Chiến đấu (CMB)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-CMB-01 | Combo rìu 2 nhịp | P1 | Nhịp 2 chỉ ra khi bấm trong cửa sổ combo |
| FR-CMB-02 | Đòn nặng (giữ phím) phá khiên, hất lùi | P1 | Quái có khiên chỉ mất khiên khi trúng đòn nặng |
| FR-CMB-03 | Chém lên và chém xuống khi đang trên không | P2 | Chém xuống trúng quái thì nảy lên |
| FR-CMB-04 | Hit-stop, rung màn hình, nháy trắng khi trúng | P1 | Mỗi đòn trúng kích hoạt đủ 3 phản hồi |
| FR-CMB-05 | Máu người chơi, i-frame sau khi bị đánh | P1 | Không mất máu liên tiếp trong thời gian bất tử |
| FR-CMB-06 | Chết khi máu về 0, hồi sinh ở checkpoint gần nhất | P1 | Hồi sinh ≤ 2 giây, đầy máu, giữ viên đá và lối đã mở |
| FR-CMB-07 | Chặt cây chết và vật thể phá được | P2 | Cây đổ sau số nhát quy định, có thể làm cầu/bệ |

### 3.3 Năng lực viên đá (ABL)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-ABL-01 | Đá đỏ: rìu lửa (phím kỹ năng) làm tan băng, đốt mạng nhện/dây leo, thắp đống lửa | P1 | Vật cản tan/cháy trong ≤ 1 giây; quái băng nhận x2 sát thương |
| FR-ABL-02 | Đá vàng: dash trên không 1 lần mỗi lần rời đất | P1 | Dash được nạp lại khi chạm đất; vượt khe đúng khoảng cách thiết kế |
| FR-ABL-03 | Đá xanh: gieo hạt tại ô đất đánh dấu, mọc bệ đỡ cố định | P2 | Bệ mọc trong ≤ 0,5 giây, đứng được; chỉ gieo được ở ô đánh dấu |
| FR-ABL-04 | Thông báo năng lực mới bằng hình ảnh + gợi ý phím | P1 | Hiển thị 1 lần khi nhận đá, không dùng đoạn chữ dài |

### 3.4 Màn chơi và tiến trình (LVL)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-LVL-01 | Chuyển phòng qua cửa/mép màn hình | P1 | Chuyển ≤ 1 giây, nhân vật xuất hiện đúng cửa tương ứng |
| FR-LVL-02 | Vật cản khóa theo năng lực | P1 | Chỉ đúng năng lực mới phá được; trạng thái đã phá được lưu |
| FR-LVL-03 | Lối tắt một chiều mở từ phía bên kia | P1 | Sau boss 1, đi từ vách đá về lều ≤ 30 giây |
| FR-LVL-04 | Checkpoint hồi máu, lưu game, đặt điểm hồi sinh | P1 | Tương tác tại checkpoint ghi file lưu thành công |
| FR-LVL-05 | Bẫy (gai băng, vực) gây sát thương và đưa về điểm an toàn gần nhất | P1 | Không phải về checkpoint khi rơi vực |

### 3.5 Cốt truyện và hồi màu (NAR)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-NAR-01 | Nhặt và đọc mẩu giấy (chữ + bản đồ phác) | P1 | Mở lại được từ menu tạm dừng |
| FR-NAR-02 | Hồi màu khi nhận đá: vòng màu loang từ tâm | P1 | Hiệu ứng 2–3 giây; vùng giữ màu sau khi tải lại game |
| FR-NAR-03 | Lều tiều phu thay đổi theo số viên đá | P2 | 4 trạng thái hình ảnh (0–3 viên) |
| FR-NAR-04 | Ending: đặt 3 viên vào phiến đá (cây chưa hồi sinh), tháo cán rìu trả về gốc cây, hồi màu toàn cảnh, credits | P1 | Chơi hết từ boss cuối đến credits không lỗi |

### 3.6 Âm thanh (AUD)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-AUD-01 | Nhạc nền xếp lớp theo số viên đá | P2 | Thêm lớp mượt, không lệch nhịp |
| FR-AUD-02 | Hiệu ứng âm: bước chân, chém, trúng đòn, lửa, dash, gió | P1 | Mỗi hành động chính có âm thanh riêng |
| FR-AUD-03 | Nhạc boss riêng | P2 | Bật khi vào đấu trường, tắt khi boss chết |

### 3.7 Giao diện và hệ thống (SYS)

| Mã | Yêu cầu | Ưu tiên | Tiêu chí nghiệm thu |
| --- | --- | --- | --- |
| FR-SYS-01 | Menu chính: Chơi mới, Tiếp tục, Tùy chọn, Thoát | P1 | “Tiếp tục” mờ khi chưa có file lưu |
| FR-SYS-02 | Menu tạm dừng: Tiếp tục, Mẩu giấy, Tùy chọn, Về menu | P1 | Game dừng hoàn toàn khi mở |
| FR-SYS-03 | HUD: máu, viên đá đã có | P1 | Cập nhật ngay khi thay đổi |
| FR-SYS-04 | Tùy chọn âm lượng (nhạc, hiệu ứng), toàn màn hình | P2 | Lưu giữa các lần mở game |
| FR-SYS-05 | Đổi phím | P3 | Phím mới hoạt động và được lưu |
| FR-SYS-06 | Lưu/tải game 1 slot, tự lưu tại checkpoint | P1 | Tải lại đúng trạng thái (mục 9) |

## 4. Use case

Tác nhân duy nhất là Người chơi; hệ thống game phản hồi. 6 use case dưới đây bao phủ luồng chính của game.

| Mã | Tên | Yêu cầu liên quan |
| --- | --- | --- |
| UC-01 | Bắt đầu / tiếp tục game | FR-SYS-01, FR-SYS-06 |
| UC-02 | Chiến đấu với quái | FR-CMB-01…07 |
| UC-03 | Nhận viên đá | FR-ABL-01…04, FR-NAR-02 |
| UC-04 | Mở đường bằng năng lực (backtrack) | FR-LVL-02, FR-LVL-03 |
| UC-05 | Chết và hồi sinh | FR-CMB-06, FR-LVL-04 |
| UC-06 | Đánh boss và hoàn thành game | FR-CMB, FR-NAR-04 |

### UC-01 Bắt đầu / tiếp tục game

- **Điều kiện trước:** game đã khởi động, đang ở menu chính.
- **Luồng chính:**
  1. Người chơi chọn “Chơi mới”.
  2. Hệ thống tạo file lưu mới, tải phòng Lều tiều phu.
  3. Người chơi nhận quyền điều khiển.
- **Luồng phụ:** chọn “Tiếp tục” → tải file lưu, đặt nhân vật tại checkpoint cuối. Chọn “Chơi mới” khi đã có file lưu → hỏi xác nhận ghi đè.
- **Ngoại lệ:** file lưu hỏng → thông báo và cho chơi mới.
- **Điều kiện sau:** người chơi ở trong game với trạng thái đúng.

### UC-02 Chiến đấu với quái

- **Điều kiện trước:** quái phát hiện người chơi trong tầm.
- **Luồng chính:**
  1. Quái chuyển sang trạng thái tấn công, có báo hiệu trước đòn.
  2. Người chơi né hoặc chém.
  3. Đòn trúng: quái mất máu, bị giật lùi, hit-stop.
  4. Quái hết máu → hiệu ứng chết, biến mất.
- **Luồng phụ:** người chơi bị trúng → mất máu, i-frame; máu về 0 → UC-05.
- **Điều kiện sau:** quái bị tiêu diệt, hồi sinh khi người chơi rời phòng và quay lại.

### UC-03 Nhận viên đá

- **Điều kiện trước:** người chơi đến vị trí viên đá, đã vượt thử thách/boss bảo vệ.
- **Luồng chính:**
  1. Người chơi tương tác với viên đá.
  2. Hệ thống khóa điều khiển, chạy hiệu ứng hồi màu 2–3 giây.
  3. Hiển thị mẩu giấy đi kèm (nếu có) và gợi ý phím năng lực mới.
  4. Nhạc thêm một lớp, game tự lưu.
- **Điều kiện sau:** năng lực được mở vĩnh viễn, vùng giữ màu.

### UC-04 Mở đường bằng năng lực

- **Điều kiện trước:** người chơi đứng trước vật cản khóa theo năng lực.
- **Luồng chính:**
  1. Người chơi dùng đúng năng lực (ví dụ rìu lửa vào băng).
  2. Vật cản tan/cháy, lối đi mở ra.
  3. Hệ thống ghi trạng thái vật cản vào dữ liệu lưu.
- **Luồng phụ:** chưa có năng lực hoặc dùng sai → vật cản phản hồi (âm thanh “coong”, rung nhẹ), không thay đổi.
- **Điều kiện sau:** lối đi mở vĩnh viễn.

### UC-05 Chết và hồi sinh

- **Điều kiện trước:** máu người chơi về 0.
- **Luồng chính:**
  1. Hoạt cảnh chết, màn hình chuyển trắng (tuyết).
  2. Nhân vật xuất hiện ở checkpoint gần nhất, đầy máu.
  3. Quái trong khu hồi sinh; boss hồi đầy máu.
- **Điều kiện sau:** viên đá, lối đã mở, mẩu giấy được giữ nguyên.

### UC-06 Đánh boss và hoàn thành game

- **Điều kiện trước:** người chơi vào đấu trường boss.
- **Luồng chính:**
  1. Cửa đấu trường khóa, nhạc boss bật, thanh máu boss hiện.
  2. Boss đánh theo giai đoạn 1; dưới 50% máu chuyển giai đoạn 2.
  3. Boss hết máu → cửa mở, phần thưởng xuất hiện.
  4. Với boss cuối: người chơi đặt 3 viên vào phiến đá → cây chưa hồi sinh → trả cán rìu về gốc cây → ending → credits → menu chính.
- **Luồng phụ:** người chơi chết → UC-05, hồi sinh ngay trước cửa đấu trường.
- **Điều kiện sau:** boss được ghi là đã hạ, không xuất hiện lại.

## 5. Thông số tinh chỉnh gameplay

Giá trị khởi điểm để prototype, sẽ chỉnh sau chơi thử. Đơn vị: 1 ô (tile) = 16 px, giả định nhân vật cao 2 ô; máu tính theo “vạch”.

### 5.1 Nhân vật

| Thông số | Giá trị | Ghi chú |
| --- | --- | --- |
| Tốc độ chạy tối đa | 8 ô/giây |  |
| Thời gian tăng tốc / giảm tốc | 0,08 / 0,06 giây |  |
| Độ cao nhảy tối thiểu / tối đa | 1,5 / 4 ô | Theo thời gian giữ phím |
| Tốc độ rơi tối đa | 18 ô/giây |  |
| Coyote time / jump buffer | 0,1 / 0,1 giây |  |
| Lăn: quãng đường / thời gian | 4 ô / 0,35 giây |  |
| Lăn: i-frame / hồi | 0,25 / 0,5 giây |  |
| Máu khởi đầu | 5 vạch | +1 vạch mỗi viên đá (tối đa 8) |
| I-frame sau khi bị đánh | 1,0 giây | Nhân vật nhấp nháy |

### 5.2 Rìu và năng lực

| Thông số | Giá trị | Ghi chú |
| --- | --- | --- |
| Sát thương nhịp 1 / nhịp 2 | 1 / 1 |  |
| Cửa sổ combo | 0,4 giây |  |
| Tầm chém | 1,5 ô ngang × 1,5 ô dọc |  |
| Đòn nặng: nạp / sát thương | 0,6 giây / 3 | Phá khiên |
| Hit-stop thường / đòn nặng | 0,05 / 0,1 giây |  |
| Rìu lửa: sát thương / hồi | 2 / 1,5 giây | x2 với quái băng |
| Dash: quãng đường / thời gian | 5 ô / 0,2 giây | 1 lần mỗi lần rời đất |
| Gieo hạt: thời gian mọc / chiều cao bệ | 0,5 giây / 3 ô | Chỉ ở ô đánh dấu |

### 5.3 Quy tắc thiết kế màn từ thông số

- Khe nhảy thường ≤ 5 ô ngang; khe cần dash 7–9 ô.
- Bệ cao ≤ 3,5 ô với nhảy thường; vách cần leo bằng bệ hạt ≥ 5 ô.
- Khoảng cách giữa 2 checkpoint ≤ 3 phút chơi.

## 6. Đặc tả quái và boss

6 loại quái dựng từ 3 khung AI và 2 boss, mỗi boss 2 giai đoạn. Mọi đòn đánh có báo hiệu tối thiểu 0,4 giây.

### 6.1 Quái thường

| Tên | Khung AI | Khu | Máu | Sát thương | Hành vi / cách khắc chế |
| --- | --- | --- | --- | --- | --- |
| Sói tuyết | Lao tới | 1 | 3 | 1 | Gầm gừ rồi vồ thẳng; lăn qua rồi chém sau lưng |
| Nhện băng | Đi tuần | 1 | 2 | 1 | Bò trên trần/sàn, rơi xuống khi người chơi đi qua |
| Quạ xám | Bắn xa | 1 | 2 | 1 | Bay lượn, ném đá băng; chém lên khi nó hạ thấp |
| Giáp băng | Đi tuần | 2 | 5 | 1 | Có khiên phía trước; đòn nặng hoặc rìu lửa |
| Dơi hang | Lao tới | 2 | 2 | 1 | Ngủ trong bóng tối, bị ánh lửa làm tỉnh |
| Tinh linh sương | Bắn xa | 2–3 | 4 | 1 | Bắn loạt 3 viên; dash áp sát |

Quái hồi sinh khi người chơi rời phòng và quay lại. Sói tuyết xuất hiện lại ở khu 3 để người chơi cảm nhận sự mạnh lên.

### 6.2 Boss 1 – Người gác vách băng

Một sinh vật băng khổng lồ ôm viên đá vàng. Kiểm tra: chiến đấu cơ bản + rìu lửa. Máu: 40.

| Giai đoạn | Chiêu | Báo hiệu | Cách đối phó |
| --- | --- | --- | --- |
| 1 (100–50%) | Đập tay xuống đất | Giơ tay 0,6 giây | Lăn sang bên, chém vào tay |
| 1 | Quét ngang | Lùi người 0,5 giây | Nhảy qua |
| 1 | Mưa băng từ trần | Gầm, bóng đổ trên sàn | Tránh vùng bóng |
| 2 (50–0%) | Khoác giáp băng | Lóe sáng xanh | Rìu lửa phá giáp, nếu không chỉ gây 0 sát thương |
| 2 | Các chiêu trên nhanh hơn 20% | Như trên | Như trên |

### 6.3 Boss cuối – Bóng của nhóm phiêu lưu

Khối bóng đen quấn quanh gốc cây cội nguồn, sinh ra từ lòng tham của nhóm phiêu lưu ngày trước. Kiểm tra: tất cả năng lực. Máu: 30 mỗi giai đoạn (tổng 60). Tổng 7 chiêu, tái sử dụng code và VFX năng lực của người chơi.

**Giai đoạn 1 – Những người cầm đá.** Một khối bóng có 3 cánh tay, mỗi tay là bóng một người từng giữ một viên đá. Tay đổi màu tương ứng khi ra chiêu; các tay thỉnh thoảng giằng co nhau, tạo khoảng hở để người chơi tấn công.

| Chiêu | Báo hiệu | Cách đối phó | Tái sử dụng |
| --- | --- | --- | --- |
| Tay đỏ: ném 3 cầu lửa theo vòng cung | Tay đỏ rực lên 0,6 giây | Lăn/dash qua khe giữa các cầu | VFX rìu lửa |
| Tay vàng: lao dash xuyên màn hình | Vệt sáng vàng vẽ đường lao 0,5 giây | Nhảy hoặc lăn khỏi đường lao | Code dash |
| Tay xanh: rễ đâm lên theo hàng | Đất nứt xanh 0,5 giây | Dash qua hoặc đứng trên bệ hạt | Hiệu ứng gieo hạt |
| Giằng co: 2 tay khóa nhau 2 giây | Các tay quay vào nhau | Chém vào lõi (cửa sổ sát thương) | – |

**Giai đoạn 2 – Bóng người tiều phu.** Khối bóng tan ra, để lại một bóng người cầm đúng chiếc rìu có cán từ cây cội nguồn. Nó dùng bộ chiêu giống người chơi, tượng trưng cho sự thờ ơ và khúc cây ông đã mang đi.

| Chiêu | Báo hiệu | Cách đối phó | Tái sử dụng |
| --- | --- | --- | --- |
| Combo rìu 2 nhịp | Giơ rìu 0,4 giây | Lăn, phản công sau nhịp 2 | Animation chém |
| Đòn nặng bổ xuống, rìu cắm đất 1,5 giây | Nạp rìu 0,7 giây | Né rồi chém khi rìu cắm đất | Animation đòn nặng |
| Lăn né đòn của người chơi (ngẫu nhiên 30%) | – | Đánh nhử, đợi kết thúc lăn | Animation lăn |

Khi bóng tiều phu bị hạ, nó không chết mà quỳ xuống, đặt rìu xuống đất rồi tan biến, dẫn vào ending.

Nếu năng lực xanh lá bị cắt, chiêu tay xanh thay bằng dây leo băng (đốt bằng rìu lửa).

## 7. Đặc tả màn chơi

18 phòng chia 3 khu theo bố cục chữ L ngược: khu 1 đi ngang sang phải, khu 2 đi xuống, khu 3 đi lên. CP = checkpoint.

| Mã | Tên phòng | Nội dung | Nối với | Cần năng lực |
| --- | --- | --- | --- | --- |
| R1-01 | Lều tiều phu | CP, đống lửa xám, hướng dẫn chạy/nhảy | R1-02, R2-07 (lối tắt) | – |
| R1-02 | Bìa rừng | Chặt cây chết (dạy chém) | R1-01, R1-03 | – |
| R1-03 | Bãi săn | Săn thú, 2 sói tuyết (dạy né) | R1-02, R1-04 | – |
| R1-04 | Rừng chết | Lối băng xuống hầm (thấy nhưng chưa qua), vách cao, bóng cây cội nguồn ở xa | R1-03, R1-05, R2-01 | Đỏ (để xuống) |
| R1-05 | Thung lũng tuyết | Nhện băng, quạ xám, platforming nhẹ | R1-04, R1-06 | – |
| R1-06 | Xác thám hiểm | Nhặt mẩu giấy 1 + đá đỏ, hồi màu lần đầu, mạng nhện đốt thử | R1-05 | – |
| R2-01 | Cửa hầm băng | CP, hang tối cần lửa soi | R1-04, R2-02 | Đỏ |
| R2-02 | Đường hầm tối | Dơi hang, giáp băng | R2-01, R2-03 | – |
| R2-03 | Hồ băng | Tan băng thành nước trong, platforming | R2-02, R2-04 | Đỏ |
| R2-04 | Giếng sâu | Rơi xuống dần, tinh linh sương | R2-03, R2-05 | – |
| R2-05 | Phòng chờ boss | CP, mẩu giấy 2 (cảnh báo về boss) | R2-04, R2-06 | – |
| R2-06 | Đấu trường vách đá | Boss 1, nhận đá vàng | R2-05, R2-07 | – |
| R2-07 | Vách đá | Lối tắt về lều, khe dạy dash | R2-06, R1-01, R3-01 | Vàng (để lên) |
| R3-01 | Sườn núi | Dash qua khe, sói tuyết quay lại | R2-07, R3-02 | Vàng |
| R3-02 | Vườn đá | CP, mẩu giấy 3 + đá xanh, hồi màu | R3-01, R3-03 | – |
| R3-03 | Vách rễ | Gieo hạt leo lên, kết hợp dash + lửa | R3-02, R3-04 | Xanh |
| R3-04 | Chân cây cội nguồn | CP trước boss, khoảng yên tĩnh | R3-03, R3-05 | – |
| R3-05 | Ngọn cây | Boss cuối, phiến đá, ending | R3-04 | – |

**Backtrack:** R1-04 → R2-01 (bằng đá đỏ), R2-07 → R1-01 (lối tắt sau boss 1), và 1 phòng bí mật tùy chọn ở R1-05 mở bằng dash (thưởng +1 vạch máu, P2).

## 8. Điều khiển và giao diện

### 8.1 Sơ đồ phím

| Hành động | Bàn phím | Tay cầm (Xbox) |
| --- | --- | --- |
| Di chuyển | A / D (hoặc mũi tên) | Cần trái / D-pad |
| Nhìn lên / xuống (chém lên/xuống) | W / S | Cần trái lên/xuống |
| Nhảy | Space | A |
| Chém / giữ để đòn nặng | J | X |
| Lăn né | K | B |
| Rìu lửa (đá đỏ) | L | Y |
| Dash (đá vàng) | Shift | RB |
| Gieo hạt (đá xanh) | I | LB |
| Tương tác | E | D-pad lên |
| Tạm dừng | Esc | Start |

### 8.2 Màn hình

| Màn hình | Thành phần | Ghi chú |
| --- | --- | --- |
| Menu chính | Logo, Chơi mới, Tiếp tục, Tùy chọn, Thoát | Nền tuyết rơi trắng đen |
| HUD | Máu (góc trên trái), 3 ô viên đá | Ô đá xám → có màu khi nhận |
| Thanh máu boss | Tên boss, thanh máu (cạnh dưới) | Chỉ hiện trong đấu trường |
| Tạm dừng | Tiếp tục, Mẩu giấy, Tùy chọn, Về menu |  |
| Mẩu giấy | Ảnh giấy rách, chữ viết tay, bản đồ phác | Chuyển trái/phải giữa các mẩu đã nhặt |
| Tùy chọn | Âm lượng nhạc/hiệu ứng, toàn màn hình, đổi phím (P3) |  |
| Gợi ý năng lực | Biểu tượng phím + hình minh họa ngắn | Hiện 1 lần khi nhận đá |
| Credits | Danh sách người làm trên nền đã hồi màu | Về menu chính sau khi kết thúc |

Nguyên tắc UI: tối giản, dùng chất liệu giấy/gỗ, không che quá 10% màn hình khi chơi.

## 9. Dữ liệu lưu game

1 slot lưu, định dạng JSON (Unity JsonUtility, lưu tại Application.persistentDataPath), tự ghi khi chạm checkpoint, nhận viên đá hoặc hạ boss. Tùy chọn (âm lượng, phím) lưu riêng trong `settings.json`.

| Trường | Kiểu | Mô tả |
| --- | --- | --- |
| version | số | Phiên bản cấu trúc file lưu |
| checkpoint\_id | chuỗi | Phòng/checkpoint hồi sinh, ví dụ "R2-05" |
| max\_hp | số | Máu tối đa hiện tại |
| stones | danh sách | Các viên đã có: "red", "yellow", "green" |
| notes | danh sách | Mẩu giấy đã nhặt |
| opened\_obstacles | danh sách | ID vật cản đã phá |
| shortcuts | danh sách | Lối tắt đã mở |
| bosses\_defeated | danh sách | Boss đã hạ |
| secrets | danh sách | Phòng bí mật đã lấy thưởng |
| play\_time | số (giây) | Tổng thời gian chơi |
| game\_completed | đúng/sai | Đã xem ending chưa |

```json
{
  "version": 1,
  "checkpoint_id": "R2-05",
  "max_hp": 6,
  "stones": ["red"],
  "notes": [1, 2],
  "opened_obstacles": ["R1-04_ice", "R2-03_ice"],
  "shortcuts": [],
  "bosses_defeated": [],
  "secrets": [],
  "play_time": 812,
  "game_completed": false
}
```

Trạng thái hồi màu không lưu riêng: suy ra từ `stones` khi tải phòng.

## 10. Yêu cầu phi chức năng

| Mã | Nhóm | Yêu cầu | Tiêu chí đo |
| --- | --- | --- | --- |
| NFR-01 | Hiệu năng | Khung hình ổn định | ≥ 60 FPS trên cấu hình tối thiểu, kể cả khi hồi màu |
| NFR-02 | Hiệu năng | Độ trễ input | < 100 ms từ bấm phím đến hành động trên màn hình |
| NFR-03 | Hiệu năng | Thời gian tải | Mở game ≤ 5 giây; chuyển phòng ≤ 1 giây |
| NFR-04 | Hiệu năng | Bộ nhớ | RAM sử dụng ≤ 1 GB |
| NFR-05 | Độ tin cậy | Không crash | 0 crash trong 10 lượt chơi thử trọn vẹn |
| NFR-06 | Độ tin cậy | Không kẹt | Không có vị trí nào khiến người chơi không thể tiếp tục (softlock) |
| NFR-07 | Độ tin cậy | An toàn file lưu | Ghi vào file tạm rồi đổi tên; tắt game giữa chừng không làm hỏng file |
| NFR-08 | Khả dụng | Dễ hiểu | ≥ 4/5 người thử tìm được đá đỏ mà không cần hỏi |
| NFR-09 | Khả dụng | Thời lượng | Người thử hoàn thành trong 30–40 phút (trung vị) |
| NFR-10 | Khả dụng | Độ khó | Boss 1 hạ được trong ≤ 5 lần thử; boss cuối ≤ 8 lần |
| NFR-11 | Tiếp cận | Độ rõ hình ảnh | Quái, bẫy phân biệt được khi ảnh chuyển trắng đen; tương phản chữ ≥ 4,5:1 |
| NFR-12 | Tiếp cận | Không phụ thuộc màu | Mọi thông tin quan trọng có dấu hiệu hình dạng hoặc âm thanh kèm theo |
| NFR-13 | Tương thích | Thiết bị | Windows 10/11; bàn phím và tay cầm XInput; màn hình 16:9 từ 1280×720 |
| NFR-14 | Bảo trì | Mã nguồn | Thông số mục 5 nằm trong ScriptableObject, chỉnh trong Inspector không cần sửa code; dùng Git + Git LFS với .gitignore chuẩn Unity |
| NFR-15 | Bản địa hóa | Ngôn ngữ | Tiếng Việt; toàn bộ chữ nằm trong file ngôn ngữ để thêm tiếng Anh sau (P3) |
| NFR-16 | Pháp lý | Giấy phép asset | Mọi asset/âm thanh bên ngoài có ghi nguồn và giấy phép trong credits |

## 11. Ma trận truy vết và câu hỏi mở

### 11.1 Ma trận truy vết

Nối mỗi nhóm yêu cầu với mục tiêu thiết kế, use case và tuần triển khai.

| Nhóm yêu cầu | Mục tiêu thiết kế | Use case | Tuần làm | Kiểm thử |
| --- | --- | --- | --- | --- |
| FR-MOV | Core loop: di chuyển chắc tay | UC-02 | 1–2 | Chơi thử nội bộ cuối tuần 2 |
| FR-CMB | Core loop: chặt chém | UC-02, UC-05 | 1–3 | Chơi thử nội bộ cuối tuần 3 |
| FR-ABL | Core loop: nâng cấp | UC-03, UC-04 | 4–6 | Kiểm tra từng vật cản |
| FR-LVL | Core loop: khám phá, backtrack | UC-04, UC-05 | 3–6 | Chơi hết map, kiểm softlock |
| FR-NAR | Aesthetic: hồi màu, cốt truyện | UC-03, UC-06 | 4–7 | Kiểm tra lưu/tải màu |
| FR-AUD | Aesthetic: âm thanh xếp lớp | UC-03, UC-06 | 7 | Nghe thử từng mốc |
| FR-SYS | Hệ thống | UC-01 | 3, 7 | Kiểm tra lưu/tải, menu |
| NFR | Chất lượng | Tất cả | 8 | Chơi thử 3–5 người |

### 11.2 Câu hỏi mở

- [ ] Engine: đã chốt Unity 6.6 (6000.6.7f1) (URP 2D). Còn lại: dùng Input System mới và Cinemachine cho camera.
- [ ] Nhóm 5 người đã chốt; còn lại: gán tên thành viên vào từng vai trò (mục 2.5).
- [ ] Chốt tên chính thức của game.
- [ ] Nền tảng phát hành: itch.io, Steam hay chỉ nộp bài.
- [ ] Giữ hay cắt twist “đặt rìu xuống” ở ending.
- [ ] Nguồn nhạc: tự sáng tác hay dùng nhạc có giấy phép.

### 11.3 Lịch sử phiên bản

| Phiên bản | Nội dung |
| --- | --- |
| 1.0 | Bản đầu tiên: đầy đủ 11 mục. Bản 1.1: chốt engine Unity 6.6 (6000.6.7f1), nhóm 5 người và phân vai |
