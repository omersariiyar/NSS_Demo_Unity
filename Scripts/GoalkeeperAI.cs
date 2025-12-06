using UnityEngine;
using System.Collections;

public class GoalkeeperAI : MonoBehaviour
{
    [Header("Kaleci Ayarları")]
    public float patrolSpeed = 3f;          // Normal pozisyon alma hızı
    public float diveSpeed = 12f;           // Topa atlama hızı
    public float diveTriggerDistance = 7f;  // Topu takip etme mesafesi
    public float patrolAreaHeight = 5f;     // Kalede ne kadarlık bir alanda gezebileceği

    private BallPhysics ballPhysics;
    private Transform ballTransform;
    private ShootingManager shootingManager;
    private Rigidbody2D ballRb;

    private Vector2 startPosition;
    private bool isDiving = false;
    private bool startPositionSet = false;

    void Start()
    {
        if (!startPositionSet)
        {
            startPosition = transform.position;
            startPositionSet = true;
        }
        FindReferences();
    }

    void OnEnable()
    {
        isDiving = false;
        FindReferences();
    }

    void FindReferences()
    {
        ballPhysics = FindFirstObjectByType<BallPhysics>();
        shootingManager = FindFirstObjectByType<ShootingManager>();

        if (ballPhysics != null)
        {
            ballTransform = ballPhysics.transform;
            ballRb = ballPhysics.GetComponent<Rigidbody2D>();
        }
        else
        {
            ballTransform = null;
            ballRb = null;
        }
    }

    void Update()
    {
        if (ballPhysics == null || ballTransform == null || ballRb == null)
        {
            FindReferences();
            if (ballPhysics == null || ballTransform == null || ballRb == null)
            {
                return;
            }
        }

        if (isDiving)
        {
            return;
        }

        if (ballPhysics.IsStopped)
        {
            Patrol();
            return;
        }

        float distanceToBall = Vector2.Distance(transform.position, ballTransform.position);
        Vector2 ballVelocity = ballRb.linearVelocity;

        bool ballComingTowardsGoal = false;

        if (startPosition.x < 0)
        {
            ballComingTowardsGoal = ballVelocity.x < -0.5f;
        }
        else
        {
            ballComingTowardsGoal = ballVelocity.x > 0.5f;
        }

        if (distanceToBall <= diveTriggerDistance && ballComingTowardsGoal)
        {
            StartCoroutine(DiveCoroutine());
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (ballTransform == null) return;

        float targetY = Mathf.Clamp(ballTransform.position.y, startPosition.y - patrolAreaHeight / 2, startPosition.y + patrolAreaHeight / 2);
        Vector2 targetPosition = new Vector2(startPosition.x, targetY);

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
    }

    IEnumerator DiveCoroutine()
    {
        isDiving = true;
        Debug.Log("Kaleci atlıyor!");

        float targetY = Mathf.Clamp(ballTransform.position.y, startPosition.y - patrolAreaHeight / 2, startPosition.y + patrolAreaHeight / 2);
        Vector2 divePosition = new Vector2(startPosition.x, targetY);

        float timeout = 1f;
        float elapsed = 0f;

        while (Vector2.Distance(transform.position, divePosition) > 0.05f && elapsed < timeout)
        {
            transform.position = Vector2.MoveTowards(transform.position, divePosition, diveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        isDiving = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ballPhysics == null || ballTransform == null) return;

        if (other.transform == ballTransform && ballPhysics.GetHeight() <= 2.5f)
        {
            Debug.Log("Kaleci topu kurtardı!");

            ballPhysics.StopBall();

            if (shootingManager != null)
            {
                MatchManager.instance.EndPlaySession();
            }
        }
        else if (other.transform == ballTransform)
        {
            Debug.Log("Top kalecinin üzerinden geçti! Yükseklik: " + ballPhysics.GetHeight());
        }
    }
}
