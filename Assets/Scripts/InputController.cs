using UnityEngine;
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

        Vector2 screenPos = Mouse.current.position.ReadValue();
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
}
