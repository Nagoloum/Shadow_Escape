// Assets/Scripts/Environment/ButtonSwitch.cs
using UnityEngine;

public class ButtonSwitch : MonoBehaviour
{
    public Door[] doorsToOpen;
    public bool staysPressed = true;

    private bool isPressed = false;

    public void Press()
    {
        if (isPressed && staysPressed) return;
        isPressed = true;

        foreach (Door d in doorsToOpen)
            if (d != null) d.Open();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.green;

        if (UIManager.Instance != null)
            UIManager.Instance.HideInteractionPrompt();
    }
}
