// Assets/Scripts/Player/PlayerController.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //  PARAMÈTRES DE MOUVEMENT
    // ══════════════════════════════════════════════
    [Header("=== MOVEMENT ===")]
    public float normalSpeed = 5f;
    public float crouchSpeed = 2.5f;

    // ══════════════════════════════════════════════
    //  ÉTAT DU JOUEUR
    // ══════════════════════════════════════════════
    [Header("=== STATE (lecture seule) ===")]
    [SerializeField] private bool isInShadow = false;
    [SerializeField] private bool isCrouching = false;

    public bool IsInShadow => isInShadow;
    public bool IsCrouching => isCrouching;

    // Nombre de zones d'ombre qui couvrent le joueur (peut être dans plusieurs)
    private int shadowCount = 0;

    // ══════════════════════════════════════════════
    //  INVENTAIRE DES CLÉS
    // ══════════════════════════════════════════════
    [Header("=== INVENTORY ===")]
    private Dictionary<KeyColor, int> keys = new Dictionary<KeyColor, int>();

    // ══════════════════════════════════════════════
    //  VISUELS
    // ══════════════════════════════════════════════
    [Header("=== VISUALS ===")]
    public Color colorNormal = new Color(1f, 1f, 1f, 1f);   // blanc
    public Color colorShadow = new Color(0.2f, 0.3f, 0.8f, 0.7f); // bleu semi-transparent
    public Color colorExposed = new Color(1f, 0.2f, 0.2f, 1f);   // rouge

    // ══════════════════════════════════════════════
    //  COMPOSANTS
    // ══════════════════════════════════════════════
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private float currentSpeed;

    // ══════════════════════════════════════════════
    //  INITIALISATION
    // ══════════════════════════════════════════════
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;         // Top-down : pas de gravité
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Init inventaire
        foreach (KeyColor c in System.Enum.GetValues(typeof(KeyColor)))
            keys[c] = 0;

        currentSpeed = normalSpeed;
    }

    // ══════════════════════════════════════════════
    //  UPDATE — INPUTS
    // ══════════════════════════════════════════════
    void Update()
    {
        // Lecture des inputs
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Accroupissement (Shift)
        isCrouching = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentSpeed = isCrouching ? crouchSpeed : normalSpeed;

        // Interaction (E)
        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();

        // Mise à jour visuels
        UpdateVisuals();

        // Mise à jour HUD visibilité
        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerVisibility(isInShadow);
    }

    void FixedUpdate()
    {
        // Déplacement physique
        Vector2 velocity = moveInput.normalized * currentSpeed;
        rb.linearVelocity = velocity;

        // Rotation vers la direction de déplacement
        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }

    // ══════════════════════════════════════════════
    //  OMBRES — le joueur entre/sort d'une zone d'ombre
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

        if (isInShadow)
            sr.color = colorShadow;
        else
            sr.color = colorExposed;
    }

    // ══════════════════════════════════════════════
    //  INTERACTIONS
    // ══════════════════════════════════════════════
    void TryInteract()
    {
        // Cherche tous les objets interactifs à portée
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.2f);

        foreach (Collider2D hit in hits)
        {
            // Porte
            Door door = hit.GetComponent<Door>();
            if (door != null) { door.TryOpen(this); return; }

            // Bouton
            ButtonSwitch btn = hit.GetComponent<ButtonSwitch>();
            if (btn != null) { btn.Press(); return; }
        }
    }

    // ══════════════════════════════════════════════
    //  INVENTAIRE DES CLÉS
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
        if (keys[color] > 0)
        {
            keys[color]--;
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateKeyDisplay(color, keys[color]);
        }
    }

    // ══════════════════════════════════════════════
    //  DÉTECTION — appelé par l'IA ennemie
    // ══════════════════════════════════════════════
    public void GetSpotted()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.AddDetection();
    }

    // ══════════════════════════════════════════════
    //  GIZMOS (debug visuel dans l'éditeur)
    // ══════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.2f); // rayon d'interaction
    }
}
