using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//놓는 물체
[CreateAssetMenu(fileName = "ObjectSO", menuName = "ScriptableObject/ObjectSO")]
public class ObjectSO : ScriptableObject
{
    [SerializeField] private string object_name;
    [SerializeField] private int width;
    [SerializeField] private int length;
    [SerializeField] private int money;


    //fbx?


    //type [책상인지 뭔지]
}