using System.Collections;
using UnityEngine;

public class BulletParent : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField]protected float lifeTime = 1f;
    [SerializeField]protected float damage = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    protected virtual void Update()
    {
        MoveBullet();
    }

    protected virtual void MoveBullet()
    {
        // Override in subclasses if movement differs
    }

    public virtual void HandleImpact(Vector3 impactPoint)
    {
        Destroy(gameObject);
    }

    protected virtual IEnumerator DeleteBulletRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        HandleImpact(transform.position);
    }
}
