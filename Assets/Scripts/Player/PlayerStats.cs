using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    CharacterScriptableObject characterData;

    //Current Stats
    float currentHealth;
    float currentMoveSpeed;
    float currentRecovery;
    float currentMight;
    float currentProjectileSpeed;
    float currentMagnet;

    public float CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            if (currentHealth != value)
            {
                currentHealth = value;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentHealthDisplay.text = "Health: " + currentHealth;
                }
            }
        }
    }
    public float CurrentRecovery
    {
        get { return currentRecovery; }
        set
        {
            if (currentRecovery != value)
            {
                currentRecovery = value;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentRecoveryDisplay.text = "Recovery: " + currentRecovery;
                }

            }
        }
    }
    public float CurrentMoveSpeed
    {
        get { return currentMoveSpeed; }
        set
        {
            if (currentMoveSpeed != value)
            {
                currentMoveSpeed = value;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentMoveSpeedDisplay.text = "Move Speed: " + currentMoveSpeed;
                }
            }
        }
    }
    public float CurrentMight
    {
        get { return currentMight; }
        set
        {
            if (currentMight != value)
            {
                currentMight = value;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentMightDisplay.text = "Might: " + currentMight;
                }
            }
        }
    }
    public float CurrentProjectileSpeed
    {
        get { return currentProjectileSpeed; }
        set
        {
            if (currentProjectileSpeed != value)
            {
                currentProjectileSpeed = value;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + currentProjectileSpeed;
                }
            }
        }
    }
    public float CurrentMagnet
    {
        get { return currentMagnet; }
        set
        {
            if (currentMagnet != value)
            {
                currentMagnet = value;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentMagnetDisplay.text = "Magnet: " + currentMagnet;
                }
            }
        }
    }

    //Experience and level of the player
    [Header("Experience/Level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap;

    [System.Serializable]
    public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int experienceCapIncrease;
    }
    
    public List<LevelRange> levelRanges;

    InventoryManager inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    public GameObject firstPassiveItem;
    public GameObject secondPassiveItem;
    public GameObject secondWeapon;

    //I-frames
    [Header("I-frames")]
    public float invicibilityDuration;
    float invicibilityTimer;
    bool isInvicible;
    void Awake()
    {
        characterData = CharacterSelector.GetData();
        CharacterSelector.Instance.DestroySingleton();

        inventory = GetComponent<InventoryManager>();

        //Assign the variables 
        CurrentHealth = characterData.MaxHealth;
        CurrentRecovery = characterData.Recovery;
        CurrentMight = characterData.Might;
        CurrentMoveSpeed = characterData.MoveSpeed;
        CurrentProjectileSpeed = characterData.ProjectileSpeed;
        CurrentMagnet = characterData.Magnet;

        //Spawn the starting weapons
        SpawnWeapon(characterData.StartingWeapon);
        SpawnWeapon(secondWeapon);
        SpawnPassiveItem(firstPassiveItem);
        SpawnPassiveItem(secondPassiveItem);
    }

    void Start()
    {
        experienceCap = levelRanges[0].experienceCapIncrease;

        GameManager.Instance.AssignChosenCharacterUI(characterData);
    }
    void Update()
    {
        if (invicibilityTimer > 0)
        {
            invicibilityTimer -= Time.deltaTime;
        }    
        else if (isInvicible) 
        {
            isInvicible = false;
        }

        Recover();
    }
    public void IncreaseExperience(int amount)
    {
        experience += amount;

        LevelUpChecker();
    }

    void LevelUpChecker()
    {
        if (experience >= experienceCap)
        {
            level++;
            experience -= experienceCap;

            int experienceCapIncrease = 0;
            foreach(LevelRange range in levelRanges)
            {
                if (level >= range.startLevel && level <= range.endLevel)
                {
                    experienceCapIncrease = range.experienceCapIncrease;
                    break;
                }
            }
            experienceCap += experienceCapIncrease;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isInvicible)
        {
            CurrentHealth -= damage;

            invicibilityTimer = invicibilityDuration;
            isInvicible = true;

            if (CurrentHealth <= 0)
            {
                Death();
            }
        }  
    }

    public void Death()
    {
        if (!GameManager.Instance.isGameOver)
        {
            GameManager.Instance.AssignLevelReachedUI(level);
            GameManager.Instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponUISlots, inventory.passiveItemUISlots);
            GameManager.Instance.GameOver();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += amount;
            
            if (CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }

    }

    void Recover()
    {
        if (CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += CurrentRecovery * Time.deltaTime;

            if (CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }

    public void SpawnWeapon(GameObject weapon)
    {
        if (weaponIndex >= inventory.weaponSlots.Count - 1)
        {
            Debug.LogError("Inventory slots already full");
            return;
        }

        //Spawn the starting weapon
        GameObject spawnedWeapon = Instantiate(weapon, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform); //set the weapon to a child of the player

        inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>());

        weaponIndex++;
    }public void SpawnPassiveItem(GameObject passiveItem)
    {
        if (passiveItemIndex >= inventory.passiveItemSlots.Count - 1)
        {
            Debug.LogError("Inventory slots already full");
            return;
        }

        //Spawn the starting weapon
        GameObject spawnedPassiveItem = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform); //set the weapon to a child of the player

        inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>());

        passiveItemIndex++;
    }
}
