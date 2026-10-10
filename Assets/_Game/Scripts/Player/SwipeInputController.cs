// SwipeInputController.cs
using UnityEngine;

public class SwipeInputController : MonoBehaviour
{
    public PlayerController player;
    public float minSwipeDistance = 50f; // đơn vị pixel

    private Vector2 startTouchPos;
    private bool isTouching;

    void Awake()
    {
        if (player == null) player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (GameManager.Instance != null &&
    GameManager.Instance.CurrentState != GameManager.GameState.Playing) { isTouching = false; return; }
        HandleTouchInput();
        HandleEditorTestInput(); // chuột/bàn phím — chỉ để tiện test trong Editor, không ảnh hưởng khi build Android
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                startTouchPos = touch.position;
                isTouching = true;
                break;
            case TouchPhase.Ended:
                if (isTouching) DetectSwipe(touch.position);
                isTouching = false;
                break;
        }
    }

    void HandleEditorTestInput()
    {
        if (Input.GetMouseButtonDown(0)) { startTouchPos = Input.mousePosition; isTouching = true; }
        if (Input.GetMouseButtonUp(0) && isTouching) { DetectSwipe(Input.mousePosition); isTouching = false; }

        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) player.MoveLeft();
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) player.MoveRight();
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) player.Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) player.Slide();
    }

    void DetectSwipe(Vector2 endPos)
    {
        Vector2 delta = endPos - startTouchPos;
        if (delta.magnitude < minSwipeDistance) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) player.MoveRight(); else player.MoveLeft();
        }
        else
        {
            if (delta.y > 0) player.Jump(); else player.Slide();
        }
    }
}