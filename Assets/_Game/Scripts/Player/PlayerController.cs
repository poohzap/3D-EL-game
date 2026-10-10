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

    [Header("Hiệu ứng Boots (Bật nhảy & Bouncy Steps)")]
    [Tooltip("Lực nảy mỗi bước chạy khi Boots đang kích hoạt (thấp hơn jumpForce = 9)")]
    public float bootsStepBounceForce = 4.0f;
    [Tooltip("Hệ số tăng lực nhảy cao khi bấm Jump lúc có Boots")]
    public float bootsJumpMultiplier  = 1.5f;

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

    private bool isJumping;        // Đang trong cú nhảy cao chủ động (bấm Jump)
    private bool isAutoVaulting;   // Đang trong cú bật vọt dốc tự động

    public bool IsAlive  { get; private set; } = true;
    public bool IsFlying => powerUps != null && powerUps.IsFlying;
    public bool IsSliding => isSliding;
    public bool IsJumping => isJumping;
    public bool HasBoots => powerUps != null && powerUps.IsJumpBoosted;

    // ── Lifecycle ────────────────────────────────────────────────────────────
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        powerUps = GetComponent<PowerUpManager>();
    }

    void Start()
    {
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

        // --- Trục Y: bay hoặc nhảy/trọng lực/nảy Boots ---
        float deltaY;
        if (IsFlying)
        {
            float newY = Mathf.MoveTowards(transform.position.y, flightHeight, flightRiseSpeed * Time.deltaTime);
            deltaY           = newY - transform.position.y;
            verticalVelocity = 0f;
            isJumping        = false;
            isAutoVaulting   = false;
        }
        else
        {
            bool hasBoots = HasBoots;

            if (controller.isGrounded && verticalVelocity <= 0f)
            {
                isJumping      = false;
                isAutoVaulting = false;

                if (hasBoots && !isSliding)
                {
                    // Boots đang kích hoạt & đang chạy bộ: mỗi bước chân nảy lên một chút
                    verticalVelocity = bootsStepBounceForce;
                }
                else
                {
                    verticalVelocity = -1f;
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

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
        if (isSliding) return;

        bool hasBoots = HasBoots;

        // Cho phép nhảy khi:
        // 1. Đang chạm đất thông thường (controller.isGrounded)
        // 2. HOẶC đang trong nhịp nảy bước của Boots (hasBoots && !isJumping)
        if (!controller.isGrounded && !(hasBoots && !isJumping)) return;

        // Nếu đã đang trong cú nhảy cao chủ động (isJumping == true) thì không cho nhảy kép trên không
        if (isJumping && !controller.isGrounded) return;

        float boost = hasBoots ? bootsJumpMultiplier : 1f;
        verticalVelocity = jumpForce * boost;
        isJumping        = true;
        isAutoVaulting   = false;

        Animator anim = GetAnimator();
        if (anim != null) anim.SetTrigger("JumpTrigger");
    }

    public void Slide()
    {
        if (IsFlying) return;
        if (isSliding) return;

        // Nếu đang ở trên không (đang nhảy hoặc đang nảy Boots), ép rơi nhanh xuống đất để trượt ngay
        if (!controller.isGrounded)
        {
            verticalVelocity = -15f;
        }

        isJumping        = false;
        isAutoVaulting   = false;
        isSliding        = true;
        slideTimer       = slideDuration;

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
        isAutoVaulting   = true;
        isJumping        = false;
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
        // Nhảy hợp lệ khi người chơi chủ động bấm Jump hoặc leo dốc AutoVault
        bool isAirborneAction = (isJumping || isAutoVaulting) && !controller.isGrounded;

        switch (behavior)
        {
            // ────────────────────────────────────────────────────────────────
            // Barrier_Single: chỉ nhảy mới qua
            // Bắt buộc phải bấm Jump (hoặc AutoVault) trên không mới vượt qua.
            // Nếu chỉ chạy bước nảy Boots thông thường mà không bấm Jump → va chạm.
            // ────────────────────────────────────────────────────────────────
            case ObstacleBehavior.JumpOnly:
            {
                bool passingByJump = isAirborneAction && !isSliding;
                if (passingByJump) return true;   // đang nhảy → qua
                return TryShieldOrDie();
            }

            // ────────────────────────────────────────────────────────────────
            // TrafficBarrier_2: nhảy HOẶC slide đều qua
            // ────────────────────────────────────────────────────────────────
            case ObstacleBehavior.JumpOrSlide:
            {
                bool passingByJump  = isAirborneAction && !isSliding;
                bool passingBySlide = isSliding;
                if (passingByJump || passingBySlide) return true;
                return TryShieldOrDie();
            }

            // ────────────────────────────────────────────────────────────────
            // Shipping Container: KHÔNG thể qua trừ khi có JumpBoots
            // Bắt buộc có Boots VÀ phải bấm nhảy cao (isJumping) vượt qua nó
            // ────────────────────────────────────────────────────────────────
            case ObstacleBehavior.RequiresJumpBoots:
            {
                bool hasBoots = powerUps != null && powerUps.IsJumpBoosted;
                if (hasBoots && isAirborneAction) return true;  // Boots + đang nhảy cao
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
        IsAlive        = false;
        isJumping      = false;
        isAutoVaulting = false;
        if (GameManager.Instance != null) GameManager.Instance.EndGame();
    }
}