using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public KeyColor keyColor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null)
        {
            p.AddKey(keyColor);
            Destroy(gameObject);
        }
    }
}
