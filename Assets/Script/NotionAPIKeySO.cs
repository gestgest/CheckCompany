using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NotionAPIKeySO", menuName = "ScriptableObject/API")]
public class NotionAPIKeySO : ScriptableObject
{
    [SerializeField] private string apiKey;

    public string GetAPIKey()
    {
        return apiKey;
    }
}
