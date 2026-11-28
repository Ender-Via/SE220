# Credits và nguồn asset

File duy nhất ghi nguồn, giấy phép và ghi công cho mọi asset không do nhóm tự vẽ hoặc tự sáng tác.
Đáp ứng SRS NFR-16 ("mọi asset/âm thanh bên ngoài có ghi nguồn và giấy phép trong credits") và là nguồn cho màn hình credits (FR-NAR-04).

**Khi thêm asset mới:** thêm một dòng vào bảng tổng hợp ở mục 1 và một mục chi tiết ở mục 2 hoặc 3, trong cùng commit đưa asset vào.

## 1. Bảng tổng hợp

| Asset | Tác giả | Giấy phép | Ghi công | Dùng cho |
|---|---|---|---|---|
| [Pixel Valley – Winter Forest](#21-pixel-valley--winter-forest-16x16) | Kauzz | Miễn phí phi thương mại; thương mại nên ủng hộ | **Tác giả đề nghị** | Tileset khu 1 |
| [Skull Wolf Pixel Art](#22-skull-wolf-pixel-art) | atari-boy | Dùng thương mại, được sửa | Không bắt buộc, tác giả mong được ghi | Sói tuyết |
| [2D Pixel Art Spider Sprites](#23-2d-pixel-art-spider-sprites) | Elthen | CC BY-NC 4.0 + quyền thương mại, cấm NFT | Không bắt buộc | Nhện băng |
| [Crow Sprite](#24-crow-sprite) | Smithy Games | CC0 1.0 | Không cần | Quạ xám |
| [Bat Pixel art (FREE)](#25-bat-pixel-art-free) | QuimeraGames | CC0 (theo nhóm) – tác giả xác nhận dùng tự do | Không cần | Dơi hang |
| [Tinh linh sương](#31-tinh-linh-sương) | ChatGPT (AI) | Nhóm sở hữu | Không cần | Quái khu 2–3 |
| [Giáp băng](#32-giáp-băng) | ChatGPT (AI) | Nhóm sở hữu | Không cần | Quái khu 2 |
| [Dơi hang – khung vẽ thêm](#33-dơi-hang--khung-vẽ-thêm) | ChatGPT (AI) | Theo giấy phép `Bat.zip` | Theo `Bat.zip` | Quái khu 2 |

## 2. Asset bên thứ ba

Quy tắc chung: không sửa file gốc trong `Assets/ThirdParty/`. Mọi chỉnh sửa (đổi màu, cắt) làm bản sao trong `Assets/_Project/Art/`.

### 2.1. Pixel Valley | Winter Forest (16x16)

- **Tác giả:** Kauzz
- **Trang tải:** https://kauzz.itch.io/kpwf
- **Giá:** "Name your own price" (tự chọn giá, có thể 0)
- **Ngày đưa vào:** 2026-11-17
- **Thư mục:** `Assets/ThirdParty/WinterForestTileset/`
- **Điều khoản (theo trang itch.io):** miễn phí cho game phi thương mại; game thương mại thì tác giả đề nghị ủng hộ (tùy ý) hoặc mua; tác giả đề nghị **ghi công** để người chơi tìm tới trang. Trang không nói rõ việc chỉnh sửa hay phân phối lại.
- **Việc cần làm:**
  - [ ] Ghi tên vào credits trong game.
  - [ ] Nếu phát hành có thu tiền: ủng hộ tác giả trên itch.io trước khi phát hành.
- **Kỹ thuật:** `Enviroment.png` (giữ tên gốc, sai chính tả) cắt thành 74 ô 16×16 `SPR_WinterTerrain_r<hàng>_c<cột>` và 21 vật trang trí `SPR_WinterProp_<Tên>`. Tile và palette ở `Assets/_Project/Art/Environment/WinterForest/` (`PAL_WinterForest`, Cell Size 0,5).

### 2.2. Skull Wolf Pixel Art

- **Tác giả:** atari-boy
- **Trang tải:** https://atari-boy.itch.io/skull-wolf-pixel-art
- **Giá:** tải về không trả tiền
- **Ngày đưa vào:** 2026-11-20
- **Thư mục:** `Assets/ThirdParty/AtariBoy_SkullWolf/`
- **Dùng cho:** Sói tuyết (khu 1, khu 3)
- **Điều khoản (theo trang itch.io):** dùng cá nhân và thương mại; được chỉnh sửa; không bắt buộc ghi công nhưng tác giả rất mong được ghi và được nhắn tin báo.
- **Việc cần làm:**
  - [ ] Ghi tên vào credits trong game.
  - [ ] (Không bắt buộc) Nhắn tin báo tác giả khi game phát hành.
- **Kỹ thuật:** `SkullWolf_Sheet.png` (tên gốc `Massacre Sprite Sheet.png`) lưới 7×4 ô 64×64, 22 frame `SPR_SkullWolf_r<hàng>_c<cột>`: hàng 0 Idle (6), 1 Attack/vồ (5), 2 Hit (4), 3 Dead (7). `SkullWolf.aseprite` (tên gốc `Massacre.aseprite`) là file gốc 32 màu, dùng khi đổi bảng màu. Clip và Animator ở `Assets/_Project/Art/Characters/Enemies/SkullWolf/`.

### 2.3. 2D Pixel Art Spider Sprites

- **Tác giả:** Elthen
- **Trang tải:** https://elthen.itch.io/2d-pixel-art-spider-sprites
- **Giấy phép:** https://www.patreon.com/elthen/posts/licensing-27430241 (file "Elthen Assets License")
- **Ngày đưa vào:** 2026-11-20
- **Thư mục:** `Assets/ThirdParty/Spider_v2/`
- **Dùng cho:** Nhện băng (khu 1)
- **Điều khoản:** CC BY-NC 4.0 có bổ sung quyền **dùng trong dự án thương mại**; không phân phối lại hoặc bán lại asset; không bắt buộc ghi công; **cấm dùng trong mọi công nghệ blockchain** (NFT, P2E, tiền mã hóa), dù đã chỉnh sửa. Đã kiểm tra: không có bản giấy phép mới hơn bản "Bought Before 1 November 2025".
- **Việc cần làm:**
  - [ ] (Không bắt buộc) Ghi tên vào credits trong game.
  - [ ] Giữ repo private: không đưa file gốc lên nơi công khai.
- **Kỹ thuật:** `Spider_Sheet.png` lưới 9×16 ô 32×32, 86 frame `SPR_PixelSpider_r<hàng>_c<cột>`; hàng 8–15 trùng hàng 0–7. Clip dùng hàng 0 Idle, 1 Walk, 2 Jump, 3 Fall, 5 Hit, 6 Dead, 7 WebShot. Nhện rất nhỏ (~12 px): trong scene đặt Scale = 2. Clip và Animator ở `Assets/_Project/Art/Characters/Enemies/PixelSpider/`.

### 2.4. Crow Sprite

- **Tác giả:** Smithy Games
- **Trang tải:** https://smithygames.itch.io/crow-sprite
- **Giá:** miễn phí
- **Ngày đưa vào:** 2026-11-20
- **Thư mục:** `Assets/ThirdParty/SmithyGames_Crow/`
- **Dùng cho:** Quạ xám (khu 1)
- **Điều khoản:** CC0 1.0 Universal (Public Domain): dùng tự do, kể cả thương mại, được sửa, không cần ghi công. Tác giả chỉ đề nghị không nhận hình là của mình. Đã xác nhận trên trang itch.io (2026-11-29): mô tả và mục "Asset license: Creative Commons Zero v1.0 Universal".
- **Việc cần làm:**
  - [ ] (Tùy chọn) Ghi tên vào credits trong game.
- **Kỹ thuật:** `Crow.png` lưới 7×6 ô 48×48, 41 frame `SPR_GreyCrow_r<hàng>_c<cột>`. Hàng 5 (Fly) có frame chữ "CAW" ở cột 5, không đưa vào clip. Clip và Animator ở `Assets/_Project/Art/Characters/Enemies/GreyCrow/`.

### 2.5. Bat Pixel art (FREE)

- **Tác giả:** QuimeraGames
- **Trang tải:** https://quimeragames.itch.io/bat-pixel-art-free (file `Bat.zip`)
- **Giá:** "Name your own price", tác giả ghi "completely free"
- **Ngày đưa vào:** 2026-11-28
- **Điều khoản:** nhóm xếp vào CC0. Trên trang itch.io (đọc 2026-11-29) **không có mục giấy phép ghi rõ CC0**; khi được hỏi có dùng cho game thương mại không, tác giả trả lời trong phần bình luận: "Yes, you can use it freely."
- **Nội dung gốc:** khung 50×50, Idle (2), Fly (4), Fall (3), Hit (2), Dead (1), màu tím. File gốc **không** nằm trong project; chỉ có bản đã ghép dải và đổi sang bảng màu xám trong `Assets/_Project/Art/Characters/Enemies/CaveBat/` (dùng Idle, Fly, Fall, Hit).
- **Việc cần làm:**
  - [ ] (Tùy chọn) Ghi tên vào credits trong game.
  - [ ] (Tùy chọn) Chụp lại bình luận xác nhận của tác giả để lưu, phòng khi trang thay đổi.

## 3. Hình tạo bằng AI (ChatGPT, OpenAI)

Không phải asset bên thứ ba: tạo theo prompt của LEAD, nhóm sở hữu nội dung theo điều khoản của OpenAI. Ghi lại để minh bạch.

### 3.1. Tinh linh sương

- **Kiểu được chọn:** Shroud. **Ngày tạo:** 2026-11-27.
- **Bản gốc:** `Docs/Frost-Mist-Spirit-Concept-Sheet.png`, `Docs/Frost-Mist-Spirit-GIFs/`
- **Trong game:** `Assets/_Project/Art/Characters/Enemies/FrostSpirit/` gồm Idle, Telegraph, Shoot, Hit, Dead (cắt từ GIF, thu về pixel gốc ×3) và đạn `SPR_FrostSpirit_Shard` (cắt từ `06-manh-bang.gif`).

### 3.2. Giáp băng

- **Kiểu được chọn:** Glacier. **Ngày tạo:** 2026-11-27 đến 2026-11-28.
- **Bản gốc:** `Docs/Ice-Armor-Concept-Sheet.png`, `Docs/Ice-Armor-GIFs/`
- **Trong game:** `Assets/_Project/Art/Characters/Enemies/IceArmor/` gồm Block, ShieldBreak, Hit, Dead (cắt từ GIF, ×2) và thư mục `Glacier/` tách lớp thân/khiên cho Idle, Telegraph, Attack, Walk (`Body_Walk_v3`).

### 3.3. Dơi hang – khung vẽ thêm

- **Ngày tạo:** 2026-11-28. Vẽ dựa trên sprite dơi xám của `Bat.zip`, nên **giấy phép của `Bat.zip` vẫn áp dụng**.
- **Trong game:** `Assets/_Project/Art/Characters/Enemies/CaveBat/ActionStrips/` gồm Wake, Telegraph, Dive_v2, Dead.

## 4. Dòng credits đề xuất trong game

```
Asset bên ngoài
  Tileset "Pixel Valley – Winter Forest" – Kauzz (kauzz.itch.io)
  Sói tuyết – "Skull Wolf" – atari-boy (atari-boy.itch.io)
  Nhện băng – "2D Pixel Art Spider Sprites" – Elthen (elthen.itch.io)
  Quạ xám – "Crow Sprite" – Smithy Games (smithygames.itch.io)
  Dơi hang – "Bat Pixel art" – QuimeraGames (quimeragames.itch.io)
```

## 5. Việc còn mở

- [ ] **Winter Forest:** ủng hộ tác giả nếu phát hành có thu tiền (mục 2.1).
- [ ] **Nhạc và SFX:** chưa có, thêm vào đây khi ARTE chọn nguồn (câu hỏi mở trong SRS).
