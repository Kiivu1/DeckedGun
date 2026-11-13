using UnityEngine;

// Golpea al jugador cuando está a menos de attackRange, como mucho una vez cada attackCooldown.
// Se usa la distancia y no colisiones: el collider del enemigo es trigger y el jugador usa CharacterController.
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 2.5f;      // Debe ser mayor que el Stopping Distance del NavMeshAgent (2)
    [SerializeField] private float attackCooldown = 1.2f;   // Segundos entre golpes de ESTE enemigo
    [SerializeField] private float maxFirstAttackDelay = 0.5f; // Retraso aleatorio al entrar en rango, para que no golpeen todos a la vez

    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttackTime;
    private bool wasInRange;

    void Awake()
    {
        // Busca al jugador por su tag (el Player.prefab ya tiene el tag "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        bool inRange = FlatDistanceToPlayer() <= attackRange;

        // Al entrar en rango, espera un poco antes del primer golpe
        if (inRange && !wasInRange)
        {
            nextAttackTime = Mathf.Max(nextAttackTime, Time.time + Random.Range(0f, maxFirstAttackDelay));
        }
        wasInRange = inRange;

        if (inRange && Time.time >= nextAttackTime)
        {
            playerHealth.TakeDamage(damage);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // Distancia en el plano XZ, para que la altura del pivote de cada uno no influya
    float FlatDistanceToPlayer()
    {
        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }

    // Dibuja el rango de ataque en la vista Scene al seleccionar el enemigo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
