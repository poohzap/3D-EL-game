// SwipeInputController.cs
using UnityEngine;

public class SwipeInputController : MonoBehaviour
{
    public PlayerController player;
    public float minSwipeDistance = 50f; // đơn vị pixel

    private Vector2 startTouchPos;
    private bool isTouching;

    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.LeftArrow)) player.MoveLeft();
        if (Input.GetKeyDown(KeyCode.RightArrow)) player.MoveRight();
        if (Input.GetKeyDown(KeyCode.UpArrow)) player.Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow)) player.Slide();
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