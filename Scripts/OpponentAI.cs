using UnityEngine;
using System.Collections.Generic;

public class OpponentAI : MonoBehaviour
{
    [Header("AI Ayarları")]
    public float moveSpeed = 5f;
    public float chaseAreaRadius = 10f;

    private BallPhysics ballPhysics;
    private Transform ballTransform;
    private ShootingManager shootingManager;
    private SpriteRenderer spriteRenderer;

    private static List<OpponentAI> allOpponents = new List<OpponentAI>();

    void OnEnable()
    {
        if (!allOpponents.Contains(this))
        {
            allOpponents.Add(this);
        }
    }

    void OnDisable()
    {
        allOpponents.Remove(this);
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ballPhysics = FindObjectOfType<BallPhysics>();
        shootingManager = FindObjectOfType<ShootingManager>();

        if (ballPhysics != null)
        {
            ballTransform = ballPhysics.transform;
        }
        else
        {
            Debug.LogError("Sahnede BallPhysics scriptine sahip bir obje bulunamadı!");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (ballTransform == null || ballPhysics.IsStopped)
        {
            FaceBall();
            return;
        }

        FaceBall();

        float distanceToBall = Vector2.Distance(transform.position, ballTransform.position);

        if (distanceToBall <= chaseAreaRadius)
        {
            Vector2 direction = (ballTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, ballTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void FaceBall()
    {
        if (ballTransform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == ballTransform && ballPhysics.GetHeight() < 2.5f)
        {
            MatchManager.instance.starPlayerLostBall++;
            Debug.Log("Rakip oyuncu topa dokundu!");

            ballPhysics.StopBall();
            MatchManager.instance.EndPlaySession();
        }
        else if (other.transform == ballTransform)
        {
            Debug.Log("Top rakip oyuncunun üzerinden geçti! Yükseklik: " + ballPhysics.GetHeight());
        }
    }
}
