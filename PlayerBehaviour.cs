using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject sword_in_hand;

    private bool isInCombatMode = false; 
    private int clickCount = 0;
    private float lastClickTime = 0;
    private float timeBetweenClicks = 0.3f;

    public bool InteractionCompleted { get { return interactionCompleted; } }

    public enum WeaponType { None = -1, Fists = 0, Sword = 1 } 
    public WeaponType currentWeapon = WeaponType.None;
    public bool hasFists = false; 
    public bool hasSword = false; 
    public Image topEdge;
    public Image bottomEdge;
    public Image leftEdge;
    public Image rightEdge;
    public float lowHpThreshold = 40f;
    public float maxEdgeAlpha = 0.5f;
    private bool isBlinking = false;
    private GameObject currentEnemy; 
    public float maxHP = 100f;      
    public float currentHP;      
    public Slider hpSlider;     
    public LayerMask enemyLayer; 
    public List<Collider> attackColliders;
    public float attackDamage = 0; 
    public float fadeDuration = 1f;
    public Image fadeImage; 
    public bool isAttacking = false;
    float combatWalkSpeed = 5f;
    public AudioSource backAudioSource; 


  
    void Start()
    {
	int savedWeaponType = PersistentObjectManager.instance.weaponType;
        currentWeapon = (WeaponType)savedWeaponType;
        animator.SetInteger("WeaponType", savedWeaponType);
	SwitchWeapon(currentWeapon);	
	if (currentWeapon == WeaponType.Sword && hasSword)
        {
            sword_in_hand.SetActive(true);  
        }
        else
        {
            sword_in_hand.SetActive(false); 
        }

        controller = GetComponent<CharacterController>();
        if (footStepsAudioSource != null && footStepsClip != null)
        {
            footStepsAudioSource.clip = footStepsClip;
        }

        cameraOffset = playerCamera.transform.position - cameraTarget.position;
        SceneManager.sceneLoaded += OnSceneLoaded;
        sword_in_hand.SetActive(PersistentObjectManager.instance.hasSwordInHand);
	hasFists = PersistentObjectManager.instance.hasFists;
        hasSword = PersistentObjectManager.instance.hasSword;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PersistentObjectManager.instance != null)
        {
            int weaponType = PersistentObjectManager.instance.weaponType;

            animator.SetInteger("WeaponType", weaponType);
        }
    }

    void Update()
    {
        HandleMovement();
        CheckInteractionComplete();
        HandleCombat();
        HandleWeaponSwitch(); 
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
    	if (isAttacking) return;

        float currentSpeed = isInCombatMode ? combatWalkSpeed : (Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed);
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
    
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
    
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
    
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            moveDirection.y = -1f; 
    
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    
            UpdateAnimation(direction.magnitude, Input.GetKey(KeyCode.LeftShift));
        }
        else
        {
            UpdateAnimation(0f, false);
        }
    
        PlayFootSteps(horizontal, vertical, currentSpeed);
    }

    void UpdateAnimation(float movementMagnitude, bool isRunning)
    {
        animator.SetFloat("Speed", movementMagnitude);
        animator.SetBool("isRunning", isRunning);
    }

    void PlayFootSteps(float dx, float dz, float currentSpeed)
    {
        if (!(Mathf.Abs(dx) < 0.01f && Mathf.Abs(dz) < 0.01f)) 
        {
            if (!footStepsAudioSource.isPlaying)
            {
                footStepsAudioSource.Play();
            }
    
            if (isInCombatMode)
            {
                footStepsAudioSource.pitch = 0.75f;
            }
            else if (currentSpeed == runSpeed)
            {
                footStepsAudioSource.pitch = 1.5f; 
            }
            else
            {
                footStepsAudioSource.pitch = 1.0f; 
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
        if (PersistentObjectManager.instance != null)
        {
            int weaponType = PersistentObjectManager.instance.weaponType;
    
            if (Input.GetMouseButton(1))
            {
                EnterCombatMode();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                ExitCombatMode();
            }
    
            if (isInCombatMode && (currentWeapon == WeaponType.Fists || currentWeapon == WeaponType.Sword))
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
        isAttacking = true; 
    	animator.SetTrigger("SingleAttack");
    	StartCoroutine(AttackAnimationLock(1f)); 
        StartCoroutine(ActivateAttackColliders()); 
		
        if (currentEnemy != null)
        {
            AttackEnemy(currentEnemy, attackDamage); 
        }
    }

    void ExecuteComboAttack()
    {
        isAttacking = true; 
        animator.SetTrigger("ComboAttack");
        StartCoroutine(AttackAnimationLock(1f));
        StartCoroutine(ActivateAttackColliders()); 
        if (currentEnemy != null)
        {
            AttackEnemy(currentEnemy, attackDamage); 
        }
    }

    IEnumerator AttackAnimationLock(float extraWaitTime)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + extraWaitTime);
        isAttacking = false; 
    }

    IEnumerator ActivateAttackColliders()
    {
        EnableAllAttackColliders(); 
        yield return new WaitForSeconds(0.5f); 
        DisableAllAttackColliders();
        isAttacking = false;
    }

    void EnableAllAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = true;
        }
    }

    void DisableAllAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = false;
        }
    }

    void AttackEnemy(GameObject enemy, float damage)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage); 
        }
    }

    void HandleWeaponSwitch() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasFists)
        {
            SwitchWeapon(WeaponType.Fists);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && hasSword)
        {
            SwitchWeapon(WeaponType.Sword);
        }
    }

    void SwitchWeapon(WeaponType weaponType)
    {
        currentWeapon = weaponType;
        animator.SetInteger("WeaponType", (int)currentWeapon);
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetWeaponType((int)weaponType);
        }
        if (currentWeapon == WeaponType.Sword)
        {
            sword_in_hand.SetActive(true);
        }
        else
        {
            sword_in_hand.SetActive(false);
        }
    }

    void Die()
    {
        animator.SetTrigger("Die"); 
        StartCoroutine(WaitForDeathAnimation());
    }

    void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }
    }

    

    IEnumerator WaitForDeathAnimation()
    {
        float deathAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;
        
        yield return new WaitForSeconds(deathAnimationTime);

        yield return StartCoroutine(FadeOut(fadeDuration));

        SceneManager.LoadScene("DeathScreen");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) 
        {
            currentEnemy = other.gameObject;
        }
        else if (other.CompareTag("EnemyAttack"))  
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(enemy.attackDamage); 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentEnemy = null; 
        }
        
    }
	
    IEnumerator FadeOut(float duration)
    {
        float currentTime = 0f;
        Color fadeColor = fadeImage.color; 

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, currentTime / duration);  
            fadeImage.color = fadeColor;  
            yield return null;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage; 
        if (currentHP < 0)
        {
            currentHP = 0;
        }
    
        PersistentObjectManager.instance.SetPlayerHP(currentHP);

        UpdateHPUI(); 
	UpdateEdgeEffect();

        if (currentHP == 0)
        {
            Die(); 
        }
     }
	
	IEnumerator BlinkEdgeEffect()
        {
        	isBlinking = true;
        	float blinkDuration = 0.5f; 
       		float minAlpha = 0f;
        	float maxAlpha = maxEdgeAlpha;
        	bool increasing = true; 
    
        	while (currentHP <= lowHpThreshold) 
        	{
            		float startAlpha = increasing ? minAlpha : maxAlpha;
            		float endAlpha = increasing ? maxAlpha : minAlpha;
            		float elapsedTime = 0f;
    
            		while (elapsedTime < blinkDuration)
            		{
             		   	float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / blinkDuration);
                		SetEdgeEffect(alpha);
                		elapsedTime += Time.deltaTime;
                		yield return null;
            		}
           	 	increasing = !increasing; 
        	}
    
        	SetEdgeEffect(0f); 
        	isBlinking = false;
	}

    void UpdateEdgeEffect()
    {
        if (currentHP <= lowHpThreshold)
        {
            if (!isBlinking)
            {
                StartCoroutine(BlinkEdgeEffect());
            }
        }
        else
        {
            StopCoroutine(BlinkEdgeEffect());
            SetEdgeEffect(0f);
        }
    }
    
    void SetEdgeEffect(float alpha)
    {
        SetAlpha(topEdge, alpha);
        SetAlpha(bottomEdge, alpha);
        SetAlpha(leftEdge, alpha);
        SetAlpha(rightEdge, alpha);
    }
    
    void SetAlpha(Image edge, float alpha)
    {
        if (edge != null)
        {
            Color color = edge.color;
            color.a = alpha;
            edge.color = color;
        }
    }

    public void StartTransitionToCredits()
    {
        StartCoroutine(TransitionToCredits());
    }

    private IEnumerator TransitionToCredits()
    {
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.gameObject.SetActive(true);

            float fadeDuration = 4f;
            float initialVolume = backAudioSource != null ? backAudioSource.volume : 0f; 

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                float alpha = Mathf.Clamp01(t / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);

                if (backAudioSource != null) 
                {
                    backAudioSource.volume = Mathf.Lerp(initialVolume, 0, t / fadeDuration);
                }

                yield return null;
            }

            fadeImage.color = new Color(0, 0, 0, 1f);
            if (backAudioSource != null) backAudioSource.volume = 0f;
        }

        yield return new WaitForSeconds(1f); 
        SceneManager.LoadScene("Credits"); 
    }

}
