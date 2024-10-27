using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Key : MonoBehaviour
{
    public enum ItemType { Key, Gold, Weapon, Life }  
    public enum WeaponType { None, Fists, Sword }
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
    public string messageTakeLife = "Press E to take the life boost";

    public int goldAmount = 10;
    public int lifeBoostAmount = 30;
    public bool isBigTreasure = false; // האם זה "האוצר הגדול"
    public Image fadeImage; // תמונת הפייד

    public Animator chestAnimator;
    public Animator playerAnimator;
    public AudioClip takeKeySound;
    public AudioClip takeGoldSound;
    public AudioClip takeWeaponSound;
    public AudioClip takeLifeSound;
    public AudioClip openChestSound;
    
    private AudioSource audioSource;
    private bool isInRange = false;
    private bool chestOpened = false;
    private bool itemAvailable = false;
    private string generatedItemId;


    void Start()
    {
        generatedItemId = $"{gameObject.name}_{transform.position}";
        audioSource = transform.parent.GetComponent<AudioSource>();

        if (PersistentObjectManager.instance != null &&
            PersistentObjectManager.instance.IsContainerOpen(generatedItemId))
        {
            SetChestOpenedState();  
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
            ItemType.Life => messageTakeLife,  
            _ => ""
        };
    }

    void OpenChest()
    {
        chestOpened = true;
        chestAnimator.SetTrigger("Open");

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
    
                    if (audioSource != null && takeKeySound != null)
                    {
                        audioSource.PlayOneShot(takeKeySound);
                    }
                }
                break;
    
            case ItemType.Gold:
                GoldManager.Instance.AddGold(goldAmount);
                Debug.Log($"אספת {goldAmount} זהב!");

                if (isBigTreasure)
                {
					PlayerBehaviour player1 = FindObjectOfType<PlayerBehaviour>();
                    if (player1 != null)
                    {
                        player1.StartTransitionToCredits();
                    }
                }
                else if (audioSource != null && takeGoldSound != null)
                {
                    audioSource.PlayOneShot(takeGoldSound);
                }
                break;
    
            case ItemType.Weapon:
                Debug.Log($"נשק {weaponType} נאסף!");

                CavePlayerBehaviour player = FindObjectOfType<CavePlayerBehaviour>();
                if (player != null)
                {
                    player.AddWeapon(weaponType.ToString());
                }

                if (audioSource != null && takeWeaponSound != null)
                {
                    audioSource.PlayOneShot(takeWeaponSound);
                }
                break;

            case ItemType.Life:
                CavePlayerBehaviour playerHealth = FindObjectOfType<CavePlayerBehaviour>();
                if (playerHealth != null)
                {
                    playerHealth.AddHealth(lifeBoostAmount);
                }

                if (audioSource != null && takeLifeSound != null)
                {
                    audioSource.PlayOneShot(takeLifeSound);
                }
                break;
        }

        interactionText.gameObject.SetActive(false);
        gameObject.SetActive(false);  
        PersistentObjectManager.instance?.CollectItem(generatedItemId);
        Debug.Log($"הפריט {generatedItemId} נוסף לרשימת הפריטים שנאספו.");
    }

    

    void SetChestOpenedState()
    {
        chestOpened = true;
        chestAnimator.Play("Open", 0, 1.0f);  
        itemAvailable = false;
    }
}