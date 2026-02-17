// Assets/Scripts/LevelData.cs
using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Header("Informations du niveau")]
    public string levelName = "Level 1";
    public string levelDescription = "Tutorial";
    public string objective = "Reach the exit";
    public int levelIndex = 1;       // 1 à 5

    [Header("Conditions de score (étoiles)")]
    public int scoreFor3Stars = 900;
    public int scoreFor2Stars = 600;
    public int scoreFor1Star = 300;

    [Header("Paramètres du niveau")]
    public float timeLimit = 0f;   // 0 = pas de limite
    public int maxDetections = 3;   // détections max avant défaite
}