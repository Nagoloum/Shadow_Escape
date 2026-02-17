// Assets/Scripts/Managers/LevelManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //  SINGLETON
    // ══════════════════════════════════════════════
    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ══════════════════════════════════════════════
    //  DONNÉES DES 5 NIVEAUX (Inspector)
    // ══════════════════════════════════════════════
    [Header("=== DONNÉES DES NIVEAUX ===")]
    public LevelData[] levels = new LevelData[5];

    // ══════════════════════════════════════════════
    //  ÉTAT DE LA PARTIE
    // ══════════════════════════════════════════════
    [Header("=== ÉTAT EN COURS (lecture seule) ===")]
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private float currentTime = 0f;
    [SerializeField] private int detectionCount = 0;
    [SerializeField] private bool isLevelRunning = false;

    // ══════════════════════════════════════════════
    //  UPDATE — TIMER
    // ══════════════════════════════════════════════
    void Update()
    {
        if (!isLevelRunning) return;

        currentTime += Time.deltaTime;

        // Limite de temps
        LevelData data = GetCurrentLevelData();
        if (data != null && data.timeLimit > 0f && currentTime >= data.timeLimit)
            FailLevel("Time's up!");
    }

    // ══════════════════════════════════════════════
    //  CHARGER UN NIVEAU
    // ══════════════════════════════════════════════

    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > 5)
        {
            Debug.LogError("LevelManager: numéro invalide → " + levelNumber);
            return;
        }
        if (!IsLevelUnlocked(levelNumber))
        {
            Debug.LogWarning("LevelManager: niveau " + levelNumber + " verrouillé !");
            return;
        }

        currentLevelIndex = levelNumber;
        currentTime = 0f;
        detectionCount = 0;
        isLevelRunning = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Level" + levelNumber);
    }

    public void RestartLevel()
    {
        if (currentLevelIndex < 1 || currentLevelIndex > 5) return;
        LoadLevel(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        int next = currentLevelIndex + 1;
        if (next > 5)
        {
            LoadMainMenu();
        }
        else
        {
            UnlockLevel(next);
            LoadLevel(next);
        }
    }

    public void LoadMainMenu()
    {
        currentLevelIndex = 0;
        isLevelRunning = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ══════════════════════════════════════════════
    //  CALLBACK SCÈNE CHARGÉE
    // ══════════════════════════════════════════════
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        isLevelRunning = true;
        currentTime = 0f;

        LevelData data = GetCurrentLevelData();
        if (data != null && UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameHUD();
            UIManager.Instance.SetLevelName(data.levelName);
            UIManager.Instance.SetObjective(data.objective);
        }
    }

    // ══════════════════════════════════════════════
    //  VICTOIRE ET DÉFAITE
    // ══════════════════════════════════════════════

    public void CompleteLevel()
    {
        if (!isLevelRunning) return;
        isLevelRunning = false;

        int score = CalculateScore();
        int stars = CalculateStars(score);

        SaveLevelProgress(currentLevelIndex, stars, currentTime);

        if (currentLevelIndex < 5)
            UnlockLevel(currentLevelIndex + 1);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowVictoryScreen(currentTime, detectionCount, score);
    }

    public void FailLevel(string reason = "You were spotted!")
    {
        if (!isLevelRunning) return;
        isLevelRunning = false;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowDefeatScreen(reason);
    }

    // ══════════════════════════════════════════════
    //  DÉTECTIONS
    // ══════════════════════════════════════════════

    public void AddDetection()
    {
        detectionCount++;

        LevelData data = GetCurrentLevelData();
        if (data != null && detectionCount >= data.maxDetections)
            FailLevel("Too many detections!");
    }

    // ══════════════════════════════════════════════
    //  CALCUL DU SCORE
    // ══════════════════════════════════════════════

    int CalculateScore()
    {
        int score = 1000;
        score -= Mathf.FloorToInt(currentTime);   // -1 par seconde
        score -= detectionCount * 100;             // -100 par détection
        if (detectionCount == 0) score += 200;     // bonus discrétion
        return Mathf.Max(0, score);
    }

    int CalculateStars(int score)
    {
        LevelData data = GetCurrentLevelData();
        if (data == null) return 0;
        if (score >= data.scoreFor3Stars) return 3;
        if (score >= data.scoreFor2Stars) return 2;
        if (score >= data.scoreFor1Star) return 1;
        return 0;
    }

    // ══════════════════════════════════════════════
    //  SAUVEGARDE / CHARGEMENT
    // ══════════════════════════════════════════════

    void SaveLevelProgress(int levelNumber, int stars, float time)
    {
        string starsKey = "Stars_Level_" + levelNumber;
        string bestTimeKey = "BestTime_Level_" + levelNumber;

        int savedStars = PlayerPrefs.GetInt(starsKey, 0);
        float savedTime = PlayerPrefs.GetFloat(bestTimeKey, float.MaxValue);

        if (stars > savedStars) PlayerPrefs.SetInt(starsKey, stars);
        if (time < savedTime) PlayerPrefs.SetFloat(bestTimeKey, time);

        PlayerPrefs.Save();
    }

    public int GetStarsForLevel(int n) => PlayerPrefs.GetInt("Stars_Level_" + n, 0);
    public float GetBestTimeForLevel(int n) => PlayerPrefs.GetFloat("BestTime_Level_" + n, float.MaxValue);

    // ══════════════════════════════════════════════
    //  DÉVERROUILLAGE
    // ══════════════════════════════════════════════

    public void UnlockLevel(int levelNumber)
    {
        int current = PlayerPrefs.GetInt("UnlockedLevels", 1);
        if (levelNumber > current)
        {
            PlayerPrefs.SetInt("UnlockedLevels", levelNumber);
            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber == 1) return true;
        return levelNumber <= PlayerPrefs.GetInt("UnlockedLevels", 1);
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey("UnlockedLevels");
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey("Stars_Level_" + i);
            PlayerPrefs.DeleteKey("BestTime_Level_" + i);
        }
        PlayerPrefs.Save();
    }

    // ══════════════════════════════════════════════
    //  GETTERS
    // ══════════════════════════════════════════════
    public int GetCurrentLevelIndex() => currentLevelIndex;
    public float GetCurrentTime() => currentTime;
    public bool IsRunning() => isLevelRunning;

    public LevelData GetCurrentLevelData()
    {
        if (currentLevelIndex < 1 || currentLevelIndex > 5) return null;
        if (levels == null || levels.Length < currentLevelIndex) return null;
        return levels[currentLevelIndex - 1];
    }
}
