using UnityEngine;

public class TimeButton : MonoBehaviour
{
    private float click_time = 0;
    private static float MIN_CLICK_TIME = 0.5f; 
    private static float MINUTE_ADD_TIME = 1f; 
    private bool isClick = false;
    private bool isOnEvent = false;

    public void ButtonDown()
    {
        isClick = true;   
    }
    public void ButtonUP()
    {
        isClick = false;
        isOnEvent = false;
        
        click_time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isClick)
        {
            return;
        }
        click_time += Time.deltaTime;
        if (!isOnEvent)
        {
            if (click_time >= MIN_CLICK_TIME)
            {
                //버튼 색이 빨간색으로 바뀌는 함수?
                isOnEvent = true;
            }
        }
        
        //꾹 누르면 1초마다 이벤트중
        else
        {
            if (click_time >= MINUTE_ADD_TIME)
            {
                click_time -= MINUTE_ADD_TIME;
                AddTime();
            }
        }
    }

    //꾸욱 누르면 발생하는 함수
    private void AddTime()
    {
        GameManager.instance.AddDateMinute(3600 * 20);
    }
}
