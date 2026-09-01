using UnityEngine;

public class CategoryPanel : Panel
{
    private CategoryElement categoryElement; //왼쪽 toggle 버튼 => 
    public override void OnPanel()
    {
        base.OnPanel();
        categoryElement.IsSelected = true;
        //categoryElement IsSelected = true
    }
    public override void OffPanel()
    {
        base.OffPanel();
        categoryElement.IsSelected = false;
    }

    public void SetCategoryElement(CategoryElement categoryElement)
    {
        this.categoryElement = categoryElement;
    }
}
