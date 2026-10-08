// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Tốc độ")]
    public float forwardSpeed   = 8f;   // TrackManager sẽ tăng dần
    public float laneDistance   = 3f;   // khoảng cách giữa các làn
    public float laneChangeSpeed = 12f;

    [Header("Nhảy / Trượt")]
    public float jumpForce            = 9f;
    public float gravity              = -25f;
    public float slideDuration        = 0.7f;
    [Range(0.1f, 1f)] public float slideHeightMultiplier = 0.5f;

    [Header("Bay (Rocket)")]
    public float flightHeight    = 4f;   // độ cao khi bay
    public float flightRiseSpeed = 6f;

    private CharacterController controller;
    private PowerUpManager       powerUps;
    private Animator             animator;

    private int   currentLane = 0;   // -1 = trái, 0 = giữa, 1 = phải
    private float verticalVelocity;
    private bool  isSliding;
    private float slideTimer;
    private float originalHeight;
    private Vector3 originalCenter;

    public bool IsAlive  { get; private set; } = true;
    public bool IsFlying => powerUps != null && powerUps.IsFlying;
    public bool IsSliding => isSliding;

    // ── Lifecycle ────────────────────────────────────────────────────────────
    void Awake()
    {
        controller    = GetComponent<CharacterController>();
        powerUps      = GetComponent<PowerUpManager>();
        originalHeight = controller.height;
        originalCenter = controller.center;
    }

    Animator GetAnimator()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        return animator;
    }

    void Update()
    {
        if (!IsAlive) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        UpdateSlideTimer();

        // --- Trục X: đổi làn ---
        float targetX = currentLane * laneDistance;
        float newX    = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float deltaX  = newX - transform.position.x;

        // --- Trục Z: luôn chạy tới ---
        float deltaZ = forwardSpeed * Time.deltaTime;

        // --- Trục Y: bay hoặc nhảy/trọng lực ---
        float deltaY;
        if (IsFlying)
        {
            float newY = Mathf.MoveTowards(transform.position.y, flightHeight, flightRiseSpeed * Time.deltaTime);
            deltaY          = newY - transform.position.y;
            verticalVelocity = 0f;
        }
        else
        {
            if (controller.isGrounded && verticalVelocity <= 0f)
                verticalVelocity = -1f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            deltaY = verticalVelocity * Time.deltaTime;
        }

        controller.Move(new Vector3(deltaX, deltaY, deltaZ));
    }

    void UpdateSlideTimer()
    {
        if (!isSliding) return;
        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f) EndSlide();
    }

    // ── Hành động ────────────────────────────────────────────────────────────
    public void MoveLeft()
    {
        if (currentLane > -1)
        {
            currentLane--;
            Animator anim = GetAnimator();
            if (anim != null) anim.SetTrigger("DodgeLeftTrigger");
        }
    }

    public void MoveRight()
    {
        if (currentLane < 1)
        {
            currentLane++;
            Animator anim = GetAnimator();
            if (anim != null) anim.SetTrigger("DodgeRightTrigger");
        }
    }

    public void Jump()
    {
        if (IsFlying) return;
        if (!controller.isGrounded || isSliding) return;

        float boost = (powerUps != null && powerUps.IsJumpBoosted) ? 1.5f : 1f;
        verticalVelocity = jumpForce * boost;

        Animator anim = GetAnimator();
        if (anim != null) anim.SetTrigger("JumpTrigger");
    }

    public void Slide()
    {
        if (IsFlying) return;
        if (!controller.isGrounded || isSliding) return;

        isSliding  = true;
        slideTimer = slideDuration;

        float newHeight  = originalHeight * slideHeightMultiplier;
        float heightDiff = originalHeight - newHeight;
        controller.height = newHeight;
        controller.center = new Vector3(originalCenter.x, originalCenter.y - heightDiff / 2f, originalCenter.z);

        Animator anim = GetAnimator();
        if (anim != null) anim.SetTrigger("SlideTrigger");
    }

    void EndSlide()
    {
        isSliding         = false;
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

    /// <summary>Được gọi bởi VaultObstacle để tự động vọt qua dốc.</summary>
    public void AutoVault(float force)
    {
        if (IsFlying) return;
        if (isSliding) EndSlide();
        verticalVelocity = force;
    }

    // ── Va chạm ──────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (!IsAlive) return;

        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision(other);
        }
        else if (other.CompareTag("Coin"))
        {
            CurrencyManager.AddRunCoin();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("PowerUp"))
        {
            PowerUpPickup pickup = other.GetComponent<PowerUpPickup>();
            if (pickup != null && powerUps != null) powerUps.Activate(pickup.type);
            Destroy(other.gameObject);
        }
    }

    void HandleObstacleCollision(Collider other)
    {
        // Kiểm tra VaultObstacle (dốc leo)
        VaultObstacle vault = other.GetComponent<VaultObstacle>();
        if (vault != null)
        {
            AutoVault(vault.vaultForce);
            return;
        }

        // Đọc hành vi từ ObstacleData
        ObstacleData data = other.GetComponentInParent<ObstacleData>();
        ObstacleBehavior behavior = data != null ? data.behavior : ObstacleBehavior.JumpOnly;

        bool survived = EvaluateObstacleBehavior(behavior, other.gameObject);
        if (!survived) Die();
    }

    /// <summary>
    /// Kiểm tra xem player có đáp ứng điều kiện qua obstacle không.
    /// Trả về TRUE = sống sót, FALSE = chết.
    /// </summary>
    bool EvaluateObstacleBehavior(ObstacleBehavior behavior, GameObject obsObject)
    {
        switch (behavior)
        {
            // ────────────────────────────────────────────────────────────────
            // Barrier_Single: chỉ nhảy mới qua
            // Nếu player đang ở trên không (verticalVelocity > 0 hoặc không chạm đất)
            // nghĩa là đang nhảy → an toàn.  Ngược lại → hit.
            // ────────────────────────────────────────────────────────────────
            case ObstacleBehavior.JumpOnly:
            {
                bool passingByJump = !controller.isGrounded && !isSliding;
                if (passingByJump) return true;   // đang nhảy → qua
                return TryShieldOrDie();
            }

            // ────────────────────────────────────────────────────────────────
            // TrafficBarrier_2: nhảy HOẶC slide đều qua
            // ────────────────────────────────────────────────────────────────
            case ObstacleBehavior.JumpOrSlide:
            {
                bool passingByJump  = !controller.isGrounded && !isSliding;
                bool passingBySlide = isSliding;
                if (passingByJump || passingBySlide) return true;
                return TryShieldOrDie();
            }

            // ────────────────────────────────────────────────────────────────
            // Shipping Container: KHÔNG thể qua trừ khi có JumpBoots
            // Dù đang nhảy/trượt cũng không qua được nếu không có Boots
            // (cần JumpBoots để nhảy đủ cao)
            // ────────────────────────────────────────────────────────────────
            case ObstacleBehavior.RequiresJumpBoots:
            {
                bool hasBoots = powerUps != null && powerUps.IsJumpBoosted;
                if (hasBoots && !controller.isGrounded) return true;  // Boots + đang nhảy
                return TryShieldOrDie();
            }
        }

        return TryShieldOrDie();
    }

    /// <summary>
    /// Thử dùng shield hấp thụ đòn. Nếu không có shield → Die.
    /// Trả về TRUE nếu thoát (shield đỡ), FALSE nếu phải Die.
    /// </summary>
    bool TryShieldOrDie()
    {
        if (powerUps != null && powerUps.TryAbsorbHit())
            return true;   // shield đỡ được
        return false;      // caller sẽ gọi Die()
    }

    void Die()
    {
        IsAlive = false;
        if (GameManager.Instance != null) GameManager.Instance.EndGame();
    }
}