
[System.Serializable]
public class CardInst
{
    public CardWeapon weapon;
    public int currAmmo;
    public CardInst(CardWeapon wp)
    {
        weapon = wp;
        if(weapon != null){currAmmo = wp.cardAmmoBase;}
    }
}
