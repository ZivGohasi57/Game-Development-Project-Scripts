using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CavePlayerBehaviour : MonoBehaviour
{
    public GameObject playerCamera;
    public Transform cameraTarget;
    public Animator animator;
    public GameObject sword;
    public GameObject sword_in_hand;
    public Text pickText;
    public Text openChestText; // יש להשאיר רק אם זה רלוונטי בעתיד

    public AudioClip footStepsClip;
    public AudioSource footStepsAudioSource;

    CharacterController controller;
    float speed = 10f;
    float runSpeed = 20f;
    public float mouseSensitivity = 5f;
    public float verticalClampAngle = 45f;

    private bool isInCombatMode = false;  // מצב הקרב
    private int clickCount = 0;
    private float lastClickTime = 0;
    private float timeBetweenClicks = 0.3f;

    private Vector3 cameraOffset;
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private Vector3 cameraVelocity = Vector3.zero;

    private GameObject currentJar;  // לשמירת הכד בטווח

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (footStepsAudioSource == null)
            Debug.LogError("AudioSource(s) are not assigned!");

        if (animator == null)
            Debug.LogError("Animator is not assigned!");

        if (footStepsAudioSource != null && footStepsClip != null)
            footStepsAudioSource.clip = footStepsClip;

        cameraOffset = playerCamera.transform.position - cameraTarget.position;

        pickText.gameObject.SetActive(false);
        openChestText.gameObject.SetActive(false);

        // טען את מצב החרב
        if (PersistentObjectManager.instance != null)
        {
            sword_in_hand.SetActive(PersistentObjectManager.instance.hasSwordInHand);
            sword.SetActive(PersistentObjectManager.instance.hasSwordOnWall);
            animator.SetInteger("WeaponType", PersistentObjectManager.instance.weaponType); // טען את סוג הנשק
        }
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
        HandleCombat();
        HandleWeaponChange(); // הוספת הפונקציה לשינוי סוג הנשק
    }

    void LateUpdate()
    {
        HandleCamera();
    }

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, -verticalClampAngle, verticalClampAngle);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 targetPosition = cameraTarget.position + rotation * cameraOffset;

        playerCamera.transform.position = Vector3.SmoothDamp(
            playerCamera.transform.position, 
            targetPosition, 
            ref cameraVelocity, 
            0.1f
        );

        playerCamera.transform.LookAt(cameraTarget);
    }

    void HandleCombat()
    {
        int weaponType = animator.GetInteger("WeaponType");

        if (Input.GetMouseButton(1))
        {
            EnterCombatMode();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            ExitCombatMode();
        }

        if (isInCombatMode && (weaponType == 0 || weaponType == 1))
        {
            if (Input.GetMouseButtonDown(0))
            {
                float timeSinceLastClick = Time.time - lastClickTime;
                if (timeSinceLastClick <= timeBetweenClicks)
                {
                    clickCount++;
                }
                else
                {
                    clickCount = 1;
                }

                lastClickTime = Time.time;

                if (clickCount == 1)
                {
                    ExecuteSingleAttack();
                }
                else if (clickCount == 2)
                {
                    ExecuteComboAttack();
                    clickCount = 0;
                }
            }
        }

        // עדכון PersistentObjectManager בסיום הפעולה
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetWeaponType(animator.GetInteger("WeaponType"));
        }
    }

    void EnterCombatMode()
    {
        isInCombatMode = true;
        animator.SetBool("isInCombatMode", true);
    }

    void ExitCombatMode()
    {
        isInCombatMode = false;
        animator.SetBool("isInCombatMode", false);
    }

    void ExecuteSingleAttack()
    {
        Debug.Log("Single Attack");
        animator.SetTrigger("SingleAttack");

        if (currentJar != null)
        {
            ReplaceWithBrokenJar(currentJar);
        }
    }

    void ExecuteComboAttack()
    {
        Debug.Log("Combo Attack");
        animator.SetTrigger("ComboAttack");

        if (currentJar != null)
        {
            ReplaceWithBrokenJar(currentJar);
        }
    }

    void ReplaceWithBrokenJar(GameObject jar)
    {
        Jar jarScript = jar.GetComponent<Jar>();
        if (jarScript != null)
        {
            jarScript.Break();  // הפעלת הפונקציה לשבירת הכד
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jar"))
        {
            currentJar = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Jar"))
        {
            currentJar = null;
        }
    }

    void HandleMovement()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
    
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
    
        if (controller.isGrounded)
        {
            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + currentYaw;
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
    
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
    
                Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
                moveDirection *= currentSpeed;
    
                moveDirection.y = -5f;
    
                controller.Move(moveDirection * Time.deltaTime);
                UpdateAnimation(direction.magnitude, Input.GetKey(KeyCode.LeftShift));
    
                // עדכון Pitch של הסאונד בהתאם למצב ריצה או הליכה
                if (Input.GetKey(KeyCode.LeftShift)) // אם השחקן רץ
                {
                    footStepsAudioSource.pitch = 2f; // מהירות סאונד גבוהה יותר בריצה
                }
                else // אם השחקן בהליכה
                {
                    footStepsAudioSource.pitch = 1.0f; // מהירות סאונד רגילה בהליכה
                }
    
                // הפעל את סאונד הצעדים אם הוא לא כבר מתנגן
                if (!footStepsAudioSource.isPlaying)
                {
                    footStepsAudioSource.Play();
                }
            }
            else
            {
                UpdateAnimation(0, false);
                // עצור את הסאונד אם השחקן לא זז
                if (footStepsAudioSource.isPlaying)
                {
                    footStepsAudioSource.Stop();
                }
            }
        }
        else
        {
            Vector3 gravity = new Vector3(0, -20f, 0);
            controller.Move(gravity * Time.deltaTime);
        }
    
        // פונקציה לשמירת סאונד הצעדים הוסרה מכאן, נשארה בתוך התנאי.
    }

    void UpdateAnimation(float movementMagnitude, bool isRunning)
    {
        animator.SetFloat("Speed", movementMagnitude);
        animator.SetBool("isRunning", isRunning);
    }

    void PlayFootSteps(float dx, float dz)
    {
        if (!(Mathf.Abs(dx) < 0.01f && Mathf.Abs(dz) < 0.01f))
        {
            if (!footStepsAudioSource.isPlaying)
            {
                footStepsAudioSource.Play();
            }
        }
        else
        {
            if (footStepsAudioSource.isPlaying)
            {
                footStepsAudioSource.Stop();
            }
        }
    }

    void HandleInteraction()
    {
        HandleSwordInteraction();
        // הוצא את HandleChestInteraction מכאן
    }

    void HandleSwordInteraction()
    {
        RaycastHit hit;
        float distance = Vector3.Distance(transform.position, sword.transform.position);

        if (distance < 15f)
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 15f))
            {
                if (hit.collider != null && hit.collider.gameObject == sword)
                {
                    pickText.gameObject.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        CollectSword();
                    }
                }
                else
                {
                    pickText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            pickText.gameObject.SetActive(false);
        }
    }

    void CollectSword()
    {
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetHasSword(true);
            PersistentObjectManager.instance.SetHasSwordOnWall(false);
        }

        sword_in_hand.SetActive(true);
        sword.SetActive(false);

        int newWeaponType = 1; // עדכון סוג הנשק
        animator.SetInteger("WeaponType", newWeaponType);

        // עדכון PersistentObjectManager
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetWeaponType(newWeaponType);
        }

        pickText.gameObject.SetActive(false);
    }

    void HandleWeaponChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            // לחצן 0: אי אפשר להילחם
            PersistentObjectManager.instance.SetWeaponType(-1);
            animator.SetInteger("WeaponType", -1);
            Debug.Log("WeaponType set to -1, combat disabled.");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && PersistentObjectManager.instance.hasWeaponInHand)
        {
            // לחצן 1: נשק רגיל
            PersistentObjectManager.instance.SetWeaponType(0);
            animator.SetInteger("WeaponType", 0);
            Debug.Log("WeaponType set to 0, regular weapon equipped.");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && PersistentObjectManager.instance.hasSwordInHand)
        {
            // לחצן 2: חרב
            PersistentObjectManager.instance.SetWeaponType(1);
            animator.SetInteger("WeaponType", 1);
            Debug.Log("WeaponType set to 1, sword equipped.");
        }
    }
}