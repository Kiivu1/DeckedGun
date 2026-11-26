using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryCard inventory;
    [SerializeField] private Camera playerCamera;

    [Header("Muzzle Points")]
    [SerializeField] private Transform pistolShootPoint;
    [SerializeField] private Transform shotgunShootPoint;

    [Header("Shooting")]
    [SerializeField, Min(0f)] private float shotInterval = 0.2f;
    [SerializeField, Min(1f)] private float aimDistance = 100f;
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Spread")]
    // Maximum time between shots to consider them part of a rapid sequence.
    [SerializeField, Min(0f)] private float rapidShotWindow = 0.4f;

    // Spread added by each consecutive rapid shot, in degrees.
    [SerializeField, Min(0f)] private float spreadPerShot = 0.5f;

    [Header("Weapon Models")]
    [SerializeField] private GameObject pistolModel;
    [SerializeField] private GameObject shotgunModel;

    // Track the previous successful shot and its accumulated spread.
    private float lastShotTime = float.NegativeInfinity;
    private float currentSpread = 0f;
    private CardWeapon lastShotWeapon;

    private float nextShotTime;

    void Update()
    {
        // Keep the visible model synchronized with the selected weapon.
        UpdateWeaponModel();

        // La tienda desactiva InventoryCard, incluso si no pausa el juego.
        if (inventory == null || !inventory.isActiveAndEnabled || Time.timeScale == 0f)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryShoot();
    }

    void UpdateWeaponModel(){
        CardWeapon weapon = inventory != null ? inventory.GetCurrentWeapon(): null;

        bool showPistol = weapon != null && weapon.weaponType == WeaponType.Pistol;

        bool showShotgun = weapon != null && weapon.weaponType == WeaponType.Shotgun;

        // Only change visibility when necessary.
        if (pistolModel != null && pistolModel.activeSelf != showPistol)
        {
            pistolModel.SetActive(showPistol);
        }

        if (shotgunModel != null && shotgunModel.activeSelf != showShotgun)
        {
            shotgunModel.SetActive(showShotgun);
        }
    }

   void TryShoot()
    {
        if (Time.time < nextShotTime) return;

        // Get the currently selected weapon.
        CardWeapon weapon = inventory.GetCurrentWeapon();
        if (weapon == null || weapon.bulletPrefab == null) return;

        // Select the muzzle associated with the weapon type.
        Transform shootPoint = weapon.weaponType == WeaponType.Shotgun
            ? shotgunShootPoint
            : pistolShootPoint;

        // Check the required references before creating projectiles.
        if (shootPoint == null || playerCamera == null)
        {
            Debug.LogWarning(
                "Assign the selected weapon's muzzle point and Player Camera.",this);
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

        // Combine base spread with the additional rapid-fire spread.
        float totalSpread = weapon.baseSpreadAngle + shotSpread;
        float spreadRadius = Mathf.Tan(totalSpread * Mathf.Deg2Rad);

        Quaternion shootRotation = Quaternion.LookRotation(direction);

        // Spawn at least one projectile per shot.
        int projectileCount = Mathf.Max(1, weapon.projectilesPerShot);

        for (int i = 0; i < projectileCount; i++){
            // Choose an independent random offset for each projectile..
            Vector2 spread = Random.insideUnitCircle;

            Vector3 localDirection = new Vector3(
                spread.x * spreadRadius,
                spread.y * spreadRadius,
                1f
            ).normalized;

            // Convert the local projectile direction into world space.
            Vector3 worldDirection = shootRotation * localDirection;

            GameObject bulletObject = Instantiate(
                weapon.bulletPrefab,
                shootPoint.position,
                Quaternion.LookRotation(worldDirection));

                bulletObject.GetComponent<BulletBasicDebug>().Damage = weapon.damage;
        }

        
        //consume one ammo per shot
        inventory.ConsumeAmmo();

        // Update spread and cooldown only after a successful shot.
        currentSpread = shotSpread;
        lastShotTime = Time.time;
        lastShotWeapon = weapon;
        nextShotTime = Time.time + shotInterval;
    }
}