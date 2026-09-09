using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 8f;
    public float acceleration = 20f;
    public float deceleration = 25f;
    public float rotationSpeed = 10f;

    [Header("Shooting")]
    public GameObject Bullet;
    public Transform projectileParent;
    public float fireRate = 0.15f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float nextFireTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // WASD input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontal, vertical).normalized;

        // Shoot toward the mouse while holding left click. E remains supported as a fallback.
        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.E)) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    void Move()
    {
        Vector2 targetVelocity = movement * maxSpeed;

        float speedChange;

        if (movement != Vector2.zero)
            speedChange = acceleration;
        else
            speedChange = deceleration;

        rb.velocity = Vector2.MoveTowards(
            rb.velocity,
            targetVelocity,
            speedChange * Time.fixedDeltaTime
        );
    }

    void Rotate()
    {
        if (movement == Vector2.zero)
            return;

        float angle = Mathf.Atan2(
            movement.y,
            movement.x
        ) * Mathf.Rad2Deg - 90f;

        float newAngle = Mathf.LerpAngle(
            rb.rotation,
            angle,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newAngle);
    }

    void Shoot()
    {
        if (Bullet == null)
        {
            Debug.LogWarning("Bullet prefab is not assigned!");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("A camera tagged MainCamera is required to aim at the mouse.");
            return;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector2 aimDirection = (Vector2)(mainCamera.ScreenToWorldPoint(mouseScreenPosition) - transform.position);

        if (aimDirection.sqrMagnitude < 0.001f)
            return;

        aimDirection.Normalize();
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        GameObject projectile = Instantiate(
            Bullet,
            transform.position,
            Quaternion.Euler(0f, 0f, angle),
            projectileParent
        );

        Projectile projectileMovement = projectile.GetComponent<Projectile>();
        if (projectileMovement != null)
            projectileMovement.SetDirection(aimDirection);
    }
}
