// Assets/Scripts/Enemy/EnemyAI.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //  ÉTATS DE L'ENNEMI
    // ══════════════════════════════════════════════
    public enum EnemyState
    {
        Patrol,      // Patrouille normale
        Suspicious,  // A entendu/vu quelque chose
        Alert,       // A repéré le joueur
        Search,      // Cherche le joueur perdu
        ReturnPatrol // Retourne à sa patrouille
    }

    // ══════════════════════════════════════════════
    //  PATROUILLE
    // ══════════════════════════════════════════════
    [Header("=== PATROUILLE ===")]
    public Transform[] waypoints;        // Points de passage
    public float patrolSpeed = 2f;
    public float waitAtWaypoint = 1.5f; // Temps d'attente à chaque point

    // ══════════════════════════════════════════════
    //  VISION
    // ══════════════════════════════════════════════
    [Header("=== VISION ===")]
    public float viewRadius = 6f;             // Portée de vue
    [Range(10f, 360f)]
    public float viewAngle = 90f;            // Angle du cône de vision
    public float suspicionRadius = 3f;        // Détection proche sans cône
    public LayerMask playerLayer;             // Layer du joueur
    public LayerMask obstacleLayer;           // Layer des murs/obstacles

    // ══════════════════════════════════════════════
    //  ALERTE
    // ══════════════════════════════════════════════
    [Header("=== ALERTE ===")]
    public float alertSpeed = 4f;          // Vitesse en alerte
    public float searchDuration = 5f;          // Durée de recherche après perte
    public float suspicionTime = 1.5f;        // Durée avant alerte confirmée

    // ══════════════════════════════════════════════
    //  VISUELS
    // ══════════════════════════════════════════════
    [Header("=== VISUELS ===")]
    public SpriteRenderer bodyRenderer;        // Sprite du garde
    public Color colorPatrol = Color.white;
    public Color colorSuspicious = Color.yellow;
    public Color colorAlert = Color.red;
    public GameObject exclamationMark;         // ! au-dessus de la tête
    public GameObject questionMark;            // ? au-dessus de la tête

    // ══════════════════════════════════════════════
    //  ÉTAT INTERNE
    // ══════════════════════════════════════════════
    [Header("=== ÉTAT (lecture seule) ===")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private PlayerController playerController;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float suspicionTimer = 0f;
    private float searchTimer = 0f;
    private Vector2 lastKnownPosition = Vector2.zero;

    // ══════════════════════════════════════════════
    //  INITIALISATION
    // ══════════════════════════════════════════════
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Cherche le joueur par tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }

        // Cache les icônes par défaut
        if (exclamationMark != null) exclamationMark.SetActive(false);
        if (questionMark != null) questionMark.SetActive(false);

        SetState(EnemyState.Patrol);
    }

    // ══════════════════════════════════════════════
    //  UPDATE PRINCIPAL
    // ══════════════════════════════════════════════
    void Update()
    {
        // Ne fait rien si le niveau n'est pas en cours
        if (LevelManager.Instance != null && !LevelManager.Instance.IsRunning()) return;

        // Cherche le joueur en permanence
        bool playerVisible = CanSeePlayer();

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                if (playerVisible)
                {
                    suspicionTimer = 0f;
                    SetState(EnemyState.Suspicious);
                }
                break;

            case EnemyState.Suspicious:
                UpdateSuspicious(playerVisible);
                break;

            case EnemyState.Alert:
                UpdateAlert(playerVisible);
                break;

            case EnemyState.Search:
                UpdateSearch();
                break;

            case EnemyState.ReturnPatrol:
                UpdateReturnPatrol();
                break;
        }
    }

    // ══════════════════════════════════════════════
    //  MACHINE À ÉTATS
    // ══════════════════════════════════════════════

    void SetState(EnemyState newState)
    {
        currentState = newState;

        // Icônes (! et ?)
        if (exclamationMark != null) exclamationMark.SetActive(newState == EnemyState.Alert);
        if (questionMark != null) questionMark.SetActive(newState == EnemyState.Suspicious || newState == EnemyState.Search);

        // Couleur du corps (TOUJOURS visible, même si icônes buguent)
        if (bodyRenderer != null)
        {
            switch (newState)
            {
                case EnemyState.Patrol:
                case EnemyState.ReturnPatrol:
                    bodyRenderer.color = colorPatrol;     // Blanc
                    break;
                case EnemyState.Suspicious:
                case EnemyState.Search:
                    bodyRenderer.color = colorSuspicious; // Jaune
                    break;
                case EnemyState.Alert:
                    bodyRenderer.color = colorAlert;      // Rouge
                    break;
            }
        }
    }

    // ── Patrouille ─────────────────────────────────
    void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        float dist = Vector2.Distance(transform.position, target.position);

        if (dist < 0.15f)
        {
            // Attend au waypoint
            waitTimer += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            if (waitTimer >= waitAtWaypoint)
            {
                waitTimer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
        else
        {
            MoveTowards(target.position, patrolSpeed);
        }
    }

    // ── Suspicion ──────────────────────────────────
    void UpdateSuspicious(bool playerStillVisible)
    {
        rb.linearVelocity = Vector2.zero; // S'arrête

        if (playerStillVisible)
        {
            suspicionTimer += Time.deltaTime;
            lastKnownPosition = playerTransform.position;

            if (suspicionTimer >= suspicionTime)
            {
                // Alerte confirmée !
                SetState(EnemyState.Alert);
                if (playerController != null)
                    playerController.GetSpotted();
            }
        }
        else
        {
            // A perdu le joueur de vue → retour patrouille
            suspicionTimer = 0f;
            SetState(EnemyState.ReturnPatrol);
        }
    }

    // ── Alerte ─────────────────────────────────────
    void UpdateAlert(bool playerStillVisible)
    {
        if (playerStillVisible)
        {
            lastKnownPosition = playerTransform.position;
            MoveTowards(lastKnownPosition, alertSpeed);
        }
        else
        {
            // A perdu le joueur → cherche
            searchTimer = 0f;
            SetState(EnemyState.Search);
        }
    }

    // ── Recherche ──────────────────────────────────
    void UpdateSearch()
    {
        searchTimer += Time.deltaTime;

        // Se déplace vers la dernière position connue
        float dist = Vector2.Distance(transform.position, lastKnownPosition);
        if (dist > 0.2f)
            MoveTowards(lastKnownPosition, patrolSpeed);
        else
            rb.linearVelocity = Vector2.zero;

        if (searchTimer >= searchDuration)
            SetState(EnemyState.ReturnPatrol);
    }

    // ── Retour patrouille ──────────────────────────
    void UpdateReturnPatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        float dist = Vector2.Distance(transform.position, target.position);

        if (dist < 0.2f)
            SetState(EnemyState.Patrol);
        else
            MoveTowards(target.position, patrolSpeed);
    }

    // ══════════════════════════════════════════════
    //  DÉTECTION DU JOUEUR
    // ══════════════════════════════════════════════

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        if (playerController == null) return false;

        // Si le joueur est dans l'ombre et non accroupi → visible seulement à très courte portée
        // Si dans l'ombre et accroupi → invisible
        if (playerController.IsInShadow && playerController.IsCrouching) return false;

        Vector2 dirToPlayer = (playerTransform.position - transform.position);
        float distance = dirToPlayer.magnitude;

        // Détection de proximité immédiate (même hors cône)
        if (distance <= suspicionRadius)
        {
            if (playerController.IsInShadow) return false; // Dans l'ombre = invisible même de près
            return !Physics2D.Raycast(transform.position, dirToPlayer.normalized, distance, obstacleLayer);
        }

        // Hors portée de vue
        if (distance > viewRadius) return false;

        // Dans le cône de vision ?
        float angleToPlayer = Vector2.Angle(transform.up, dirToPlayer.normalized);
        if (angleToPlayer > viewAngle / 2f) return false;

        // Dans l'ombre : portée réduite de moitié
        if (playerController.IsInShadow && distance > viewRadius * 0.5f) return false;

        // Obstacle entre le garde et le joueur ?
        if (Physics2D.Raycast(transform.position, dirToPlayer.normalized, distance, obstacleLayer))
            return false;

        return true;
    }

    // ══════════════════════════════════════════════
    //  MOUVEMENT
    // ══════════════════════════════════════════════

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;

        // NE CHANGE PAS LA ROTATION - le sprite garde son orientation
        // Si tu veux que le sprite regarde dans la direction, décommente :
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
        
    }

    // ══════════════════════════════════════════════
    //  GIZMOS — Visualisation du cône dans l'éditeur
    // ══════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        // Rayon de vision
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Rayon de suspicion
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suspicionRadius);

        // Cône de vision
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 leftBound = Quaternion.Euler(0, 0, viewAngle / 2f) * transform.up * viewRadius;
        Vector3 rightBound = Quaternion.Euler(0, 0, -viewAngle / 2f) * transform.up * viewRadius;
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
