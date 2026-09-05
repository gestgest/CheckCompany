using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
        Application.Quit();
    }
}
