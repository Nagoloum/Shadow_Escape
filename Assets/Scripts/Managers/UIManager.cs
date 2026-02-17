// Assets/Scripts/Managers/UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //  SINGLETON
    // ══════════════════════════════════════════════
    public static UIManager Instance { get; private set; }

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
    //  RÉFÉRENCES — ÉCRANS
    // ══════════════════════════════════════════════
    [Header("=== SCREENS ===")]
    public GameObject mainMenu;
    public GameObject gameHUD;
    public GameObject pauseMenu;
    public GameObject victoryScreen;
    public GameObject defeatScreen;
    public GameObject levelSelectScreen;
    public GameObject optionsScreen;

    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — MAIN MENU
    // ══════════════════════════════════════════════
    [Header("=== MAIN MENU BUTTONS ===")]
    public GameObject newGameButton;      // Toujours visible
    public GameObject continueButton;     // Visible seulement si niveau 2+ débloqué

    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — HUD
    // ══════════════════════════════════════════════
    [Header("=== HUD : TOP BAR ===")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI timerText;

    [Header("=== HUD : PLAYER STATUS ===")]
    public Image visibilityIndicator;
    public TextMeshProUGUI statusText;
    public Color hiddenColor = new Color(0f, 0.4f, 1f, 1f);
    public Color exposedColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("=== HUD : KEYS ===")]
    public TextMeshProUGUI redKeyCount;
    public TextMeshProUGUI blueKeyCount;
    public TextMeshProUGUI greenKeyCount;
    public TextMeshProUGUI yellowKeyCount;

    [Header("=== HUD : BOTTOM BAR ===")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI interactionPrompt;

    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — VICTORY SCREEN
    // ══════════════════════════════════════════════
    [Header("=== VICTORY SCREEN ===")]
    public TextMeshProUGUI victoryTimeText;
    public TextMeshProUGUI victoryDetectionsText;
    public TextMeshProUGUI victoryScoreText;
    public Image[] victoryStars;   // 3 images étoiles
    public GameObject nextLevelButton; // Caché si niveau 5 terminé

    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — DEFEAT SCREEN
    // ══════════════════════════════════════════════
    [Header("=== DEFEAT SCREEN ===")]
    public TextMeshProUGUI defeatReasonText;

    // ══════════════════════════════════════════════
    //  VARIABLES INTERNES
    // ══════════════════════════════════════════════
    private float gameTime = 0f;
    private bool isTimerRunning = false;

    // ══════════════════════════════════════════════
    //  DÉMARRAGE
    // ══════════════════════════════════════════════
    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
        // Timer HUD (le vrai timer est dans LevelManager, celui-ci est juste l'affichage)
        if (isTimerRunning && LevelManager.Instance != null)
        {
            SafeSetText(timerText, FormatTime(LevelManager.Instance.GetCurrentTime()));
        }

        // Escape → Pause / Dépause
        if (gameHUD != null && gameHUD.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ShowPauseMenu();
        }
        else if (pauseMenu != null && pauseMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                HidePauseMenu();
        }
    }

    // ══════════════════════════════════════════════
    //  NAVIGATION ENTRE ÉCRANS
    // ══════════════════════════════════════════════

    public void ShowMainMenu()
    {
        HideAllScreens();
        SafeSetActive(mainMenu, true);
        isTimerRunning = false;
        Time.timeScale = 1f;
        RefreshMainMenuButtons();
    }

    public void ShowGameHUD()
    {
        HideAllScreens();
        SafeSetActive(gameHUD, true);
        isTimerRunning = true;
        Time.timeScale = 1f;
    }

    public void ShowPauseMenu()
    {
        SafeSetActive(pauseMenu, true);
        Time.timeScale = 0f;
    }

    public void HidePauseMenu()
    {
        SafeSetActive(pauseMenu, false);
        Time.timeScale = 1f;
    }

    public void ShowOptions()
    {
        SafeSetActive(optionsScreen, true);
    }

    public void HideOptions()
    {
        SafeSetActive(optionsScreen, false);
    }

    public void ShowLevelSelect()
    {
        HideAllScreens();
        SafeSetActive(levelSelectScreen, true);
    }

    public void ShowVictoryScreen(float time, int detections, int score)
    {
        HideAllScreens();
        SafeSetActive(victoryScreen, true);
        isTimerRunning = false;
        Time.timeScale = 1f;

        SafeSetText(victoryTimeText, "Time: " + FormatTime(time));
        SafeSetText(victoryDetectionsText, "Detections: " + detections);
        SafeSetText(victoryScoreText, "Score: " + score);

        // Étoiles
        if (victoryStars != null)
        {
            int stars = CalculateStars(score);
            for (int i = 0; i < victoryStars.Length; i++)
                if (victoryStars[i] != null)
                    victoryStars[i].color = (i < stars) ? Color.yellow : Color.gray;
        }

        // Cache le bouton "Next Level" si c'était le niveau 5
        if (nextLevelButton != null && LevelManager.Instance != null)
            nextLevelButton.SetActive(LevelManager.Instance.GetCurrentLevelIndex() < 5);
    }

    public void ShowDefeatScreen(string reason)
    {
        HideAllScreens();
        SafeSetActive(defeatScreen, true);
        SafeSetText(defeatReasonText, reason);
        isTimerRunning = false;
        Time.timeScale = 1f;
    }

    void HideAllScreens()
    {
        SafeSetActive(mainMenu, false);
        SafeSetActive(gameHUD, false);
        SafeSetActive(pauseMenu, false);
        SafeSetActive(victoryScreen, false);
        SafeSetActive(defeatScreen, false);
        SafeSetActive(levelSelectScreen, false);
        SafeSetActive(optionsScreen, false);
    }

    // ══════════════════════════════════════════════
    //  LOGIQUE NEW GAME / CONTINUE
    // ══════════════════════════════════════════════

    /// <summary>
    /// Affiche "Continue" seulement si au moins le niveau 2 est débloqué
    /// </summary>
    void RefreshMainMenuButtons()
    {
        if (LevelManager.Instance == null) return;

        bool hasProgress = LevelManager.Instance.IsLevelUnlocked(2);

        if (continueButton != null)
            continueButton.SetActive(hasProgress);

        // newGameButton est toujours visible
        if (newGameButton != null)
            newGameButton.SetActive(true);
    }

    // ══════════════════════════════════════════════
    //  MISES À JOUR HUD
    // ══════════════════════════════════════════════

    public void SetLevelName(string levelName)
    {
        SafeSetText(levelNameText, levelName);
    }

    public void SetObjective(string objective)
    {
        SafeSetText(objectiveText, "Objective: " + objective);
    }

    public void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt == null) return;
        interactionPrompt.text = text;
        interactionPrompt.gameObject.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }

    public void UpdatePlayerVisibility(bool isHidden)
    {
        Color c = isHidden ? hiddenColor : exposedColor;
        string label = isHidden ? "HIDDEN" : "EXPOSED";
        if (visibilityIndicator != null) visibilityIndicator.color = c;
        if (statusText != null) { statusText.text = label; statusText.color = c; }
    }

    public void UpdateKeyDisplay(KeyColor color, int count)
    {
        string s = count.ToString();
        switch (color)
        {
            case KeyColor.Red: SafeSetText(redKeyCount, s); break;
            case KeyColor.Blue: SafeSetText(blueKeyCount, s); break;
            case KeyColor.Green: SafeSetText(greenKeyCount, s); break;
            case KeyColor.Yellow: SafeSetText(yellowKeyCount, s); break;
        }
    }

    // ══════════════════════════════════════════════
    //  BOUTONS (liés dans l'Inspector)
    // ══════════════════════════════════════════════

    // --- Main Menu ---
    public void OnNewGameButton()
    {
        // Efface toute progression et démarre niveau 1
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetAllProgress();
            LevelManager.Instance.LoadLevel(1);
        }
    }

    public void OnContinueButton()
    {
        // Reprend au dernier niveau débloqué
        if (LevelManager.Instance != null)
        {
            int lastUnlocked = PlayerPrefs.GetInt("UnlockedLevels", 1);
            LevelManager.Instance.LoadLevel(lastUnlocked);
        }
    }

    public void OnLevelsButton() => ShowLevelSelect();
    public void OnOptionsButton() => ShowOptions();

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Menu Pause ---
    public void OnResumeButton() => HidePauseMenu();

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        ShowMainMenu();
    }

    // --- Victory Screen ---
    public void OnNextLevelButton()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadNextLevel();
    }

    // --- Options ---
    public void OnBackFromOptions() => HideOptions();

    // ══════════════════════════════════════════════
    //  UTILITAIRES
    // ══════════════════════════════════════════════

    void SafeSetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    void SafeSetText(TextMeshProUGUI tmp, string text)
    {
        if (tmp != null) tmp.text = text;
    }

    string FormatTime(float time)
    {
        int m = Mathf.FloorToInt(time / 60f);
        int s = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", m, s);
    }

    int CalculateStars(int score)
    {
        if (score >= 900) return 3;
        if (score >= 600) return 2;
        if (score >= 300) return 1;
        return 0;
    }
}
