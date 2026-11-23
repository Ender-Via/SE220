# Nguồn asset: 2D Pixel Art Spider Sprites

- **Tác giả:** Elthen
- **Trang tải:** https://elthen.itch.io/2d-pixel-art-spider-sprites
- **Giấy phép:** https://www.patreon.com/elthen/posts/licensing-27430241 (file "Elthen Assets License")
- **Ngày đưa vào project:** 2026-11-20
- **Dùng cho:** Nhện băng (khu 1)

## Điều khoản sử dụng (theo file giấy phép của tác giả)

- CC BY-NC 4.0 có bổ sung quyền: **được dùng trong dự án thương mại**.
- Không được phân phối lại (upload asset lên trang khác) hoặc bán lại asset.
- Không bắt buộc ghi công, nhưng tác giả rất cảm kích nếu có.
- Cấm tuyệt đối dùng trong bất kỳ công nghệ liên quan blockchain (NFT, P2E, tiền mã hóa), dù đã chỉnh sửa.
- Đã kiểm tra: không có bản giấy phép mới hơn bản "Bought Before 1 November 2025".

## Việc nhóm cần làm

- [ ] Ghi tên "Elthen – 2D Pixel Art Spider Sprites (https://elthen.itch.io/2d-pixel-art-spider-sprites)" vào credits (không bắt buộc).
- [ ] Giữ repo private: không đưa file gốc lên nơi công khai.

## Ghi chú kỹ thuật

- Không sửa ảnh gốc trong thư mục này. Mọi chỉnh sửa (đổi màu, cắt) làm bản sao trong `Assets/_Project/Art/`.
- `Spider_Sheet.png` (tên gốc `Spider Sprite Sheet.png`): lưới 9×16 ô 32×32, cắt trong `.meta` thành 86 frame `SPR_PixelSpider_r<hàng>_c<cột>`. Hàng 8–15 trùng y hệt hàng 0–7, chỉ dùng hàng 0–7.
- Hàng dùng trong clip: 0 Idle, 1 Walk, 2 Jump, 3 Fall (1 frame, rơi từ trần), 5 Hit, 6 Dead, 7 WebShot. Hàng 4 không dùng.
- Nhện rất nhỏ (~12 px): trong scene đặt Scale = 2 (giữ PPU 32 theo quy ước).
- Clip và Animator: `Assets/_Project/Art/Characters/Enemies/PixelSpider/`.
