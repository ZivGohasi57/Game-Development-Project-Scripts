using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager instance = null;

    // Existing variables
    public bool hasSwordInHand = false;
    public bool hasSwordOnWall = true;
    public bool hasWeaponInHand = false;
    public int weaponType = -1; // 0 - אגרופים, 1 - חרב 
    
    public float playerHP = 100f;  
    public HPManager hpManager;    
    public string lastSceneName;  // משתנה לשמירת הסצנה האחרונה
	public WeaponUIManager weaponCanvasManager;  // קנבס לנשק

    private HashSet<string> deadEnemies = new HashSet<string>(); 
    private HashSet<string> collectedItems = new HashSet<string>();  
    private HashSet<string> openDoors = new HashSet<string>();  
    private HashSet<string> openedContainers = new HashSet<string>();  
    public HashSet<string> collectedWeapons = new HashSet<string>();  // נשקים שנאספו
	public bool hasFists = false;  // האם אגרופים נאספו
    public bool hasSword = false;  // האם החרב נאספה

    // Variables for mission management
    public int currentMissionIndex = 0;  // Tracks the current mission index
    public List<string> missions = new List<string>  // List of missions
    {
        "Go to the bar and meet one of the castle's guards.",
        "Go to your house with the purple roof and disguise yourself as one of the castle's guards.",
        "Go to the castle.",
        "Search for the treasure notes 0 of 4.",
        "Search for the treasure notes 1 of 4.",
        "Search for the treasure notes 2 of 4.",
        "Search for the treasure notes 3 of 4.",
        "Find the well around the lake and follow its path.",
        "Find the weapon (The Blue Way!)"
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Preserve this object between scenes
        }
        else
        {
            Destroy(gameObject);  // Destroys duplicate instances if any
        }
    }

    // Function to advance to the next mission
    public void AdvanceMission()
    {
        if (currentMissionIndex < missions.Count - 1)  // Ensures we don't exceed the mission list
        {
            currentMissionIndex++;
        }
        else
        {
            Debug.Log("All missions completed!");
        }
    }

    // Function to get the current mission text
    public string GetCurrentMissionText()
    {
        return missions[currentMissionIndex];
    }

    // Function to update the UI of the mission after scene load
    public void UpdateMissionUI(MissionManager missionManager)
    {
        if (missionManager != null)
        {
            missionManager.UpdateMissionByIndex(currentMissionIndex);
        }
    }

    // Function to handle mission update on scene load
    public void OnSceneLoaded(MissionManager missionManager)
    {
        UpdateMissionUI(missionManager);
    }

    // HP Management functions
    public void UpdatePlayerHPUI()
    {
        if (hpManager != null)
        {
            hpManager.SetHP(playerHP);  // Update the UI with the current player HP
        }
    }

    public void SetPlayerHP(float hp)
    {
        playerHP = hp;

        // Update HP in the HPManager
        if (hpManager != null)
        {
            hpManager.SetHP(playerHP);  // Use SetHP to update the UI with the new value
        }
    }

    public float GetPlayerHP()
    {
        return playerHP;
    }
 
    // Managing sword state
    public void SetHasSword(bool hasSword)
    {
        this.hasSword = hasSword;
        weaponCanvasManager?.UpdateWeaponUI();
    }

    public void SetHasSwordOnWall(bool hasSword)
    {
        hasSwordOnWall = hasSword;
    }

    // Managing other weapon state
    public void SetHasWeapon(bool hasWeapon)
    {
        hasWeaponInHand = hasWeapon;
    }

    // Setting and getting the current weapon type
    public void SetWeaponType(int type)
    {
        weaponType = type;

        // אם קנבס הנשק קיים, עדכן את ה-UI של הנשק
        if (weaponCanvasManager != null)
        {
            weaponCanvasManager.UpdateWeaponUI();
        }
    }

    public int GetWeaponType()
    {
        return weaponType;
    }

    // Managing collected weapons
    public void AddWeapon(string weaponName)
    {
        if (!collectedWeapons.Contains(weaponName))
        {
            collectedWeapons.Add(weaponName);
            Debug.Log($"Collected weapon: {weaponName}");
        }
    }

    public bool HasCollectedWeapon(string weaponName)
    {
        return collectedWeapons.Contains(weaponName);
    }

    // Checking and saving collected items
    public bool HasItem(string itemName)
    {
        return collectedItems.Contains(itemName);
    }

    public void CollectItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
        }
    }

    // Saving and checking door open state
    public void SetDoorOpen(string doorId)
    {
        if (!openDoors.Contains(doorId))
        {
            openDoors.Add(doorId);
        }
    }

    public bool IsDoorOpen(string doorId)
    {
        return openDoors.Contains(doorId);
    }

    // Saving and checking container open state
    public void SetContainerOpen(string containerId)
    {
        if (!openedContainers.Contains(containerId))
        {
            openedContainers.Add(containerId);
        }
    }

    public bool IsContainerOpen(string containerId)
    {
        return openedContainers.Contains(containerId);
    }

    public void SetLastScene(string sceneName)
    {
        lastSceneName = sceneName;
    }

    // Get the last scene name
    public string GetLastScene()
    {
        return lastSceneName;
    }

    public void RespawnLife()
    {
        playerHP = 100f;  // Restore player's health to full
        UpdatePlayerHPUI();  // Update UI to reflect the renewed health
    }

    // Function to mark an enemy as dead
    public void SetEnemyDead(string enemyId)
    {
        if (!deadEnemies.Contains(enemyId))
        {
            deadEnemies.Add(enemyId);
        }
    }

    // Function to check if an enemy is dead
    public bool IsEnemyDead(string enemyId)
    {
        return deadEnemies.Contains(enemyId);
    }

    // Save the current weapon state (can be used before scene transitions)
    public void SaveWeaponState(int currentWeaponType)
    {
        SetWeaponType(currentWeaponType);
    }

    // Load the current weapon state after transitioning to a new scene
    public void LoadWeaponState()
    {
        int loadedWeaponType = GetWeaponType();
        Debug.Log($"Loaded weapon type: {loadedWeaponType}");
    }
	

    public void SetWeaponCanvasManager(WeaponUIManager manager)
    {
        weaponCanvasManager = manager;
        weaponCanvasManager.UpdateWeaponUI();
    }


	public void SetHasFists(bool hasFists)
    {
        this.hasFists = hasFists;
        weaponCanvasManager?.UpdateWeaponUI();
    }

    
}