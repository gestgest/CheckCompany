using NUnit.Framework;
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
            CreateCategoryElement(panels[i]);
        }
    }

    public void CreateCategoryElement(Panel panel)
    {
        GameObject obj = Instantiate(_categoryElementPrefab, parent);
        CategoryElement tmp = obj.GetComponent<CategoryElement>();

       CategoryPanel categoryPanel = panel as CategoryPanel;
       categoryPanel.categoryElement = tmp;
        tmp.Init(panel.GetSprite());

        _categoryElements.Add(tmp);
    }

}
