using UnityEngine;
using UnityEngine.UI;

public class Key : MonoBehaviour
{
    public enum ItemType { Key, Gold, Weapon }
    public enum WeaponType { None, Sword, Axe, Bow }
    public enum ContainerType { Chest, Jar }

    public ItemType itemType;
    public WeaponType weaponType = WeaponType.None;
    public ContainerType containerType = ContainerType.Chest;

    public Door linkedDoor;
    public Text interactionText;

    [Header("Messages")]
    public string messageOpenChest = "Press E to open chest";
    public string messageTakeKey = "Press E to take the key";
    public string messageTakeGold = "Press E to take the gold";
    public string messageTakeWeapon = "Press E to take the weapon";

    public int goldAmount = 10;
    public Animator chestAnimator;
    public Animator playerAnimator;

    // משתני סאונד
    public AudioClip takeKeySound;      // סאונד ללקיחת מפתח
    public AudioClip takeGoldSound;     // סאונד ללקיחת זהב
    public AudioClip openChestSound;    // סאונד לפתיחת תיבה
    private AudioSource audioSource;     // מקור הסאונד

    private bool isInRange = false;
    private bool chestOpened = false;
    private bool itemAvailable = false;
    private string generatedItemId;

    void Start()
    {
        generatedItemId = $"{gameObject.name}_{transform.position}";

        // קבלת רכיב AudioSource מההורה
        audioSource = transform.parent.GetComponent<AudioSource>();

        // בדיקה אם התיבה כבר נפתחה
        if (PersistentObjectManager.instance != null &&
            PersistentObjectManager.instance.IsContainerOpen(generatedItemId))
        {
            SetChestOpenedState();  // השארת התיבה פתוחה
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (containerType == ContainerType.Chest && !chestOpened)
            {
                OpenChest();
            }
            else if (itemAvailable || containerType == ContainerType.Jar)
            {
                CollectItem();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;

            if (containerType == ContainerType.Chest && !chestOpened)
            {
                ShowInteractionText(messageOpenChest);
            }
            else if (containerType == ContainerType.Jar)
            {
                ShowInteractionText(GetPickupMessage());
                itemAvailable = true;
            }

            if (playerAnimator == null)
            {
                playerAnimator = other.GetComponent<Animator>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            interactionText.gameObject.SetActive(false);
        }
    }

    void ShowInteractionText(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
            interactionText.gameObject.SetActive(true);
        }
    }

    string GetPickupMessage()
    {
        return itemType switch
        {
            ItemType.Key => messageTakeKey,
            ItemType.Gold => messageTakeGold,
            ItemType.Weapon => messageTakeWeapon,
            _ => ""
        };
    }

    void OpenChest()
    {
        chestOpened = true;
        chestAnimator.SetTrigger("Open");

        // השמעת סאונד לפתיחת התיבה
        if (audioSource != null && openChestSound != null)
        {
            audioSource.PlayOneShot(openChestSound);
        }

        PersistentObjectManager.instance?.SetContainerOpen(generatedItemId);

        float animationLength = chestAnimator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(EnableItemPickup), animationLength);
    }

    void EnableItemPickup()
    {
        itemAvailable = true;
        ShowInteractionText(GetPickupMessage());
    }

    void CollectItem()
    {
        switch (itemType)
        {
            case ItemType.Key:
                if (linkedDoor != null)
                {
                    linkedDoor.hasKey = true;
                    Debug.Log("המפתח נאסף! הדלת עודכנה.");
    
                    // השמעת סאונד ללקיחת מפתח דרך ההורה
                    if (audioSource != null && takeKeySound != null)
                    {
                        audioSource.PlayOneShot(takeKeySound);
                    }
                }
                break;
    
            case ItemType.Gold:
                GoldManager.Instance.AddGold(goldAmount);
                Debug.Log($"אספת {goldAmount} זהב!");
    
                // השמעת סאונד ללקיחת זהב דרך ההורה
                if (audioSource != null && takeGoldSound != null)
                {
                    audioSource.PlayOneShot(takeGoldSound);
                }
                break;
    
            case ItemType.Weapon:
                Debug.Log($"נשק {weaponType} נאסף!");
    
                if (playerAnimator != null)
                {
                    playerAnimator.SetInteger("WeaponType", (int)weaponType);
                }
    
                // עדכון ה- PersistentObjectManager עם סוג הנשק הנוכחי
                if (PersistentObjectManager.instance != null)
                {
                    PersistentObjectManager.instance.SetWeaponType((int)weaponType);
                    
                    // כאן אנו מוסיפים את התנאים ללחצנים
                    if (weaponType == WeaponType.Sword)
                    {
                        PersistentObjectManager.instance.hasSwordInHand = true;
                    }
                    else if (weaponType != WeaponType.None)
                    {
                        PersistentObjectManager.instance.hasWeaponInHand = true; // אם זה נשק אחר
                    }

                    Debug.Log($"PersistentObjectManager: weaponType עודכן לערך {weaponType}");
                }
                break;
        }

        interactionText.gameObject.SetActive(false);
        gameObject.SetActive(false);  // החזרת המפתח למצב פעיל
        PersistentObjectManager.instance?.CollectItem(generatedItemId);
        Debug.Log($"הפריט {generatedItemId} נוסף לרשימת הפריטים שנאספו.");
    }

    void SetChestOpenedState()
    {
        chestOpened = true;
        chestAnimator.Play("Open", 0, 1.0f);  // הפעלת האנימציה במצב פתוח מלא
        itemAvailable = false;
    }
}