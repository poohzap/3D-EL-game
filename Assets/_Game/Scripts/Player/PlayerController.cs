// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Tốc độ")]
    public float forwardSpeed = 8f;   // TrackManager sẽ tăng dần giá trị này theo độ khó
    public float laneDistance = 3f;   // khoảng cách giữa các làn, phải khớp với TrackManager
    public float laneChangeSpeed = 12f;

    [Header("Nhảy / Trượt")]
    public float jumpForce = 9f;
    public float gravity = -25f;
    public float slideDuration = 0.7f;
    [Range(0.1f, 1f)] public float slideHeightMultiplier = 0.5f;

    [Header("Bay (Rocket)")]
    public float flightHeight = 4f;     // độ cao khi bay, TrackManager sẽ đọc giá trị này để rải coin trên không
    public float flightRiseSpeed = 6f;

    private CharacterController controller;
    private PowerUpManager powerUps;

    private Animator animator; 
    private int currentLane = 0;   // -1 = trái, 0 = giữa, 1 = phải
    private float verticalVelocity;
    private bool isSliding;
    private float slideTimer;
    private float originalHeight;
    private Vector3 originalCenter;

    public bool IsAlive { get; private set; } = true;
    public bool IsFlying => powerUps != null && powerUps.IsFlying;

    public bool IsSliding => isSliding;
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        powerUps = GetComponent<PowerUpManager>();
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
        // Không xử lý di chuyển khi đã chết hoặc game đã kết thúc
        if (!IsAlive) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        UpdateSlideTimer();

        // --- Trục X: đổi làn mượt bằng MoveTowards ---
        float targetX = currentLane * laneDistance;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float deltaX = newX - transform.position.x;

        // --- Trục Z: luôn chạy về phía trước ---
        float deltaZ = forwardSpeed * Time.deltaTime;

        // --- Trục Y: bay hoặc nhảy/trọng lực bình thường ---
        float deltaY;
        if (IsFlying)
        {
            float newY = Mathf.MoveTowards(transform.position.y, flightHeight, flightRiseSpeed * Time.deltaTime);
            deltaY = newY - transform.position.y;
            verticalVelocity = 0f;
        }
        else
        {
            if (controller.isGrounded && verticalVelocity <= 0f)
                verticalVelocity = -1f; // chỉ ép xuống khi KHÔNG phải vừa nhảy — tránh đè mất lực Jump() vừa gán
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

    // ----- Các hành động do input gọi vào -----

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
        if (IsFlying) return;                       // đang bay thì không cần/không cho nhảy
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

        isSliding = true;
        slideTimer = slideDuration;

        float newHeight = originalHeight * slideHeightMultiplier;
        float heightDiff = originalHeight - newHeight;
        controller.height = newHeight;
        // Hạ tâm collider xuống để nhân vật "cúi" sát đất, thay vì co lại quanh tâm cũ
        controller.center = new Vector3(originalCenter.x, originalCenter.y - heightDiff / 2f, originalCenter.z);

        Animator anim = GetAnimator();
        if (anim != null) anim.SetTrigger("SlideTrigger");
    }

    void EndSlide()
    {
        isSliding = false;
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

    // Được gọi bởi VaultObstacle (Mục 6.4) khi chạm vào dốc leo — tự động vọt lên,
    // không cần người chơi bấm Jump, và luôn thành công (không đòi hỏi canh thời điểm).
    public void AutoVault(float force)
    {
        if (IsFlying) return;      // đang bay thì khỏi cần vọt
        if (isSliding) EndSlide(); // đang trượt thì huỷ trượt để còn nhảy lên được
        verticalVelocity = force;
    }

    // ----- Va chạm với vật cản / coin / item (CharacterController tự bắn OnTriggerEnter) -----

    void OnTriggerEnter(Collider other)
    {
        if (!IsAlive) return;

        if (other.CompareTag("Obstacle"))
        {
            VaultObstacle vault = other.GetComponent<VaultObstacle>();
            if (vault != null)
            {
                AutoVault(vault.vaultForce); // dốc leo: luôn vọt qua an toàn, không tính là va chạm thua
            }
            else if (powerUps != null && powerUps.IsShieldActive)
            {
                Destroy(other.gameObject); // có khiên: phá huỷ vật cản thay vì thua
            }
            else
            {
                Die();
            }
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

    void Die()
    {
        IsAlive = false;
        if (GameManager.Instance != null) GameManager.Instance.EndGame();
    }
}