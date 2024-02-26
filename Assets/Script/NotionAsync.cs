using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NotionAsync : MonoBehaviour
{
    /*
    private const string apiUrl = "https://api.notion.com/v1/database/a3043281d65b4a238301fd1019a40a6a";
    private const string apiKey = "secret_b5JDoyw1hpTiF6NLIsjvzhewY7Pr4yEvbSMJ3Y5DHtE";
    //https://www.notion.so/0250a3b8cbbe4b02ac08ffda81e61bab?v=73d1307224bc41bb8c7969a4cd1429f5&pvs=4
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
            Debug.LogError("½ÇÆÐ : "+ request.error);
        }
    }
    */
}
