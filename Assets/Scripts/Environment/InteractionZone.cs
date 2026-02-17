using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    public string promptText = "Press E to interact";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && UIManager.Instance != null)
            UIManager.Instance.ShowInteractionPrompt(promptText);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && UIManager.Instance != null)
            UIManager.Instance.HideInteractionPrompt();
    }
}
