using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 250f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (rb == null)
            return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
