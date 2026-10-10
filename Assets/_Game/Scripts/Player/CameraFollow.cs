// CameraFollow.cs — Assets/_Game/Scripts/Player/CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Tham chiếu (kéo trong Inspector hoặc auto-find)")]
    public Transform target;              // Player Transform
    public PowerUpManager targetPowerUps; // Player PowerUpManager

    [Header("Cấu hình góc nhìn")]
    [Tooltip("Khoảng cách camera so với player (X, Y, Z)")]
    public Vector3 offset = new Vector3(0f, 3.2f, -5.5f);

    [Tooltip("Camera nâng thêm bao nhiêu khi bay")]
    public float flyExtraHeight = 3.5f;

    [Tooltip("Góc nghiêng cố định của camera (độ) - giúp góc nhìn luôn ổn định tuyệt đối")]
    public float pitchAngle = 16f;

    [Tooltip("Độ mượt khi bám theo X (đổi làn)")]
    public float smoothSpeedX = 14f;

    [Tooltip("Độ mượt khi bám theo Y")]
    public float smoothSpeedY = 8f;

    private float currentX;
    private float currentY;
    private PlayerController playerController;

    void Awake()
    {
        FindTargetIfNull();
    }

    void Start()
    {
        FindTargetIfNull();
        if (target != null)
        {
            currentX = target.position.x * 0.6f;
            currentY = target.position.y + offset.y;
            SnapToTarget();
        }
    }

    void FindTargetIfNull()
    {
        if (target == null)
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null)
            {
                playerController = pc;
                target = pc.transform;
                if (targetPowerUps == null)
                    targetPowerUps = pc.GetComponent<PowerUpManager>();
            }
        }
        else if (playerController == null)
        {
            playerController = target.GetComponent<PlayerController>();
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        currentX = target.position.x * 0.6f;
        float targetBaseY = target.position.y;
        if (targetBaseY < -2f) targetBaseY = 0f; // Chống rơi xuống vực kéo cam theo

        float extraY = (targetPowerUps != null && targetPowerUps.IsFlying) ? flyExtraHeight : 0f;
        currentY = targetBaseY + offset.y + extraY;

        transform.position = new Vector3(currentX, currentY, target.position.z + offset.z);
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTargetIfNull();
            if (target == null) return;
        }

        // 1. Trục Z: Khóa chặt khoảng cách sau lưng player (offset.z = -5.5m)
        // Dù chạy nhanh đến đâu thì player luôn nằm ở đúng cự ly chuẩn, không bao giờ bị tụt lại hay vượt lên
        float desiredZ = target.position.z + offset.z;

        // 2. Trục X: Lướt nhẹ nhàng theo làn xe của player (giảm rung lắc khi đổi làn gấp)
        float targetX = target.position.x * 0.6f;
        currentX = Mathf.Lerp(currentX, targetX, smoothSpeedX * Time.deltaTime);

        // 3. Trục Y: Bám theo mặt đất / độ cao bay, có chặn min để cam không bao giờ cắm xuống đất
        float playerY = target.position.y;
        if (playerY < 0f) playerY = 0f; // Không bao giờ cho Y âm ảnh hưởng camera

        // Khi chỉ nảy bước nhỏ với Boots (không phải nhảy cao chủ động hay bay),
        // giữ camera ổn định ở mặt đường (0f) để góc nhìn không bị rung lắc
        if (playerController != null && playerController.HasBoots && !playerController.IsJumping && !playerController.IsFlying)
        {
            playerY = 0f;
        }

        float extraY = (targetPowerUps != null && targetPowerUps.IsFlying) ? flyExtraHeight : 0f;
        float desiredY = playerY + offset.y + extraY;
        currentY = Mathf.Lerp(currentY, desiredY, smoothSpeedY * Time.deltaTime);

        // Gán vị trí
        transform.position = new Vector3(currentX, currentY, desiredZ);

        // Góc quay cố định hướng về chân trời: hoàn toàn loại bỏ lỗi giật/chúi camera xuống đất
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }
}