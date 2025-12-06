using UnityEngine;

public class TeammateAI : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    [Tooltip("Eğer bu true ise, oyuncu topu aldığında kaleye şut çeker.")]
    public bool canGoal = false;
    public float moveSpeed = 4f;
    [Tooltip("Oyuncunun topu kovalamaya başlayacağı mesafe.")]
    public float chaseRadius = 8f;

    [Header("Şut Parametreleri (canGoal true ise)")]
    public float shotPower = 25f;
    public float shotLoft = 10f;

    private BallPhysics ballPhysics;
    private ShootingManager shootingManager;
    private Transform goalTarget;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ballPhysics = FindObjectOfType<BallPhysics>();
        shootingManager = FindObjectOfType<ShootingManager>();

        // Hedef kaleyi "GoalPost" tag'i ile bul
        GameObject goalObject = GameObject.FindGameObjectWithTag("GoalPost");
        if (goalObject != null)
        {
            goalTarget = goalObject.transform;
        }
        else if (canGoal)
        {
            Debug.LogError("Sahnede 'GoalPost' tag'ıne sahip bir obje bulunamadı! TeammateAI şut çekemeyecek.");
        }

        if (ballPhysics == null)
        {
            Debug.LogError("Sahnede BallPhysics scriptine sahip bir obje bulunamadı!");
            this.enabled = false;
        }
        if (shootingManager == null)
        {
            Debug.LogError("Sahnede ShootingManager scriptine sahip bir obje bulunamadı!");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (ballPhysics.transform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

        float distanceToBall = Vector2.Distance(transform.position, ballPhysics.transform.position);

        if (distanceToBall <= chaseRadius)
        {
            transform.position = Vector2.MoveTowards(transform.position, ballPhysics.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball") || ballPhysics.IsStopped)
        {
            return;
        }

        if (ballPhysics.GetHeight() >= 2.5f)
        {
            Debug.Log("Top takım arkadaşının üzerinden geçti! Yükseklik: " + ballPhysics.GetHeight());
            return;
        }

        if (canGoal)
        {
            if (goalTarget == null)
            {
                Debug.LogError("TeammateAI şut çekemiyor çünkü sahnede 'GoalPost' tag'ine sahip bir hedef bulunamadı.");
                return;
            }

            Debug.Log("Takım arkadaşı kaleye şut çekiyor!");

            if (goalTarget.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }

            MatchManager.instance.starPlayerPass++;

            ballPhysics.StopBall();

            Vector2 direction = (goalTarget.position - transform.position).normalized;

            MatchManager.instance.lastShooterIsTeammate = true;

            ballPhysics.Shoot(direction, shotPower, shotLoft, 0f);
        }
        else
        {
            Debug.Log("Bu oyuncu şut çekemez! Oyun yeniden başlatılıyor.");

            ballPhysics.StopBall();

            if (shootingManager != null)
            {
                MatchManager.instance.EndPlaySession();
            }
        }
    }
}
