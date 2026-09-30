// PowerUpManager.cs
using System;
using System.Collections;
using UnityEngine;

public enum PowerUpType { Shield, JumpBoots, Rocket }

public class PowerUpManager : MonoBehaviour
{
    [Header("Thời lượng hiệu lực (giây)")]
    public float shieldDuration = 6f;
    public float jumpBootsDuration = 8f;
    public float rocketDuration = 6f;

    public bool IsShieldActive { get; private set; }
    public bool IsJumpBoosted { get; private set; }
    public bool IsFlying { get; private set; }
    public float RemainingTime { get; private set; }
    public PowerUpType? CurrentType { get; private set; }

    public event Action<PowerUpType?, float> OnPowerUpChanged;

    private Coroutine activeRoutine;

    public void Activate(PowerUpType type)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        ClearAllEffects();

        switch (type)
        {
            case PowerUpType.Shield: IsShieldActive = true; break;
            case PowerUpType.JumpBoots: IsJumpBoosted = true; break;
            case PowerUpType.Rocket: IsFlying = true; break;
        }

        CurrentType = type;
        activeRoutine = StartCoroutine(CountDown(GetDuration(type)));
    }

    float GetDuration(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield: return shieldDuration;
            case PowerUpType.JumpBoots: return jumpBootsDuration;
            case PowerUpType.Rocket: return rocketDuration;
            default: return 0f;
        }
    }

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
        IsJumpBoosted = false;
        IsFlying = false;
    }
}