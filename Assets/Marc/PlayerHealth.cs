using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float vidaActual = 100f;
    public Slider barraDeVida;

    [Header("Invencibilidad")]
    public float tiempoInvencible = 0.5f;   // Segundos sin recibir daño tras un golpe (evita que varios golpes a la vez se sumen)
    public Color colorGolpe = Color.white;  // Color de la barra mientras dura la invencibilidad

    [Header("Plataformas")]
    public float danoPlataforma = 20f;
    public float esperaPlataforma = 1f;     // Segundos entre cada daño/curación de las plataformas

    private float finInvencibilidad = 0f;     // Time.time hasta el que no se recibe daño
    private float tiempoSiguientePlataforma = 0f;
    private bool muerto = false;
    private Image rellenoBarra;
    private Color colorRelleno;

    // Se lanza al morir el jugador
    public event Action OnPlayerDeath;

    public bool EsInvencible => Time.time < finInvencibilidad;

    void Start()
    {
        barraDeVida.maxValue = vidaMaxima;
        barraDeVida.value = vidaActual;

        // Imagen de relleno de la barra, para hacerla parpadear al recibir daño
        if (barraDeVida.fillRect != null)
        {
            rellenoBarra = barraDeVida.fillRect.GetComponent<Image>();
            if (rellenoBarra != null) colorRelleno = rellenoBarra.color;
        }
    }

    // Quita vida al jugador, salvo que sea invencible. La usan los enemigos y la plataforma roja
    public void TakeDamage(float cantidad)
    {
        if (muerto || EsInvencible) return;

        vidaActual -= cantidad;
        ActualizarVida();

        finInvencibilidad = Time.time + tiempoInvencible;
        if (rellenoBarra != null)
        {
            StopAllCoroutines();
            StartCoroutine(ParpadeoBarra());
        }

        if (vidaActual <= 0f) Morir();
    }

    // Cura al jugador. La invencibilidad no bloquea las curaciones
    public void Heal(float cantidad)
    {
        if (muerto) return;

        vidaActual += cantidad;
        ActualizarVida();
    }

    // Este método es exclusivo para detectar choques en Character Controllers
    void OnControllerColliderHit(ControllerColliderHit colision)
    {
        // Las plataformas solo actúan una vez cada esperaPlataforma segundos
        if (Time.time < tiempoSiguientePlataforma) return;

        if (colision.gameObject.CompareTag("Roja"))
        {
            TakeDamage(danoPlataforma);
            tiempoSiguientePlataforma = Time.time + esperaPlataforma;
        }
        else if (colision.gameObject.CompareTag("Verde"))
        {
            Heal(danoPlataforma);
            tiempoSiguientePlataforma = Time.time + esperaPlataforma;
        }
    }

    void ActualizarVida()
    {
        // Clamp evita que la vida baje de 0 o suba del máximo
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        barraDeVida.value = vidaActual;
    }

    void Morir()
    {
        muerto = true;
        Debug.Log("Player died");
        OnPlayerDeath?.Invoke();

        // De momento se reinicia la escena. Más adelante, pantalla de Game Over
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // La barra cambia a colorGolpe mientras el jugador es invencible
    IEnumerator ParpadeoBarra()
    {
        rellenoBarra.color = colorGolpe;
        yield return new WaitForSeconds(tiempoInvencible);
        rellenoBarra.color = colorRelleno;
    }
}
