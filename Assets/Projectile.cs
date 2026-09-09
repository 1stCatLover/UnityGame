using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 250f;

    [Header("Lifetime")]
    public float lifetime = 10f;

    private Rigidbody2D rb;
    private RectTransform rectTransform;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rectTransform = GetComponent<RectTransform>();

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    void Start()
    {
        // Provides a useful fallback when the projectile is placed in a scene directly.
        if (movement == Vector2.zero)
            movement = transform.up;
    }   

    public void SetDirection(Vector2 direction)
    {
        movement = direction.normalized;
    }

    void FixedUpdate()
    {
        if (rectTransform != null && rectTransform.parent is RectTransform)
        {
            rectTransform.anchoredPosition += movement * moveSpeed * Time.fixedDeltaTime;
            return;
        }

        if (rb == null)
            return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
