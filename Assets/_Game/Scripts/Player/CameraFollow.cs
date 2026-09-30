// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Tham chiếu (kéo trong Inspector)")]
    public Transform target;              // kéo Player vào đây
    public PowerUpManager targetPowerUps; // kéo component PowerUpManager của Player vào đây

    [Header("Cấu hình")]
    public Vector3 offset = new Vector3(0f, 4f, -6f);
    public float flyExtraHeight = 3f; // camera nâng thêm bao nhiêu khi Player đang bay
    public float smoothTime = 0.15f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredOffset = offset;
        if (targetPowerUps != null && targetPowerUps.IsFlying)
            desiredOffset += Vector3.up * flyExtraHeight;

        Vector3 desiredPos = target.position + desiredOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}