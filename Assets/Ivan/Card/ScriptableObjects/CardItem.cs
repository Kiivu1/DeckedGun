using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CardItem", menuName = "Cards/CardItem")]
public class CardItem : ScriptableObject
{
    public string cardName;     //nombre de la tarjeta
    public string cardDesc;     //descripcion
    public Sprite cardSprite;   //sprite of the Card
    public int cardValue;       //valor de la tarjeta(tienda o no)
}
