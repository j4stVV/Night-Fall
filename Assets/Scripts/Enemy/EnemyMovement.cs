using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    protected Transform player;
    protected EnemyStats enemy;

    protected Vector2 knockbackVeclocity;
    protected float knockbackDuration;

    public enum OutOffFrameAction { none, respawnAtEdge, despawn }
    public OutOffFrameAction outOffFrameAction = OutOffFrameAction.respawnAtEdge;

    protected bool spawnedOutOffFrame = false;  

    protected virtual void Start()
    {
        spawnedOutOffFrame = !SpawnManager.IsWithinBoundaries(transform);
        enemy = GetComponent<EnemyStats>();

        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
    }

    protected virtual void Update()
    {
        if (knockbackDuration > 0)
        {
            transform.position += (Vector3)knockbackVeclocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            Move();
            HandleOutOffFrameAction();
        }
    }

    protected virtual void HandleOutOffFrameAction()
    {
        if (!SpawnManager.IsWithinBoundaries(transform))
        {
            switch (outOffFrameAction)
            {
                case OutOffFrameAction.none: default:
                    break;
                case OutOffFrameAction.respawnAtEdge:
                    transform.position = SpawnManager.GeneratePosition();
                    break;
                case OutOffFrameAction.despawn:
                    if (!spawnedOutOffFrame) Destroy(gameObject);
                    break;
            }
        }
        else
        {
            spawnedOutOffFrame = false;
        }
    }

    public virtual void KnockBack(Vector2 veclocity, float duration)
    {
        if (knockbackDuration > 0) return;

        knockbackVeclocity = veclocity;
        knockbackDuration = duration;
    }

    public virtual void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemy.currentMoveSpeed * Time.deltaTime);
    }
}
