using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MatchManager : MonoBehaviour
{
    public GameObject StatsPanel;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI StatsText;
    public static MatchManager instance;

    [Header("Maç Durumu")]
    public float matchTime = 0f;
    public bool isMatchOver = false;
    private bool isMatchPaused = true;

    [Header("Skor")]
    public int ourTeamScore = 0;
    public int opponentTeamScore = 0;
    public int starPlayerGoals = 0;
    public int starPlayerAssists = 0;
    public int starPlayerPass = 0;
    public int starPlayerLostBall = 0;

    // Son vuruşu kimin yaptığını takip et
    [HideInInspector] public bool lastShooterIsTeammate = false;

    [Header("Referanslar")]
    public TextMeshProUGUI timerText;
    public ScrollViewPopulator scrollViewPopulator;
    public GameSpawner gameSpawner;
    public BallPhysics ballPhysics;
    public ShootingManager shootingManager;
    public GameObject matchReviewPanel;
    public GameObject gamePanel; // Oyuncuları, topu vb. içeren ana panel/obje
    public Button matchEndButton;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        gamePanel.SetActive(false);
        matchReviewPanel.SetActive(true);
        scrollViewPopulator.StartSpawning();
    }

    void Update()
    {
        if (isMatchOver || isMatchPaused) return;

        matchTime += Time.deltaTime * 1.5f;

        if (matchTime >= 90f)
        {
            matchTime = 90f;
            isMatchOver = true;
            Debug.Log("MAÇ BİTTİ!");
            OnMatchEnd();
        }
        UpdateTimerUI();

        ScoreText.text = $"Senin Takımın: {ourTeamScore}  Karşı Takım: {opponentTeamScore}";
        StatsText.text = $"Toplam Gol: {starPlayerGoals} \nToplam Asist: {starPlayerAssists} \nToplam Pas: {starPlayerPass} \nToplam Top Kaybı: {starPlayerLostBall}";
    }

    public void StartPlaySession()
    {
        PauseMatch();
        matchReviewPanel.SetActive(false);
        gamePanel.SetActive(true);

        // Önceki oyuncuları sil
        gameSpawner.ClearSpawnedObjects();

        if (ballPhysics != null)
        {
            ballPhysics.isOutOfPlay = false;
            ballPhysics.transform.position = Vector3.zero;
        }

        gameSpawner.SpawnAllObjects();

        if (shootingManager != null)
        {
            shootingManager.ResetSystem();
        }
    }

    public void EndPlaySession()
    {
        if (ballPhysics != null)
        {
            ballPhysics.StopBall();
            ballPhysics.transform.position = Vector3.zero;
        }

        gamePanel.SetActive(false);
        matchReviewPanel.SetActive(true);

        // Anlatıma devam et
        scrollViewPopulator.StartSpawning();
    }

    public void RecordEvent(ScrollViewItemData eventData)
    {
        if (eventData.categoryText == "Bizim Takım Gol") ourTeamScore++;
        else if (eventData.categoryText == "Karşı Takım Gol") opponentTeamScore++;
    }

    public void PauseMatch()
    {
        isMatchPaused = true;
    }

    public void ResumeMatch()
    {
        if (!isMatchOver) isMatchPaused = false;
    }



    void UpdateTimerUI()
    {
        if (timerText != null) timerText.text = $"{(int)matchTime}'";
    }

    void OnMatchEnd()
    {
        if (scrollViewPopulator != null)
        {
            scrollViewPopulator.SpawnMatchEndItem();
        }

        if (matchEndButton != null)
        {
            matchEndButton.interactable = true;
            matchEndButton.gameObject.SetActive(true);
        }
    }

    // Gol olduğunda çağrılır (GoalCheck'ten)
    public void OnGoalScored()
    {
        ourTeamScore++;

        if (lastShooterIsTeammate)
        {
            // Takım arkadaşı gol attı, oyuncuya asist
            starPlayerAssists++;
            if (scrollViewPopulator != null)
            {
                scrollViewPopulator.SpawnTeammateGoalItem();
            }
        }
        else
        {
            // Oyuncu gol attı
            starPlayerGoals++;
            if (scrollViewPopulator != null)
            {
                scrollViewPopulator.SpawnStarPlayerGoalItem();
            }
        }

        lastShooterIsTeammate = false;
    }

    public void OnStarPlayerGoal()
    {
        OnGoalScored();
    }

    public void StatsButton()
    {
        StatsPanel.SetActive(true);
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}
