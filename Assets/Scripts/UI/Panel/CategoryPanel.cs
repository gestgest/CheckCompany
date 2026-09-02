using UnityEngine;

public class CategoryPanel : Panel
{
    private CategoryElement categoryElement; //왼쪽 toggle 버튼 => 

    public override void OnPanel()
    {
        base.OnPanel();
        SetSelected(true);
    }

    public override void OffPanel()
    {
        base.OffPanel();
        SetSelected(false);
    }

    public void SetCategoryElement(CategoryElement categoryElement)
    {
        this.categoryElement = categoryElement;
    }

    //ShopPanel.Awake보다 먼저 On/Off가 불리면 아직 버튼이 없다.
    //선택 표시가 없다고 패널 전환까지 막을 이유는 없으므로 경고만 남긴다.
    private void SetSelected(bool isSelected)
    {
        if (categoryElement == null)
        {
            Debug.LogWarning(
                $"[CategoryPanel] '{name}' : CategoryElement가 아직 없습니다. " +
                "이 패널이 ShopPanel의 panels에 들어있는지 확인할 것.", this);
            return;
        }

        categoryElement.IsSelected = isSelected;
    }
}
