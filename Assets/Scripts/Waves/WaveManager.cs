using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Genera oleadas infinitas de enemigos. Cada oleada tiene más enemigos en total que la anterior
// (WaveConfig.GetTotalEnemies), pero nunca hay más de config.maxAlive vivos a la vez:
// cuando muere uno, aparece el siguiente de la cola.
public class WaveManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WaveConfig config;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRadius = 2f;          // Dispersión aleatoria alrededor de cada punto
    [SerializeField] private float minDistanceToPlayer = 8f;  // No aparecer encima del jugador

    private int currentWave;                                  // 0 = aún no ha empezado
    private WaveSpawnQueue queue;
    private readonly List<EnemyHealth> aliveEnemies = new List<EnemyHealth>();
    private Transform player;
    private readonly System.Random rng = new System.Random();

    // Eventos para UI / sonido / recompensas. El parámetro es el número de oleada
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    public int CurrentWave => currentWave;
    public WaveSpawnQueue Queue => queue;                     // Para la UI: Queue.Remaining, Queue.Alive

    void Awake()
    {
        if (config == null || config.enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError($"{name}: falta asignar el WaveConfig, su enemyPrefab o los spawnPoints. Oleadas desactivadas.");
            enabled = false;
            return;
        }

        // Sin EnemyHealth no sabríamos cuándo muere un enemigo y la oleada nunca acabaría
        if (config.enemyPrefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError($"{name}: el prefab {config.enemyPrefab.name} no tiene EnemyHealth. Oleadas desactivadas.");
            enabled = false;
            return;
        }

        // Busca al jugador por su tag, igual que EnemyChase
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Start()
    {
        // Aviso una sola vez por punto mal colocado, en vez de en cada intento de spawn
        foreach (Transform point in spawnPoints)
        {
            if (point != null && SnapToNavMesh(point.position) == null)
                Debug.LogWarning($"{point.name} no está sobre el NavMesh: no aparecerán enemigos ahí");
        }

        StartCoroutine(WaveLoop());
    }

    void OnDestroy()
    {
        // No dejar suscripciones colgando al recargar la escena
        foreach (EnemyHealth enemy in aliveEnemies)
        {
            if (enemy != null) enemy.OnDeath -= HandleEnemyDeath;
        }
    }

    // Bucle principal: oleada → descanso → oleada con más enemigos → ...
    // Es infinito; termina cuando muere el jugador (se recarga la escena)
    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(config.timeBeforeFirstWave);

        int waveNumber = 1;
        while (true)
        {
            BeginWave(waveNumber);
            yield return StartCoroutine(RunWave()); // Espera a que la oleada termine

            Debug.Log($"Oleada {waveNumber} completada");
            OnWaveCompleted?.Invoke(waveNumber);

            yield return new WaitForSeconds(config.timeBetweenWaves);
            waveNumber++;
        }
    }

    // Prepara la oleada: calcula su total y crea la cola
    void BeginWave(int waveNumber)
    {
        currentWave = waveNumber;
        int total = config.GetTotalEnemies(waveNumber);
        queue = new WaveSpawnQueue(total, config.maxAlive);
        aliveEnemies.Clear();

        Debug.Log($"Oleada {waveNumber}: {total} enemigos, máx {queue.MaxAlive} a la vez");
        OnWaveStarted?.Invoke(waveNumber);
    }

    // Una oleada completa: va sacando enemigos mientras haya hueco, hasta que no quede ninguno
    IEnumerator RunWave()
    {
        while (!queue.IsComplete)
        {
            PurgeDestroyedEnemies();

            if (queue.CanSpawn) TrySpawnEnemy();

            yield return new WaitForSeconds(config.spawnInterval);
        }
    }

    // Intenta hacer aparecer un enemigo. Si ningún punto sirve, el enemigo sigue en la cola y se reintenta en el siguiente tick
    bool TrySpawnEnemy()
    {
        // Una posición candidata por cada punto, con un desplazamiento aleatorio
        List<Vector3> candidates = new List<Vector3>(spawnPoints.Length);
        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            candidates.Add(point.position + new Vector3(offset.x, 0f, offset.y));
        }

        Vector3? playerPos = player != null ? player.position : (Vector3?)null;
        if (!TryPickSpawnPosition(candidates, SnapToNavMesh, playerPos, minDistanceToPlayer, rng, out Vector3 spawnPos))
            return false;

        GameObject enemyObj = Instantiate(config.enemyPrefab, spawnPos, Quaternion.identity);
        enemyObj.name = $"Enemy_W{currentWave}_{queue.Total - queue.Remaining + 1}";

        EnemyHealth health = enemyObj.GetComponent<EnemyHealth>();
        queue.RegisterSpawn();
        aliveEnemies.Add(health);
        health.OnDeath += HandleEnemyDeath;
        return true;
    }

    // Prueba los candidatos en orden aleatorio y devuelve el primero que está sobre el NavMesh
    // y lejos del jugador. Probarlos todos en el mismo tick evita que la oleada se pare
    // si el jugador se queda encima de un punto. La distancia se mide sobre la posición ya ajustada
    // al NavMesh, que es donde aparecerá el enemigo de verdad.
    // Es estática y recibe el NavMesh como función para poder probarla sin escena.
    public static bool TryPickSpawnPosition(IList<Vector3> candidates, Func<Vector3, Vector3?> snapToNavMesh,
        Vector3? playerPos, float minDistanceToPlayer, System.Random random, out Vector3 position)
    {
        // Barajado Fisher-Yates de los índices
        int[] order = new int[candidates.Count];
        for (int i = 0; i < order.Length; i++) order[i] = i;
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        foreach (int index in order)
        {
            Vector3? snapped = snapToNavMesh(candidates[index]);
            if (snapped == null) continue;

            if (playerPos.HasValue)
            {
                Vector3 toPlayer = playerPos.Value - snapped.Value;
                toPlayer.y = 0f;
                if (toPlayer.magnitude < minDistanceToPlayer) continue;
            }

            position = snapped.Value;
            return true;
        }

        position = default;
        return false;
    }

    // Ajusta una posición al NavMesh más cercano (el NavMeshAgent da error si aparece fuera)
    Vector3? SnapToNavMesh(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, spawnRadius + 1f, NavMesh.AllAreas)) return hit.position;
        return null;
    }

    // Al morir un enemigo se libera su hueco; el siguiente saldrá en el próximo tick de RunWave
    void HandleEnemyDeath(EnemyHealth enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;

        // Remove devuelve false si no estaba en la lista: así una muerte doble no descuenta dos veces
        if (aliveEnemies.Remove(enemy)) queue.RegisterDeath();
    }

    // Red de seguridad: un enemigo destruido sin pasar por Die() (borrado a mano, caída al vacío...)
    // nunca lanza OnDeath. Sin esto la oleada se quedaría esperando a un "vivo" que ya no existe
    void PurgeDestroyedEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
                queue.RegisterDeath();
            }
        }
    }

    // Dibuja la zona de cada punto de spawn en la vista Scene
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.yellow;
        foreach (Transform point in spawnPoints)
        {
            if (point != null) Gizmos.DrawWireSphere(point.position, spawnRadius);
        }
    }
}
