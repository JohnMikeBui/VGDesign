using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject failPanel;
    public GameObject successPanel;

    [Header("Timer")]
    public float startSeconds = 90f;

    [Header("Win/Lose")]
    public int scoreThreshold = 10;
    public string nextSceneName = "paris"; 

    float t;
    bool running;

    void Start()
    {
        Time.timeScale = 1f;
        t = Mathf.Max(0f, startSeconds);
        running = true;
        if (failPanel) failPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);
        
        // Debug warning if timer text is not assigned
        if (timerText == null)
        {
            Debug.LogError("GameTimer: Timer Text (TextMeshProUGUI) is not assigned! Please assign it in the Inspector.");
        }
        
        UpdateText(t);
    }

    void Update()
    {
        if (!running) return;

        t -= Time.deltaTime;
        if (t <= 0f)
        {
            t = 0f;
            UpdateText(t);
            running = false;

            int score = ScoreManager.Instance ? ScoreManager.Instance.GetScore() : 0;

            if (score < scoreThreshold) ShowFail();
            else ShowSuccess();
            return;
        }

        UpdateText(t);
    }

    void UpdateText(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        int m = s / 60;
        int sec = s % 60;
        if (timerText) timerText.text = $"{m:0}:{sec:00}";
    }

    void ShowFail()
    {
        Time.timeScale = 0f;
        if (failPanel) failPanel.SetActive(true);
    }

    void ShowSuccess()
    {
        Time.timeScale = 0f;
        if (successPanel) successPanel.SetActive(true);
    }

    public void OnStartNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnReplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnPlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenu");
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
