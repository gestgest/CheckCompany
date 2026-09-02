using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : Panel
{
    [SerializeField] private GameObject _categoryElementPrefab;
    [SerializeField] private Transform parent;

    private List<CategoryElement> _categoryElements;
    protected void Awake()
    {
        _categoryElements = new List<CategoryElement>();
        for(int i = 0; i < panels.Length; i++)
        {
            //for문 돌려서 Panel SO 정보값을 버튼으로 변환
            CreateCategoryElement(panels[i], i);
        }
    }

    //왼쪽 버튼 생성
    public void CreateCategoryElement(Panel panel, int index)
    {
        //CategoryPanel이 아니면 선택 표시를 켜고 끌 대상이 없다.
        //캐스팅 결과를 그냥 쓰면 여기서 NullReference로 죽어서 나머지 버튼도 안 생긴다.
        CategoryPanel categoryPanel = panel as CategoryPanel;
        if (categoryPanel == null)
        {
            Debug.LogError(
                $"[ShopPanel] '{name}' : panels[{index}] '{panel.name}'이 CategoryPanel이 아닙니다.",
                panel);
            return;
        }

        GameObject obj = Instantiate(_categoryElementPrefab, parent);
        CategoryElement tmp = obj.GetComponent<CategoryElement>();

        if (tmp == null)
        {
            Debug.LogError(
                $"[ShopPanel] '{name}' : _categoryElementPrefab에 CategoryElement가 없습니다.",
                _categoryElementPrefab);
            Destroy(obj);
            return;
        }

        categoryPanel.SetCategoryElement(tmp);
        tmp.Init(panel.GetSprite(), index, SwitchingCategory);

        _categoryElements.Add(tmp);
    }

    //왼쪽 버튼을 누르면 그 카테고리 패널로 바꾼다.
    private void SwitchingCategory(int index)
    {
        //SwitchingPanel(index)를 직접 부르면 화면만 바뀌고 PanelManager의 indexList는
        //그대로라, 뒤로가기나 PushIndexList가 실제 화면과 다른 경로를 걷게 된다.
        //씬의 다른 탭 버튼들도 전부 SwitchingIndexList를 쓴다.
        PanelManager.instance.SwitchingIndexList(index);
    }
}
