// Assets/Scripts/Player/PlayerController.cs
// Compatible avec le NOUVEAU Input System (Input System Package)
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //  PARAMETRES DE MOUVEMENT
    // ══════════════════════════════════════════════
    [Header("=== MOVEMENT ===")]
    public float normalSpeed = 5f;
    public float crouchSpeed = 2.5f;

    // ══════════════════════════════════════════════
    //  ETAT DU JOUEUR
    // ══════════════════════════════════════════════
    [Header("=== STATE (lecture seule) ===")]
    [SerializeField] private bool isInShadow = false;
    [SerializeField] private bool isCrouching = false;

    public bool IsInShadow => isInShadow;
    public bool IsCrouching => isCrouching;

    private int shadowCount = 0;
    private float currentSpeed = 5f;

    // ══════════════════════════════════════════════
    //  INVENTAIRE
    // ══════════════════════════════════════════════
    private Dictionary<KeyColor, int> keys = new Dictionary<KeyColor, int>();

    // ══════════════════════════════════════════════
    //  VISUELS
    // ══════════════════════════════════════════════
    [Header("=== VISUALS ===")]
    public Color colorShadow = new Color(0.2f, 0.3f, 0.8f, 0.7f);
    public Color colorExposed = new Color(1f, 0.2f, 0.2f, 1f);

    // ══════════════════════════════════════════════
    //  COMPOSANTS
    // ══════════════════════════════════════════════
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput = Vector2.zero;

    // ══════════════════════════════════════════════
    //  INITIALISATION
    // ══════════════════════════════════════════════
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        foreach (KeyColor c in System.Enum.GetValues(typeof(KeyColor)))
            keys[c] = 0;

        currentSpeed = normalSpeed;
    }

    // ══════════════════════════════════════════════
    //  UPDATE — INPUTS (nouveau Input System)
    // ══════════════════════════════════════════════
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Deplacement
        float x = 0f, y = 0f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        moveInput = new Vector2(x, y);

        // Accroupissement
        isCrouching = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        currentSpeed = isCrouching ? crouchSpeed : normalSpeed;

        // Interaction (E)
        if (keyboard.eKey.wasPressedThisFrame)
            TryInteract();

        // Visuels + HUD
        UpdateVisuals();
        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerVisibility(isInShadow);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * currentSpeed;

        // NE CHANGE PAS LA ROTATION - le sprite garde son orientation
        // Si tu veux que le sprite regarde dans la direction du mouvement,
        // décommente les lignes ci-dessous et utilise un sprite qui regarde vers le haut
        /*
        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
        */
    }

    // ══════════════════════════════════════════════
    //  OMBRES
    // ══════════════════════════════════════════════
    public void EnterShadow()
    {
        shadowCount++;
        isInShadow = shadowCount > 0;
    }

    public void ExitShadow()
    {
        shadowCount = Mathf.Max(0, shadowCount - 1);
        isInShadow = shadowCount > 0;
    }

    // ══════════════════════════════════════════════
    //  VISUELS
    // ══════════════════════════════════════════════
    void UpdateVisuals()
    {
        if (sr == null) return;
        sr.color = isInShadow ? colorShadow : colorExposed;
    }

    // ══════════════════════════════════════════════
    //  INTERACTION
    // ══════════════════════════════════════════════
    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (Collider2D hit in hits)
        {
            Door door = hit.GetComponent<Door>();
            if (door != null) { door.TryOpen(this); return; }

            ButtonSwitch btn = hit.GetComponent<ButtonSwitch>();
            if (btn != null) { btn.Press(); return; }
        }
    }

    // ══════════════════════════════════════════════
    //  CLES
    // ══════════════════════════════════════════════
    public void AddKey(KeyColor color)
    {
        keys[color]++;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateKeyDisplay(color, keys[color]);
    }

    public bool HasKey(KeyColor color) => keys[color] > 0;

    public void UseKey(KeyColor color)
    {
        if (keys[color] <= 0) return;
        keys[color]--;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateKeyDisplay(color, keys[color]);
    }

    // ══════════════════════════════════════════════
    //  DETECTION
    // ══════════════════════════════════════════════
    public void GetSpotted()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.AddDetection();
    }

    // ══════════════════════════════════════════════
    //  GIZMOS
    // ══════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}
