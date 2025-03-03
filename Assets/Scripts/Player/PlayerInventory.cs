using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;
        public Image image;

        public void Assign(Item assignedItem)
        {
            item = assignedItem;
            if (item is Weapon)
            {
                Weapon weapon = item as Weapon;
                image.enabled = true;
                image.sprite = weapon.data.icon;
            }
            else
            {
                Passive passive = item as Passive;
                image.enabled = true;
                image.sprite= passive.data.icon;
            }
            Debug.Log(string.Format("Assigned {0} to player", item.name));
        }

        public void Clear()
        {
            item = null;
            image.enabled= false;
            image.sprite= null;
        }

        public bool IsEmpty() {  return item == null; }
    }

    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);

    [System.Serializable]
    public class UpgradeUI
    {
        public TextMeshProUGUI upgradeNameDisplay;
        public TextMeshProUGUI upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();
    public List<PassiveData> availablePassives = new List<PassiveData>();
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();    
    }

    //Check if the inventory has an item of a certaint type
    public bool Has(ItemData type) { return Get(type); }

    public Item Get(ItemData type)
    {
        if (type is WeaponData) return Get(type as WeaponData);
        else if (type is PassiveData) return Get(type as PassiveData);
        return null;
    }

    //Find a passive of a certaint type in the inventory
    public Passive Get(PassiveData type)
    {
        foreach (Slot s in passiveSlots)
        {
            Passive passive = s.item as Passive;
            if (passive && passive.data == type) return passive;
        }
        return null;
    }

    //Find a weapon of a certain type in the inventory
    public Weapon Get(WeaponData type)
    {
        foreach (Slot s in weaponSlots)
        {
            Weapon weapon = s.item as Weapon;
            if (weapon && weapon.data == type) return weapon;
        }
        return null;
    }

    //Remove a weapon of a particular type
    public bool Remove(WeaponData data, bool removeUpgradeAvailability = false)
    {
        //Remove this weapon from the upgrade pool
        if (removeUpgradeAvailability) availableWeapons.Remove(data);

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon weapon = weaponSlots[i].item as Weapon;
            if (weapon.data == data)
            {
                weaponSlots[i].Clear();
                weapon.OnUnequip();
                Destroy(weapon.gameObject);
                return true;
            }
        }

        return false;
    }

    //Remove a passive item of a particular type
    public bool Remove(PassiveData data, bool removeUpgradeAvailability = false)
    {
        if (removeUpgradeAvailability) availablePassives.Remove(data);

        for (int i = 0;i < passiveSlots.Count; i++)
        {
            Passive passive = passiveSlots[i].item as Passive;
            if (passive.data == data)
            {
                passiveSlots[i].Clear();    
                passive.OnUnequip();
                Destroy(passive.gameObject);
                return true;
            }
        }

        return false;
    }

    //If an ItemData is passed, determine what type it is and call the respective overload
    public bool Remove(ItemData data, bool removeUpgradeAvailability = false)
    {
        if (data is PassiveData) return Remove(data as PassiveData, removeUpgradeAvailability);
        else if (data is WeaponData) return Remove(data as WeaponData, removeUpgradeAvailability);
        return false;
    }

    //Find an empty slot and add a weapon of certain type, returns the slot number that the item was put in 
    public int Add(WeaponData data)
    {
        int slotNum = -1;

        for (int i = 0; i < weaponSlots.Capacity; i++)
        {
            if (weaponSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        //If there is no empty slot, exit
        if (slotNum < 0) return slotNum;

        //Otherwise create the weapon in the slot
        //Get the type of the weapon want to spawn
        Type weaponType = Type.GetType(data.behaviour);

        if (weaponType != null)
        {
            GameObject go = new GameObject(data.baseStats.name + "Cotroller");
            Weapon spawnedWeapon = (Weapon)go.AddComponent(weaponType);
            spawnedWeapon.transform.SetParent(transform);
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.Initialise(data); 
            spawnedWeapon.OnEquip();

            //Assign the weapn to the slot
            weaponSlots[slotNum].Assign(spawnedWeapon);

            //Close the level up UI if it is on
            if (GameManager.Instance != null && GameManager.Instance.choosingUpgrade)
            {
                GameManager.Instance.EndLevelUp();
            }
            return slotNum;
        }
        else
        {
            Debug.LogWarning(string.Format("Invalid weapon type specified for {0}", data.name));
        }
        return -1;
    }

    //Find an empty slot and add a passive of certain type, return the slot number that item was put in
    public int Add(PassiveData data)
    {
        int slotNum = -1;

        for (int i = 0; i < passiveSlots.Capacity; i++)
        {
            if (passiveSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        //If there is no empty slot, exit
        if (slotNum < 0) return slotNum;

        //Otherwise create the passive in the slot
        GameObject go = new GameObject(data.baseStats.name + "Passive");
        Passive passive = go.AddComponent<Passive>();
        passive.Initialise(data);
        passive.transform.SetParent(transform);
        passive.transform.localPosition = Vector2.zero;

        //Assign the passive to the slot
        passiveSlots[slotNum].Assign(passive);

        if (GameManager.Instance != null && GameManager.Instance.choosingUpgrade)
        {
            GameManager.Instance.EndLevelUp();
        }
        player.RecalculateStats();

        return slotNum;
    }

    public int Add(ItemData data)
    {
        if (data is WeaponData) return Add(data as WeaponData);
        else if (data is PassiveData) return Add(data as PassiveData);
        return -1;
    }

    // Overload so that we can use both ItemData or Item to level up an
    // item in the inventory.
    public bool LevelUp(ItemData data)
    {
        Item item = Get(data);
        if (item) return LevelUp(item);
        return false;
    }

    // Levels up a selected weapon in the player inventory.
    public bool LevelUp(Item item)
    {
        // Tries to level up the item.
        if (!item.DoLevelUp())
        {
            Debug.LogWarning(string.Format(
                "Failed to level up {0}.",
                 item.name
            ));
            return false;
        }

        // Close the level up screen afterwards.
        if (GameManager.Instance != null && GameManager.Instance.choosingUpgrade)
        {
            GameManager.Instance.EndLevelUp();
        }

        // If it is a passive, recalculate player stats.
        if (item is Passive) player.RecalculateStats();
        return true;
    }

    // Checks a list of slots to see if there are any slots left.
    int GetSlotsLeft(List<Slot> slots)
    {

        int count = 0;
        foreach (Slot s in slots)
        {
            if (s.IsEmpty()) count++;
        }
        return count;
    }


    //Determine what upgrade options should appear
    // Determines what upgrade options should appear.
    void ApplyUpgradeOptions()
    {
        // <availableUpgrades> is the list of possible upgrades that we will populate from
        // <allPossibleUpgrades>, which is a list of all available weapons and passives.
        List<ItemData> availableUpgrades = new List<ItemData>(availableWeapons.Count + availablePassives.Count);
        List<ItemData> allPossibleUpgrades = new List<ItemData>(availableWeapons);
        allPossibleUpgrades.AddRange(availablePassives);

        // We need to know how many weapon / passive slots are left.
        int weaponSlotsLeft = GetSlotsLeft(weaponSlots);
        int passiveSlotsLeft = GetSlotsLeft(passiveSlots);

        // Filters through the available weapons and passives and add those
        // that can possibly be an option.
        foreach (ItemData data in allPossibleUpgrades)
        {
            // If a weapon of this type exists, allow for the upgrade if the
            // level of the weapon is not already maxed out.
            Item obj = Get(data);
            if (obj)
            {
                if (obj.currentLevel < data.maxLevel) availableUpgrades.Add(data);
            }
            else
            {
                // If we don't have this item in the inventory yet, check if
                // we still have enough slots to take new items.
                if (data is WeaponData && weaponSlotsLeft > 0) availableUpgrades.Add(data);
                else if (data is PassiveData && passiveSlotsLeft > 0) availableUpgrades.Add(data);
            }
        }

        // Iterate through each slot in the upgrade UI and populate the options.
        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            // If there are no more available upgrades, then we abort.
            if (availableUpgrades.Count <= 0) return;

            // Pick an upgrade, then remove it so that we don't get it twice.
            ItemData chosenUpgrade = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Count)];
            availableUpgrades.Remove(chosenUpgrade);

            // Ensure that the selected weapon data is valid.
            if (chosenUpgrade != null)
            {
                // Turns on the UI slot.
                EnableUpgradeUI(upgradeOption);

                // If our inventory already has the upgrade, we will make it a level up.
                Item item = Get(chosenUpgrade);
                if (item)
                {
                    upgradeOption.upgradeButton.onClick.AddListener(() => LevelUp(item)); //Apply button functionality
                    if (item is Weapon)
                    {
                        Weapon.Stats nextLevel = ((WeaponData)chosenUpgrade).GetLevelData(item.currentLevel + 1);
                        upgradeOption.upgradeDescriptionDisplay.text = nextLevel.description;
                        upgradeOption.upgradeNameDisplay.text = chosenUpgrade.name + " - " + nextLevel.name;
                        upgradeOption.upgradeIcon.sprite = chosenUpgrade.icon;
                    }
                    else
                    {
                        Passive.Modifier nextLevel = ((PassiveData)chosenUpgrade).GetLevelData(item.currentLevel + 1);
                        upgradeOption.upgradeDescriptionDisplay.text = nextLevel.description;
                        upgradeOption.upgradeNameDisplay.text = chosenUpgrade.name + " - " + nextLevel.name;
                        upgradeOption.upgradeIcon.sprite = chosenUpgrade.icon;
                    }
                }
                else
                {
                    if (chosenUpgrade is WeaponData)
                    {
                        WeaponData data = chosenUpgrade as WeaponData;
                        upgradeOption.upgradeButton.onClick.AddListener(() => Add(chosenUpgrade)); //Apply button functionality
                        upgradeOption.upgradeDescriptionDisplay.text = data.baseStats.description;  //Apply initial description
                        upgradeOption.upgradeNameDisplay.text = data.baseStats.name;    //Apply initial name
                        upgradeOption.upgradeIcon.sprite = data.icon;
                    }
                    else
                    {
                        PassiveData data = chosenUpgrade as PassiveData;
                        upgradeOption.upgradeButton.onClick.AddListener(() => Add(chosenUpgrade)); //Apply button functionality
                        upgradeOption.upgradeDescriptionDisplay.text = data.baseStats.description;  //Apply initial description
                        upgradeOption.upgradeNameDisplay.text = data.baseStats.name;    //Apply initial name
                        upgradeOption.upgradeIcon.sprite = data.icon;
                    }

                }
            }
        }
    }

    void RemoveUpgradeOptions()
    {
        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        RemoveUpgradeOptions();
        ApplyUpgradeOptions();
    } 

    void DisableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }

    void EnableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }
}
