// TrackManager.cs — Assets/_Game/Scripts/World/TrackManager.cs
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Tham chiếu (kéo trong Inspector)")]
    public PlayerController player;
    public PowerUpManager   playerPowerUps;

    [Header("Prefabs")]
    public GameObject   trackTilePrefab;
    public GameObject[] obstaclePrefabs;    // Các prefab obstacle thông thường
    public GameObject   coinPrefab;
    public GameObject[] powerUpPrefabs;

    [Header("Cấu hình đường chạy")]
    public float tileLength  = 20f;
    public int   tilesAhead  = 6;
    public int   safeZoneTiles = 3;

    [Header("Độ khó (tăng dần, có giới hạn)")]
    public float baseSpeed          = 8f;
    public float maxSpeed           = 20f;
    public float speedRampDistance  = 1200f;
    [Range(0f, 1f)] public float minObstacleDensity = 0.35f;
    [Range(0f, 1f)] public float maxObstacleDensity = 0.85f;
    [Range(0f, 1f)] public float powerUpSpawnChance = 0.06f;

    [Header("Seed ngẫu nhiên (0 = tự sinh ngẫu nhiên)")]
    public int seed = 0;

    private System.Random rng;
    private readonly Queue<GameObject> tilePool    = new Queue<GameObject>();
    private readonly List<GameObject>  activeTiles = new List<GameObject>();
    private float nextSpawnZ;
    private int   tilesSpawned;

    void Awake()
    {
        int usedSeed = seed != 0 ? seed : System.Environment.TickCount;
        rng = new System.Random(usedSeed);
        Debug.Log("TrackManager seed = " + usedSeed);
    }

    void Start()
    {
        // ── Kiểm tra các reference bắt buộc ───────────────────────────────
        if (coinPrefab == null)
            Debug.LogError("[TrackManager] coinPrefab chưa được gán trong Inspector!", this);
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            Debug.LogError("[TrackManager] obstaclePrefabs trống! Kéo các obstacle prefab vào Inspector.", this);
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            Debug.LogWarning("[TrackManager] powerUpPrefabs trống — không có powerup nào spawn.", this);
        if (player == null)
            Debug.LogError("[TrackManager] player chưa được gán trong Inspector!", this);

        Debug.Log($"[TrackManager] Start — coinPrefab={coinPrefab}, obstacles={obstaclePrefabs?.Length ?? 0}, powerups={powerUpPrefabs?.Length ?? 0}");

        for (int i = 0; i < tilesAhead; i++) SpawnTile();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        while (player.transform.position.z + tilesAhead * tileLength > nextSpawnZ)
            SpawnTile();

        DespawnPassedTiles();
        UpdateDifficulty();
    }

    void UpdateDifficulty()
    {
        float t = Mathf.Clamp01(player.transform.position.z / speedRampDistance);
        player.forwardSpeed = Mathf.Lerp(baseSpeed, maxSpeed, t);
    }

    float CurrentObstacleDensity()
    {
        float t = Mathf.Clamp01(player.transform.position.z / speedRampDistance);
        return Mathf.Lerp(minObstacleDensity, maxObstacleDensity, t);
    }

    // ── Pool ─────────────────────────────────────────────────────────────────
    GameObject GetTileFromPool()
    {
        if (tilePool.Count > 0) return tilePool.Dequeue();
        return Instantiate(trackTilePrefab);
    }

    void SpawnTile()
    {
        GameObject tile = GetTileFromPool();
        tile.transform.position = new Vector3(0f, 0f, nextSpawnZ);
        tile.SetActive(true);
        activeTiles.Add(tile);

        PopulateTile(tile, nextSpawnZ);

        nextSpawnZ += tileLength;
        tilesSpawned++;
    }

    void DespawnPassedTiles()
    {
        float despawnZ = player.transform.position.z - tileLength * 2f;

        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = activeTiles[i];
            if (tile.transform.position.z < despawnZ)
            {
                tile.GetComponent<TrackTile>().ClearContent();
                tile.SetActive(false);
                tilePool.Enqueue(tile);
                activeTiles.RemoveAt(i);
            }
        }
    }

    // ── Populate ─────────────────────────────────────────────────────────────
    void PopulateTile(GameObject tile, float tileStartZ)
    {
        Transform content   = tile.GetComponent<TrackTile>().contentRoot;
        bool      isSafeZone = tilesSpawned < safeZoneTiles;

        bool isFlying = playerPowerUps != null && playerPowerUps.IsFlying;

        if (isFlying)
        {
            // ── Khi Rocket đang hoạt động ──────────────────────────────────
            // Không có obstacle dưới đất, chỉ spawn coin trên trời.
            SpawnFlightCoins(content);
        }
        else if (isSafeZone)
        {
            SpawnCoinLine(content, RandomLane());
        }
        else
        {
            SpawnObstaclesAndCoins(content, tileStartZ);

            // PowerUp chỉ spawn khi không đang bay và không safe-zone
            if (rng.NextDouble() < powerUpSpawnChance)
                SpawnPowerUp(content);
        }
    }

    void SpawnObstaclesAndCoins(Transform content, float tileStartZ)
    {
        if (rng.NextDouble() < CurrentObstacleDensity())
        {
            List<int> lanes = new List<int> { -1, 0, 1 };
            Shuffle(lanes);

            int        blockedCount = rng.Next(1, 3);
            List<int>  blocked      = lanes.GetRange(0, blockedCount);
            float      localZ       = tileLength * 0.5f;

            foreach (int lane in blocked)
            {
                GameObject prefab = obstaclePrefabs[rng.Next(obstaclePrefabs.Length)];
                GameObject obs    = Instantiate(prefab, content);
                Vector3    p      = obs.transform.localPosition;
                obs.transform.localPosition = new Vector3(lane * player.laneDistance, p.y, localZ);
            }

            // Spawn coin dãy ở các làn trống
            foreach (int lane in lanes)
            {
                if (!blocked.Contains(lane)) SpawnCoinLine(content, lane);
            }
        }
        else
        {
            // Tile không có obstacle → coin dãy ngẫu nhiên 1 làn
            SpawnCoinLine(content, RandomLane());
        }
    }

    // ── Coin dãy mặt đất (5 coin) ────────────────────────────────────────────
    void SpawnCoinLine(Transform content, int lane)
    {
        if (coinPrefab == null) return;

        const int coinCount = 5;
        float     spacing   = tileLength / (coinCount + 1);

        for (int i = 1; i <= coinCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, content);
            Vector3    p    = coin.transform.localPosition;
            coin.transform.localPosition = new Vector3(lane * player.laneDistance, p.y, i * spacing);
        }
    }

    // ── Coin trên trời khi đang bay (Rocket) ─────────────────────────────────
    void SpawnFlightCoins(Transform content)
    {
        float flightHeight = player.flightHeight;
        bool  zigzag       = rng.Next(2) == 0;
        const int steps    = 6;
        float spacing      = tileLength / (steps + 1);
        int   lane         = RandomLane();

        for (int i = 1; i <= steps; i++)
        {
            if (zigzag) lane = (i % 2 == 0) ? 1 : -1;

            GameObject coin = Instantiate(coinPrefab, content);
            coin.transform.localPosition = new Vector3(lane * player.laneDistance, flightHeight, i * spacing);
        }
    }

    // ── PowerUp ───────────────────────────────────────────────────────────────
    void SpawnPowerUp(Transform content)
    {
        GameObject prefab = powerUpPrefabs[rng.Next(powerUpPrefabs.Length)];
        GameObject pu     = Instantiate(prefab, content);
        Vector3    p      = pu.transform.localPosition;
        pu.transform.localPosition = new Vector3(RandomLane() * player.laneDistance, p.y, tileLength * 0.5f);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────
    int RandomLane() => rng.Next(-1, 2);

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}