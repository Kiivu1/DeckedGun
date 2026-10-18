using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CardWeapon", menuName = "Cards/Item/Weapon")]
public class CardWeapon : CardItem
{
    public int cardAmmoBase;        //The BaseAmmo
    public GameObject bulletPrefab; //thing to shoot
}
