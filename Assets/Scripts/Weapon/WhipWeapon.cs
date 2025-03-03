using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WhipWeapon : ProjectileWeapon
{
    int currenSpawnCount;
    float currentSpawnYOffset;

    protected override bool Attack(int attackCount = 1)
    {
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab has not been set for {0}", name));
            ActivateCooldown();
            return false;
        }

        if (!CanAttack())
        {
            return false;   
        }
        if (currentCooldown <= 0)
        {
            currenSpawnCount = 0;
            currentSpawnYOffset = 0;    
        }

        float spawnDir = Mathf.Sign(movement.lastMoveVector.x) * (currenSpawnCount % 2 != 0 ? -1 : 1);
        Vector2 spawnOffset = new Vector2(
            spawnDir * Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            currentSpawnYOffset);
        Projectile prefab = Instantiate(
            currentStats.projectilePrefab, owner.transform.position + (Vector3)spawnOffset,
            Quaternion.identity);
        prefab.owner = owner;

        if (spawnDir < 0)
        {
            prefab.transform.localScale = new Vector3(
                -Mathf.Abs(prefab.transform.localScale.x), 
                prefab.transform.localScale.y, 
                prefab.transform.localScale.z);
            Debug.Log(spawnDir + " | " + prefab.transform.localScale);
        }

        //Assign the stats
        prefab.weapon = this;
        ActivateCooldown();
        attackCount--;

        //Determine where the next projectile should spawn
        currenSpawnCount++;
        if (currenSpawnCount > 1 && currenSpawnCount % 2 == 0)
        {
            currentSpawnYOffset += 1;
        }

        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInterval;
        }

        return true;
    }
}
