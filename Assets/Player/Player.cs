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

        Vector2 aimDirection;
        Camera aimCamera;

        RectTransform parentRect = projectileParent as RectTransform;
        Canvas parentCanvas = parentRect != null ? parentRect.GetComponentInParent<Canvas>() : null;

        if (parentCanvas != null)
        {
            aimCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (parentCanvas.worldCamera != null ? parentCanvas.worldCamera : mainCamera);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                Input.mousePosition,
                aimCamera,
                out Vector2 mouseLocalPosition
            );

            Vector2 playerScreenPosition = RectTransformUtility.WorldToScreenPoint(
                aimCamera,
                transform.position
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                playerScreenPosition,
                aimCamera,
                out Vector2 playerLocalPosition
            );

            aimDirection = mouseLocalPosition - playerLocalPosition;
        }
        else
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            aimDirection = (Vector2)(mainCamera.ScreenToWorldPoint(mouseScreenPosition) - transform.position);
        }

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

        if (parentCanvas != null && projectile.transform is RectTransform projectileRect)
        {
            Camera uiCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (parentCanvas.worldCamera != null ? parentCanvas.worldCamera : mainCamera);

            Vector2 playerScreenPosition = RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                transform.position
            );

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                playerScreenPosition,
                uiCamera,
                out Vector2 playerLocalPosition
            ))
            {
                projectileRect.anchoredPosition = playerLocalPosition;
            }
        }

        Projectile projectileMovement = projectile.GetComponent<Projectile>();
        if (projectileMovement != null)
            projectileMovement.SetDirection(aimDirection);
    }
}
