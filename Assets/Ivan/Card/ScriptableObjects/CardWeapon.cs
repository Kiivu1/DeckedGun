using UnityEngine;
using System.Collections;

public enum WeaponType
{
    Pistol,
    Shotgun
}

[CreateAssetMenu(fileName = "CardWeapon", menuName = "Cards/Item/Weapon")]
public class CardWeapon : CardItem
{
    public int cardAmmoBase;        //The BaseAmmo
    public GameObject bulletPrefab; //thing to shoot

    public float damage = 10f;      //damage of each bullet

    [Header("Weapon Type")]
    public WeaponType weaponType = WeaponType.Pistol;

    [Header("Projectiles")]
    //Num of projectiles
    [Min(1)]
    public int projectilesPerShot = 1;

     // Spread applied to every shot, including the first one.
    [Range(0f, 15f)]
    public float baseSpreadAngle = 0f;

    [Header("Rapid Fire Spread")]
    [Range(0f, 15f)]
    public float spreadAngle = 0f;
}
