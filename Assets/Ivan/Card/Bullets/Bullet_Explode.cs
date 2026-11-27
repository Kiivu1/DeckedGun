using System.Collections;
using UnityEngine;

public class Bullet_Explode : MonoBehaviour
{

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 3f; // Seconds before self-destruction

    [Header("Damage")]
    [SerializeField] private float damage = 10f; // Overwritten by the card that shoots it (CardWeapon.damage)

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    void Start()
    {
        // Start the timer to delete the bullet automatically
        StartCoroutine(DeleteBulletRoutine());
    }

    void Update()
    {
        MoveBullet();
    }

    void MoveBullet()
    {
    }

    IEnumerator DeleteBulletRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}