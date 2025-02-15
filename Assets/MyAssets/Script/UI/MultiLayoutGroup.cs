using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MultiLayoutGroup : MonoBehaviour
{
    //여러가지 gameobject 그룹들 => transform
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private RectTransform downObejct_RectTransform;
    
    private float before_pos_y = 0;

    void Start()
    {
        //RerollScreen();
    }
    private void LateUpdate()
    {
        Debug.Log("오브젝트 : " + downObejct_RectTransform.anchoredPosition.y); 

        //만약 그 전 값과 현재 y값이 같다 => 바뀐게 없음
        if (before_pos_y == downObejct_RectTransform.anchoredPosition.y)
        {
            RerollScreen();
            //바뀌었다면
            // if(before_pos_y != downObejct_RectTransform.anchoredPosition.y)
            //     isReroll = false;
        }
    }

    public void SetDownObjectPos(RectTransform rectTransform)
    {
        this.downObejct_RectTransform = rectTransform;   
    }
    public void SetBeforePosY()
    {
        before_pos_y = downObejct_RectTransform.anchoredPosition.y;
        Debug.Log("이후 : " + before_pos_y); 

    }
    
    //화면 새로고침
    
    void RerollScreen()
    {
        verticalLayoutGroup.enabled = false;
        verticalLayoutGroup.enabled = true;
        Debug.Log("리롤"); 
    }
    
}
