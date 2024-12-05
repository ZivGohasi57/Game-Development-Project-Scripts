using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
    public bool requiresKey = false; 
    public bool requiresTaskCompletion = false; 
    public bool hasKey = false;
    public bool taskCompleted = false; 

    public Text interactionText;
    [Header("Messages")]
    public string messageLocked = "Key require"; 
    public string messageTaskIncomplete = "Task didnt complete"; 
    public string messagePressToOpen = "Press E to open"; 

    public Animator doorAnimator; 

    
    public AudioClip openDoorSound;  
    private AudioSource audioSource;   
    public bool isUnlocked = false; 
    private bool isInRange = false; 
    private string doorId;

    void Start()
    {
        doorId = $"{gameObject.name}_{transform.position}";

        audioSource = GetComponent<AudioSource>();
        
        if (PersistentObjectManager.instance != null &&
            PersistentObjectManager.instance.IsDoorOpen(doorId))
        {
            SetDoorOpenedState(); 
        }

        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isInRange && !isUnlocked && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            ShowInteractionText();
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

    public void TryOpenDoor()
    {
        if (requiresKey && !hasKey)
        {
            interactionText.text = messageLocked;
        }
        else if (requiresTaskCompletion && !taskCompleted)
        {
            interactionText.text = messageTaskIncomplete;
        }
        else
        {
            OpenDoor();
            PersistentObjectManager.instance?.SetDoorOpen(doorId);
        }
    }

    void ShowInteractionText()
    {
        if (interactionText == null) return;

        if (requiresKey && !hasKey)
        {
            interactionText.text = messageLocked;
        }
        else if (requiresTaskCompletion && !taskCompleted)
        {
            interactionText.text = messageTaskIncomplete;
        }
        else
        {
            interactionText.text = messagePressToOpen;
        }

        interactionText.gameObject.SetActive(true);
    }

    void OpenDoor()
    {
        isUnlocked = true;
        interactionText.gameObject.SetActive(false); 
        doorAnimator.SetBool("DoorOpens", true);

        if (audioSource != null && openDoorSound != null)
        {
            audioSource.PlayOneShot(openDoorSound);
        }
    }

    void SetDoorOpenedState()
    {
        isUnlocked = true;
        doorAnimator.SetBool("DoorOpens", true); 
    }
}
