using UnityEngine;

public enum ShapeBehavior
{
    WhiteCut,
    YellowCatch
}

public class FallingShape : MonoBehaviour
{
    private Rigidbody2D rb;
    private float fallSpeed = 2f;
    public ShapeBehavior behavior = ShapeBehavior.WhiteCut;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
    }

    public void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
