using UnityEngine;

public class QuitGame : MonoBehaviour
{

    
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.X))
            Quit();

    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
