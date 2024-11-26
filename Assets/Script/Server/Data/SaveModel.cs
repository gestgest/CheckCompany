using UnityEngine;

public class SaveModel
{
    //세이브 구조체 느낌
    //MVC 데이터 모델 느낌

    //JSON 변환 함수
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

}
