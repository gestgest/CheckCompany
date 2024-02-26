using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NotionAsync : MonoBehaviour
{
    private const string apiUrl = "YOUR_NOTION_API_ENDPOINT";
    private const string apiKey = "secret_b5JDoyw1hpTiF6NLIsjvzhewY7Pr4yEvbSMJ3Y5DHtE";

    void Start()
    {
        StartCoroutine(GetNotionData());
    }

    IEnumerator GetNotionData()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parse and handle the Notion API response data here
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
}
