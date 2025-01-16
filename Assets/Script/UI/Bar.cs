using UnityEngine;

public class Bar : MonoBehaviour
{
    private int value;
    private int max_value;
    private RectTransform my_rect;
    
    static int WIDTH = 250;

    public void Init(int value, int max_value)
    {
        my_rect = GetComponent<RectTransform>();
        this.max_value = max_value;
        SetValue(value);
    }

    public void SetValue(int value)
    {
        this.value = value;
        Vector3 s = transform.localScale;
        float f = (float)value / max_value;

        my_rect.anchoredPosition = new Vector3((f - 1) / 2 * WIDTH, 0, 0);
        my_rect.sizeDelta = new Vector2(f * WIDTH, my_rect.sizeDelta.y);
    }
}
