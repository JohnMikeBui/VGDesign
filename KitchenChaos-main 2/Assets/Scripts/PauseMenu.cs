using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;
    public GameObject howToPlayPanel;
    public TextMeshProUGUI pauseButtonLabel;

    [Header("Effects")]
    public Volume blurVolume;

    bool paused = false;
    bool showingHowToPlay = false;
    void Start()
    {
        ApplyState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (showingHowToPlay)
            {
                CloseHowToPlay();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        paused = !paused;
        ApplyState();
    }

    void ApplyState()
    {
        if (menuPanel) menuPanel.SetActive(paused && !showingHowToPlay);
        if (blurVolume) blurVolume.enabled = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        if (pauseButtonLabel)
            pauseButtonLabel.text = paused ? "Resume" : "Pause";
    }

    public void OnResumeButton()
    {
        paused = false;
        ApplyState();
    }

    public void OnRestart()
    {
        paused = false;
        showingHowToPlay = false;
        ApplyState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenu()
    {
        paused = false;
        showingHowToPlay = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenu");
    }

    public void OnHowToPlayButton()
    {
        showingHowToPlay = true;
        if (menuPanel) menuPanel.SetActive(false);
        if (howToPlayPanel) howToPlayPanel.SetActive(true);
    }
    public void OnBackFromHowToPlay()
    {
        CloseHowToPlay();
    }
    void CloseHowToPlay()
    {
        showingHowToPlay = false;
        if (howToPlayPanel) howToPlayPanel.SetActive(false);
        if (menuPanel) menuPanel.SetActive(paused);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    

    void OnDisable()
    {
        paused = false;
        showingHowToPlay = false;
        ApplyState();
    }
}
