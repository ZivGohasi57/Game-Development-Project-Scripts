using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager instance = null;
    public bool hasSwordInHand = false;
    public bool hasSwordOnWall = true;
    public bool hasWeaponInHand = false;
    public int weaponType = -1;
    public MissionManager missionManager;
    public float playerHP = 100f;  
    public HPManager hpManager;    
    public string lastSceneName; 
    public WeaponUIManager weaponCanvasManager;
    private HashSet<string> deadEnemies = new HashSet<string>(); 
    private HashSet<string> collectedItems = new HashSet<string>();  
    private HashSet<string> openDoors = new HashSet<string>();  
    private HashSet<string> openedContainers = new HashSet<string>();  
    public HashSet<string> collectedWeapons = new HashSet<string>(); 
    public bool hasFists = false; 
    public bool hasSword = false; 
    public bool returningFromScene2 = false;


    public int currentMissionIndex = 0; 
    public List<string> missions = new List<string> 
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetReturningFromScene2(bool returning)
    {
        returningFromScene2 = returning;
    }

    
    public void AdvanceMission()
    {
		if (missionManager != null)
		{
        	missionManager.AdvanceMission();
		}
    }

    public string GetCurrentMissionText()
    {
        return missions[currentMissionIndex];
    }

    public void UpdateMissionUI(MissionManager missionManager)
    {
        if (missionManager != null)
        {
            missionManager.UpdateMissionByIndex(currentMissionIndex);
        }
    }

    public void OnSceneLoaded(MissionManager missionManager)
    {
        UpdateMissionUI(missionManager);
    }

    public void UpdatePlayerHPUI()
    {
        if (hpManager != null)
        {
            hpManager.SetHP(playerHP); 
        }
    }

    public void SetPlayerHP(float hp)
    {
        playerHP = hp;

        if (hpManager != null)
        {
            hpManager.SetHP(playerHP); 
        }
    }

    public float GetPlayerHP()
    {
        return playerHP;
    }
 
    public void SetHasSword(bool hasSword)
    {
        this.hasSword = hasSword;
        this.hasSwordInHand = hasSword;
        weaponCanvasManager?.UpdateWeaponUI();
        
    }

    public void SetHasSwordOnWall(bool hasSword)
    {
        hasSwordOnWall = hasSword;
    }

    public void SetHasWeapon(bool hasWeapon)
    {
        hasWeaponInHand = hasWeapon;
    }

    public void SetWeaponType(int type)
    {
        weaponType = type;

        if (weaponCanvasManager != null)
        {
            weaponCanvasManager.UpdateWeaponUI();
        }
    }

    public int GetWeaponType()
    {
        return weaponType;
    }

    public void AddWeapon(string weaponName)
    {
        if (!collectedWeapons.Contains(weaponName))
        {
            collectedWeapons.Add(weaponName);
        }
    }

    public bool HasCollectedWeapon(string weaponName)
    {
        return collectedWeapons.Contains(weaponName);
    }

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

    public string GetLastScene()
    {
        return lastSceneName;
    }

    public void RespawnLife()
    {
        playerHP = 30f; 
        UpdatePlayerHPUI(); 
    }

    public void SetEnemyDead(string enemyId)
    {
        if (!deadEnemies.Contains(enemyId))
        {
            deadEnemies.Add(enemyId);
        }
    }

    public bool IsEnemyDead(string enemyId)
    {
        return deadEnemies.Contains(enemyId);
    }

    public void SaveWeaponState(int currentWeaponType)
    {
        SetWeaponType(currentWeaponType);
    }

    public void LoadWeaponState()
    {
        int loadedWeaponType = GetWeaponType();
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

	 public void ClearData()
    {
        hasSwordInHand = false;
        hasSwordOnWall = true;
        hasWeaponInHand = false;
        weaponType = -1;
        playerHP = 100f;
        lastSceneName = string.Empty;
        deadEnemies.Clear();
        collectedItems.Clear();
        openDoors.Clear();
        openedContainers.Clear();
        collectedWeapons.Clear();
    }
}
