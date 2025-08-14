using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScrollSelectionController : MonoBehaviour
{
    public static ScrollSelectionController Instance;
    public ScrollRect scrollRect;
    public RectTransform content;
    public float itemWidth = 100f;     // Width of each item
    public float spacing = 10f; 
    public int currentIndex = 0;        // Spacing between items

    public List<Sprite> imgProfile;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    void OnScrollValueChanged(Vector2 pos)
    {
        // Calculate the center position of the viewport
        RectTransform viewport = scrollRect.viewport;
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);
        float viewportCenter = (viewportCorners[0].x + viewportCorners[3].x) / 2f;

        // Iterate through the content's children to find closest to viewport center
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform item = content.GetChild(i) as RectTransform;
            Vector3[] itemCorners = new Vector3[4];
            item.GetWorldCorners(itemCorners);
            float itemCenter = (itemCorners[0].x + itemCorners[3].x) / 2f;
            float distance = Mathf.Abs(itemCenter - viewportCenter);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        // closestIndex is now the selected (center-most) item
        Debug.Log("Selected item index: " + closestIndex);
        currentIndex = closestIndex;
        PlayerPrefs.SetInt("SavedProfileIndex", currentIndex);
        PlayerPrefs.Save();
        // (Optional) Call a method to highlight/select this item
    }
}