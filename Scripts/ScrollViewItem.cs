using UnityEngine;
using TMPro;

public class ScrollViewItem : MonoBehaviour
{
    public TextMeshProUGUI mainTextComponent;

    public void Setup(ScrollViewItemData data)
    {
        if (mainTextComponent == null)
        {
            Debug.LogError("Lütfen prefab üzerindeki ScrollViewItem scriptine TextMeshPro referanslarını atayın!");
            return;
        }

        mainTextComponent.text = data.mainText;

        if (MatchManager.instance != null)
        {
            MatchManager.instance.RecordEvent(data);
        }
    }
}
