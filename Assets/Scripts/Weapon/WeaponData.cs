using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Night Fall/Weapon Data")]
public class WeaponData : ItemData
{
    [HideInInspector]
    public string behaviour;
    public Weapon.Stats baseStats;
    public Weapon.Stats[] linearGrowth;
    public Weapon.Stats[] randomGrowth;

    public Weapon.Stats GetLevelData(int level)
    {
        if (level - 2 < linearGrowth.Length)
        {
            return linearGrowth[level - 2];
        }
        if (randomGrowth.Length > 0)
        {
            return randomGrowth[Random.Range(0, randomGrowth.Length)];
        }

        Debug.LogWarning(string.Format("Weapon doesn't have its level up stats configured for level {0}!", level));
        return new Weapon.Stats();
    }
}
