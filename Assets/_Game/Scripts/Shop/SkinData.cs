// SkinData.cs — Assets/_Game/Scripts/Shop/SkinData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "EndlessRunner/Skin Data")]
public class SkinData : ScriptableObject
{
    public string skinId;         // khoá duy nhất, dùng để lưu PlayerPrefs — không được trùng giữa các skin
    public string displayName;
    public int price;             // giá 0 = skin mặc định, luôn mở khoá sẵn
    public GameObject modelPrefab; // model 3D của skin, sẽ được gắn làm con của Player
    public Sprite icon;            // ảnh hiển thị trong danh sách Shop
}