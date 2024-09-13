using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;



public class FireStoreManager : MonoBehaviour
{
    static FirebaseFirestore db;
    public GameManager gameManager;

    //나중에 싱글톤으로 해야한다
    public void Init()
    {
        // Firebase Firestore 초기화
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firebase Firestore Initialized");
        });
    }

    public void SetFirestore(string key, int value)
    {
        // 저장할 데이터
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { key, value }
        };
        /*
            { key, value },
            { key, value }
        */
        //일단 employees에 Employee가 들어있어야 한다

        // "users" 컬렉션에 새로운 문서 생성
        //db.Collection("user").AddAsync(user).ContinueWithOnMainThread(task =>
        db.Collection("user").Document("milkan660").SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("데이터 입력 성공");
            }
            else
            {
                Debug.LogError("Error adding data: " + task.Exception);
            }
        });
    }

    public async Task<object> GetFirestore(string id, string key)
    {
        Dictionary<string, object> result = null;
        // 특정 문서 가져오기 => 임시 
        DocumentReference docRef = db.Collection("user").Document(id);
        await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    result = snapshot.ConvertTo<Dictionary<string, object>>();
                    Debug.Log("Get Data : " + result[key]);
                    gameManager.Money = (int)result[key];
                }
                else
                {
                    Debug.Log("No such document!");
                }
            }
            else
            {
                Debug.LogError("Failed to get document: " + task.Exception);
            }
        });


        return result[key];
    }

}
//mDatabaseRef.Child("users").Child(userId).Child("username").SetValueAsync(name);