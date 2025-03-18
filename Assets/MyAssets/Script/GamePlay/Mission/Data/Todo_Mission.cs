using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class Todo_Mission
{
    [FirestoreProperty]
    public string Title { get; set; }
    [FirestoreProperty]
    public bool IsDone { get; set; }

    public Todo_Mission()
    {
        
    }
    public Todo_Mission(string title, bool isDone = false)
    {
        this.Title = title;
        this.IsDone = isDone;
    }
    
    public void Set_Todo_Mission(string title, bool isDone = false)
    {
        this.Title = title;
        this.IsDone = isDone;
    }

    public void Set_Todo_Mission(object data)
    {
        Dictionary<string, object> tmp = data as Dictionary<string, object> ;
        Set_Todo_Mission((string)tmp["Title"], (bool)tmp["IsDone"]);
    }

}
