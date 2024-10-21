using System.Collections.Generic;
using UnityEngine;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager instance = null;

    public bool hasSwordInHand = false;
    public bool hasSwordOnWall = true;
    public bool hasWeaponInHand = false; // משתנה חדש לתיאור נשק שאינו חרב
    public int weaponType = -1; // נשמר את סוג הנשק (0 - None, 1 - Sword, 2 - Axe, וכו')

    // אוספים לשמירת מצבי פריטים, דלתות ומיכלים שנפתחו
    private HashSet<string> collectedItems = new HashSet<string>();  
    private HashSet<string> openDoors = new HashSet<string>();  
    private HashSet<string> openedContainers = new HashSet<string>();  

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // שמירת האובייקט בין סצנות
        }
        else
        {
            Destroy(gameObject);  // הריסת עותק נוסף אם קיים
        }
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