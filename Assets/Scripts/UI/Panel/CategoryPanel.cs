using UnityEngine;

public class CategoryPanel : Panel
{
    public CategoryElement categoryElement;
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
}
