// SlideBendEffect.cs
using UnityEngine;

// Nghiêng người về trước khi trượt — chỉ xoay, KHÔNG dịch Position.
// Vì pivot của model KayKit nằm ở lòng bàn chân, xoay quanh đó tự nhiên khiến
// thân trên đổ xuống thấp, trong khi chân vẫn đứng đúng mặt track — không bị chìm.
public class SlideBendEffect : MonoBehaviour
{
    [Tooltip("Kéo GameObject chứa model đang hiển thị vào đây (ModelSlot)")]
    public Transform modelSlot;

    [Tooltip("Góc nghiêng người về trước khi trượt (độ)")]
    public float slideTiltAngle = 45f;

    [Tooltip("Tốc độ xoay vào/ra tư thế trượt (độ/giây)")]
    public float tiltSpeed = 480f;

    private PlayerController playerController;
    private Quaternion baseLocalRotation;
    private float currentTilt;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (modelSlot != null) baseLocalRotation = modelSlot.localRotation;
    }

    void LateUpdate()
    {
        if (modelSlot == null || playerController == null) return;

        float targetTilt = playerController.IsSliding ? slideTiltAngle : 0f;
        currentTilt = Mathf.MoveTowards(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        modelSlot.localRotation = baseLocalRotation * Quaternion.AngleAxis(currentTilt, Vector3.right);
    }
}