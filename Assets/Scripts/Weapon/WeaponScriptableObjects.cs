using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponScriptableObjects", menuName = "ScriptableObjects/Weapon")]
public class WeaponScriptableObjects : ScriptableObject
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab { get => prefab; private set => prefab = value; }
    //Base stats for weapons
    [SerializeField] private float damage;
    public float Damage { get =>  damage; private set => damage = value; } 

    [SerializeField] private float speed;
    public float Speed { get =>  speed; private set => speed = value; }

    [SerializeField] private float cooldownDuration;
    public float CooldownDuration { get => cooldownDuration; private set => cooldownDuration = value; }

    [SerializeField] private int pierce;
    public int Pierce { get => pierce; private set => pierce = value; }

    [SerializeField] private int level;
    public int Level { get => level; private set => level = value; }

    [SerializeField] private GameObject nextLevelPrefab;
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField] private Sprite icon;
    public Sprite Icon { get => icon; private set => icon = value; }
}
