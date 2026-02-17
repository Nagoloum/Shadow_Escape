// Assets/Scripts/Environment/Door.cs
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType { Normal, Locked, Button }

    public DoorType doorType = DoorType.Normal;
    public KeyColor requiredColor = KeyColor.Red;
    public bool isOpen = false;

    public void TryOpen(PlayerController player)
    {
        if (isOpen) return;

        if (doorType == DoorType.Normal)
        {
            Open();
        }
        else if (doorType == DoorType.Locked)
        {
            if (player.HasKey(requiredColor))
            {
                player.UseKey(requiredColor);
                Open();
            }
            else
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowInteractionPrompt("Need " + requiredColor + " key!");
            }
        }
    }

    public void Open()
    {
        isOpen = true;
        GetComponent<Collider2D>().enabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        if (UIManager.Instance != null)
            UIManager.Instance.HideInteractionPrompt();
    }
}
