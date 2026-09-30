// TrackTile.cs — Assets/_Game/Scripts/World/TrackTile.cs
using UnityEngine;

public class TrackTile : MonoBehaviour
{
    [Tooltip("Nơi chứa vật cản/coin/item được sinh ra trong tile này, để dọn sạch khi thu hồi tile")]
    public Transform contentRoot;

    // Xoá toàn bộ vật cản/coin/item đã sinh trong tile này (gọi khi tile bị thu hồi vào pool)
    public void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}