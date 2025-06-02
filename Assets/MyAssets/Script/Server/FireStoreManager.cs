using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine.Serialization;


public class FireStoreManager : MonoBehaviour
{
    static FirebaseFirestore db;
    
    [Header("Listening to channels")]
    [SerializeField] private VoidEventChannelSO _initFirebaseChannelEvent;

    [SerializeField] DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    [SerializeField] SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;
    [SerializeField] SendFirebaseEventChannelSO _newSendFirebaseEventChannelSO;

    private void OnEnable()
    {
        _initFirebaseChannelEvent._onEventRaised += Init;
        
        _deleteFirebaseEventChannelSO.OnDeleteEventRaised += DeleteFirestoreDataKey;
        _sendFirebaseEventChannelSO.OnSendEventRaised += SetFirestoreData;
        _newSendFirebaseEventChannelSO.OnSendEventRaised += SetNewFirestoreData;
    }

    private void OnDisable()
    {
        _initFirebaseChannelEvent._onEventRaised -= Init;
        
        _deleteFirebaseEventChannelSO.OnDeleteEventRaised -= DeleteFirestoreDataKey;
        _sendFirebaseEventChannelSO.OnSendEventRaised -= SetFirestoreData;
        _newSendFirebaseEventChannelSO.OnSendEventRaised -= SetNewFirestoreData;
    }

    //이미 app과 db는 싱글톤이다.
    public void Init()
    {
        // Firebase Firestore 초기화 => 게임 시작할땐 GamaManager필요없다.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firestore 작동");

            //이거 스타트가 아닌 로그인 이후 작동해야 한다. 
            if(GameManager.instance != null)
                GameManager.instance.GameServerStart(); //=> 대충 서버에서 데이터 들어오는 함수
        });
    }

    //키 Delete
    public void DeleteFirestoreDataKey(string collection_name, string document_name, string key)
    {
        //db.Collection(collection_name).Document(document_name).DeleteAsync(); => 문서 제거

        SetFirestoreData(collection_name, document_name, key, FieldValue.Delete);
    }

    public void SetFirestoreData(string collection_name, string document_name, string key, object value)
    {
        // 저장할 데이터
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { key, value }
        };
        /*
            { key, value },
            { key, value }
        */
        //일단 employees에 Employee가 들어있어야 한다
        
        // "users" 컬렉션에 새로운 문서 생성
        //db.Collection("GamePlayUser").AddAsync(user).ContinueWithOnMainThread(task =>
        //db.Collection(collection_name).Document(document_name).SetAsync(data).ContinueWithOnMainThread(task =>
        db.Collection(collection_name).Document(document_name).UpdateAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log(collection_name +"/" + document_name + "/" + key  +  " : 데이터 입력 성공");
            }
            else
            {
                Debug.LogError("Error adding data: " + task.Exception);
            }
        });
    }

    public void SetNewFirestoreData(string collection_name, string document_name, string key, object value)
    {
        // 저장할 데이터
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { key, value }
        };
        /*
            { key, value },
            { key, value }
        */
        //일단 employees에 Employee가 들어있어야 한다
        
        // "users" 컬렉션에 새로운 문서 생성
        //db.Collection("GamePlayUser").AddAsync(user).ContinueWithOnMainThread(task =>
        db.Collection(collection_name).Document(document_name).SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log(collection_name +"/" + document_name + "/" + key  +  " : 데이터 입력 성공");
            }
            else
            {
                Debug.LogError("Error adding data: " + task.Exception);
            }
        });
    }

    public async Task<object> GetFirestoreData(string collection_name, string id, string key)
    {
        Dictionary<string, object> result = null;
        // 특정 문서 가져오기 => 임시 
        DocumentReference docRef = db.Collection(collection_name).Document(id);

        await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    result = snapshot.ConvertTo<Dictionary<string, object>>();
                    //Debug.Log("받기 성공했다네 : " + key);
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

        //key값이 없는 경우
        if (!result.ContainsKey(key))
        {
            return null;
        }
        
        return result[key];
    }
}
//mDatabaseRef.Child("users").Child(userId).Child("username").SetValueAsync(name);