using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStats : MonoBehaviour
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] private CharacterData.Stats actualStats;

    public CharacterData.Stats Stats
    {
        get { return actualStats; }
        set { actualStats = value; }
    }

    float health;

    public float CurrentHealth
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = value;
                UpdateHealthBar();
            }
        }
    }

    [Header("Visual")]
    public ParticleSystem damageEffect;
    //public ParticleSystem blockEffect;

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

    PlayerCollector collector;

    PlayerInventory inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    //I-frames
    [Header("I-frames")]
    public float invicibilityDuration;
    float invicibilityTimer;
    bool isInvicible;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TextMeshProUGUI levelText;

    void Awake()
    {
        characterData = UICharacterSelector.GetData();

        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();

        baseStats = actualStats = characterData.stats;
        collector.SetRadius(actualStats.magnet);
        health = actualStats.maxHealth;
    }

    void Start()
    {
        //Spawn the starting weapon
        inventory.Add(characterData.StartingWeapon);

        //Initialize the exp cap as the first exp cap increase
        experienceCap = levelRanges[0].experienceCapIncrease;

        GameManager.Instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
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

    public void RecalculateStats()
    {
        actualStats = baseStats;
        foreach (PlayerInventory.Slot slot in inventory.passiveSlots)
        {
            Passive passive = slot.item as Passive;
            if (passive)
            {
                actualStats += passive.GetBoosts();
            }
        }
        collector.SetRadius(actualStats.magnet);
    }

    public void IncreaseExperience(int amount)
    {
        experience += amount;

        LevelUpChecker();

        UpdateExpBar();
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

            UpdateLevelText();

            GameManager.Instance.StartLevelUp();

            if (experience >= experienceCap)
                LevelUpChecker();
        }
    }

    void UpdateExpBar()
    {
        expBar.fillAmount = (float)experience / experienceCap;
    }

    void UpdateLevelText()
    {
        levelText.text = "LV" + level.ToString();
    }

    public void TakeDamage(float damage)
    {
        if (!isInvicible)
        {
            //Take armor into account before dealing damage
            damage -= actualStats.armor;

            if (damage > 0)
            {
                //Deals damage
                CurrentHealth -= damage;

                //If there is a damage effect assigned, play it
                if (damageEffect) Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);

                if (CurrentHealth <= 0)
                {
                    Death();
                }
            }
            /*else
            {
                if (blockEffect) Destroy(Instantiate(blockEffect, transform.position, Quaternion.identity), 5f);
            }*/
            invicibilityTimer = invicibilityDuration;
            isInvicible = true;
            UpdateHealthBar();
        }  
    }

    void UpdateHealthBar()
    {
        healthBar.fillAmount = CurrentHealth / actualStats.maxHealth;
    }
    public void Death()
    {
        if (!GameManager.Instance.isGameOver)
        {
            GameManager.Instance.AssignLevelReachedUI(level);
            GameManager.Instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponSlots, inventory.passiveSlots);
            GameManager.Instance.GameOver();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += amount;
            
            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }

    void Recover()
    {
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += Stats.recovery * Time.deltaTime;
            //CurrentHealth += Recovery * Time.deltaTime;

            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }
}