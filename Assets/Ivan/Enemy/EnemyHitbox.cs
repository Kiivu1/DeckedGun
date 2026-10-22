using System.Collections;
using UnityEngine;

// Detecta cuando una bala toca al enemigo.
// Requisitos en el GameObject:
//  - Un Collider con "Is Trigger" activado (la hitbox).
//  - Un Rigidbody en modo Kinematic y sin gravedad. Unity solo lanza OnTriggerEnter si
//    al menos uno de los dos objetos tiene Rigidbody, y las balas no lo tienen.
public class EnemyHitbox : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private Color hitColor = Color.red; // Color al que cambia el enemigo al recibir un impacto
    [SerializeField] private float flashTime = 0.1f;     // Segundos que se queda con hitColor tras el impacto

    private Renderer rend;    // Renderer del enemigo, para poder cambiarle el color
    private Color baseColor;  // Color original, para restaurarlo después del parpadeo

    void Awake()
    {
        // Busca el Renderer en este objeto o en sus hijos (por si la malla está en un hijo)
        rend = GetComponentInChildren<Renderer>();

        // Guarda el color inicial. Usar .material crea una copia del material solo para este enemigo,
        // así el parpadeo no cambia el color de los demás objetos que comparten material
        if (rend != null) baseColor = rend.material.color;
    }

    // Unity llama a este método automáticamente cuando otro collider entra en nuestro trigger
    void OnTriggerEnter(Collider other)
    {
        // El collider de la bala está en un hijo (BulletBasic) y el script en el padre (BulletDebug),
        // por eso se busca el componente hacia arriba en la jerarquía
        BulletBasicDebug bullet = other.GetComponentInParent<BulletBasicDebug>();

        // Si lo que ha entrado no es una bala (suelo, jugador...), lo ignoramos
        if (bullet == null) return;

        OnHit(bullet.gameObject);
    }

    // Qué pasa cuando una bala impacta. Es público para poder llamarlo desde otros scripts
    // (por ejemplo, si más adelante se dispara con raycast). Aquí iría la lógica de quitar vida.
    public void OnHit(GameObject bullet)
    {
        Debug.Log($"{name} hit by {bullet.name}");

        // La bala desaparece al impactar
        Destroy(bullet);

        if (rend != null)
        {
            // Si ya había un parpadeo en curso lo cortamos para que no se solapen
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    // Corrutina: pone el enemigo en hitColor, espera flashTime segundos y vuelve al color original
    IEnumerator FlashRoutine()
    {
        rend.material.color = hitColor;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = baseColor;
    }
}
