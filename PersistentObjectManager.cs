using System.Collections.Generic;
using UnityEngine;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager instance = null;

    public bool hasSwordInHand = false;
    public bool hasSwordOnWall = true;
    public bool hasWeaponInHand = false;
    public int weaponType = -1; 
    
    public float playerHP = 100f;  // שמירת HP של השחקן
    public HPManager hpManager;    // רפרנס ל-HPManager שאחראי על עדכון הקנבס

    private HashSet<string> collectedItems = new HashSet<string>();  
    private HashSet<string> openDoors = new HashSet<string>();  
    private HashSet<string> openedContainers = new HashSet<string>();  

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // שומר את האובייקט הזה בין סצנות
        }
        else
        {
            Destroy(gameObject);  // הורסת עותקים כפולים אם יש
        }
    }

    // פונקציה לעדכון HP דרך ה-HPManager
    public void UpdatePlayerHPUI()
    {
        if (hpManager != null)
        {
            hpManager.SetHP(playerHP);  // עדכון ה-UI עם ה-HP הנוכחי של השחקן
        }
    }

    // עדכון HP של השחקן
    public void SetPlayerHP(float hp)
    {
        playerHP = hp;

        // עדכון ה-HP ב-HPManager
        if (hpManager != null)
        {
            hpManager.SetHP(playerHP);  // שים לב לשימוש ב-SetHP כדי לעדכן את ה-UI עם הערך החדש
        }
    }

    public float GetPlayerHP()
    {
        return playerHP;
    }

    // ניהול מצב החרב
    public void SetHasSword(bool hasSword)
    {
        hasSwordInHand = hasSword;
    }

    public void SetHasSwordOnWall(bool hasSword)
    {
        hasSwordOnWall = hasSword;
    }

    // ניהול מצב נשק אחר
    public void SetHasWeapon(bool hasWeapon)
    {
        hasWeaponInHand = hasWeapon;
    }

    // הגדרת סוג הנשק
    public void SetWeaponType(int type)
    {
        weaponType = type;
    }

    // בדיקת פריט שנאסף ושמירתו
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

    // שמירת מצב פתיחת דלת ובדיקת מצב
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

    // שמירת מצב מיכל (תיבה/חבית) שנפתח ובדיקתו
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
}