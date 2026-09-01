using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData] [Serializable]
public class Todo_Mission
{
    [FirestoreProperty][SerializeField] 
    public string Title { get; set; }
    [FirestoreProperty][SerializeField] 
    public bool IsDone { get; set; }

    public Todo_Mission()
    {
        
    }
    public Todo_Mission(string title, bool isDone = false)
    {
        Set_Todo_Mission(title, isDone);
    }
    
    public void Set_Todo_Mission(string title, bool isDone = false)
    {
        this.Title = title;
        this.IsDone = isDone;

        //토글 isOn
    }

    public void Set_Todo_Mission(object data)
    {
        Dictionary<string, object> tmp = data as Dictionary<string, object> ;
        string title = ConvertJSON.SafeGet<string>(tmp, "Title", "");
        bool isDone = ConvertJSON.SafeGet<bool>(tmp, "IsDone", false);
        
        Set_Todo_Mission(title, isDone);
    }

}
