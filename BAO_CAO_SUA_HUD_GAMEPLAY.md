# BÁO CÁO SỬA HUD GAMEPLAY (Portrait 1080×1920)

> **Dự án**: Endless Runner 3D (*Hero Rush*) — Unity 6 (`6000.5.9f1`), URP
> **Scene áp dụng**: `Assets/_Game/Scenes/Gameplay.unity` (GameplayCanvas)
> **Ngày**: 09/10/2026
> **Mục tiêu**: Làm đẹp HUD trong Gameplay bằng sprite `Assets/_Game/Art/UI/HUD/` theo yêu cầu gốc —
> ô điểm (⭐ + `D6`), ô xu (🪙 + RunCoins), nút pause, panel power-up (icon + thanh Filled),
> font trắng-đậm-viền tối, chỉ sửa `GameplayUI.cs` + UI trong GameplayCanvas.

---

## 1. Tình trạng trước khi sửa (lỗi tìm được)

Lần chỉnh trước **không có kết quả** vì nhiều lỗi chồng nhau. Chi tiết:

| # | Lỗi | Bằng chứng | Hệ quả |
|---|-----|-----------|--------|
| 1 | `SetupGameplayHUD.cs` crash vì tạo component **abstract**: `new GameObject(name, typeof(RectTransform), typeof(TMP_Text))` | `TMP_Text` là lớp abstract → Unity ném `ArgumentException` ngay ở lần tạo text đầu tiên (`ScoreValue`) | Tool chết giữa chừng, `SaveScene()` không chạy → **scene không bao giờ được cập nhật** |
| 2 | 6 icon trong `Assets/_Game/Art/UI/HUD/` import sai định dạng (`textureType: Default`, `spriteMode: 0`) | `icon_star`, `icon_coin`, `icon_pause`, `icon_shield`, `icon_jump_boots`, `icon_rocket` đều `textureType: 0` | `AssetDatabase.LoadAssetAtPath<Sprite>()` trả `null` → mọi icon HUD = null |
| 3 | `iconBoots` (và `iconShield`, `iconRocket`) trỏ nhầm sprite | `UISpritePatcher.cs` dùng `GUID_BOOTS = e79abcc6...` = **`icon_magnet.png`** | Nhặt Boots sẽ hiện **icon nam châm** thay vì giày |
| 4 | Nút Pause tạo ở trạng thái active nhưng `GameManager` **không có logic pause** | `GameManager.cs` không có `pause`/`Time.timeScale` | Nút bấm không làm gì (theo yêu cầu phải để inactive + báo lại) |
| 5 | `GameplayUI.cs` cấu hình `fillMethod = Radial360` nhưng không có UI object nào tồn tại | Scene YAML chỉ serialize các field **cũ** (`scoreText`, `coinText`, ...); các field Pretty HUD đều vắng → mặc định `null` | HUD vẫn là text cũ: `"Điểm: 24"`, `"New Text"` — không có pill/icon/thanh |
| 6 | Font yêu cầu ở `Assets/_Game/Art/UI/Fonts/` | **Folder này không tồn tại** trong dự án | Phải dùng font thay thế (xem mục 4.3) |

---

## 2. Các file đã sửa

| File | Loại thay đổi |
|------|---------------|
| `Assets/_Game/Editor/SetupGameplayHUD.cs` | **Viết lại toàn bộ** — tool tạo HUD vào scene (menu + batch) |
| `Assets/_Game/Editor/SetupHUDSprites.cs` | **Viết lại** — đảm bảo 7 file HUD là Sprite + 9-slice pill |
| `Assets/_Game/Scripts/UI/GameplayUI.cs` | **Viết lại phần HUD** — giữ nguyên 100% tên field public cũ |
| `Assets/_Game/Editor/UISpritePatcher.cs` | Sửa 3 hằng GUID icon power-up sang folder HUD |
| `Assets/_Game/Art/UI/HUD/icon_star.png.meta` | `textureType: Sprite`, `spriteMode: Single`, `alphaIsTransparency: 1` |
| `Assets/_Game/Art/UI/HUD/icon_coin.png.meta` | *(như trên)* |
| `Assets/_Game/Art/UI/HUD/icon_pause.png.meta` | *(như trên)* |
| `Assets/_Game/Art/UI/HUD/icon_shield.png.meta` | *(như trên)* |
| `Assets/_Game/Art/UI/HUD/icon_jump_boots.png.meta` | *(như trên)* |
| `Assets/_Game/Art/UI/HUD/icon_rocket.png.meta` | *(như trên)* |

> `hud_pill.png` đã đúng từ trước: Sprite + 9-slice `border = (124.5, 0, 124.5, 0)` (texture 1003×249).

---

## 3. Layout HUD mới (Canvas reference 1080×1920, chừa ~96px phía trên)

```
┌──────────────────────────────────────────────┐
│  [⏸ Pause]                     ⭐ 000042     │  ← Pause góc trái (ẨN), Score pill góc phải
│   (inactive)                  🪙 128         │  ← Coin pill ngay dưới Score pill, canh phải
│                                              │
│              ┌───────────────┐               │
│              │ 🚀 ▓▓▓▓▓░░░░░  │               │  ← PowerUp panel giữa trên (ẩn khi không có item)
│              └───────────────┘               │
└──────────────────────────────────────────────┘
```

Thông số cụ thể:

| Thành phần | Anchor / Vị trí | Kích thước | Nội dung |
|---|---|---|---|
| **ScorePill** | Top-Right, `(-24, -96)` | `240 × 74` | Nền `hud_pill` (9-slice, đen 60%) |
| ├ ScoreIcon | trong pill, trái `(16, 0)` | `44 × 44` | `icon_star` |
| └ ScoreValue | trong pill, phải `(-18, 0)` | `150 × 56` | `Score.ToString("D6")`, font 42, đậm, viền tối |
| **CoinPill** | Top-Right, `(-24, -182)` | `240 × 66` | Nền `hud_pill` |
| ├ CoinPillIcon | trong pill, trái `(14, 0)` | `40 × 40` | `icon_coin` |
| └ CoinValue | trong pill, phải `(-18, 0)` | `150 × 50` | `RunCoins`, font 36 |
| **PauseButton** | Top-Left, `(24, -96)` | `66 × 66` | `icon_pause` — **inactive** (xem mục 5) |
| **PowerUpPanel** | Top-Center, `(0, -96)` | `260 × 76` | Nền `hud_pill`, ẩn khi `CurrentType == null` |
| ├ PowerUpPanelIcon | trái `(18, 0)` | `48 × 48` | `icon_shield` / `icon_jump_boots` / `icon_rocket` theo loại |
| └ PowerUpBarTrack | phải `(-16, 0)` | `170 × 16` | Nền đen 55% |
| &nbsp;&nbsp;└ PowerUpBarFill | stretch trong track | — | `Image.Type.Filled`, **Horizontal**, vàng gold, `remaining / duration` |

---

## 4. Chi tiết sửa theo từng file

### 4.1 `SetupGameplayHUD.cs` (tool chính)
- **Sửa crash**: dùng `TextMeshProUGUI` (lớp con cụ thể) thay cho `TMP_Text` abstract.
- Tạo đầy đủ hierarchy theo bảng mục 3; gán trực tiếp vào các field của `GameplayUI`.
- **Idempotent**: chạy lại nhiều lần không nhân bản — `CleanupOldHUD()` xóa object cũ trước khi tạo mới.
- Giữ nguyên `CanvasScaler`: `ScaleWithScreenSize`, reference `1080×1920`, `matchWidthOrHeight = 0` (không đổi cấu hình scene đang chạy).
- **9-slice**: `hud_pill` set `Image.Type.Sliced` cho mọi pill/panel.
- **Nút Pause**: tạo đúng vị trí góc trái nhưng `SetActive(false)` ngay (vì chưa có logic pause).
- **Thanh Filled**: `FillMethod.Horizontal`, `fillOrigin = 0` (trái → phải), `fillAmount = remaining/duration`.
- Thêm 2 chế độ chạy:
  - Menu: `Tools/Setup Gameplay HUD (Portrait)`
  - Batch: `-executeMethod SetupGameplayHUD.SetupAndExit` → tự `Verify()` rồi thoát mã `0`/`1` (dùng để kiểm tra tự động không cần mở Editor).
- Thêm `Verify()`: kiểm tra **tất cả** field HUD + sprite/font khác null, in rõ field nào thiếu.

### 4.2 `GameplayUI.cs`
- **Giữ nguyên toàn bộ tên field public** (cả field cũ `scoreText/coinText/powerUpText/iconShield/...` và field mới
  `hudFont/hudPillSprite/iconStar/iconCoin/iconPause/scorePillBg/scoreIcon/scoreValueText/coinPillBg/coinPillIcon/coinValueText/pauseButton/pauseButtonIcon/powerUpPanel/powerUpPanelIcon/powerUpFillImage/powerUpTimeText`)
  → **không mất reference** đang có trong scene/prefab.
- `Awake()` → `InitializePrettyHUD()`: cấu hình pill (Sliced, đen 60%), icon, text (Bold, trắng, viền tối), thanh Filled, ẩn panel.
- `Update()`: cập nhật `Score.ToString("D6")` và `RunCoins`; tự ẩn 3 text cũ khi HUD mới đang hoạt động (fallback về HUD cũ nếu chưa chạy tool).
- `RefreshPowerUpPanel()` (mỗi frame, không phụ thuộc hoàn toàn vào event):
  - `CurrentType == null` → ẩn panel.
  - Có item → hiện panel + đúng icon + thanh `remaining/duration`.
  - **Shield không có timer** (`duration <= 0`) → chỉ hiện icon, ẩn thanh (đúng yêu cầu).
- **Chống NullReference toàn diện**: mọi truy cập field đều guard `!= null` → chạy được cả khi chưa chạy tool.
- Gỡ 3 method cũ chỉ dùng nội bộ (`UpdatePowerUpPanelFill`, `UpdateNewPowerUpPanel`, `ApplyFontToLegacyTexts`) — đã kiểm tra không có nơi nào khác gọi.
- **Không sửa** script gameplay nào khác (PlayerController, PowerUpManager, GameManager, TrackManager… giữ nguyên).

### 4.3 Font (trắng, đậm, viền tối)
- Folder `Assets/_Game/Art/UI/Fonts/` **không tồn tại** → dùng:
  - `LiberationSans SDF` (`Assets/TextMesh Pro/Resources/Fonts & Materials/`)
  - Material preset `LiberationSans SDF - Outline` → **viền tối**
  - `FontStyles.Bold` → **đậm**, `Color.white` → **trắng**
- Nếu sau này bạn thêm font riêng vào folder đó, chỉ cần kéo vào field `hudFont` là xong.

### 4.4 `UISpritePatcher.cs`
- `GUID_BOOTS`: `e79abcc6...` (icon_magnet — **sai**) → `f524cf87...` (`HUD/icon_jump_boots.png`)
- `GUID_SHIELD` → `71349bda...`, `GUID_ROCKET` → `fa29d06c...` (đều là sprite trong folder HUD)

---

## 5. Việc cần bạn xác nhận / lưu ý

1. **Nút Pause đang được để INACTIVE** vì `GameManager` **chưa có logic tạm dừng** (không có `Time.timeScale`/pause).
   Khi bạn bổ sung pause, chỉ cần xóa dòng `pauseButton.gameObject.SetActive(false);` trong `GameplayUI.InitializePrettyHUD()` (có ghi chú `TODO` ngay tại đó).
2. **Font**: chưa có file trong `Assets/_Game/Art/UI/Fonts/` → tạm dùng LiberationSans SDF. Cần bạn gửi font asset nếu muốn font riêng.
3. **Thanh Filled không có nhãn số giây** vì yêu cầu gốc chỉ liệt kê icon + thanh.
   Field `powerUpTimeText` vẫn được giữ sẵn (đang null) — muốn hiện `3.5s` thì nói tôi thêm.

---

## 6. Cách chạy setup (tạo HUD vào scene)

> ⚠️ HUD **chỉ được tạo vào scene khi chạy tool**. Sửa code thôi chưa đủ.

**Cách A — Trong Unity (nhanh nhất):**
1. Mở Unity, đợi recompile xong (không có lỗi đỏ trong Console).
2. Menu: **`Tools → Setup Gameplay HUD (Portrait)`**
3. Console hiện: `[SetupGameplayHUD] ✅ Setup HUD hoàn tất...` và **không có `[Verify] ❌`**.
4. `Ctrl+S` lưu scene.

**Cách B — Batchmode (đóng Unity trước):**
```powershell
& "D:\unity\Editor\6000.5.9f1\Editor\Unity.exe" `
  -batchmode -projectPath "D:\game\3D EL game" `
  -executeMethod SetupGameplayHUD.SetupAndExit `
  -logFile "C:\Users\ADMIN\AppData\Local\Temp\opencode\unity_hud_setup.log"
```
Mã thoát `0` = OK, `1` = còn field thiếu (đọc `[Verify] ❌` trong log).

---

## 7. Checklist nghiệm thu

| # | Hạng mục | Cách kiểm tra |
|---|----------|---------------|
| 1 | Game view `1080×1920` Portrait thấy đúng layout | Play scene Gameplay |
| 2 | Ô điểm góc phải trên = ⭐ + 6 chữ số (`D6`) | Chạy, quan sát điểm tăng |
| 3 | Ô xu ngay dưới, canh phải = 🪙 + RunCoins | Nhặt xu, số tăng |
| 4 | Panel power-up giữa trên: ẩn khi không có item | Lúc đầu không thấy panel |
| 5 | Nhặt item → hiện đúng icon + thanh chạy đầy dần về 0 | Nhặt Shield/Boots/Rocket |
| 6 | Nhặt Boots → icon **giày**, không phải nam châm | Kiểm tra icon panel |
| 7 | Không còn chữ cũ `"Điểm: 24"` / `"New Text"` đè lên | Quan sát HUD |
| 8 | Console **không** có NullReference / Missing | Xem Console khi Play |
| 9 | Nút pause không hiện (đúng vì chưa có logic pause) | Góc trái trên trống |

---

## 8. Ghi chú thêm

- Mô hình AI của tôi **không đọc được file ảnh** — mọi ảnh chụp/paste hình cần kèm **text** (log Console, mô tả) để tôi xử lý.
- Toàn bộ thay đổi nằm ngoài các script gameplay (không đụng `PlayerController`, `TrackManager`, `PowerUpManager`, `GameManager`, `CurrencyManager`).
- `_Recovery` / scene `MainMenu`, `Splash`, `Loading` không bị ảnh hưởng.
