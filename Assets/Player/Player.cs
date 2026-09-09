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

    [Header("Movement Boundary")]
    public float boundaryRadius = 250f;
    public bool showBoundary = true;
    public Color boundaryColor = new Color(0.1f, 0.8f, 1f, 0.8f);
    [Min(1)]
    public int boundaryDensity = 32;
    [Min(1f)]
    public float boundarySquareSize = 8f;
    public Color boundarySquareColor = Color.white;
    public Transform boundarySquareParent;
    public GameObject boundarySquarePrefab;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float nextFireTime;
    private RectTransform playerRect;
    private RectTransform boundaryParent;
    private Vector2 boundaryCenter;
    private Vector2 currentVelocity;
    private bool usesUIPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        playerRect = transform as RectTransform;
        boundaryParent = playerRect != null ? playerRect.parent as RectTransform : null;
        usesUIPosition = playerRect != null && boundaryParent != null;
        boundaryCenter = usesUIPosition ? playerRect.anchoredPosition :
            (rb != null ? rb.position : (Vector2)transform.position);

        if (rb == null && !usesUIPosition)
        {
            Debug.LogError("Player needs a Rigidbody2D or a RectTransform parent to move.", this);
            enabled = false;
            return;
        }

        if (showBoundary && boundaryRadius > 0f && boundaryParent != null)
            CreateBoundaryVisual();
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
        ClampToBoundary();
        Rotate();
    }

    void LateUpdate()
    {
        // Clamp once more after physics moves the Rigidbody2D so the player
        // cannot visually pass through the circle edge.
        ClampToBoundary();
    }

    void Move()
    {
        Vector2 targetVelocity = movement * maxSpeed;
        float speedChange = movement != Vector2.zero ? acceleration : deceleration;
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            speedChange * Time.fixedDeltaTime
        );

        if (usesUIPosition)
        {
            playerRect.anchoredPosition += currentVelocity * Time.fixedDeltaTime;
            if (rb != null)
                rb.velocity = Vector2.zero;
            return;
        }

        if (rb != null)
            rb.velocity = currentVelocity;
    }

    void Rotate()
    {
        if (movement == Vector2.zero)
            return;

        float angle = Mathf.Atan2(
            movement.y,
            movement.x
        ) * Mathf.Rad2Deg - 90f;

        float currentAngle = usesUIPosition ? transform.localEulerAngles.z : rb.rotation;
        float newAngle = Mathf.LerpAngle(
            currentAngle,
            angle,
            rotationSpeed * Time.fixedDeltaTime
        );

        if (usesUIPosition)
            transform.localRotation = Quaternion.Euler(0f, 0f, newAngle);
        else if (rb != null)
            rb.MoveRotation(newAngle);
    }

    void ClampToBoundary()
    {
        if (boundaryRadius <= 0f)
            return;

        if (usesUIPosition)
        {
            Vector2 offset = playerRect.anchoredPosition - boundaryCenter;
            float playerRadius = Mathf.Max(playerRect.rect.width, playerRect.rect.height) * 0.5f;
            float allowedRadius = Mathf.Max(0f, boundaryRadius - playerRadius);

            if (offset.sqrMagnitude > allowedRadius * allowedRadius)
            {
                Vector2 directionToPlayer = offset.normalized;
                playerRect.anchoredPosition = boundaryCenter + directionToPlayer * allowedRadius;

                float outwardSpeed = Vector2.Dot(currentVelocity, directionToPlayer);
                if (outwardSpeed > 0f)
                    currentVelocity -= directionToPlayer * outwardSpeed;
            }

            return;
        }

        if (rb == null)
            return;

        Vector2 worldOffset = rb.position - boundaryCenter;
        if (worldOffset.sqrMagnitude > boundaryRadius * boundaryRadius)
        {
            Vector2 directionToPlayer = worldOffset.normalized;
            rb.position = boundaryCenter + directionToPlayer * boundaryRadius;

            float outwardSpeed = Vector2.Dot(rb.velocity, directionToPlayer);
            if (outwardSpeed > 0f)
                rb.velocity -= directionToPlayer * outwardSpeed;
        }
    }

    void CreateBoundaryVisual()
    {
        GameObject boundaryObject = new GameObject(
            "Player Movement Boundary",
            typeof(RectTransform),
            typeof(Boundary)
        );

        RectTransform boundaryRect = boundaryObject.GetComponent<RectTransform>();
        boundaryRect.SetParent(boundaryParent, false);
        boundaryRect.anchorMin = new Vector2(0.5f, 0.5f);
        boundaryRect.anchorMax = new Vector2(0.5f, 0.5f);
        boundaryRect.pivot = new Vector2(0.5f, 0.5f);
        boundaryRect.sizeDelta = Vector2.one * (boundaryRadius * 2f);
        boundaryRect.anchoredPosition = boundaryCenter;

        Boundary boundary = boundaryObject.GetComponent<Boundary>();
        boundary.radius = boundaryRadius;
        boundary.thickness = 3f;
        boundary.color = boundaryColor;
        boundary.density = Mathf.Max(1, boundaryDensity);
        boundary.squareSize = Mathf.Max(1f, boundarySquareSize);
        boundary.squareColor = boundarySquareColor;
        boundary.squareParent = boundarySquareParent != null
            ? boundarySquareParent
            : boundaryParent;
        boundary.squarePrefab = boundarySquarePrefab;
        boundary.raycastTarget = false;
        boundary.SetVerticesDirty();
        boundary.CreateBoundarySquares();

        // Keep the ring behind the player and any projectiles.
        boundaryObject.transform.SetAsFirstSibling();
    }

    void Shoot()
    {
        if (Bullet == null)
        {
            Debug.LogWarning("Bullet prefab is not assigned!");
            return;
        }

        Transform spawnParent = projectileParent != null ? projectileParent : transform.parent;
        RectTransform parentRect = spawnParent as RectTransform;
        Canvas parentCanvas = parentRect != null ? parentRect.GetComponentInParent<Canvas>() : null;
        Camera mainCamera = Camera.main;

        if (parentCanvas == null && mainCamera == null)
        {
            Debug.LogWarning("A camera tagged MainCamera is required to aim world-space projectiles.");
            return;
        }

        Vector2 aimDirection;
        Camera aimCamera;

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
            spawnParent
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
