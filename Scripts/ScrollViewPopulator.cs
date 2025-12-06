using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ScrollViewItemData
{
    [TextArea(2, 5)]
    public string mainText;
    public string categoryText;
}

public class ScrollViewPopulator : MonoBehaviour
{
    [Header("UI Referansları")]
    public Transform contentParent;
    public GameObject itemPrefab;
    public GameObject matchReviewPanel; // Anlatım panelinin kendisi
    public ScrollRect scrollRect; // Otomatik kaydırma için referans

    [Header("Spawn Ayarları")]
    public float spawnInterval = 1f; // Her bir anlatım olayının arasındaki saniye

    [Header("Olasılık Ayarları (%)")]
    [Range(0, 100)] public int goalChance = 5;       // Gol olma ihtimali
    [Range(0, 100)] public int actionChance = 30;     // "Top Karakterde" gelme ihtimali
    // Geriye kalan ihtimal (%100 - goal - action) diğer olaylar için kullanılır.

    [Header("Veri Listesi")]
    public List<ScrollViewItemData> itemsToSpawn = new List<ScrollViewItemData>
    {
    };

    private Coroutine spawningCoroutine;

    private List<ScrollViewItemData> goalItems = new List<ScrollViewItemData>();
    private List<ScrollViewItemData> actionItems = new List<ScrollViewItemData>();
    private List<ScrollViewItemData> otherItems = new List<ScrollViewItemData>();

    void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInParent<ScrollRect>();
            if (scrollRect == null) scrollRect = FindFirstObjectByType<ScrollRect>();
        }

        CategorizeItems();
    }

    void CategorizeItems()
    {
        goalItems.Clear();
        actionItems.Clear();
        otherItems.Clear();

        foreach (var item in itemsToSpawn)
        {
            if (item.categoryText.Contains("Gol") && !item.categoryText.Contains("Kaçırma"))
            {
                goalItems.Add(item);
            }
            else if (item.categoryText == "Top Karakterde")
            {
                actionItems.Add(item);
            }
            else
            {
                otherItems.Add(item);
            }
        }
    }

    public void StartSpawning()
    {
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
        }
        spawningCoroutine = StartCoroutine(SpawnItemsSequentially());
    }

    IEnumerator SpawnItemsSequentially()
    {
        MatchManager.instance.ResumeMatch();

        while (!MatchManager.instance.isMatchOver)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (MatchManager.instance.isMatchOver) yield break;

            ScrollViewItemData selectedItem = SelectRandomItemWeighted();

            GameObject newItem = Instantiate(itemPrefab, contentParent);
            ScrollViewItem itemScript = newItem.GetComponent<ScrollViewItem>();
            if (itemScript != null)
            {
                itemScript.Setup(selectedItem);
            }

            if (scrollRect != null && contentParent != null)
            {
                yield return new WaitForEndOfFrame();
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
                Canvas.ForceUpdateCanvases();
                
                RectTransform contentRect = contentParent as RectTransform;
                if (contentRect != null)
                {
                    float contentHeight = contentRect.rect.height;
                    float viewportHeight = scrollRect.viewport != null ? scrollRect.viewport.rect.height : scrollRect.GetComponent<RectTransform>().rect.height;
                    
                    if (contentHeight > viewportHeight)
                    {
                        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, contentHeight - viewportHeight);
                    }
                }
                
                scrollRect.velocity = Vector2.zero;
            }

            if (selectedItem.categoryText == "Top Karakterde")
            {
                yield return new WaitForSeconds(spawnInterval);

                // Oyunu başlat
                MatchManager.instance.StartPlaySession();
                yield break;
            }
        }
    }

    ScrollViewItemData SelectRandomItemWeighted()
    {
        int randomValue = Random.Range(0, 100);

        if (randomValue < goalChance && goalItems.Count > 0)
        {
            return goalItems[Random.Range(0, goalItems.Count)];
        }
        else if (randomValue < goalChance + actionChance && actionItems.Count > 0)
        {
            return actionItems[Random.Range(0, actionItems.Count)];
        }
        else if (otherItems.Count > 0)
        {
            return otherItems[Random.Range(0, otherItems.Count)];
        }
        
        return itemsToSpawn[Random.Range(0, itemsToSpawn.Count)];
    }

    IEnumerator StartPlaySessionAfterDelay()
    {
        yield return null;
        MatchManager.instance.StartPlaySession();
    }

    public void SpawnMatchEndItem()
    {
        ScrollViewItemData matchEndData = new ScrollViewItemData
        {
            categoryText = "Maç Bitti",
            mainText = $"Maç sona erdi! Skor: {MatchManager.instance.ourTeamScore} - {MatchManager.instance.opponentTeamScore}"
        };
        SpawnSpecificItem(matchEndData);
    }

    public void SpawnStarPlayerGoalItem()
    {
        ScrollViewItemData starGoalData = new ScrollViewItemData
        {
            categoryText = "Yıldız Oyuncu Gol",
            mainText = $"GOOOL! Yıldız oyuncumuz fileleri havalandırdı! ({MatchManager.instance.starPlayerGoals}. golü)"
        };
        SpawnSpecificItem(starGoalData);
    }

    public void SpawnTeammateGoalItem()
    {
        ScrollViewItemData teammateGoalData = new ScrollViewItemData
        {
            categoryText = "Takım Arkadaşı Gol",
            mainText = $"GOOOL! Harika asist!({MatchManager.instance.starPlayerAssists}. asist)"
        };
        SpawnSpecificItem(teammateGoalData);
    }

    void SpawnSpecificItem(ScrollViewItemData itemData)
    {
        GameObject newItem = Instantiate(itemPrefab, contentParent);
        ScrollViewItem itemScript = newItem.GetComponent<ScrollViewItem>();
        if (itemScript != null)
        {
            itemScript.Setup(itemData);
        }

        StartCoroutine(ScrollToBottomCoroutine());
    }

    IEnumerator ScrollToBottomCoroutine()
    {
        if (scrollRect != null && contentParent != null)
        {
            yield return new WaitForEndOfFrame();
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
            Canvas.ForceUpdateCanvases();
            
            RectTransform contentRect = contentParent as RectTransform;
            if (contentRect != null)
            {
                float contentHeight = contentRect.rect.height;
                float viewportHeight = scrollRect.viewport != null ? scrollRect.viewport.rect.height : scrollRect.GetComponent<RectTransform>().rect.height;
                
                if (contentHeight > viewportHeight)
                {
                    contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, contentHeight - viewportHeight);
                }
            }
            
            scrollRect.velocity = Vector2.zero;
        }
    }
}
