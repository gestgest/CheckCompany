using UnityEngine;

public class SubPanel : Panel
{
    [SerializeField] private MiniPanel [] miniPanels;
    [SerializeField] private GameObject background;

    private int set_index = -1;
    public override void SwitchingPanel(int index)
    {
        miniPanels[index].gameObject.SetActive(true);
        set_index = index;
        //값 전달
        
        background.SetActive(true);
    }
    public void SwitchingOffMiniPanel()
    {
        miniPanels[set_index].gameObject.SetActive(false);
        set_index = -1;
        background.SetActive(false);
    }

    public Panel GetPanel(int index)
    {
        return miniPanels[index];
    }
}
