using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveItemScriptableObject", menuName = "ScriptableObjects/Passive Item")]
public class PassiveItemScriptableObjects : ScriptableObject
{
    [SerializeField] private float multipler;
    public float Multipler { get =>  multipler; private set => multipler = value;}

    [SerializeField] private int level;
    public int Level { get => level; private set => level = value; }

    [SerializeField] private GameObject nextLevelPrefab;
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField] private Sprite icon;
    public Sprite Icon { get => icon; private set => icon = value; }
}
