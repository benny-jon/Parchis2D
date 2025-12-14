using UnityEngine;

public class RingAnimator : MonoBehaviour
{
    [SerializeField] public float degressPerSecond = 120f;

    void Update()
    {
        transform.Rotate(0, 0, degressPerSecond * Time.unscaledDeltaTime);
    }
}
