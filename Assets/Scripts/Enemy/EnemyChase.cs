using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Busca al jugador por su tag (el Player.prefab ya tiene el tag "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Recalcula el camino hacia la posición actual del jugador
        agent.SetDestination(player.position);
    }
}
