// Assets/Scripts/Managers/UIManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

            // Déplace UIManager à la racine si nécessaire
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
            Debug.Log("✓ UIManager préservé entre les scènes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ══════════════════════════════════════════════
    //  REFERENCES — ECRANS
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
    //  REFERENCES — MAIN MENU
    // ══════════════════════════════════════════════
    [Header("=== MAIN MENU BUTTONS ===")]
    public GameObject newGameButton;
    public GameObject continueButton;

    // ══════════════════════════════════════════════
    //  REFERENCES — HUD
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
    //  REFERENCES — VICTORY SCREEN
    // ══════════════════════════════════════════════
    [Header("=== VICTORY SCREEN ===")]
    public TextMeshProUGUI victoryTimeText;
    public TextMeshProUGUI victoryDetectionsText;
    public TextMeshProUGUI victoryScoreText;
    public Image[] victoryStars;
    public GameObject nextLevelButton;

    // ══════════════════════════════════════════════
    //  REFERENCES — DEFEAT SCREEN
    // ══════════════════════════════════════════════
    [Header("=== DEFEAT SCREEN ===")]
    public TextMeshProUGUI defeatReasonText;

    // ══════════════════════════════════════════════
    //  PILE D'ECRANS (Screen Stack)
    //
    //  Principe : chaque ecran qu'on "ouvre par-dessus"
    //  un autre est empile. Quand on fait "Back", on
    //  depile et on reaffiche l'ecran precedent.
    //
    //  Exemple :
    //    MainMenu → Options       : pile = [MainMenu]
    //    MainMenu → LevelSelect   : pile = [MainMenu]
    //    PauseMenu → Options      : pile = [PauseMenu]
    // ══════════════════════════════════════════════
    private Stack<GameObject> screenStack = new Stack<GameObject>();

    // ══════════════════════════════════════════════
    //  VARIABLES INTERNES
    // ══════════════════════════════════════════════
    private bool isTimerRunning = false;

    // ══════════════════════════════════════════════
    //  DEMARRAGE
    // ══════════════════════════════════════════════
    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
        // Timer HUD
        if (isTimerRunning && LevelManager.Instance != null)
            SafeSetText(timerText, FormatTime(LevelManager.Instance.GetCurrentTime()));

        // Vérifier que le clavier est disponible
        if (Keyboard.current == null)
        {
            Debug.LogWarning("⚠️ Keyboard.current est NULL - Input System pas configuré ?");
            return;
        }

        // ESC ou ESPACE pour pause
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("🎮 Touche pause détectée (ESC ou ESPACE)");

            // Si le jeu tourne → pause
            if (gameHUD != null && gameHUD.activeSelf)
            {
                Debug.Log("✓ GameHUD actif → Ouverture menu pause");
                ShowPauseMenu();
            }
            // Si en pause → reprendre (toggle)
            else if (pauseMenu != null && pauseMenu.activeSelf)
            {
                Debug.Log("✓ Déjà en pause → Fermeture menu pause");
                HidePauseMenu();
            }
            // Sinon → retour (dans les menus)
            else
            {
                Debug.Log("⚠️ Ni GameHUD ni PauseMenu actif");
                if (gameHUD != null)
                    Debug.Log("   GameHUD existe mais est inactif");
                else
                    Debug.Log("   GameHUD est NULL !");

                GoBack();
            }
        }
    }

    // ══════════════════════════════════════════════
    //  NAVIGATION PRINCIPALE
    // ══════════════════════════════════════════════

    /// <summary>
    /// Revient a l'ecran precedent dans la pile.
    /// Si la pile est vide, ne fait rien.
    /// </summary>
    public void GoBack()
    {
        if (screenStack.Count == 0) return;

        // Desactive l'ecran actuel
        GameObject current = GetCurrentActiveScreen();
        if (current != null) SafeSetActive(current, false);

        // Reactiver le precedent
        GameObject previous = screenStack.Pop();
        SafeSetActive(previous, true);

        // Cas special : si on revient sur le PauseMenu, remet le jeu en pause
        if (previous == pauseMenu)
            Time.timeScale = 0f;
    }

    /// <summary>
    /// Ouvre un ecran "par-dessus" l'ecran actuel.
    /// L'ecran actuel est empile (il sera restaure par GoBack).
    /// </summary>
    void PushScreen(GameObject newScreen)
    {
        // Empile l'ecran actuel
        GameObject current = GetCurrentActiveScreen();
        if (current != null)
        {
            screenStack.Push(current);
            SafeSetActive(current, false);
        }

        // Affiche le nouvel ecran
        SafeSetActive(newScreen, true);
    }

    /// <summary>
    /// Affiche un ecran en effacant toute la pile (navigation "racine").
    /// Utilise pour les transitions majeures (ex: aller au menu principal).
    /// </summary>
    void GoToScreen(GameObject newScreen)
    {
        screenStack.Clear();
        HideAllScreens();
        SafeSetActive(newScreen, true);
    }

    // ══════════════════════════════════════════════
    //  ECRANS SPECIFIQUES
    // ══════════════════════════════════════════════

    public void ShowMainMenu()
    {
        GoToScreen(mainMenu);
        isTimerRunning = false;
        Time.timeScale = 1f;
        RefreshMainMenuButtons();
    }

    public void ShowGameHUD()
    {
        Debug.Log("📋 ShowGameHUD() appelé");

        if (gameHUD == null)
        {
            Debug.LogError("❌ GameHUD est NULL ! Assigne-le dans l'Inspector");
            return;
        }

        GoToScreen(gameHUD);
        isTimerRunning = true;
        Time.timeScale = 1f;

        Debug.Log("✓ GameHUD affiché. gameHUD.activeSelf = " + gameHUD.activeSelf);
    }

    // Pause : s'ouvre PAR-DESSUS le HUD (HUD reste en arriere)
    public void ShowPauseMenu()
    {
        Debug.Log("📋 ShowPauseMenu() appelé");

        if (pauseMenu == null)
        {
            Debug.LogError("❌ PauseMenu est NULL ! Assigne-le dans l'Inspector");
            return;
        }

        Debug.Log("   Activation de : " + pauseMenu.name);
        PushScreen(pauseMenu);
        Time.timeScale = 0f;

        Debug.Log("✓ Menu pause affiché. Time.timeScale = " + Time.timeScale);
        Debug.Log("   PauseMenu.activeSelf = " + pauseMenu.activeSelf);
    }

    public void HidePauseMenu()
    {
        Debug.Log("📋 HidePauseMenu() appelé");
        GoBack();
        Time.timeScale = 1f;
        Debug.Log("✓ Menu pause fermé. Time.timeScale = " + Time.timeScale);
    }

    // Options : s'ouvre PAR-DESSUS l'ecran actuel (MainMenu ou PauseMenu)
    public void ShowOptions()
    {
        PushScreen(optionsScreen);
    }

    public void HideOptions()
    {
        GoBack();
    }

    // Level Select : s'ouvre PAR-DESSUS le MainMenu
    public void ShowLevelSelect()
    {
        PushScreen(levelSelectScreen);
    }

    public void ShowVictoryScreen(float time, int detections, int score)
    {
        GoToScreen(victoryScreen);
        isTimerRunning = false;
        Time.timeScale = 1f;

        SafeSetText(victoryTimeText, "Time: " + FormatTime(time));
        SafeSetText(victoryDetectionsText, "Detections: " + detections);
        SafeSetText(victoryScoreText, "Score: " + score);

        if (victoryStars != null)
        {
            int stars = CalculateStars(score);
            for (int i = 0; i < victoryStars.Length; i++)
                if (victoryStars[i] != null)
                    victoryStars[i].color = (i < stars) ? Color.yellow : Color.gray;
        }

        if (nextLevelButton != null && LevelManager.Instance != null)
            nextLevelButton.SetActive(LevelManager.Instance.GetCurrentLevelIndex() < 5);
    }

    public void ShowDefeatScreen(string reason)
    {
        GoToScreen(defeatScreen);
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
    void RefreshMainMenuButtons()
    {
        if (LevelManager.Instance == null) return;

        bool hasProgress = LevelManager.Instance.IsLevelUnlocked(2);
        if (continueButton != null) continueButton.SetActive(hasProgress);
        if (newGameButton != null) newGameButton.SetActive(true);
    }

    // ══════════════════════════════════════════════
    //  MISES A JOUR HUD
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
    //  BOUTONS (lies dans l'Inspector)
    // ══════════════════════════════════════════════

    // --- Main Menu ---
    public void OnNewGameButton()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetAllProgress();
            LevelManager.Instance.LoadLevel(1);
        }
    }

    public void OnContinueButton()
    {
        if (LevelManager.Instance != null)
        {
            int lastUnlocked = PlayerPrefs.GetInt("UnlockedLevels", 1);
            LevelManager.Instance.LoadLevel(lastUnlocked);
        }
    }

    // Options s'ouvre par-dessus l'ecran actuel
    public void OnOptionsButton() => ShowOptions();

    // Level Select s'ouvre par-dessus MainMenu
    public void OnLevelsButton() => ShowLevelSelect();

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Pause ---
    public void OnResumeButton()
    {
        HidePauseMenu();
    }

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

    // --- Victory ---
    public void OnNextLevelButton()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadNextLevel();
    }

    // --- Back universel (bouton "Back" dans Options, LevelSelect, etc.) ---
    public void OnBackButton() => GoBack();

    // ══════════════════════════════════════════════
    //  UTILITAIRE — ecran actif
    // ══════════════════════════════════════════════

    /// <summary>
    /// Retourne l'ecran qui est actuellement actif.
    /// </summary>
    GameObject GetCurrentActiveScreen()
    {
        if (mainMenu != null && mainMenu.activeSelf) return mainMenu;
        if (gameHUD != null && gameHUD.activeSelf) return gameHUD;
        if (pauseMenu != null && pauseMenu.activeSelf) return pauseMenu;
        if (victoryScreen != null && victoryScreen.activeSelf) return victoryScreen;
        if (defeatScreen != null && defeatScreen.activeSelf) return defeatScreen;
        if (levelSelectScreen != null && levelSelectScreen.activeSelf) return levelSelectScreen;
        if (optionsScreen != null && optionsScreen.activeSelf) return optionsScreen;
        return null;
    }

    // ══════════════════════════════════════════════
    //  UTILITAIRES INTERNES
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
