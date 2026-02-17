// Assets/Scripts/Environment/ShadowZone.cs
using UnityEngine;

public class ShadowZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null) p.EnterShadow();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null) p.ExitShadow();
    }
}
