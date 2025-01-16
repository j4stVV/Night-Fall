using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Stats")]
    public GameObject prefab;
    public float damage;
    public float speed;
    public float cooldownDuration;
    private float currentCooldown;
    public float pierce;

    protected PlayerMovement playerMovement;

    protected virtual void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        currentCooldown = cooldownDuration;
    }


    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0)
        {
            Attack();
        }
    }
    protected virtual void Attack()
    {
        currentCooldown = cooldownDuration; currentCooldown += Time.deltaTime;
    }
}
