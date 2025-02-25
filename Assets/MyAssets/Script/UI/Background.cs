using UnityEngine;

public class Background : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        gameObject.GetComponent<RectTransform>().position = new Vector2(Screen.width / 2, Screen.height / 2);
    }
}
