using UnityEngine;
using System.Collections.Generic;

public class GameSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> teammatePrefabs;
    public List<GameObject> opponentPrefabs;

    [Header("Spawn Miktarları")]
    public int teammateCount = 3;
    public int opponentCount = 3;

    [Header("Spawn Alanı")]
    public Vector2 spawnAreaCenter = Vector2.zero;
    public Vector2 spawnAreaSize = new Vector2(20, 10);

    [Header("Yerleştirme Ayarları")]
    [Tooltip("Objelerin birbirine en az ne kadar yakın spawn olabileceği.")]
    public float minDistanceBetweenObjects = 1.5f;
    [Tooltip("Topun oyunculardan en az ne kadar uzakta spawn olacağı.")]
    public float minDistanceFromBall = 3f; // Bu değişkeni kullanacağız
    [Tooltip("Yeni bir pozisyon bulmak için deneme sayısı.")]
    private int maxSpawnAttempts = 50;

    private Bounds goalPostBounds;
    private bool isGoalPostFound = false;
    private Transform ballTransform;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        FindGoalPost();
        FindBall();
        SpawnAllObjects();
    }

    void OnEnable()
    {
        ClearSpawnedObjects();
        FindGoalPost();
        FindBall();
        SpawnAllObjects();
    }

    void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");

        if (ballObj == null)
        {
            BallPhysics bp = FindFirstObjectByType<BallPhysics>();
            if (bp != null) ballObj = bp.gameObject;
        }

        if (ballObj != null)
        {
            ballTransform = ballObj.transform;
        }
    }

    void FindGoalPost()
    {
        GameObject goalObject = GameObject.FindGameObjectWithTag("GoalPost");
        if (goalObject != null)
        {
            Collider2D goalCollider = goalObject.GetComponent<Collider2D>();
            if (goalCollider != null)
            {
                goalPostBounds = goalCollider.bounds;
                isGoalPostFound = true;
            }
            else
            {
                Debug.LogWarning("'GoalPost' objesinde Collider2D bulunamadı. Sadece pozisyon merkezini yasak bölge olarak kabul ediliyor.");
                goalPostBounds = new Bounds(goalObject.transform.position, Vector3.one * 2);
                isGoalPostFound = true;
            }
        }
        else
        {
            Debug.LogWarning("Sahnede 'GoalPost' tag'ine sahip bir obje bulunamadı! Kale alanı kontrolü yapılmayacak.");
        }
    }

    public void SpawnAllObjects()
    {
        SpawnGroup(teammatePrefabs, teammateCount, minDistanceBetweenObjects);
        SpawnGroup(opponentPrefabs, opponentCount, minDistanceBetweenObjects);
    }

    void SpawnGroup(List<GameObject> prefabs, int count, float minDistance)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning(gameObject.name + " için spawn edilecek prefab listesi boş.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Count)];
            SpawnObject(prefabToSpawn, minDistance);
        }
    }

    void SpawnObject(GameObject prefab, float minDistance)
    {
        if (prefab == null) return;

        Vector3 spawnPosition = Vector3.zero;
        bool positionFound = false;
        int attempts = 0;

        while (!positionFound && attempts < maxSpawnAttempts)
        {
            attempts++;
            float randomX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
            float randomY = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2);
            spawnPosition = new Vector3(randomX, randomY, 0);

            if (IsPositionValid(spawnPosition, minDistance))
            {
                GameObject spawnedObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
                spawnedPositions.Add(spawnPosition);
                spawnedObjects.Add(spawnedObj);
                positionFound = true;
            }
        }

        if (!positionFound)
        {
            Debug.LogError(prefab.name + " için geçerli bir spawn pozisyonu bulunamadı! Alanı genişletin veya obje sayısını/mesafesini azaltın.");
        }
    }

    bool IsPositionValid(Vector3 position, float minDistance)
    {
        if (isGoalPostFound && goalPostBounds.Contains(position))
        {
            return false;
        }
        if (ballTransform != null)
        {
            if (Vector3.Distance(position, ballTransform.position) < minDistanceFromBall)
            {
                return false;
            }
        }

        foreach (var spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    public void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
        spawnedPositions.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(new Vector3(spawnAreaCenter.x, spawnAreaCenter.y, 0), new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));

        if (Application.isPlaying && ballTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ballTransform.position, minDistanceFromBall);
        }
    }
}