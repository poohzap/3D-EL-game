// PowerUpManager.cs
using System;
using System.Collections;
using UnityEngine;

public enum PowerUpType { Shield, JumpBoots, Rocket }

public class PowerUpManager : MonoBehaviour
{
    [Header("Thời lượng hiệu lực (giây)")]
    public float shieldDuration  = 6f;
    public float jumpBootsDuration = 8f;
    public float rocketDuration  = 10f;   // Rocket bay 10 giây

    // ── Trạng thái active ────────────────────────────────────────────────────
    /// <summary>Shield đang cho 1 mạng ảo (tối đa 1).</summary>
    public bool IsShieldActive   { get; private set; }
    public bool IsJumpBoosted    { get; private set; }
    public bool IsFlying         { get; private set; }

    /// <summary>
    /// TRUE khi shield đang "sạc" 1 mạng ảo (khác với IsShieldActive timer còn chạy).
    /// Khi bị hit: mất mạng ảo → HasVirtualLife = false, nhưng IsShieldActive vẫn còn
    /// thời gian nếu chưa hết.  Dùng HasVirtualLife để biết còn đỡ được không.
    /// </summary>
    public bool HasVirtualLife   { get; private set; }

    public float           RemainingTime { get; private set; }
    public PowerUpType?    CurrentType   { get; private set; }

    public event Action<PowerUpType?, float> OnPowerUpChanged;

    private Coroutine activeRoutine;

    // ── Kích hoạt ────────────────────────────────────────────────────────────
    public void Activate(PowerUpType type)
    {
        // Shield chồng thêm: nếu timer shield đang chạy thì chỉ tái nạp mạng ảo
        if (type == PowerUpType.Shield && IsShieldActive)
        {
            HasVirtualLife = true;   // nạp lại mạng ảo (tối đa 1)
            // Không cần reset timer — chỉ nạp thêm máu ảo
            return;
        }

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        ClearAllEffects();

        switch (type)
        {
            case PowerUpType.Shield:
                IsShieldActive = true;
                HasVirtualLife = true;   // 1 mạng ảo
                break;
            case PowerUpType.JumpBoots: IsJumpBoosted = true; break;
            case PowerUpType.Rocket:    IsFlying       = true; break;
        }

        CurrentType   = type;
        activeRoutine = StartCoroutine(CountDown(GetDuration(type)));
    }

    // ── Xử lý khi bị obstacle đánh trúng (gọi từ PlayerController) ──────────
    /// <summary>
    /// Gọi khi player va chạm obstacle mà lẽ ra phải Die.
    /// Trả về TRUE nếu shield hấp thụ hit (tiêu 1 mạng ảo), FALSE nếu phải Die.
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (IsShieldActive && HasVirtualLife)
        {
            HasVirtualLife = false;   // tiêu mạng ảo
            // Shield timer vẫn còn chạy; khi hết timer thì IsShieldActive = false
            OnPowerUpChanged?.Invoke(CurrentType, RemainingTime);
            return true;
        }
        return false;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    float GetDuration(PowerUpType type) => type switch
    {
        PowerUpType.Shield    => shieldDuration,
        PowerUpType.JumpBoots => jumpBootsDuration,
        PowerUpType.Rocket    => rocketDuration,
        _                     => 0f
    };

    IEnumerator CountDown(float duration)
    {
        RemainingTime = duration;
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            OnPowerUpChanged?.Invoke(CurrentType, Mathf.Max(RemainingTime, 0f));
            yield return null;
        }

        ClearAllEffects();
        CurrentType = null;
        OnPowerUpChanged?.Invoke(null, 0f);
    }

    void ClearAllEffects()
    {
        IsShieldActive = false;
        HasVirtualLife = false;
        IsJumpBoosted  = false;
        IsFlying       = false;
    }
}