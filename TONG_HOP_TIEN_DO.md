# BÁO CÁO TỔNG HỢP TIẾN ĐỘ & CÁC THAY ĐỔI ĐÃ TRIỂN KHAI

> **Dự án**: Endless Runner 3D (*Hero Rush*)  
> **Nền tảng**: Unity 6 (6000.5.9f1) — Universal Render Pipeline (URP)  
> **Workspace**: `D:\game\3D EL game`  
> **Thời gian cập nhật**: 09/10/2026  

---

## 1. Hệ thống Obstacle & Phân loại hành vi mới

Đã triển khai hệ thống vật cản đa dạng với cơ chế va chạm chuyên biệt, tích hợp trực tiếp 3D model từ thư mục `Assets/_Game/Art/test/`:

| Tên Obstacle | 3D Model | Prefab | Cấu hình & Hành vi |
|---|---|---|---|
| **Barrier_Single** | `Barrier_Single.fbx` | [`Obstacle_BarrierSingle.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Obstacles/Obstacle_BarrierSingle.prefab) | `ObstacleData` = `JumpOnly`<br>• **Bắt buộc phải nhảy** mới vượt qua được.<br>• Nếu trượt hoặc chạy bộ đâm vào sẽ mất mạng. |
| **TrafficBarrier_2** | `TrafficBarrier_2.fbx` | [`Obstacle_TrafficBarrier2.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Obstacles/Obstacle_TrafficBarrier2.prefab) | `ObstacleData` = `JumpOrSlide`<br>• **Nhảy hoặc Trượt (Slide)** đều vượt qua an toàn.<br>• Đã phóng to kích thước model thêm 35-40% (`Scale = 1.35 - 1.4`), BoxCollider `(2.8, 1.0, 0.7)` chắn vừa vặn làn đường. |
| **Shipping Container** | `Shipping Container.fbx` | [`Obstacle_ShippingContainer.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Obstacles/Obstacle_ShippingContainer.prefab) | `ObstacleData` = `RequiresJumpBoots`<br>• **Không thể nhảy qua bình thường** (chiều cao 2.5m). Bắt buộc phải có hiệu ứng **Boots (Giày bật nhảy)** mới nhảy qua được.<br>• Đã sửa góc xoay nằm ngang bẹp xuống và trải dài dọc theo trục Z của đường chạy (`Rotation: 0, 0, 0`), BoxCollider `(2.6, 2.5, 6.2)`. |

* Các script liên quan:
  * [`ObstacleData.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/World/ObstacleData.cs): Định nghĩa enum `ObstacleBehavior` (`JumpOnly`, `JumpOrSlide`, `RequiresJumpBoots`).
  * [`PlayerController.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Player/PlayerController.cs): Kiểm tra điều kiện khi va chạm trong `EvaluateObstacleBehavior()`.

---

## 2. Hệ thống Vật phẩm (PowerUp) & Cơ chế bay Rocket

Đã chuyển đổi toàn bộ vật phẩm sang model 3D thực tế và tinh chỉnh kích thước chuẩn:

| Vật phẩm | 3D Model | Prefab | Cơ chế & Tinh chỉnh |
|---|---|---|---|
| **Rocket** | `Rocket.fbx` | [`PowerUp_Rocket.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/PowerUps/PowerUp_Rocket.prefab) | • **Thời lượng**: 10 giây bay.<br>• **Thu nhỏ kích thước**: Scale giảm về `0.15` (thay vì kích thước khổng lồ 10m ban đầu), tạo thành item gọn gàng (~1m).<br>• Đầu tên lửa nghiêng nhẹ hướng lên trên. |
| **Boots** | `Boots.fbx` | [`PowerUp_JumpBoots.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/PowerUps/PowerUp_JumpBoots.prefab) | • Tăng lực nhảy thêm 1.5×, giúp nhảy vượt qua Shipping Container.<br>• Scale model được chuẩn hóa về `0.3`. |
| **Shield** | `shield.fbx` | [`PowerUp_Shield.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/PowerUps/PowerUp_Shield.prefab) | • Cung cấp **1 mạng ảo** (`HasVirtualLife`). Khi va chạm sai điều kiện, mạng ảo hấp thụ 1 lần va chạm qua `TryAbsorbHit()`, người chơi không chết.<br>• Scale model chuẩn hóa về `0.35`. |
| **Coin** | `Coin.fbx` | [`Coin.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Coins/Coin.prefab) | • Model đồng xu 3D mạ vàng URP Lit Shader, xoay dọc theo làn chạy, có tag `Coin`. |

### Cơ chế Spawn Coin trên không khi có Rocket
Nâng cấp [`TrackManager.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/World/TrackManager.cs):
1. **Khi item Rocket xuất hiện trên đường (`SpawnPowerUp`)**: Sinh ngay lập tức dãy coins trên không tại tile chứa Rocket (`SpawnFlightCoins(content)`).
2. **Khi Player nhặt / kích hoạt Rocket**: Hàm `ConvertAheadTilesForFlight()` tự động duyệt các tile phía trước người chơi, xóa sạch vật cản dưới đất và sinh dãy coin trên trời ngay tức khắc.
3. **Trong suốt thời gian bay (10 giây)**: Không sinh bất kỳ vật cản nào dưới đất, chỉ sinh coins trên không.

---

## 3. Khắc phục triệt để lỗi Góc nhìn Camera (Camera Follow)

* **Hiện tượng lỗi trước đó**:
  * Chạy được một đoạn (tốc độ tăng từ 8 lên 20 m/s) thì góc camera bị chúi xuống đất hoặc biến thành góc nhìn từ trên nóc (top-down), mất dấu nhân vật.
  * Khi bắt đầu ván, camera bị xoay nghiêng lệch `(Z: 41°, Y: 40.7°, X: -4.8m)`.
* **Giải pháp đã thực hiện trong [`CameraFollow.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Player/CameraFollow.cs)**:
  * **Cố định góc quay (Fixed Pitch Angle = 16°)**: Loại bỏ hoàn toàn việc dùng `transform.LookAt()` mỗi frame (vốn gây chúi camera khi camera bị tụt lại phía sau hoặc khi player nhảy). Góc quay luôn hướng thẳng chân trời như *Subway Surfers*.
  * **Khóa cứng cự ly trục Z**: `desiredZ = target.position.z + offset.z` (`-5.5m`), camera luôn bám sát lưng nhân vật với khoảng cách cố định dù nhân vật tăng tốc đến tối đa.
  * **Lướt êm ái theo trục X**: Lướt mượt theo chuyển động đổi làn của nhân vật (`smoothSpeedX = 14f`).
  * **Chặn đáy trục Y**: Giữ nguyên độ cao ổn định `offset.y = 3.2m`, có cơ chế chặn `playerY < 0f` để camera không bao giờ bị kéo xuống vực. Khi bay Rocket, camera tự nâng thêm `+3.5m`.
  * **Snap vị trí ban đầu**: Hàm `SnapToTarget()` khóa chặt camera ngay từ frame đầu tiên.
* **Scene [`Gameplay.unity`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scenes/Gameplay.unity)**:
  * Đặt lại Transform mặc định của **Main Camera**: `Position = (0, 4, -6)`, `Rotation = (18°, 0, 0)`.

---

## 4. Tinh chỉnh Tỉ lệ Model Nhân vật (Player)

* **Yêu cầu**: Thu nhỏ model nhân vật cho cân xứng với làn đường và chướng ngại vật.
* **Thực hiện**:
  * [`PlayerSkinApplier.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Shop/PlayerSkinApplier.cs):
    * Thêm thuộc tính `modelScale = 0.75f` (thu nhỏ 25% kích thước model nhân vật khi spawn từ SkinDatabase).
    * Đồng bộ thu nhỏ `CharacterController`: chiều cao giảm còn `1.6m` (thay vì 2m), bán kính `0.38m`, tâm `(0, 0.8, 0)`.
  * [`Gameplay.unity`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scenes/Gameplay.unity):
    * Đặt `LocalScale` của GameObject con `ModelSlot` về `{0.75, 0.75, 0.75}`.

---

## 5. Cơ chế Cộng dồn Xu vào Tổng ví (Wallet) sau mỗi ván

* **Vấn đề trước đó**: Xu nhặt trong ván đôi khi không được lưu vào tổng số coin khi thoát ra Menu hoặc khi chơi lại.
* **Giải pháp đã thực hiện**:
  * [`CurrencyManager.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Core/CurrencyManager.cs):
    * Hoàn thiện hàm `CommitRunCoinsToWallet()`: Cộng dồn toàn bộ `RunCoins` vào `TotalCoins` (`PlayerPrefs`), gọi `PlayerPrefs.Save()`, xuất log kiểm tra và reset `RunCoins = 0`.
  * [`GameManager.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Core/GameManager.cs):
    * Bổ sung gọi `CurrencyManager.CommitRunCoinsToWallet()` tại **tất cả** các trường hợp kết thúc lượt chơi:
      * Khi Game Over (`EndGame()`).
      * Khi bấm chơi lại (`RestartGame()`).
      * Khi bấm về Menu chính (`GoToMenu()`).
      * Khi thoát ứng dụng (`OnApplicationQuit()`).

---

## 6. Sửa các lỗi biên dịch & Đồng bộ Prefab vào Scene

1. **Fix compiler error CS1061**:
   * Sửa [`UISpritePatcher.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Editor/UISpritePatcher.cs) đồng bộ trường sprite `iconMagnet` $\rightarrow$ `iconBoots`.
2. **Sửa Tag Coin**:
   * Cập nhật tag từ `Untagged` thành `Coin` trên [`Coin.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Coins/Coin.prefab) để `PlayerController.OnTriggerEnter` nhận diện nhặt xu.
3. **Đăng ký đầy đủ Prefab vào TrackManager trong Scene [`Gameplay.unity`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scenes/Gameplay.unity)**:
   * `obstaclePrefabs`: [`Obstacle_BarrierSingle`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Obstacles/Obstacle_BarrierSingle.prefab), [`Obstacle_TrafficBarrier2`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Obstacles/Obstacle_TrafficBarrier2.prefab), [`Obstacle_ShippingContainer`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Obstacles/Obstacle_ShippingContainer.prefab).
   * `coinPrefab`: [`Coin.prefab`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/Coins/Coin.prefab).
   * `powerUpPrefabs`: [`PowerUp_JumpBoots`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/PowerUps/PowerUp_JumpBoots.prefab), [`PowerUp_Rocket`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/PowerUps/PowerUp_Rocket.prefab), [`PowerUp_Shield`](file:///D:/game/3D%20EL%20game/Assets/_Game/Prefabs/PowerUps/PowerUp_Shield.prefab).

---

## 7. Cơ chế Bước chạy Nảy nhẹ khi nhặt Boots (Bouncy Steps)

* **Yêu cầu**: Khi nhặt được "Boots", cho nhân vật chạy mỗi bước lại nảy lên một chút (thấp hơn khi Jump) cho đến khi Boots hết hiệu lực.
* **Chi tiết triển khai**:
  1. [`PlayerController.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Player/PlayerController.cs):
     * Thêm tham số `bootsStepBounceForce = 4.0f` (độ cao nảy ~0.32m, nhẹ nhàng vui nhộn, thấp hơn rõ rệt so với cú nhảy thường `jumpForce = 9.0f` cao 1.62m và cú siêu nhảy Boots `13.5f` cao 3.65m).
     * Khi `HasBoots` đang có hiệu lực (`PowerUpManager.IsJumpBoosted`), mỗi lần tiếp đất khi chạy bộ (`controller.isGrounded && !isSliding`), nhân vật tự động bật nảy nhẹ bước tiếp theo.
     * Khi Boots hết thời lượng (8s), nhân vật tiếp đất mượt mà và chạy bình thường trở lại.
     * **Độ nhạy điều khiển (Responsiveness)**:
       * Cho phép người chơi vuốt Jump ngay giữa nhịp nảy bước của Boots mà không bị hụt lệnh.
       * Khi vuốt Slide (trượt) giữa cú nảy/nhảy, nhân vật lập tức rơi nhanh xuống đất để thực hiện cú trượt an toàn.
       * Trong `EvaluateObstacleBehavior`: Phân biệt rõ cú nhảy cao chủ động (`isJumping`) với nhịp nảy bước chạy bộ; người chơi vẫn phải bấm Jump để vượt qua rào cản hoặc Shipping Container.
  2. [`CameraFollow.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Player/CameraFollow.cs):
     * Triệt tiêu rung lắc trục Y của camera trong lúc nhân vật nảy bước chân nhỏ, giữ camera vững chãi trên mặt đường giúp người chơi không bị chóng mặt mà vẫn quan sát rõ nhân vật nảy bật nhịp nhàng.
  3. [`SwipeInputController.cs`](file:///D:/game/3D%20EL%20game/Assets/_Game/Scripts/Player/SwipeInputController.cs):
     * Bổ sung hỗ trợ phím `W / Space` (Jump), `S` (Slide), `A / D` (Đổi làn) giúp kiểm thử trên Editor tiện lợi.

