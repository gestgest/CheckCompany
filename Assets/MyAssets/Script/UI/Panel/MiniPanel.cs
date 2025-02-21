using UnityEngine;

public class MiniPanel : Panel
{
    [SerializeField] private MiniPanel [] miniPanels;

    public override void SwitchingPanel(int index)
    {
        miniPanels[index].gameObject.SetActive(true);
    }
}
