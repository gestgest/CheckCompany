using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : Panel
{
    [SerializeField] private SubPanel [] subPanels;
    //[SerializeField] private Button[] subButtons;

    int set_index = 0;

    protected override void OnEnable()
    {
        for (int i = 0; i < subPanels.Length; i++) 
        {
            subPanels[i].gameObject.SetActive(false);
        }

        if (subPanels.Length == 0)
            return;
        subPanels[set_index].gameObject.SetActive(true);
    }

    public override void SwitchingPanel(int index)
    {
        subPanels[set_index].gameObject.SetActive(false);
        subPanels[index].gameObject.SetActive(true);
        set_index = index;
    }

}
