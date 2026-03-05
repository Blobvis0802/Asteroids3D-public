using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PersistentScoreManager : MonoBehaviour
{
    public static PersistentScoreManager Instance;

    [Header("Score Values")]
    [SerializeField] private int bigAsteroidPoints = 10;
    [SerializeField] private int mediumAsteroidPoints = 20;
    [SerializeField] private int smallAsteroidPoints = 40;

    [Header("UI Names")]
    [SerializeField] private string scoreTextName = "ScoreText";
    [SerializeField] private string highScoreTextName = "HighScoreText";

    [Header("Game Over Scene Name")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }

    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        // Proper singleton protection
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHighScore();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameOverSceneName)
        {
            CurrentScore = 0;
        }

        scoreText = GameObject.Find(scoreTextName)?.GetComponent<TextMeshProUGUI>();
        highScoreText = GameObject.Find(highScoreTextName)?.GetComponent<TextMeshProUGUI>();

        UpdateUI();
    }

    public void AddScoreBySize(string sizeClass)
    {
        int pointsToAdd = sizeClass switch
        {
            "Big" => bigAsteroidPoints,
            "Medium" => mediumAsteroidPoints,
            "Small" => smallAsteroidPoints,
            _ => 0
        };

        AddScore(pointsToAdd);
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;

        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        UpdateUI();
    }

    public void ResetHighScore()
    {
        HighScore = 0;

        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        PlayerPrefs.Save();

        UpdateUI();
        Debug.Log("Highscore Reset");
    }

    // SAFE BUTTON CALL
    public static void ResetHighScoreButton()
    {
        if (Instance != null)
        {
            Instance.ResetHighScore();
        }
    }

    private void LoadHighScore()
    {
        HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {CurrentScore}";

        if (highScoreText != null)
            highScoreText.text = $"High Score: {HighScore}";
    }

    private void OnDestroy()
    {
        // Only unsubscribe if THIS is the real instance
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void SetBigAsteroidPoints(int value) => bigAsteroidPoints = value;
    public void SetMediumAsteroidPoints(int value) => mediumAsteroidPoints = value;
    public void SetSmallAsteroidPoints(int value) => smallAsteroidPoints = value;
}