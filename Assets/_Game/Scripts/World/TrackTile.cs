// TrackTile.cs — Assets/_Game/Scripts/World/TrackTile.cs
using UnityEngine;

public class TrackTile : MonoBehaviour
{
    [Tooltip("Nơi chứa vật cản/coin/item được sinh ra trong tile này, để dọn sạch khi thu hồi tile")]
    public Transform contentRoot;

    void Awake()
    {
        // Triệt tiêu scale của tile cha: object trong Content luôn có đúng vị trí/kích thước
        // mà TrackManager đặt, bất kể root tile bị phóng to/thu nhỏ bao nhiêu.
        Vector3 s = transform.lossyScale;
        if (contentRoot != null && s.x != 0f && s.y != 0f && s.z != 0f)
        {
            contentRoot.localPosition = Vector3.zero;
            contentRoot.localRotation = Quaternion.identity;
            contentRoot.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
        }
    }

    // Xoá toàn bộ vật cản/coin/item đã sinh trong tile này (gọi khi tile bị thu hồi vào pool)
    public void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}