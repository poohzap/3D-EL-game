// SlideBendEffect.cs
using UnityEngine;

// Tạo tư thế trượt kiểu "nằm nghiêng trườn tới" (baseball slide) — nghiêng
// cả model 1 góc cố định + hạ thấp xuống, giữ nguyên tư thế suốt lúc trượt.
public class SlideBendEffect : MonoBehaviour
{
    [Tooltip("Kéo GameObject chứa model đang hiển thị vào đây")]
    public Transform modelSlot;

    [Tooltip("Góc nghiêng người khi trượt (độ) — giữ nguyên, không xoay vòng")]
    public float slideTiltAngle = 75f;

    [Tooltip("Hạ thấp người xuống bao nhiêu khi trượt (đơn vị world)")]
    public float slideDropHeight = 0.5f;

    [Tooltip("Tốc độ chuyển vào/ra tư thế trượt")]
    public float transitionSpeed = 10f;

    private PlayerController playerController;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalPosition;
    private float currentTilt;
    private float currentDrop;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (modelSlot != null)
        {
            baseLocalRotation = modelSlot.localRotation;
            baseLocalPosition = modelSlot.localPosition;
        }
    }

    void LateUpdate()
    {
        if (modelSlot == null || playerController == null) return;

        bool sliding = playerController.IsSliding;
        float targetTilt = sliding ? slideTiltAngle : 0f;
        float targetDrop = sliding ? slideDropHeight : 0f;

        currentTilt = Mathf.MoveTowards(currentTilt, targetTilt, transitionSpeed * 20f * Time.deltaTime);
        currentDrop = Mathf.MoveTowards(currentDrop, targetDrop, transitionSpeed * Time.deltaTime);

        modelSlot.localRotation = baseLocalRotation * Quaternion.AngleAxis(currentTilt, Vector3.right);
        modelSlot.localPosition = baseLocalPosition - Vector3.up * currentDrop;
    }
}