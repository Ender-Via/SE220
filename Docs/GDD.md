# Game Design Overview & SRS – Tiều phu và những viên đá màu

Oct 30, 2026 · @Kim

## 1. Tổng quan

Game 2D Metroidvania ngắn, 30–40 phút chơi, làm trong 2 tháng: một tiều phu dùng chiếc rìu và 3 viên đá màu để trả lại màu sắc cho thế giới.

**Pitch:** Thế giới chỉ còn trắng, đen và xám lạnh của băng. Một tiều phu nhặt được mẩu giấy và viên đá đỏ của nhóm thám hiểm đã chết, rồi lần theo dấu họ đến cây cội nguồn.

| Hạng mục | Nội dung |
| --- | --- |
| Thể loại | 2D Metroidvania, hành động cận chiến |
| Góc nhìn | 2D side-scroll |
| Vũ khí | Rìu đốn cây |
| Thời lượng chơi | 30–40 phút |
| Thời gian phát triển | 8 tuần |
| Nền tảng | PC (Windows), engine Unity 6.6 (6000.6.7f1) |

**Điểm nhấn:**

- Màu sắc = tiến trình = năng lực: mỗi viên đá mở kỹ năng mới và trả màu cho một vùng.
- Kể chuyện không cần lời qua mẩu giấy rách của người đi trước.
- Twist chủ đề: tiều phu đi cứu một cái cây.
- Scope nhỏ, nội dung xong từ tuần 6.

## 2. Cốt truyện và bối cảnh

Câu chuyện đi từ mùa đông chết chóc sang sự sống: hơi ấm (đỏ) → ánh sáng (vàng) → nảy mầm (xanh lá).

**Thế giới:** lạnh, phủ tuyết, nguy hiểm. Màu duy nhất còn sót lại là xanh băng nhạt, màu của giá lạnh và cái chết.

**Nhân vật chính:** tiều phu sống một mình trong túp lều, chặt cây chết lấy củi và săn thú để sống qua ngày. Cậu ước muốn thế giới có màu nhưng không có cách nào.

**Biến cố:** đi xa hơn thường lệ để tìm củi, cậu thấy xác một thành viên nhóm thám hiểm vùi trong tuyết, tay nắm mẩu giấy và viên đá đỏ.

**Mẩu giấy:** mỗi mẩu ghi vị trí viên đá hiện tại và một phần vị trí tiếp theo, kèm 2–4 câu của người đã mất. Mỗi thành viên có nét chữ và tính cách riêng; các mẩu ghi lại nơi họ thất bại.

**Đích đến:** cây cội nguồn có phiến đá tập hợp các viên đá. Đặt đủ 3 viên, màu lan ra, tuyết tan, xanh băng chuyển thành xanh lá.

**Twist (đề xuất):** cây cội nguồn từng bị chính những tiều phu chặt, đó là nguyên nhân mất màu. Cái kết: nhân vật đặt rìu xuống.

## 3. Core Loop

Vòng lặp chính: Chặt chém → Khám phá → Nâng cấp → Backtrack, và mỗi vòng kết thúc bằng một vùng được hồi màu.

| Cấp độ | Thời gian | Nội dung |
| --- | --- | --- |
| Vòng nhỏ | 1–3 phút | Chém quái, khám phá ngõ ngách, tìm checkpoint |
| Vòng vừa | 8–12 phút | Đọc mẩu giấy, đến vùng mới, vượt thử thách, nhận viên đá, hồi màu |
| Vòng lớn | Cả game | Năng lực mới → quay lại mở lối cũ → mẩu giấy kế → vùng mới |

- **Chặt chém:** combo rìu + đòn nặng, lăn né.
- **Khám phá:** đi theo mẩu giấy, nhìn thấy vật cản trước khi có cách vượt.
- **Nâng cấp:** đến từ viên đá (năng lực mới), không có cây kỹ năng hay tiền tệ.
- **Backtrack:** 1–2 chỗ mở bằng đá đỏ, 1 lối tắt về lều sau boss 1.

## 4. Aesthetic & Art Style

Tranh khắc gỗ / mực than trên nền trắng đen pha xanh băng nhạt; màu ấm chỉ xuất hiện khi được hồi sinh.

**Bảng màu cố định:**

| Nhóm | Màu | Dùng cho |
| --- | --- | --- |
| Nền chết | Đen, xám, trắng | Mọi thứ ban đầu |
| Lạnh | 2 tông xanh băng, độ bão hòa thấp | Băng, tuyết, bóng đổ |
| Sự sống | Đỏ, vàng, xanh lá, xanh nước trong | Viên đá, vùng đã hồi màu, nước tan |

- **Độ rõ:** quái, bẫy, nền phân biệt bằng sáng tối và hình khối, không dựa vào màu.
- **Hồi màu:** shader mask, vòng tròn màu loang ra từ viên đá như mực thấm giấy.
- **Băng và nước:** băng xanh đục, chết; nước tan ra xanh trong, sống.
- **Chi tiết sống:** hơi thở bốc khói, lều tiều phu đổi màu mỗi lần quay về, bóng cây cội nguồn luôn thấy ở phía xa.

**Bầu không khí:** đầu game lạnh, tĩnh, cô đơn; càng về sau càng ấm.

**Âm thanh:** đầu game gần như im lặng (gió, tiếng rìu, bước chân trên tuyết). Nhạc xếp lớp: đá đỏ thêm lớp 1, đá vàng thêm lớp 2, đá xanh thêm lớp 3, ending ghép đủ bản nhạc.

## 5. Mechanics

Cơ chế gọn: di chuyển chắc tay, rìu 2 nhịp + đòn nặng, và 3 viên đá mỗi viên phục vụ mở đường, chiến đấu và giải đố.

**Di chuyển:** chạy, nhảy (giữ phím nhảy cao hơn), lăn né có khung bất tử ngắn.

**Chiến đấu:** combo rìu 2 nhịp, đòn nặng chậm phá khiên/hất tung. Game feel: hit-stop, rung màn hình, giật lùi quái.

**Ba viên đá:**

| Viên | Ý nghĩa | Mở đường | Chiến đấu | Vị trí |
| --- | --- | --- | --- | --- |
| Đỏ | Hơi ấm, lửa | Làm tan băng, đốt mạng nhện/dây leo, soi sáng hang | Rìu lửa, quái băng yếu trước lửa | Xác thám hiểm (khu 1) |
| Vàng | Ánh sáng, năng lượng | Dash trên không qua khe xa | Né nhanh, đòn lao | Sau boss 1 (khu 2) |
| Xanh lá | Sự sống | Gieo hạt ở đất đánh dấu, mọc bệ đỡ để leo | Leo cao đánh điểm yếu boss cuối | Đầu khu 3 |

**Mở đường:** khóa theo năng lực, người chơi thấy vật cản trước khi có cách vượt. Mẩu giấy chỉ vị trí viên đá kế tiếp.

**Checkpoint:** đống lửa/cây nhỏ hồi máu và lưu game; sau khi có đá đỏ thì lửa cháy có màu.

**Không làm trong bản này:** ném rìu, đỡ đòn, trượt tường, nâng cấp rìu, bản đồ, fast travel.

## 6. Level Design

Bố cục chữ L ngược đi lên: ngang ra phải (thường ngày), xuống lòng đất (nguy hiểm), rồi leo lên cao (hy vọng); khoảng 18 phòng.

| Khu | Hướng | Phòng | Trọng tâm |
| --- | --- | --- | --- |
| 1. Lều → Rừng chết → Xác thám hiểm | Ngang, sang phải | 6 | Dạy điều khiển, gieo vật cản, nhặt đá đỏ |
| 2. Hầm băng → Vách đá | Xuống, rồi sang phải | 7 | Dùng đá đỏ, boss 1, nhận đá vàng |
| 3. Gốc cội nguồn | Lên cao | 5 | Dash leo vách, đá xanh, boss cuối, ending |

**Luồng đi:**

1. Mở đầu ở lều: chặt cây, săn thú, nhóm lửa xám yếu ớt.
2. Thấy lối băng chặn và vách cao nhưng chưa qua được.
3. Nhặt đá đỏ, tuyết quanh đó tan và hồi màu lần đầu.
4. Quay lại làm tan băng, xuống hầm băng tối.
5. Ra vách đá, đánh boss 1, nhận đá vàng, mở lối tắt về lều.
6. Dash leo vách lên khu 3, nhận đá xanh, leo lên gốc cây.
7. Boss cuối, đặt 3 viên vào phiến đá, ending.

**Nguyên tắc dẫn dắt:**

- Cho thấy giới hạn trước, trao công cụ sau, để người chơi tự quay lại vượt qua.
- Mỗi năng lực dạy theo 4 bước: giới thiệu an toàn, luyện tập, kết hợp, thử thách (boss).
- Quái khu 1 xuất hiện lại ở khu sau để người chơi thấy mình mạnh hơn.
- Thế giới phản ánh tiến bộ: màu loang, nhạc thêm lớp, lều đổi màu.
- Sau mỗi đoạn khó là một khoảng yên tĩnh (checkpoint, hồi màu, đọc giấy).
- Checkpoint ở lều, giữa hầm băng và trước mỗi boss.

## 7. Scope

Game ngắn nhưng trọn vẹn: 3 khu, 3 viên đá, 2 boss, khoảng 18 phòng, 30–40 phút (gồm cả chết và backtrack).

| Hạng mục | Số lượng |
| --- | --- |
| Thời lượng chơi | 30–40 phút (20–25 phút đi thẳng) |
| Khu vực | 3 |
| Phòng | 16–20 |
| Viên đá | 3 (đỏ, vàng, xanh lá) |
| Boss | 2 (boss 1, boss cuối), mỗi boss 2 giai đoạn, 3–4 chiêu |
| Quái thường | 5–6 loại từ 3 khung gốc (đi tuần, lao tới, bắn xa) |
| Mẩu giấy | 3 |
| Checkpoint | 4 |
| Nhạc | 1 giai điệu, 3 lớp + bản ending |

**Phương án dự phòng:** nếu tuần 6 chưa xong năng lực xanh lá, biến viên xanh thành phần thưởng của ending (không phải năng lực).

## 8. SRS – Đặc tả yêu cầu

Mức ưu tiên: **P1** = bắt buộc cho bản phát hành, **P2** = nên có, **P3** = nếu dư thời gian.

### 8.1 Yêu cầu chức năng

| ID | Nhóm | Yêu cầu | Ưu tiên |
| --- | --- | --- | --- |
| FR-01 | Di chuyển | Nhân vật chạy trái/phải, nhảy với độ cao theo thời gian giữ phím | P1 |
| FR-02 | Di chuyển | Lăn né có khung bất tử ngắn và thời gian hồi | P1 |
| FR-03 | Chiến đấu | Combo rìu 2 nhịp và đòn nặng, có hitbox riêng | P1 |
| FR-04 | Chiến đấu | Hit-stop, rung màn hình, giật lùi khi trúng đòn | P1 |
| FR-05 | Chiến đấu | Hệ thống máu cho người chơi và quái, bất tử ngắn sau khi bị đánh | P1 |
| FR-06 | Quái | 3 khung AI: đi tuần, lao tới, bắn xa; mỗi khung có 1–2 biến thể | P1 |
| FR-07 | Boss | Boss 1 và boss cuối, mỗi boss 2 giai đoạn, 3–4 chiêu | P1 |
| FR-08 | Đá đỏ | Rìu lửa làm tan băng, đốt mạng nhện/dây leo, soi sáng hang tối | P1 |
| FR-09 | Đá vàng | Dash trên không 1 lần mỗi lần nhảy | P1 |
| FR-10 | Đá xanh | Gieo hạt tại điểm đất đánh dấu, mọc bệ đỡ cố định | P2 |
| FR-11 | Mở đường | Vật cản chỉ bị phá bởi đúng năng lực; trạng thái đã mở được lưu | P1 |
| FR-12 | Hồi màu | Khi nhận đá, màu loang ra từ tâm và giữ nguyên cho vùng đó | P1 |
| FR-13 | Mẩu giấy | Nhặt và đọc lại mẩu giấy; hiển thị chữ + bản đồ phác | P1 |
| FR-14 | Checkpoint | Hồi máu, lưu tiến trình, hồi sinh tại checkpoint gần nhất | P1 |
| FR-15 | Lưu game | Lưu vị trí checkpoint, viên đá, vật cản đã mở, mẩu giấy | P1 |
| FR-16 | Âm thanh | Nhạc thêm lớp theo số viên đá đã có | P2 |
| FR-17 | Giao diện | Menu chính, tạm dừng, thanh máu, biểu tượng viên đá | P1 |
| FR-18 | Kết thúc | Cảnh đặt 3 viên vào phiến đá, hồi màu toàn cảnh, credits | P1 |
| FR-19 | Tùy chọn | Chỉnh âm lượng, đổi phím | P3 |

### 8.2 Yêu cầu phi chức năng

| ID | Yêu cầu | Tiêu chí |
| --- | --- | --- |
| NFR-01 | Hiệu năng | Ổn định 60 FPS trên máy tầm trung |
| NFR-02 | Độ trễ điều khiển | Phản hồi input dưới 100 ms |
| NFR-03 | Điều khiển | Hỗ trợ bàn phím và tay cầm |
| NFR-04 | Độ rõ hình ảnh | Quái, bẫy, nền phân biệt được khi chuyển ảnh sang trắng đen |
| NFR-05 | Dễ hiểu | Người thử hiểu mục tiêu mà không cần hướng dẫn bằng chữ |
| NFR-06 | Thời gian tải | Chuyển phòng dưới 1 giây |
| NFR-07 | Độ ổn định | Không crash, không kẹt map trong toàn bộ lượt chơi |
| NFR-08 | Ngôn ngữ | Tiếng Việt (tiếng Anh là P3) |

**Câu hỏi mở:** đã chốt Unity 6.6 (6000.6.7f1) và nhóm 5 người; còn lại: nền tảng phát hành (itch.io, Steam?).

## 9. Vertical Slice và lịch triển khai

Bản chơi thử 15–20 phút (khu 1 + khu 2 đến hết boss 1) xong vào cuối tuần 5; toàn bộ nội dung chốt vào tuần 6.

**Nội dung vertical slice:**

1. Mở đầu ở lều: chặt cây, săn thú, học điều khiển (2–4 phút).
2. Rừng chết: quái thường, lối băng chặn (4–5 phút).
3. Nhặt đá đỏ, hồi màu lần đầu (1–2 phút).
4. Backtrack làm tan băng, xuống hầm băng (4–5 phút).
5. Boss 1 trên vách đá (3–5 phút).
6. Nhận đá vàng, mẩu giấy gợi mở chặng cuối.

**Lịch 8 tuần:**

| Tuần | Việc chính | Kết quả |
| --- | --- | --- |
| 1–2 | Di chuyển, chém, game feel (hộp xám) | Điều khiển thấy “vui” |
| 3 | Quái, máu, checkpoint, hồi sinh | Vòng chơi nhỏ hoàn chỉnh |
| 4 | Đá đỏ, vật cản, khu 1, mẩu giấy | Khu 1 chơi được |
| 5 | Khu 2, boss 1, đá vàng, backtrack | Vertical slice (gray-box) |
| 6 | Khu 3, đá xanh, boss cuối, ending | Feature freeze |
| 7 | Art thật, shader hồi màu, nhạc, âm thanh | Bản beta |
| 8 | Chơi thử 3–5 người, sửa lỗi, cân bằng, build | Bản phát hành |

**Tiêu chí thành công:** người thử hiểu mục tiêu không cần giải thích, thấy “wow” khi hồi màu, và muốn biết viên đá tiếp theo ở đâu.

**Rủi ro:**

| Rủi ro | Cách xử lý |
| --- | --- |
| Phạm vi phình ra | Ý tưởng mới vào danh sách “bản sau”, không làm trước tuần 6 |
| Boss tốn thời gian | Giới hạn 2 giai đoạn, 3–4 chiêu |
| Năng lực xanh lá trễ | Chuyển viên xanh thành phần thưởng ending |
| Game feel chưa tốt | Dành 2 tuần đầu chỉ cho di chuyển và chém |
| Art/âm thanh dồn cuối | Chốt bảng màu và phong cách từ tuần 1, dùng asset pack cho tile cơ bản |
