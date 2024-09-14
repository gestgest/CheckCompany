using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    //UI컴포넌트 버튼이나 인풋 field
    private Transform selected_parent; //부모 오브젝트
    [SerializeField] private List<Selectable> selected_objects; //선택 오브젝트들

    int index;

    void Start()
    {
        selected_parent = transform;
        
        for(int i = 0; i < selected_parent.childCount; i++)
        {
            Selectable sel = selected_parent.GetChild(i).GetComponent<Selectable>();
            if(sel == null){
                continue;
            }
            selected_objects.Add(sel);
            //selected_objects.
        }
        Index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Index++;
        }
    }

    public int Index
    {
        get { return index; }
        set {
            index = value;

            if(selected_objects.Count == 0)
            {
                return;
            }
            //out of bound 해결
            if(selected_objects.Count <= index){
                index -= selected_objects.Count;
            }
            selected_objects[index].Select();
        }
    }
}
