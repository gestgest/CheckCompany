using UnityEngine;

public class SubPanel : Panel
{
    [SerializeField] private MiniPanel [] miniPanels;
    [SerializeField] private GameObject background;
    
    public override void SwitchingPanel(int index)
    {
        miniPanels[index].gameObject.SetActive(true);
        background.SetActive(true);
    }
}
