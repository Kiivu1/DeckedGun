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

    [Header("Debug")]
    public TMP_Text txt; 
    public CardWeapon debugCard;
    public Transform shootTrans;

    private InputAction shootAct;

    // Posición del mazo del arma que se está usando (0 = primera casilla). Se cambia con las teclas 1-9
    private int armaElegida = 0;

    // Cartas del mazo (solo lectura). La usa la barra de inventario para dibujarlas
    public IReadOnlyList<CardInst> Mazo => cardDeck;
    public int ArmaElegida => armaElegida;

    void Awake()
    {
        // Se dispara con el clic izquierdo
        shootAct = new InputAction(binding: "<Mouse>/leftButton");
        shootAct.performed += ctx => ShootCard();
    }

    void Start()
    {
        SyncDeck();
    }

    // Se activan aquí (y no en Start) para que vuelvan a funcionar cuando el componente se reactiva (por ejemplo, al cerrar la tienda)
    void OnEnable()
    {
        shootAct?.Enable();
    }

    void OnDisable()
    {
        shootAct?.Disable();
    }

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

    public void ShootCard()
    {
        // FIXED: Check if deck has cards BEFORE trying to access index [0] to avoid crashes
        if (cardDeck.Count > 0)
        {
            // Dispara con el arma elegida con las teclas 1-9
            armaElegida = Mathf.Clamp(armaElegida, 0, cardDeck.Count - 1);
            CardInst currCard = cardDeck[armaElegida];

            if (currCard != null && currCard.weapon != null)
            {
                SpawnBullet(currCard.weapon);

                currCard.currAmmo--;

                if (currCard.currAmmo <= 0)
                {
                    cardDeck.RemoveAt(armaElegida);
                    if (armaElegida < cardW.Count) cardW.RemoveAt(armaElegida); // Keep both lists synced

                    // Si era la última casilla, pasa a la anterior
                    if (armaElegida >= cardDeck.Count) armaElegida = Mathf.Max(0, cardDeck.Count - 1);
                }
            }
        }
        else if (cardBase != null)
        {
            // Fallback to base infinite weapon when deck is empty
            SpawnBullet(cardBase);
        }
    }

    // Spawns the weapon's bullet at shootTrans and gives it the card's damage
    void SpawnBullet(CardWeapon weapon)
    {
        if (weapon.bulletPrefab == null || shootTrans == null) return;

        GameObject bulletObj = Instantiate(weapon.bulletPrefab, shootTrans.position, shootTrans.rotation);

        BulletBasicDebug bullet = bulletObj.GetComponent<BulletBasicDebug>();
        if (bullet != null) bullet.Damage = weapon.damage;
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