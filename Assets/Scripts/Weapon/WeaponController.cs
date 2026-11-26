using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryCard inventory;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Shooting")]
    [SerializeField, Min(0f)] private float shotInterval = 0.2f;
    [SerializeField, Min(1f)] private float aimDistance = 100f;
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Spread")]
    // Maximum time between shots to consider them part of a rapid sequence.
    [SerializeField, Min(0f)] private float rapidShotWindow = 0.4f;

    // Spread added by each consecutive rapid shot, in degrees.
    [SerializeField, Min(0f)] private float spreadPerShot = 0.5f;

    // Track the previous successful shot and its accumulated spread.
    private float lastShotTime = float.NegativeInfinity;
    private float currentSpread = 0f;
    private CardWeapon lastShotWeapon;

    private float nextShotTime;

    void Update()
    {
        // La tienda desactiva InventoryCard, incluso si no pausa el juego.
        if (inventory == null || !inventory.isActiveAndEnabled || Time.timeScale == 0f)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryShoot();
    }

    void TryShoot()
    {
        if (Time.time < nextShotTime) return;

        if (shootPoint == null || playerCamera == null)
        {
            Debug.LogWarning("Asigna Shoot Point y Player Camera en WeaponController.", this);
            return;
        }

        CardWeapon weapon = inventory.GetCurrentWeapon();
        if (weapon == null || weapon.bulletPrefab == null) return;

        if (weapon.bulletPrefab.GetComponent<BulletBasicDebug>() == null)
        {
            Debug.LogWarning("El prefab necesita BulletBasicDebug en su raíz.", this);
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = ray.GetPoint(aimDistance);
        float nearestDistance = aimDistance;

        // Incluye hitboxes Trigger, pero ignora al propio jugador y las balas.
        RaycastHit[] hits = Physics.RaycastAll(
            ray, aimDistance, aimMask, QueryTriggerInteraction.Collide);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(inventory.transform)) continue;
            if (hit.collider.GetComponentInParent<BulletBasicDebug>() != null) continue;
            if (hit.distance >= nearestDistance) continue;

            nearestDistance = hit.distance;
            targetPoint = hit.point;
        }

        Vector3 direction = targetPoint - shootPoint.position;
        if (direction.sqrMagnitude < 0.0001f || Vector3.Dot(direction, ray.direction) <= 0f)
            direction = ray.direction;

        // Continue accumulating spread only when firing the same weapon quickly.
        bool isRapidShot =
            lastShotWeapon == weapon &&
            Time.time - lastShotTime <= rapidShotWindow;

        // The first shot after a pause or a weapon change is accurate.
        float shotSpread = isRapidShot
            ? Mathf.Min(currentSpread + spreadPerShot, weapon.spreadAngle)
            : 0f;

        Quaternion shootRotation = Quaternion.LookRotation(direction);

        // Punto aleatorio dentro de un círculo para desviar el disparo.
        Vector2 spread = Random.insideUnitCircle;
        // Use the accumulated spread instead of the weapon's maximum spread.
        float spreadRadius = Mathf.Tan(shotSpread * Mathf.Deg2Rad);

        Vector3 localDirection = new Vector3(
            spread.x * spreadRadius,
            spread.y * spreadRadius,
            1f
        ).normalized;

        // Convierte la desviación local a una dirección en el mundo.
        Vector3 finalDirection = shootRotation * localDirection;

        GameObject bulletObject = Instantiate(
            weapon.bulletPrefab,
            shootPoint.position,
            Quaternion.LookRotation(finalDirection)
        );

        bulletObject.GetComponent<BulletBasicDebug>().Damage = weapon.damage;

        inventory.ConsumeAmmo();

        // Update spread and cooldown only after a successful shot.
        currentSpread = shotSpread;
        lastShotTime = Time.time;
        lastShotWeapon = weapon;
        nextShotTime = Time.time + shotInterval;
    }
}