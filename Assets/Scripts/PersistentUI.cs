// Assets/Scripts/PersistentUI.cs
using UnityEngine;

/// <summary>
/// Garde le Canvas UI et tous ses enfants actifs entre les changements de scène.
/// À attacher sur un GameObject parent qui contient Canvas + UIManager.
/// </summary>
public class PersistentUI : MonoBehaviour
{
    private static PersistentUI instance;

    void Awake()
    {
        // Singleton : une seule instance persiste
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✓ PersistentUI : Canvas UI préservé entre les scènes");
        }
        else
        {
            // Si une autre instance existe déjà, détruire celle-ci
            Destroy(gameObject);
            Debug.Log("⚠ PersistentUI : Doublon détruit (normal si tu retournes au MainMenu)");
        }
    }
}
