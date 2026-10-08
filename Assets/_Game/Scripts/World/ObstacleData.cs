// ObstacleData.cs
// Gắn script này lên mỗi prefab obstacle để khai báo hành vi của nó.
using UnityEngine;

public enum ObstacleBehavior
{
    /// <summary>Chỉ nhảy mới qua được (Barrier_Single)</summary>
    JumpOnly,

    /// <summary>Nhảy HOẶC slide đều qua được (TrafficBarrier_2)</summary>
    JumpOrSlide,

    /// <summary>Không thể qua trừ khi có JumpBoots (Shipping Container)</summary>
    RequiresJumpBoots,
}

public class ObstacleData : MonoBehaviour
{
    [Tooltip("Hành vi của obstacle này khi player va chạm.")]
    public ObstacleBehavior behavior = ObstacleBehavior.JumpOnly;
}
