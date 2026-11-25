using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Barra de inventario siempre visible, abajo y centrada (estilo Minecraft / Roblox).
// Cada casilla es una carta del mazo con su munición. La casilla resaltada es el arma
// que se está usando ahora, y su nombre sale en pequeño encima de la barra.
// Va en el Player, junto a InventoryCard. La interfaz se crea sola por código.
[RequireComponent(typeof(InventoryCard))]
public class BarraInventario : MonoBehaviour
{
    [Header("Casillas")]
    public int numeroCasillas = 6;
    public float tamanoCasilla = 90f;
    public float separacion = 8f;
    public float distanciaAbajo = 30f; // Distancia desde el borde de abajo de la pantalla

    [Header("Colores")]
    public Color colorCasilla = new Color(0f, 0f, 0f, 0.55f);
    public Color colorBorde = new Color(1f, 1f, 1f, 0.25f);
    public Color colorSeleccion = new Color(1f, 0.85f, 0.2f, 1f);

    private InventoryCard inventario;
    private TMP_Text textoArmaActual;
    private readonly List<Casilla> casillas = new List<Casilla>();

    // Lo que tiene cada casilla de la barra
    private class Casilla
    {
        public RectTransform rect;
        public Outline borde;
        public Image icono;
        public TMP_Text nombre;    // Se usa si la carta no tiene sprite
        public TMP_Text municion;
    }

    void Awake()
    {
        inventario = GetComponent<InventoryCard>();

        // Oculta el texto antiguo del inventario (la lista de la esquina)
        if (inventario.txt != null) inventario.txt.gameObject.SetActive(false);

        CrearUI();
    }

    void Update()
    {
        ActualizarBarra();
    }

    void ActualizarBarra()
    {
        IReadOnlyList<CardInst> mazo = inventario.Mazo;
        int elegida = mazo.Count > 0 ? Mathf.Clamp(inventario.ArmaElegida, 0, mazo.Count - 1) : 0;

        for (int i = 0; i < casillas.Count; i++)
        {
            Casilla c = casillas[i];
            CardWeapon arma = null;
            string municion = "";

            if (i < mazo.Count && mazo[i] != null && mazo[i].weapon != null)
            {
                arma = mazo[i].weapon;
                municion = mazo[i].currAmmo.ToString();
            }
            else if (i == 0 && mazo.Count == 0 && inventario.cardBase != null)
            {
                // Sin cartas se dispara con el arma base, que tiene munición infinita
                arma = inventario.cardBase;
                municion = "∞";
            }

            PintarCasilla(c, arma, municion, i == elegida && arma != null);
        }

        // Nombre del arma actual (la elegida del mazo, o la base si no quedan cartas)
        CardWeapon actual = mazo.Count > 0 ? mazo[elegida].weapon : inventario.cardBase;
        textoArmaActual.text = actual != null ? actual.cardName : "";
    }

    void PintarCasilla(Casilla c, CardWeapon arma, string municion, bool seleccionada)
    {
        bool hayArma = arma != null;
        bool haySprite = hayArma && arma.cardSprite != null;

        c.icono.enabled = haySprite;
        if (haySprite) c.icono.sprite = arma.cardSprite;

        c.nombre.text = hayArma && !haySprite ? arma.cardName : "";
        c.municion.text = municion;

        // La casilla del arma actual se ve más grande y con el borde de color
        c.borde.effectColor = seleccionada ? colorSeleccion : colorBorde;
        c.borde.effectDistance = seleccionada ? new Vector2(4, -4) : new Vector2(2, -2);
        c.rect.localScale = seleccionada ? Vector3.one * 1.1f : Vector3.one;
    }

    // ---------- Creación de la interfaz por código ----------

    void CrearUI()
    {
        GameObject canvasGO = new GameObject("BarraInventarioCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // Por encima del HUD, por debajo de la tienda

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Contenedor de la barra, abajo y centrado
        GameObject barra = new GameObject("Barra", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        barra.transform.SetParent(canvasGO.transform, false);
        RectTransform rtBarra = barra.GetComponent<RectTransform>();
        rtBarra.anchorMin = rtBarra.anchorMax = rtBarra.pivot = new Vector2(0.5f, 0f);
        rtBarra.anchoredPosition = new Vector2(0, distanciaAbajo);
        rtBarra.sizeDelta = new Vector2(numeroCasillas * tamanoCasilla + (numeroCasillas - 1) * separacion, tamanoCasilla);

        HorizontalLayoutGroup layout = barra.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = separacion;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < numeroCasillas; i++)
        {
            casillas.Add(CrearCasilla(barra.transform, i + 1));
        }

        // Nombre del arma actual, en pequeño encima de la barra
        textoArmaActual = CrearTexto(canvasGO.transform, 26, TextAlignmentOptions.Center);
        RectTransform rtNombre = textoArmaActual.rectTransform;
        rtNombre.anchorMin = rtNombre.anchorMax = rtNombre.pivot = new Vector2(0.5f, 0f);
        rtNombre.anchoredPosition = new Vector2(0, distanciaAbajo + tamanoCasilla + 12);
        rtNombre.sizeDelta = new Vector2(600, 36);
        textoArmaActual.fontStyle = FontStyles.Bold;
    }

    Casilla CrearCasilla(Transform padre, int numero)
    {
        Casilla c = new Casilla();

        GameObject fondo = new GameObject("Casilla", typeof(RectTransform), typeof(Image), typeof(Outline));
        fondo.transform.SetParent(padre, false);
        fondo.GetComponent<Image>().color = colorCasilla;
        c.rect = fondo.GetComponent<RectTransform>();
        c.rect.sizeDelta = new Vector2(tamanoCasilla, tamanoCasilla);
        c.borde = fondo.GetComponent<Outline>();

        // Icono de la carta (si tiene sprite)
        GameObject icono = new GameObject("Icono", typeof(RectTransform), typeof(Image));
        icono.transform.SetParent(fondo.transform, false);
        c.icono = icono.GetComponent<Image>();
        c.icono.preserveAspect = true;
        RectTransform rtIcono = c.icono.rectTransform;
        rtIcono.anchorMin = Vector2.zero;
        rtIcono.anchorMax = Vector2.one;
        rtIcono.offsetMin = new Vector2(10, 10);
        rtIcono.offsetMax = new Vector2(-10, -10);

        // Nombre corto, para cartas sin sprite
        c.nombre = CrearTexto(fondo.transform, 18, TextAlignmentOptions.Center);
        RectTransform rtNombre = c.nombre.rectTransform;
        rtNombre.anchorMin = Vector2.zero;
        rtNombre.anchorMax = Vector2.one;
        rtNombre.offsetMin = new Vector2(6, 18);
        rtNombre.offsetMax = new Vector2(-6, -6);

        // Munición, abajo a la derecha
        c.municion = CrearTexto(fondo.transform, 22, TextAlignmentOptions.BottomRight);
        RectTransform rtMunicion = c.municion.rectTransform;
        rtMunicion.anchorMin = Vector2.zero;
        rtMunicion.anchorMax = Vector2.one;
        rtMunicion.offsetMin = new Vector2(4, 2);
        rtMunicion.offsetMax = new Vector2(-6, -4);
        c.municion.fontStyle = FontStyles.Bold;

        // Número de la tecla que elige esta casilla, arriba a la izquierda
        TMP_Text tecla = CrearTexto(fondo.transform, 16, TextAlignmentOptions.TopLeft);
        tecla.text = numero.ToString();
        tecla.color = new Color(1f, 1f, 1f, 0.6f);
        RectTransform rtTecla = tecla.rectTransform;
        rtTecla.anchorMin = Vector2.zero;
        rtTecla.anchorMax = Vector2.one;
        rtTecla.offsetMin = new Vector2(6, 4);
        rtTecla.offsetMax = new Vector2(-4, -4);

        return c;
    }

    TMP_Text CrearTexto(Transform padre, float tamano, TextAlignmentOptions alineacion)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);

        TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = Color.white;
        texto.raycastTarget = false;
        return texto;
    }
}
