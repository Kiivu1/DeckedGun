using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float vidaActual = 100f;
    public Slider barraDeVida;
    private float tiempoSiguienteAccion = 0f; // Controla el tiempo de espera

    void Start()
    {
        barraDeVida.maxValue = 100f;
        barraDeVida.value = vidaActual;
    }

    // Este método es exclusivo para detectar choques en Character Controllers
    void OnControllerColliderHit(ControllerColliderHit colision)
    {
        // Comprueba si ha pasado 1 segundo desde la última acción
        if (Time.time >= tiempoSiguienteAccion)
        {
            if (colision.gameObject.CompareTag("Roja"))
            {
                vidaActual -= 20f;
                ActualizarVida();
            }
            else if (colision.gameObject.CompareTag("Verde"))
            {
                vidaActual += 20f;
                ActualizarVida();
            }
        }
    }

    void ActualizarVida()
    {
        // Clamp evita que la vida baje de 0 o suba de 100
        vidaActual = Mathf.Clamp(vidaActual, 0, 100f);
        barraDeVida.value = vidaActual;

        // Añade 1 segundo de espera antes de volver a recibir daño o curación
        tiempoSiguienteAccion = Time.time + 1f;
    }
}