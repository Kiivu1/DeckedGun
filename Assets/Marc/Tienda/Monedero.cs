using System;
using UnityEngine;

// Monedero del jugador. De momento empieza con una cantidad fija y solo se gasta en la tienda.
// Si el Player no lo tiene puesto, la tienda lo añade sola al entrar en ella.
public class Monedero : MonoBehaviour
{
    public int monedasIniciales = 200;

    public int Monedas { get; private set; }

    // Se lanza cada vez que cambia la cantidad de monedas (para actualizar la UI)
    public event Action<int> AlCambiarMonedas;

    void Awake()
    {
        Monedas = monedasIniciales;
    }

    // Intenta gastar monedas. Devuelve false si no hay suficientes
    public bool Gastar(int cantidad)
    {
        if (cantidad < 0 || cantidad > Monedas) return false;

        Monedas -= cantidad;
        AlCambiarMonedas?.Invoke(Monedas);
        return true;
    }

    // Para más adelante: dar monedas al matar enemigos, recoger objetos, etc.
    public void Anadir(int cantidad)
    {
        if (cantidad <= 0) return;

        Monedas += cantidad;
        AlCambiarMonedas?.Invoke(Monedas);
    }
}
