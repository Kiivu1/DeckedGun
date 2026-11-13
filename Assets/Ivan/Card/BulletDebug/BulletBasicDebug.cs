using System.Collections;
using UnityEngine;

public class BulletBasicDebug : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;

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
        // Moves the bullet forward relative to its rotation
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    IEnumerator DeleteBulletRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}