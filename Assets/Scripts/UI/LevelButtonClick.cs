using UnityEngine;

public class LevelButtonClick : MonoBehaviour
{
    public int levelNumber; // 1 à 5

    public void OnClick()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(levelNumber);
    }
}