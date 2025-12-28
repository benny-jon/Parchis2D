using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private Camera cam;
    private Clickable2D pressedObject;   // << tracks what you pressed on

    private void Awake()
    {
        cam = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext ctx)
    {
        //Debug.Log($"Click phase={ctx.phase}, value={ctx.ReadValue<float>()}");

        // For a Button action, this will be 1 on press, 0 on release
        float value = ctx.ReadValue<float>();

        Vector2 screenPos;
        var pointerDevice = ctx.control.device as Pointer;
        if (pointerDevice != null)
        {
            screenPos = Pointer.current.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            return;
        }

        // --- UI BLOCK: if click is on UI, don't let it hit the board ---
        if (IsPointerOverUI())
        {
            // If we were tracking a press on the board, cancel it so it can't "complete" on release
            if (value <= 0.5f)
                pressedObject = null;

            return;
        }

        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        Clickable2D target = hit.collider ? hit.collider.GetComponent<Clickable2D>() : null;

        if (value > 0.5f)
        {
            // button DOWN
            pressedObject = target;

            //Debug.Log($"Started on {target}");

            if (pressedObject != null)
                pressedObject.OnClickDown();
        }
        else
        {
            //Debug.Log($"Canceled on {target}");

            // button UP
            if (pressedObject != null)
            {
                if (pressedObject == target)
                {
                    //Debug.Log($"Calling OnClick up on {pressedObject}");
                    // RELEASED ON THE SAME OBJECT → valid click
                    pressedObject.OnClickUp();
                }

                // Clear pressed object (even if release was elsewhere)
                pressedObject = null;
            }
        }
    }

    private static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Mouse / Pen / general pointer
        if (Pointer.current != null)
            return EventSystem.current.IsPointerOverGameObject();

        // Touch: must pass finger id
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue());

        return false;
    }
}
