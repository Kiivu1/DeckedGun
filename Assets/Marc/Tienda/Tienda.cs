using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Plataforma tienda: al subirte encima se abre una pantalla con las cartas en venta.
// Se pone en un objeto con collider normal (ej. un Cube aplanado). La zona que detecta
// al jugador encima y la interfaz se crean solas por código.
[RequireComponent(typeof(Collider))]
public class Tienda : MonoBehaviour
{
    [Header("Productos")]
    public List<CardWeapon> productos = new List<CardWeapon>(); // El precio de cada carta es su cardValue

    [Header("Opciones")]
    public bool pausarJuego = true;      // Congela el juego (enemigos incluidos) mientras la tienda está abierta
    public float alturaDeteccion = 1f;   // Altura de la zona encima de la plataforma que detecta al jugador

    // Componentes del jugador que está encima
    private Monedero monedero;
    private InventoryCard inventario;
    private PlayerMovement movimiento;
    private Camara camara;

    private bool abierta = false;

    // Interfaz
    private GameObject panel;
    private TMP_Text textoMonedas;
    private TMP_Text textoMensaje;
    private readonly List<(CardWeapon carta, Button boton)> casillas = new List<(CardWeapon, Button)>();

    public bool EstaAbierta => abierta;

#if UNITY_EDITOR
    // Armas que se ponen solas en la tienda si la lista de productos está vacía.
    // El precio de cada una se cambia en su asset, en el campo "Card Value".
    private static readonly string[] armasPorDefecto =
    {
        "Assets/Ivan/Card/ScriptableObjects/CardNormal.asset",  // Normal (50 monedas)
        "Assets/Ivan/Card/ScriptableObjects/CardShotgun.asset", // Shotgun (100 monedas)
    };

    // Solo en el editor: rellena la lista de productos con las armas por defecto
    void OnValidate()
    {
        if (Application.isPlaying || productos.Count > 0) return;

        foreach (string ruta in armasPorDefecto)
        {
            CardWeapon arma = UnityEditor.AssetDatabase.LoadAssetAtPath<CardWeapon>(ruta);
            if (arma != null) productos.Add(arma);
        }

        if (productos.Count > 0) UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    void Awake()
    {
        CrearZonaDeteccion();
        CrearUI();
    }

    void Update()
    {
        if (abierta && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cerrar();
        }
    }

    // Al subirse a la plataforma se abre la tienda
    void OnTriggerEnter(Collider other)
    {
        // Solo cuenta el CharacterController del jugador (no su hitbox ni otros colliders)
        if (!(other is CharacterController)) return;

        PlayerMovement jugador = other.GetComponent<PlayerMovement>();
        if (jugador == null) return;

        movimiento = jugador;
        camara = jugador.GetComponentInChildren<Camara>();
        inventario = jugador.GetComponentInChildren<InventoryCard>();
        monedero = jugador.GetComponent<Monedero>();
        if (monedero == null) monedero = jugador.gameObject.AddComponent<Monedero>();

        Abrir();
    }

    // Para volver a abrirla hay que bajarse y volver a subir
    void OnTriggerExit(Collider other)
    {
        if (!(other is CharacterController) || other.GetComponent<PlayerMovement>() != movimiento) return;

        Cerrar();
    }

    void OnDisable()
    {
        // Si la tienda desaparece estando abierta, devuelve el control al jugador
        Cerrar();
    }

    void Abrir()
    {
        if (abierta) return;

        abierta = true;
        panel.SetActive(true);
        textoMensaje.text = "";
        ActualizarUI();

        // Bloquea moverse, mirar y disparar mientras se compra
        if (movimiento != null) movimiento.enabled = false;
        if (camara != null) camara.enabled = false;
        if (inventario != null) inventario.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausarJuego) Time.timeScale = 0f;
    }

    void Cerrar()
    {
        if (!abierta) return;

        abierta = false;
        if (panel != null) panel.SetActive(false);

        if (movimiento != null) movimiento.enabled = true;
        if (camara != null) camara.enabled = true;
        if (inventario != null) inventario.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pausarJuego) Time.timeScale = 1f;
    }

    void Comprar(CardWeapon carta)
    {
        if (monedero == null || inventario == null)
        {
            textoMensaje.text = "Error: el jugador no tiene InventoryCard";
            return;
        }

        if (monedero.Gastar(carta.cardValue))
        {
            inventario.AnadirCarta(carta);
            textoMensaje.text = $"Has comprado {carta.cardName}";
        }
        else
        {
            textoMensaje.text = "No tienes suficientes monedas";
        }

        ActualizarUI();
    }

    void ActualizarUI()
    {
        int monedas = monedero != null ? monedero.Monedas : 0;
        textoMonedas.text = $"Monedas: {monedas}";

        // Las casillas que no te puedes permitir salen desactivadas
        foreach (var (carta, boton) in casillas)
        {
            boton.interactable = monedas >= carta.cardValue;
        }
    }

    // Añade un segundo BoxCollider (trigger) justo encima de la plataforma
    void CrearZonaDeteccion()
    {
        Bounds b = GetComponent<Collider>().bounds;
        Vector3 escala = transform.lossyScale;

        BoxCollider zona = gameObject.AddComponent<BoxCollider>();
        zona.isTrigger = true;

        // Las medidas del BoxCollider van en espacio local, por eso se divide por la escala
        Vector3 centroMundo = new Vector3(b.center.x, b.max.y + alturaDeteccion / 2f, b.center.z);
        zona.center = transform.InverseTransformPoint(centroMundo);
        zona.size = new Vector3(
            b.size.x / Mathf.Abs(escala.x),
            alturaDeteccion / Mathf.Abs(escala.y),
            b.size.z / Mathf.Abs(escala.z));
    }

    // ---------- Creación de la interfaz por código ----------

    void CrearUI()
    {
        GameObject canvasGO = new GameObject("TiendaCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Por encima del HUD

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Fondo oscuro de toda la pantalla
        panel = CrearImagen("Tienda", canvasGO.transform, new Color(0f, 0f, 0f, 0.6f));
        Estirar(panel.GetComponent<RectTransform>());

        // Ventana central
        GameObject ventana = CrearImagen("Ventana", panel.transform, new Color(0.1f, 0.1f, 0.13f, 0.97f));
        RectTransform rtVentana = ventana.GetComponent<RectTransform>();
        rtVentana.sizeDelta = new Vector2(1100, 700);

        TMP_Text titulo = CrearTexto(ventana.transform, "TIENDA", 56);
        titulo.fontStyle = FontStyles.Bold;
        Colocar(titulo.rectTransform, new Vector2(0, -30), new Vector2(600, 80));

        textoMonedas = CrearTexto(ventana.transform, "", 34);
        Colocar(textoMonedas.rectTransform, new Vector2(0, -110), new Vector2(600, 50));

        // Botón X para cerrar, arriba a la derecha
        GameObject cerrar = CrearImagen("Cerrar", ventana.transform, new Color(0.6f, 0.15f, 0.15f, 1f));
        RectTransform rtCerrar = cerrar.GetComponent<RectTransform>();
        rtCerrar.anchorMin = rtCerrar.anchorMax = rtCerrar.pivot = new Vector2(1f, 1f);
        rtCerrar.anchoredPosition = new Vector2(-20, -20);
        rtCerrar.sizeDelta = new Vector2(60, 60);
        cerrar.AddComponent<Button>().onClick.AddListener(Cerrar);
        TMP_Text textoX = CrearTexto(cerrar.transform, "X", 36);
        Estirar(textoX.rectTransform);
        textoX.raycastTarget = false;

        // Rejilla de casillas
        GameObject rejilla = new GameObject("Casillas", typeof(RectTransform), typeof(GridLayoutGroup));
        rejilla.transform.SetParent(ventana.transform, false);
        RectTransform rtRejilla = rejilla.GetComponent<RectTransform>();
        rtRejilla.anchorMin = new Vector2(0f, 0f);
        rtRejilla.anchorMax = new Vector2(1f, 1f);
        rtRejilla.offsetMin = new Vector2(40, 110);   // Deja hueco abajo para el mensaje
        rtRejilla.offsetMax = new Vector2(-40, -180);// Deja hueco arriba para título y monedas

        GridLayoutGroup grid = rejilla.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(220, 260);
        grid.spacing = new Vector2(30, 30);
        grid.childAlignment = TextAnchor.UpperCenter;

        foreach (CardWeapon carta in productos)
        {
            if (carta != null) CrearCasilla(carta, rejilla.transform);
        }

        textoMensaje = CrearTexto(ventana.transform, "", 30);
        RectTransform rtMensaje = textoMensaje.rectTransform;
        rtMensaje.anchorMin = rtMensaje.anchorMax = rtMensaje.pivot = new Vector2(0.5f, 0f);
        rtMensaje.anchoredPosition = new Vector2(0, 50);
        rtMensaje.sizeDelta = new Vector2(900, 50);

        TMP_Text ayuda = CrearTexto(ventana.transform, "Pulsa una casilla para comprar  ·  Esc para cerrar", 22);
        ayuda.color = new Color(1f, 1f, 1f, 0.5f);
        RectTransform rtAyuda = ayuda.rectTransform;
        rtAyuda.anchorMin = rtAyuda.anchorMax = rtAyuda.pivot = new Vector2(0.5f, 0f);
        rtAyuda.anchoredPosition = new Vector2(0, 15);
        rtAyuda.sizeDelta = new Vector2(900, 35);

        panel.SetActive(false);
    }

    // Una casilla: nombre, datos de la carta y precio. Se pulsa para comprar
    void CrearCasilla(CardWeapon carta, Transform padre)
    {
        GameObject casilla = CrearImagen(carta.cardName, padre, new Color(0.25f, 0.25f, 0.32f, 1f));
        Button boton = casilla.AddComponent<Button>();
        boton.onClick.AddListener(() => Comprar(carta));

        TMP_Text texto = CrearTexto(casilla.transform, "", 28);
        texto.text = $"<b>{carta.cardName}</b>\n\n" +
                     $"<size=22>Munición: {carta.cardAmmoBase}\nDaño: {carta.damage}</size>\n\n" +
                     $"<color=#FFD54A><b>{carta.cardValue} monedas</b></color>";
        Estirar(texto.rectTransform);
        texto.raycastTarget = false;

        casillas.Add((carta, boton));
    }

    GameObject CrearImagen(string nombre, Transform padre, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(padre, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    TMP_Text CrearTexto(Transform padre, string contenido, float tamano)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);

        TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        return texto;
    }

    // Ocupa todo el padre
    void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Centrado arriba del padre, a cierta distancia del borde superior
    void Colocar(RectTransform rt, Vector2 posicion, Vector2 tamano)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
    }
}
