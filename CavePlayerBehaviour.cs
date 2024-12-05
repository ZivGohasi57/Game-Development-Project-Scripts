using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CavePlayerBehaviour : MonoBehaviour
{
    public GameObject playerCamera;
    public Transform cameraTarget;
    public Animator animator;
    public GameObject sword;
    public GameObject sword_in_hand;
    public Text pickText;
    public Text openChestText;

    public AudioClip footStepsClip;
    public AudioSource footStepsAudioSource;

    public LayerMask enemyLayer;
    public List<Collider> attackColliders;
    public float attackDamage = 0; 

    CharacterController controller;
    float speed = 10f;
    float runSpeed = 20f;
    float combatWalkSpeed = 5f; 
    public float mouseSensitivity = 5f;
    public float verticalClampAngle = 45f;

    private bool isInCombatMode = false;
    private int clickCount = 0;
    private float lastClickTime = 0;
    private float timeBetweenClicks = 0.3f;

    private Vector3 cameraOffset;
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private Vector3 cameraVelocity = Vector3.zero;

    private GameObject currentJar;  
    private GameObject currentEnemy; 
    public float maxHP = 100f;      
    public float currentHP;          
    public Slider hpSlider;        

    public int weaponType; 
    public float damage;  
    public Image fadeImage; 
    public float fadeDuration = 1f;

    private string currentSceneName = "CaveScene";   

    public Image topEdge;
    public Image bottomEdge;
    public Image leftEdge;
    public Image rightEdge;
    public float lowHpThreshold = 40f;
    public float maxEdgeAlpha = 0.5f;
    private bool isBlinking = false;

    public enum WeaponType { None = -1, Fists = 0, Sword = 1 }

    public WeaponType currentWeapon = WeaponType.None;
    public bool hasFists = false; 
    public bool hasSword = false;
    public bool isAttacking = false;
	public AudioClip voiceSword;
	public AudioClip voiceForest;
	public AudioSource audioSource;
	public Color normalColor = Color.green; 
    public Color lowHpColor = Color.yellow; 
    public Color criticalHpColor = Color.red; 
    private Coroutine healthRegenCoroutine; 


	


    void Awake()
    {
	}

    void Start()
    {
        PersistentObjectManager.instance.SetLastScene(currentSceneName);
		hasFists = PersistentObjectManager.instance.hasFists;
        hasSword = PersistentObjectManager.instance.hasSword;

        controller = GetComponent<CharacterController>();
 		
        int savedWeaponType = PersistentObjectManager.instance.weaponType;
        currentWeapon = (WeaponType)savedWeaponType;
        SwitchWeapon(currentWeapon);	
        if (currentWeapon == WeaponType.Sword && hasSword)
        {
            sword_in_hand.SetActive(true); 
        }
        else
        {
            sword_in_hand.SetActive(false); 
        }


        if (footStepsAudioSource == null)

        if (animator == null)

        if (footStepsAudioSource != null && footStepsClip != null)
            footStepsAudioSource.clip = footStepsClip;

        cameraOffset = playerCamera.transform.position - cameraTarget.position;

        pickText.gameObject.SetActive(false);
        openChestText.gameObject.SetActive(false);

        if (PersistentObjectManager.instance != null)
        {
            sword_in_hand.SetActive(PersistentObjectManager.instance.hasSwordInHand);
            sword.SetActive(PersistentObjectManager.instance.hasSwordOnWall);
            animator.SetInteger("WeaponType", PersistentObjectManager.instance.weaponType);
			
        }
        DisableAllAttackColliders();
		currentHP = maxHP;    
        UpdateHPUI();    
    }
    

    void Update()
    {
        HandleMovement();
        HandleInteraction();
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
        if (isInCombatMode && (weaponType == 1 || weaponType == 0)) 
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
        isAttacking = true;
        animator.SetTrigger("SingleAttack");
        StartCoroutine(AttackAnimationLock(1f));
        StartCoroutine(ActivateAttackColliders());
		
		if (currentJar != null) 
		{ 
            Jar jarScript = currentJar.GetComponent<Jar>();
            if (jarScript != null)
            {
                jarScript.Break();
            }
		}

        
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
		if (currentJar != null) 
		{ 
			Jar jarScript = currentJar.GetComponent<Jar>();
            if (jarScript != null)
            {
                jarScript.Break(); 
            }
		}

        
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

    void HandleMovement()
    {
        float currentSpeed;
        if (isInCombatMode)
        {
            currentSpeed = combatWalkSpeed;
        }
        else
        {
            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed;
        }
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

                footStepsAudioSource.pitch = (currentSpeed == runSpeed) ? 2f : (isInCombatMode ? 0.75f : 1f);

                if (!footStepsAudioSource.isPlaying)
                {
                    footStepsAudioSource.Play();
                }
            }
            else
            {
                UpdateAnimation(0, false);
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
    }

    void UpdateAnimation(float movementMagnitude, bool isRunning)
    {
        animator.SetFloat("Speed", movementMagnitude);
        animator.SetBool("isRunning", isRunning);
    }

    void HandleInteraction()
    {
        HandleSwordInteraction();
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
		AddWeapon("Sword");
		PersistentObjectManager.instance.SetHasSword(true);
		SwitchWeapon(WeaponType.Sword);

        int newWeaponType = 1; 
        animator.SetInteger("WeaponType", newWeaponType);


        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetWeaponType(newWeaponType);
        }

        pickText.gameObject.SetActive(false);

      
        MissionManager missionManager = FindObjectOfType<MissionManager>();
        if (missionManager != null)
        {
            missionManager.AdvanceMission();
        }
        VoiceSwordTalk();
    }
    
    void HandleWeaponChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
          
            PersistentObjectManager.instance.SetWeaponType(-1);
            animator.SetInteger("WeaponType", -1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && PersistentObjectManager.instance.hasWeaponInHand)
        {
            
            PersistentObjectManager.instance.SetWeaponType(0);
            animator.SetInteger("WeaponType", 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && PersistentObjectManager.instance.hasSwordInHand)
        {
         
            PersistentObjectManager.instance.SetWeaponType(1);
            animator.SetInteger("WeaponType", 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentEnemy = other.gameObject;
        }
        else if (other.CompareTag("Jar"))
        {
            currentJar = other.gameObject;
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
        else if (other.CompareTag("Jar"))
        {
            currentJar = null;
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

            if (currentHP / maxHP >= 0.4f)
            {
                hpSlider.fillRect.GetComponent<Image>().color = normalColor;
            }
            else if (currentHP / maxHP >= 0.2f)
            {
                hpSlider.fillRect.GetComponent<Image>().color = lowHpColor;
            }
            else
            {
                hpSlider.fillRect.GetComponent<Image>().color = criticalHpColor;
            }
        }
    }

    

    IEnumerator WaitForDeathAnimation()
    {
        float deathAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathAnimationTime);

        yield return StartCoroutine(FadeOut(fadeDuration));

        SceneManager.LoadScene("DeathScreen");
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

    public void AddHealth(float healthToAdd)
    {
        currentHP = Mathf.Min(currentHP + healthToAdd, maxHP);
        
        PersistentObjectManager.instance.SetPlayerHP(currentHP);
    
        UpdateHPUI();
    }

	public void AddWeapon(string weaponName)
    {
        if (weaponName == "Fists" && !hasFists && !hasSword)
        {
            hasFists = true;
            SwitchWeapon(WeaponType.Fists);
            animator.SetInteger("WeaponType", (int)currentWeapon);
			PersistentObjectManager.instance.SetHasFists(true);
        }
        else if (weaponName == "Sword" && hasFists && !hasSword) /
        {
            hasSword = true;
            SwitchWeapon(WeaponType.Sword);
            animator.SetInteger("WeaponType", (int)currentWeapon);
            PersistentObjectManager.instance.SetHasSword(true);
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
	    attackDamage = 70f;
        }
        else
        {
            sword_in_hand.SetActive(false); 
	    attackDamage = 20f;
        }
     }
   

	void VoiceSwordTalk()
	{
		if (voiceSword != null)
        {
            audioSource.PlayOneShot(voiceSword);
        }	
	}

	public void VoiceForestTalk()
	{
		if (voiceForest != null)
		{
			audioSource.PlayOneShot(voiceForest);
		}
	}

	IEnumerator RegenerateHealth()
    {
        while (currentHP < maxHP * 0.3f && !isInCombatMode)
        {
            currentHP = Mathf.Min(currentHP + 1, maxHP * 0.3f);
            UpdateHPUI();
            yield return new WaitForSeconds(2f);
        }
    }
}
	
