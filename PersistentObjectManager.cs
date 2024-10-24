
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
    public int weaponType = -1; 
    
    public float playerHP = 100f;  
    public HPManager hpManager;    
    public string lastSceneName;  // משתנה לשמירת הסצנה האחרונה




    private HashSet<string> collectedItems = new HashSet<string>();  
    private HashSet<string> openDoors = new HashSet<string>();  
    private HashSet<string> openedContainers = new HashSet<string>();  

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
        hasSwordInHand = hasSword;
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

    // Setting weapon type
    public void SetWeaponType(int type)
    {
        weaponType = type;
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

    // פונקציה לקבלת שם הסצנה האחרונה
    public string GetLastScene()
    {
        return lastSceneName;
    }

	public void RespawnLife()
    {
        playerHP = 100f;  // החזרת חיים מלאים לשחקן
        UpdatePlayerHPUI();  // עדכון ה-UI כדי להציג את כמות החיים המחודשת
    }
    
}
