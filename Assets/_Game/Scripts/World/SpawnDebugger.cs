// SpawnDebugger.cs — TẠM THỜI, xoá sau khi tìm xong lỗi
using System.Text;
using UnityEngine;

public class SpawnDebugger : MonoBehaviour
{
    public Transform player;   // kéo Player vào đây
    public float delay = 3f;

    void Start() => Invoke(nameof(Report), delay);

    void Report()
    {
        Camera cam = Camera.main;
        Debug.Log($"[DBG] Camera={cam.transform.position} far={cam.farClipPlane} | Player={player.position}");
        Describe("Coin", cam);
        Describe("Obstacle", cam);
    }

    void Describe(string tag, Camera cam)
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag(tag);
        GameObject best = null;
        float bestDz = float.MaxValue;
        foreach (GameObject go in all)
        {
            float dz = go.transform.position.z - player.position.z;
            if (dz > 0f && dz < bestDz) { bestDz = dz; best = go; }
        }

        if (best == null)
        {
            Debug.LogError($"[DBG] {tag}: có {all.Length} object nhưng KHÔNG có cái nào phía trước Player");
            return;
        }

        Transform t = best.transform;
        var sb = new StringBuilder();
        sb.AppendLine($"[DBG] {tag}: tổng {all.Length}; gần nhất phía trước = '{best.name}'");
        sb.AppendLine($"  world pos = {t.position}, lossyScale = {t.lossyScale}, cách Player {bestDz:0.0} theo Z");
        sb.AppendLine($"  viewport = {cam.WorldToViewportPoint(t.position)}  (trong khung hình khi x,y trong 0..1 và z>0)");
        sb.AppendLine($"  layer={best.layer}, camera có vẽ layer này = {(cam.cullingMask & (1 << best.layer)) != 0}");

        Renderer r = best.GetComponentInChildren<Renderer>();
        if (r != null) sb.AppendLine($"  renderer enabled={r.enabled}, bounds size={r.bounds.size}");
        else sb.AppendLine("  KHÔNG có Renderer trong object này");

        for (Transform p = t.parent; p != null; p = p.parent)
            sb.AppendLine($"  cha '{p.name}': localPos={p.localPosition}, localScale={p.localScale}, active={p.gameObject.activeInHierarchy}");

        Debug.Log(sb.ToString());
    }
}