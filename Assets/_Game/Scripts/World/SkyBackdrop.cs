// SkyBackdrop.cs — Assets/_Game/Scripts/World/SkyBackdrop.cs
// Loại: MonoBehaviour — gắn lên 1 Empty GameObject tên SkyBackdrop trong scene Gameplay (tool "3." tự tạo).
using UnityEngine;

// Núi xa phía trước + mây trên trời. Cả cụm luôn bám theo Z của Player nên không bao giờ "bị bỏ lại phía sau".
// Mây xoay chậm quanh Player để trôi ngang qua màn hình.
public class SkyBackdrop : MonoBehaviour
{
    public Transform player;
    public GameObject[] mountainPrefabs;
    public GameObject[] cloudPrefabs;

    [Header("Núi (nằm ở dải phía trước, xa tới mức sương mù phủ gần hết)")]
    public int mountainCount = 12;
    public float mountainSpreadX = 160f;
    public Vector2 mountainDistanceZ = new Vector2(250f, 330f);
    public Vector2 mountainScale = new Vector2(10f, 25f);

    [Header("Mây (camera cúi ~16° nên mây phải thấp, nếu không sẽ nằm ngoài màn hình)")]
    public int cloudCount = 30;
    public Vector2 cloudRadius = new Vector2(140f, 220f);
    public Vector2 cloudHeight = new Vector2(18f, 38f);
    public Vector2 cloudScale = new Vector2(4f, 9f);
    public float cloudSpinSpeed = 0.5f; // độ/giây

    private Transform cloudLayer;

    void Start()
    {
        BuildMountains();
        BuildClouds();
    }

    void LateUpdate()
    {
        transform.position = new Vector3(0f, 0f, player.position.z);
        cloudLayer.Rotate(0f, cloudSpinSpeed * Time.deltaTime, 0f);
    }

    void BuildMountains()
    {
        if (mountainPrefabs.Length == 0) return;

        for (int i = 0; i < mountainCount; i++)
        {
            GameObject m = Instantiate(mountainPrefabs[Random.Range(0, mountainPrefabs.Length)], transform);
            m.transform.localPosition = new Vector3(Random.Range(-mountainSpreadX, mountainSpreadX), -1f, Random.Range(mountainDistanceZ.x, mountainDistanceZ.y));
            m.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            m.transform.localScale = Vector3.one * Random.Range(mountainScale.x, mountainScale.y);
        }
    }

    void BuildClouds()
    {
        cloudLayer = new GameObject("CloudLayer").transform;
        cloudLayer.SetParent(transform, false);
        if (cloudPrefabs.Length == 0) return;

        for (int i = 0; i < cloudCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(cloudRadius.x, cloudRadius.y);

            GameObject c = Instantiate(cloudPrefabs[Random.Range(0, cloudPrefabs.Length)], cloudLayer);
            c.transform.localPosition = new Vector3(Mathf.Sin(angle) * radius, Random.Range(cloudHeight.x, cloudHeight.y), Mathf.Cos(angle) * radius);
            c.transform.localScale = Vector3.one * Random.Range(cloudScale.x, cloudScale.y);
        }
    }
}
