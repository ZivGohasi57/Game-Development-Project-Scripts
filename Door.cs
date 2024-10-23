using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
    public bool requiresKey = false;  // האם הדלת דורשת מפתח
    public bool requiresTaskCompletion = false;  // האם הדלת דורשת משימה
    public bool hasKey = false;  // האם יש מפתח (ניתן לשנות דרך Inspector)
    public bool taskCompleted = false;  // האם המשימה הושלמה

    public Text interactionText;  // טקסט שיוצג לשחקן

    [Header("Messages")]
    public string messageLocked = "Key require";  // הודעה אם הדלת נעולה
    public string messageTaskIncomplete = "Task didnt complete";  // הודעה אם המשימה לא הושלמה
    public string messagePressToOpen = "Press E to open";  // הודעה לפתיחה

    public Animator doorAnimator;  // אנימטור לפתיחת הדלת

    // משתני סאונד
    public AudioClip openDoorSound;    // סאונד לפתיחת הדלת
    private AudioSource audioSource;    // מקור הסאונד

    public bool isUnlocked = false;  // האם הדלת פתוחה
    private bool isInRange = false;  // האם השחקן ליד הדלת
    private string doorId;  // מזהה ייחודי לדלת

    void Start()
    {
        doorId = $"{gameObject.name}_{transform.position}";

        // קבלת רכיב AudioSource
        audioSource = GetComponent<AudioSource>();
        
        // בדיקה אם הדלת כבר נפתחה בעבר
        if (PersistentObjectManager.instance != null &&
            PersistentObjectManager.instance.IsDoorOpen(doorId))
        {
            SetDoorOpenedState();  // השארת הדלת פתוחה
        }

        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
            if (doorAnimator == null)
            {
                Debug.LogError("Animator לא נמצא על הדלת: " + gameObject.name);
            }
        }

        if (interactionText == null)
        {
            Debug.LogError("interactionText לא מחובר ב-Inspector לדלת: " + gameObject.name);
        }
        else
        {
            interactionText.gameObject.SetActive(false);  // הסתרת הטקסט בהתחלה
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
            interactionText.gameObject.SetActive(false);  // הסתרת הטקסט ביציאה
        }
    }

    void TryOpenDoor()
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
            OpenDoor();  // פתיחת הדלת
            PersistentObjectManager.instance?.SetDoorOpen(doorId);  // שמירת מצב פתוח
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

        interactionText.gameObject.SetActive(true);  // הצגת הטקסט
    }

    void OpenDoor()
    {
        isUnlocked = true;
        interactionText.gameObject.SetActive(false);  // הסתרת הטקסט לאחר פתיחה
        doorAnimator.SetBool("DoorOpens", true);  // הפעלת האנימציה

        // השמעת סאונד לפתיחת הדלת
        if (audioSource != null && openDoorSound != null)
        {
            audioSource.PlayOneShot(openDoorSound);
        }

        Debug.Log("הדלת נפתחה: " + gameObject.name);
    }

    void SetDoorOpenedState()
    {
        isUnlocked = true;
        doorAnimator.SetBool("DoorOpens", true);  // השארת הדלת פתוחה
        Debug.Log("הדלת כבר פתוחה: " + gameObject.name);
    }
}