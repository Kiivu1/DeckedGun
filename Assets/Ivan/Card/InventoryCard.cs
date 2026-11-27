using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class InventoryCard : MonoBehaviour
{
    [Header("Cards")]
    public List<CardWeapon> cardW = new List<CardWeapon>();    // Data of current cards the player has (shop items)
    private List<CardInst> cardDeck = new List<CardInst>();    // Copied runtime cards with their own current Ammo

    public CardWeapon cardBase; // Fallback weapon when deck is empty


    [Header("Reload")]
    [SerializeField] private float maxReloadTimer;
    [SerializeField] private float currReloadTimer;


    [Header("Debug")]
    public TMP_Text txt; 
    public CardWeapon debugCard;




    // Posición del mazo del arma que se está usando (0 = primera casilla). Se cambia con las teclas 1-9
    private int armaElegida = 0;

    // Cartas del mazo (solo lectura). La usa la barra de inventario para dibujarlas
    public IReadOnlyList<CardInst> Mazo => cardDeck;
    public int ArmaElegida => armaElegida;

    void Start()
    {
        SyncDeck();
    }

    // Se activan aquí (y no en Start) para que vuelvan a funcionar cuando el componente se reactiva (por ejemplo, al cerrar la tienda)

    void Update()
    {
        ElegirArmaConTeclas();
        UpdateUI();
    }

    // Las teclas 1-9 eligen el arma de esa casilla de la barra
    void ElegirArmaConTeclas()
    {
        if (Keyboard.current == null) return;

        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                ElegirArma(i);
                return;
            }
        }
    }

    // Cambia al arma de esa posición, solo si hay una carta ahí
    public void ElegirArma(int posicion)
    {
        if (posicion >= 0 && posicion < cardDeck.Count) armaElegida = posicion;
    }

    void SyncDeck()
    {
        cardDeck.Clear();
        foreach (CardWeapon weapon in cardW)
        {
            if (weapon != null)
            {
                cardDeck.Add(new CardInst(weapon));
            }
        }
    }

    void UpdateUI()
    {
        if (txt == null) return;

        // Check cardDeck.Count instead of cardW.Count
        if (cardDeck.Count > 0)
        {
            txt.text = "";
            foreach (CardInst card in cardDeck)
            {
                if (card != null && card.weapon != null)
                {
                    // FIXED: Use '+=' so it appends all cards instead of overwriting
                    txt.text += $"{card.weapon.cardName}\nAmmo: {card.currAmmo}\n\n";
                }
            }    
        }
        else if (cardBase != null)
        {
            txt.text = $"{cardBase.cardName}\nAmmo: Infinite";
        }
        else
        {
            txt.text = "No Cards";
        }
    }

 // WeaponController consulta qué arma debe disparar.
    public CardWeapon GetCurrentWeapon()
    {
        if (cardDeck.Count == 0)
        {
            return cardBase;
        }

        armaElegida = Mathf.Clamp(
            armaElegida, 0, cardDeck.Count - 1
        );

        return cardDeck[armaElegida]?.weapon;
    }

    // WeaponController llama aquí después de crear la bala.
    public void ConsumeAmmo()
    {
        // Sin cartas se usa el arma base, con munición infinita.
        if (cardDeck.Count > 0)
        {
            armaElegida = Mathf.Clamp(
                armaElegida, 0, cardDeck.Count - 1
            );

            CardInst currCard = cardDeck[armaElegida];

            if (currCard != null && currCard.weapon != null)
            {
                currCard.currAmmo--;

                if (currCard.currAmmo <= 0)
                {
                    cardDeck.RemoveAt(armaElegida);

                    if (armaElegida < cardW.Count)
                    {
                        cardW.RemoveAt(armaElegida);
                    }

                    // Si era la última casilla, selecciona la anterior.
                    if (armaElegida >= cardDeck.Count)
                    {
                        armaElegida = Mathf.Max(0, cardDeck.Count - 1);
                    }
                }
            }
        }
    }

    // Añade una carta al mazo con la munición llena. La usa la tienda
    public void AnadirCarta(CardWeapon carta)
    {
        if (carta == null) return;

        cardW.Add(carta);
        cardDeck.Add(new CardInst(carta));
    }

    public void AddCardDebug()
    {
        AnadirCarta(debugCard);
    }
}