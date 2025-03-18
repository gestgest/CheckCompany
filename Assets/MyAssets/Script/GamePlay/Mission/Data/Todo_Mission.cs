using System.Collections.Generic;
using UnityEngine;

public class Todo_Mission
{
    string title;
    bool isDone;

    public void Set_Todo_Mission(string title, bool isDone = false)
    {
        this.title = title;
        this.isDone = isDone;
    }

    public void Set_Todo_Mission(Dictionary<string, object> data)
    {
        Set_Todo_Mission((string)data["title"], (bool)data["isDone"]);
    }
}
