// SkinDatabase.cs — Assets/_Game/Scripts/Shop/SkinDatabase.cs
using UnityEngine;

// Danh sách skin DUY NHẤT của cả game — ShopManager và PlayerSkinApplier
// cùng tham chiếu đúng 1 asset này, thêm/bớt skin chỉ cần sửa ở đây.
[CreateAssetMenu(fileName = "SkinDatabase", menuName = "EndlessRunner/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    public SkinData[] allSkins;
}