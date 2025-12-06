using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [Header("Fizik Ayarları")]
    public float gravity = 30f;
    public float curveStrength = 40f;
    public float friction = 1.5f;
    public float curveRotationSpeed = 300f;
    public float baseSpinSpeed = 250f;

    [Header("Görsel Referanslar")]
    public Transform ballVisual;
    public Transform shadow;

    [Header("Katman (Layer) Ayarları")]
    public float crossbarHeight = 2.44f; // Bu yüksekliği geçince katman değişir
    public int lowSortingOrder = 3;      // Top yerdeyken/alçakken katman sırası
    public int highSortingOrder = 10;    // Top havadayken (direği geçince) katman sırası

    // Durum Değişkenleri
    private float height = 0f;
    private float verticalVelocity = 0f;
    private float currentCurve = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer ballSpriteRenderer;
    public bool isOutOfPlay = false; // Topun aut'a çıkıp çıkmadığını kontrol eder

    public bool IsStopped
    {
        get { return rb.linearVelocity.magnitude <= 0.05f && height <= 0.01f; }
    }

    // Dışarıdan yüksekliği okumak için (Kale scripti kullanıyor)
    public float GetHeight()
    {
        return height;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = friction;

        if (ballVisual != null)
        {
            ballSpriteRenderer = ballVisual.GetComponent<SpriteRenderer>();
        }
    }

    void OnEnable()
    {
        isOutOfPlay = false;
    }

    void Update()
    {
        if (height > 0 || verticalVelocity > 0)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            height += verticalVelocity * Time.deltaTime;

            if (height <= 0)
            {
                height = 0;
                verticalVelocity = 0;
                currentCurve *= 0.6f;
            }
        }

        UpdateVisuals();
        UpdateSortingOrder();
    }

    void FixedUpdate()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > 0.3f && Mathf.Abs(currentCurve) > 0.01f)
        {
            float physicalDampener = Mathf.Clamp01(speed / 2f);

            Vector2 velocity = rb.linearVelocity;
            Vector2 curveDirection = new Vector2(velocity.y, -velocity.x).normalized;

            float efficiency = (height > 0) ? 1.0f : 0.5f;

            rb.AddForce(curveDirection * currentCurve * curveStrength * efficiency * physicalDampener);

            currentCurve = Mathf.Lerp(currentCurve, 0, Time.fixedDeltaTime * 0.5f);
        }
        else
        {
            currentCurve = 0f;
        }
    }

    void UpdateVisuals()
    {
        shadow.localPosition = Vector3.zero;

        ballVisual.localPosition = new Vector3(0, height, 0);

        float shadowScale = Mathf.Clamp(1 - (height * 0.15f), 0.4f, 1f);
        shadow.localScale = Vector3.one * shadowScale;

        float speed = rb.linearVelocity.magnitude;

        if (speed > 0.25f)
        {
            float baseSpin = speed * baseSpinSpeed * Time.deltaTime;

            float visualDampener = Mathf.Clamp01(speed / 4f);
            float curveSpin = -currentCurve * curveRotationSpeed * visualDampener * Time.deltaTime;

            ballVisual.Rotate(0, 0, baseSpin + curveSpin, Space.Self);
        }
    }

    void UpdateSortingOrder()
    {
        if (ballSpriteRenderer == null) return;

        if (height > crossbarHeight)
        {
            ballSpriteRenderer.sortingOrder = highSortingOrder;
        }
        else if (height < crossbarHeight)
        {
            ballSpriteRenderer.sortingOrder = lowSortingOrder;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Aut"))
        {
            isOutOfPlay = true;
            MatchManager.instance.EndPlaySession();
        }
    }

    public void Shoot(Vector2 direction, float speed, float loftAmount, float curveAmount)
    {
        rb.linearVelocity = direction * speed;
        verticalVelocity = loftAmount;
        currentCurve = curveAmount;
    }

    public void StopBall()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        height = 0f;
        verticalVelocity = 0f;
        currentCurve = 0f;
    }
}