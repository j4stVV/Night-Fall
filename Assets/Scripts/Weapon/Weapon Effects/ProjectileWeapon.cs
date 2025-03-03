using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    protected float currentAttackInterval;
    protected int currentAttackCount;

    protected override void Update()
    {
        base.Update();
        
        if (currentAttackInterval > 0 )
        {
            currentAttackInterval -= Time.deltaTime;
            if (currentAttackInterval <= 0)
            {
                Attack(currentAttackCount);
            }
        }
    }

    public override bool CanAttack()
    {
        if (currentAttackCount > 0) return true;
        return base.CanAttack();
    }

    protected override bool Attack(int attackCount = 1)
    {
        //If no projectile prefab is assigned, leave a warning message
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prebab has not been set for {0}", name));
            ActivateCooldown();
            return false;
        }

        // Can attack ?
        if (!CanAttack()) return false;

        //Otherwise, calculate the angle and offset of spawned projectile
        float spawnAngle = GetSpawnAngle();

        //And spawn a copy of the projectile
        Projectile prefab = Instantiate(
            currentStats.projectilePrefab,
            owner.transform.position + (Vector3)GetSpawnOffset(spawnAngle),
            Quaternion.Euler(0, 0, spawnAngle)
        );

        prefab.weapon = this;
        prefab.owner = owner;

        //reset the cooldown only if this attack was triggered by cooldown
        ActivateCooldown();
        attackCount--;

        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInterval;
        }

        return true;
    }

    //Get which direction the projectile should face when spawning
    protected virtual float GetSpawnAngle()
    {
        return Mathf.Atan2(movement.lastMoveVector.y, movement.lastMoveVector.x) * Mathf.Rad2Deg;
    }

    //Generate a random point to spawn the projectile on and rotate the facing of the point by spawnAngle
    protected virtual Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        return Quaternion.Euler(0, 0, spawnAngle) * new Vector2(
            Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            Random.Range(currentStats.spawnVariance.yMin, currentStats.spawnVariance.yMax)
        );
    }
}
