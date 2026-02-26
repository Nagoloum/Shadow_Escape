// Assets/Scripts/Environment/ExitZone.cs
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("✓ Joueur atteint la sortie !");

            if (LevelManager.Instance == null)
            {
                Debug.LogError("✗ ERREUR : LevelManager n'existe pas ! Ajoute UIManager et LevelManager dans cette scène, ou lance depuis MainMenu.");
                return;
            }

            Debug.Log("✓ LevelManager trouvé, appel CompleteLevel()");
            LevelManager.Instance.CompleteLevel();
        }
    }
}
