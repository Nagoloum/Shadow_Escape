// Assets/Scripts/Managers/LevelSelectManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("=== BOUTONS DES 5 NIVEAUX ===")]
    public LevelButtonUI[] levelButtons = new LevelButtonUI[5];

    void OnEnable()
    {
        RefreshAllButtons();
    }

    void RefreshAllButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            if (levelButtons[i] == null || LevelManager.Instance == null) continue;

            bool unlocked = LevelManager.Instance.IsLevelUnlocked(levelNumber);
            int stars = LevelManager.Instance.GetStarsForLevel(levelNumber);
            float bestTime = LevelManager.Instance.GetBestTimeForLevel(levelNumber);

            levelButtons[i].Setup(levelNumber, unlocked, stars, bestTime);
        }
    }
}

[System.Serializable]
public class LevelButtonUI
{
    public Button button;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI bestTimeText;
    public GameObject lockIcon;
    public Image[] stars;   // 3 images

    public void Setup(int number, bool unlocked, int starsEarned, float bestTime)
    {
        if (button != null) button.interactable = unlocked;

        if (levelNumberText != null)
            levelNumberText.text = unlocked ? "Level " + number : "";

        if (lockIcon != null) lockIcon.SetActive(!unlocked);

        if (stars != null)
            for (int i = 0; i < stars.Length; i++)
                if (stars[i] != null)
                    stars[i].color = (i < starsEarned) ? Color.yellow : new Color(0.3f, 0.3f, 0.3f, 0.4f);

        if (bestTimeText != null)
        {
            if (bestTime < float.MaxValue)
            {
                int m = Mathf.FloorToInt(bestTime / 60f);
                int s = Mathf.FloorToInt(bestTime % 60f);
                bestTimeText.text = string.Format("Best: {0:00}:{1:00}", m, s);
            }
            else
                bestTimeText.text = unlocked ? "Best: --:--" : "";
        }
    }
}
