using UnityEngine;

public class Gauge : MonoBehaviour
{
    private int value;
    private int max_value;
    private RectTransform my_rect;
    
    int width;

    public void Init(int value, int max_value, int width) //250
    {
        this.width = width;
        my_rect = GetComponent<RectTransform>();
        this.max_value = max_value;
        SetValue(value);
    }


    #region PROPERTY
    public void SetValue(int value)
    {
        this.value = value;
        if (value > max_value)
        {
            value = max_value;
        }
        else if (value < 0)
        {
            value = 0;
        }
        Vector3 s = transform.localScale;
        float f = (float)value / max_value;

        my_rect.anchoredPosition = new Vector3((f - 1) / 2 * width, 0, 0);
        my_rect.sizeDelta = new Vector2(f * width, my_rect.sizeDelta.y);
    }
    public void AddValue(int value)
    {
        SetValue(this.value + value);
    }
    public int GetValue()
    {
        return value;
    }
    public int GetMaxValue()
    {
        return max_value;
    }

    #endregion
}
