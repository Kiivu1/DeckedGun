using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [Header("Economía")]
    public int monedas = 100;

    [Header("Interfaz")]
    public TMP_Text textoMonedas; // Arrastra aquí un texto de tu Canvas para ver el dinero

    void Start()
    {
        ActualizarUI();
    }

    // Para actualizar las menodas mas adelante
    public void AñadirMonedas(int cantidad)
    {
        monedas += cantidad;
        ActualizarUI();
    }

    // Este método lo llamará el ShopManager al intentar comprar una carta
    // Devuelve 'true' si se pudo efectuar la compra, 'false' si no alcanza el dinero
    public bool GastarMonedas(int coste)
    {
        if (monedas >= coste)
        {
            monedas -= coste;
            ActualizarUI();
            return true;
        }

        Debug.Log("Dinero insuficiente. Tienes: " + monedas + ", Cuesta: " + coste);
        return false;
    }

    private void ActualizarUI()
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = "Monedas: " + monedas;
        }
    }
}