using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{
    public GameObject playerCamera;
    public Transform cameraTarget;
    public Animator animator;
    public string interactionCompleteBool = "HasFinishedTalking";

    public AudioClip footStepsClip;
    public AudioClip selfTalk1;
    public AudioClip selfTalk2;
    public AudioClip selfTalk3;
    public AudioClip selfTalk4;
    public AudioClip documentSound;
    public AudioClip finalDocumentSound;

    public AudioSource footStepsAudioSource;
    public AudioSource selfTalkAudioSource;

    CharacterController controller;
    float speed = 10f;
    float runSpeed = 20f;
    public float mouseSensitivity = 5f;
    public float verticalClampAngle = 45f;

    private Vector3 cameraOffset;
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private Vector3 cameraVelocity = Vector3.zero;

    private bool hasPlayedSelfTalk = false;
    private bool interactionCompleted = false;
    public bool HasChangedClothes = false;
    public int documentsCollected = 0;
    public GameObject sword_in_hand; // חרב ביד השחקן

    private bool isInCombatMode = false; // מצב הקרב
    private int clickCount = 0;
    private float lastClickTime = 0;
    private float timeBetweenClicks = 0.3f;

    public bool InteractionCompleted { get { return interactionCompleted; } }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (footStepsAudioSource == null || selfTalkAudioSource == null)
        {
            Debug.LogError("AudioSource(s) are not assigned!");
        }

        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
        }

        if (footStepsAudioSource != null && footStepsClip != null)
        {
            footStepsAudioSource.clip = footStepsClip;
        }

        cameraOffset = playerCamera.transform.position - cameraTarget.position;

        // הירשם לאירוע טעינת סצנה
        SceneManager.sceneLoaded += OnSceneLoaded;

        // עדכון מצב החרב לפי מנהל המצב
        sword_in_hand.SetActive(PersistentObjectManager.instance.hasSwordInHand);
    }

    void OnDestroy()
    {
        // בטל את הרישום לאירוע כאשר האובייקט מושמד
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // אירוע שיתרחש בכל פעם שסצנה נטענת
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // טען את weaponType מ-PersistentObjectManager
        if (PersistentObjectManager.instance != null)
        {
            int weaponType = PersistentObjectManager.instance.weaponType;

            // עדכן את האנימטור עם סוג הנשק
            animator.SetInteger("WeaponType", weaponType);

            Debug.Log($"Loaded weaponType: {weaponType} from PersistentObjectManager");
        }
    }

    void Update()
    {
        HandleMovement();
        CheckInteractionComplete();
        HandleCombat();
        HandleWeaponChange(); // פונקציה חדשה לניהול הלחצנים
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

    void HandleMovement()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg
                                + playerCamera.transform.eulerAngles.y;

            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);

            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            moveDirection.y = -1f; // משיכה קלה למטה כדי להיצמד לקרקע

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            UpdateAnimation(direction.magnitude, Input.GetKey(KeyCode.LeftShift));
        }
        else
        {
            UpdateAnimation(0f, false);
        }

        PlayFootSteps(horizontal, vertical);
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

    void CheckInteractionComplete()
    {
        bool currentState = animator.GetBool(interactionCompleteBool);
        if (currentState && !interactionCompleted)
        {
            interactionCompleted = true;
            StartCoroutine(PlaySelfTalkAfterDelay());
        }
    }

    IEnumerator PlaySelfTalkAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (!selfTalkAudioSource.isPlaying && selfTalk1 != null)
        {
            selfTalkAudioSource.PlayOneShot(selfTalk1);
        }
    }

    public void CompleteInteractionWithNPC()
    {
        animator.SetBool(interactionCompleteBool, true);
    }

    public void TriggerPlayerResponseAfterStory()
    {
        if (!selfTalkAudioSource.isPlaying && selfTalk1 != null)
        {
            selfTalkAudioSource.PlayOneShot(selfTalk1);
            StartCoroutine(UpdateMissionAfterResponse());
        }
    }

    IEnumerator UpdateMissionAfterResponse()
    {
        while (selfTalkAudioSource.isPlaying)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        MissionManager missionManagerRef = FindObjectOfType<MissionManager>();
        if (missionManagerRef != null)
        {
            missionManagerRef.TriggerNextMission();
        }
    }

    public void PlaySelfTalk3()
    {
        if (!selfTalkAudioSource.isPlaying && selfTalk3 != null)
        {
            selfTalkAudioSource.PlayOneShot(selfTalk3);
        }
    }

    public void PlaySelfTalk4()
    {
        if (!selfTalkAudioSource.isPlaying && selfTalk4 != null)
        {
            selfTalkAudioSource.PlayOneShot(selfTalk4);
        }
    }

    public void ChangeClothes()
    {
        HasChangedClothes = true;
    }

    public void CollectDocument()
    {
        documentsCollected += 1;

        if (documentsCollected < 4)
        {
            selfTalkAudioSource.PlayOneShot(documentSound);
        }
        else if (documentsCollected == 4)
        {
            selfTalkAudioSource.PlayOneShot(finalDocumentSound);
        }
    }

    void HandleCombat()
    {
        // טען את weaponType מ-PersistentObjectManager
        int weaponType = PersistentObjectManager.instance != null ? PersistentObjectManager.instance.weaponType : -1;

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
    }

    void ExecuteComboAttack()
    {
        Debug.Log("Combo Attack");
        animator.SetTrigger("ComboAttack");
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