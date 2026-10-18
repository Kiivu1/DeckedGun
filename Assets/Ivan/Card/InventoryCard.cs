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
    private InputAction addCardAct;

    void Start()
    {
        shootAct = new InputAction(binding: "<Keyboard>/2");
        shootAct.performed += ctx => ShootCard();
        shootAct.Enable();

        addCardAct = new InputAction(binding: "<Keyboard>/1");
        addCardAct.performed += ctx => AddCardDebug();
        addCardAct.Enable();

        SyncDeck();
    }

    void OnDisable()
    {
        shootAct?.Disable();
        addCardAct?.Disable();
    }

    void Update()
    {
        UpdateUI();
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
            CardInst currCard = cardDeck[0];

            if (currCard != null && currCard.weapon != null)
            {
                if (currCard.weapon.bulletPrefab != null && shootTrans != null)
                {
                    Instantiate(currCard.weapon.bulletPrefab, shootTrans.position, shootTrans.rotation);
                }

                currCard.currAmmo--;

                if (currCard.currAmmo <= 0)
                {
                    cardDeck.RemoveAt(0);
                    if (cardW.Count > 0) cardW.RemoveAt(0); // Keep both lists synced
                }
            }
        }
        else if (cardBase != null)
        {
            // Fallback to base infinite weapon when deck is empty
            if (cardBase.bulletPrefab != null && shootTrans != null)
            {
                Instantiate(cardBase.bulletPrefab, shootTrans.position, shootTrans.rotation);
            }
        }
    }

    public void AddCardDebug()
    {
        if (debugCard != null)
        {
            cardW.Add(debugCard);
            cardDeck.Add(new CardInst(debugCard));
        }
    }
}