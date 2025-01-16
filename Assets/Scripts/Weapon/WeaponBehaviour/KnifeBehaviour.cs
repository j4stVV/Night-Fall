using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeBehaviour : ProjectileWeaponBehaviour
{
    private KnifeController knifeController;

    protected override void Start()
    {
        base.Start();
        knifeController = FindObjectOfType<KnifeController>();  
    }

    void Update()
    {
        transform.position += direction * knifeController.speed * Time.deltaTime;         
    }
}
