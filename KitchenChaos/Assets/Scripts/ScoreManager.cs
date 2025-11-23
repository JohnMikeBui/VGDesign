using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int currentScore = 0;
    public TextMeshProUGUI scoreText;
    public Canvas uiCanvas;
    public GameObject pointPopupPrefab;

    public AudioClip pointSound;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        PlayPointSound();
    }

    void PlayPointSound()
    {
        if (pointSound != null && audioSource != null)
            audioSource.PlayOneShot(pointSound);
    }

    public void ShowPointPopup(Vector3 worldPosition, int points)
    {
        if (!pointPopupPrefab || !uiCanvas)
        {
            if (!pointPopupPrefab) Debug.LogWarning("PointPopup prefab not set.");
            if (!uiCanvas) Debug.LogWarning("UI Canvas not assigned.");
            return;
        }
        GameObject popup = Instantiate(pointPopupPrefab, uiCanvas.transform);
        PointPopup script = popup.GetComponent<PointPopup>();
        if (script != null)
            script.Initialize(points, worldPosition, uiCanvas);
    }

    
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
}
