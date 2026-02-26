// Assets/Scripts/EventSystemPersist.cs
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Garde l'EventSystem actif entre les changements de scène.
/// CRUCIAL pour que les boutons UI restent cliquables dans tous les niveaux.
/// À attacher sur le GameObject EventSystem dans MainMenu.
/// </summary>
public class EventSystemPersist : MonoBehaviour
{
    private static EventSystemPersist instance;

    void Awake()
    {
        // Singleton : une seule instance persiste
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✓ EventSystem préservé entre les scènes");
        }
        else
        {
            // Si une autre instance existe déjà, détruire celle-ci
            // Ça arrive quand on retourne au MainMenu ou qu'un niveau
            // a son propre EventSystem par erreur
            Destroy(gameObject);
            Debug.Log("⚠ EventSystem doublon détruit (normal si tu retournes au MainMenu)");
        }
    }

    void Update()
    {
        // Vérifie que cet EventSystem est bien le current
        // Si un autre EventSystem prend le dessus, on le désactive
        if (EventSystem.current != null && EventSystem.current.gameObject != gameObject)
        {
            Debug.LogWarning("⚠ Un autre EventSystem est actif ! Désactivation...");
            EventSystem.current.gameObject.SetActive(false);
        }
    }
}
