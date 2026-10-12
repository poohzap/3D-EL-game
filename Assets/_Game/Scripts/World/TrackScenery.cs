// TrackScenery.cs — Assets/_Game/Scripts/World/TrackScenery.cs
// Loại: MonoBehaviour — gắn lên prefab TrackTile_Straight (tool "1." trong menu Tools tự gắn giúp).
using UnityEngine;

// Mỗi lần tile được bật (lần đầu Instantiate, hoặc khi TrackManager lấy lại từ pool bằng SetActive(true))
// script này xoá cảnh vật cũ và rải cây + nhà mới hai bên đường.
public class TrackScenery : MonoBehaviour
{
    [Header("Tool 1 tự gán — object chứa cảnh vật sinh ra")]
    public Transform props;

    [Header("Kéo model vào (Kenney Nature Kit / City Kit Suburban)")]
    public GameObject[] treePrefabs;
    public GameObject[] housePrefabs;

    [Header("Cây")]
    public int treesPerSide = 4;
    public Vector2 treeDistance = new Vector2(5.5f, 10f); // khoảng cách từ tâm đường
    public Vector2 treeScale = new Vector2(1.5f, 2.5f);

    [Header("Nhà")]
    public int housesPerSide = 1;
    public Vector2 houseDistance = new Vector2(13f, 16f);
    public Vector2 houseScale = new Vector2(2f, 3f);
    public float houseYawOffset = 0f; // chỉnh nếu mặt trước của nhà không quay về phía đường

    public float tileLength = 20f;    // phải khớp TrackManager.tileLength

    void OnEnable()
    {
        Clear();

        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < treesPerSide; i++)
                Place(treePrefabs, side, Random.Range(treeDistance.x, treeDistance.y), Random.Range(-tileLength / 2f, tileLength / 2f), Random.Range(0f, 360f), treeScale);

            for (int i = 0; i < housesPerSide; i++)
                Place(housePrefabs, side, Random.Range(houseDistance.x, houseDistance.y), Random.Range(-tileLength / 2f + 4f, tileLength / 2f - 4f), -90f * side + houseYawOffset, houseScale);
        }
    }

    void Clear()
    {
        for (int i = props.childCount - 1; i >= 0; i--)
            Destroy(props.GetChild(i).gameObject);
    }

    void Place(GameObject[] prefabs, int side, float distance, float z, float yaw, Vector2 scaleRange)
    {
        if (prefabs.Length == 0) return;

        GameObject obj = Instantiate(prefabs[Random.Range(0, prefabs.Length)], props);
        obj.transform.localPosition = new Vector3(side * distance, 0f, z);
        obj.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        obj.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
    }
}
