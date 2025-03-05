using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LighningWeapon : ProjectileWeapon
{
    List<EnemyStats> allSelectedEnemies = new List<EnemyStats>();

    protected override bool Attack(int attackCount = 1)
    {
        if (!currentStats.hitEffect)
        {
            Debug.LogWarning(string.Format("Hit effect prefab has not been set for {0}", name));
            ActivateCooldown();
            return false;
        }

        if (!CanAttack())
        {
            return false;
        }

        if (currentCooldown <= 0)
        {
            allSelectedEnemies = new List<EnemyStats>(FindObjectsOfType<EnemyStats>());
            ActivateCooldown();
            currentAttackCount = attackCount;
        }

        Collider2D[] monstersColliders = GetMonsterColliders();
        Transform closestEnemy = GetClosestEnemy(monstersColliders);
        if (closestEnemy != null)
        {
            Instantiate(currentStats.hitEffect, closestEnemy.transform.position, Quaternion.identity);
            DamageArea(closestEnemy.transform.position, GetArea(), GetDamage());

        }
        if (attackCount > 0)
        {
            currentAttackCount = attackCount - 1;
            currentAttackInterval = currentStats.projectileInterval;
        }

        return true;
    }

    private Collider2D[] GetMonsterColliders()
    {
        Vector3 size = new Vector3(40, 30, 1);
        Collider2D[] colliders1 = Physics2D.OverlapBoxAll(owner.transform.position, size, 0f, LayerMask.GetMask("Monster"));

        Debug.Log("Detected Monsters: " + colliders1.Length);
        foreach (var collider in colliders1)
        {
            Debug.Log("Detected Monster: " + collider.name);
        }

        return colliders1;
    }

    Transform GetClosestEnemy(Collider2D[] monstersColliders)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector2 currentPos = owner.transform.position;

        foreach (Collider2D monsterCollider in monstersColliders)
        {
            float dist = Vector2.Distance(monsterCollider.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = monsterCollider.transform;
                minDist = dist;
            }
        }

        return tMin;
    }

    void DamageArea(Vector2 position, float radius, float damage)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D target in targets)
        {
            EnemyStats enemy = target.GetComponent<EnemyStats>();
            if (enemy)
            {
                enemy.TakeDamage(damage, transform.position);
            }
        }
    }
}
