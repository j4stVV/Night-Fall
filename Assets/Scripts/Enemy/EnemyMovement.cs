using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Transform player;
    EnemyStats enemy;

    Vector2 knockbackVeclocity;
    float knockbackDuration;

    void Start()
    {
        enemy = GetComponent<EnemyStats>();
        player = FindObjectOfType<PlayerMovement>().transform;
    }

    void Update()
    {
        if (knockbackDuration > 0)
        {
            transform.position += (Vector3)knockbackVeclocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemy.currentMoveSpeed * Time.deltaTime);
        }
    }

    public void KnockBack(Vector2 veclocity, float duration)
    {
        if (knockbackDuration > 0) return;

        knockbackVeclocity = veclocity;
        knockbackDuration = duration;
    }
}
