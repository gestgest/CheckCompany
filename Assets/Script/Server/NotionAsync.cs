using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NotionAsync : MonoBehaviour
{
    [SerializeField] private NotionAPIKeySO notionAPIKey;

    //넥슨
    private string apiKey;
    private const string apiUrl = "https://api.notion.com/v1/blocks/22968b85dcd641eea2dfe612af8d2375";
    void Start()
    {
        apiKey = notionAPIKey.GetAPIKey();
        StartCoroutine(GetNexonData());
    }
    IEnumerator GetNexonData()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " +apiKey);
        request.SetRequestHeader("Notion-Version", "2022-06-28");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("실패 : " + request.error);
        }
    }
}
