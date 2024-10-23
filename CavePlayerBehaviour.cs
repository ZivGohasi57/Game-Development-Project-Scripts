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

    public LayerMask enemyLayer;  // שכבת האויבים
    public List<Collider> attackColliders; // רשימה של קוליידרים עבור התקפות שונות
    public float attackDamage = 0; // כמות הנזק שהשחקן נותן

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
    private GameObject currentEnemy;  // לשמירת האויב בטווח
	public float maxHP = 100f;       // כמות ה-HP המקסימלית של השחקן
    public float currentHP;          // כמות ה-HP הנוכחית של השחקן
    public Slider hpSlider;          // סליידר המייצג את כמות ה-HP של השחקן	
	
	private int weaponType; // סוג הנשק, 0 - נשק רגיל, 1 - חרב
    private float damage;  // משתנה לכמות הנזק

    void Awake()
    {
	}

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
			weaponType = 1;
        }
		UpdateDamageBasedOnWeapon();
        DisableAllAttackColliders(); // לוודא שכל הקוליידרים כבויים בהתחלה
		currentHP = maxHP;           // מתחילים עם כמות החיים המקסימלית
        UpdateHPUI();                // עדכון ה-UI בתחילת המשחק
    }
    

    void Update()
    {
        HandleMovement();
        HandleInteraction();
        HandleCombat();
        //HandleWeaponChange(); // הוספת הפונקציה לשינוי סוג הנשק
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
        if (isInCombatMode && (weaponType == 1 || weaponType == 0)) // לחימה עם חרב או נשק רגיל
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
        StartCoroutine(ActivateAttackColliders()); // הפעלת כל הקוליידרים לזמן קצר כדי לפגוע באויב

        // נבדוק אם יש אויב בטווח
        if (currentEnemy != null)
        {
			UpdateDamageBasedOnWeapon();
            AttackEnemy(currentEnemy, damage);
        }
    }

    void ExecuteComboAttack()
    {
        Debug.Log("Combo Attack");
        animator.SetTrigger("ComboAttack");
        StartCoroutine(ActivateAttackColliders()); // הפעלת כל הקוליידרים לזמן קצר כדי לפגוע באויב

        // נבדוק אם יש אויב בטווח
        if (currentEnemy != null)
        {
			UpdateDamageBasedOnWeapon();		
            AttackEnemy(currentEnemy, damage);
        }
    }

    IEnumerator ActivateAttackColliders()
    {
        EnableAllAttackColliders(); // הפעלת כל הקוליידרים
        yield return new WaitForSeconds(0.5f); // זמן המכה
        DisableAllAttackColliders(); // כיבוי הקוליידרים לאחר המכה
    }

    void EnableAllAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = true; // הפעלת כל הקוליידרים
        }
    }

    void DisableAllAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = false; // כיבוי כל הקוליידרים
        }
    }

    void AttackEnemy(GameObject enemy, float damage)
    {
        // נניח שהאויב שלך משתמש בסקריפט שנקרא 'Enemy' ויש לו פונקציה 'TakeDamage'
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);  // לדוגמה, הורדת 20 נקודות חיים
            Debug.Log("פגעת באויב!");
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))  // נניח שלכל האויבים יש תגית "Enemy"
        {
            currentEnemy = other.gameObject;
        }
        else if (other.CompareTag("Jar"))
        {
            currentJar = other.gameObject;
        }
        else if (other.CompareTag("EnemyAttack"))  // בדוק אם הפגיעה באה מהאויב
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(enemy.attackDamage);  // השחקן מקבל נזק מהאויב בהתאם לכמות הנזק של האויב
                Debug.Log("השחקן נפגע! חיים נוכחיים: " + currentHP);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentEnemy = null;  // האויב יצא מהטווח
        }
        else if (other.CompareTag("Jar"))
        {
            currentJar = null;
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"נגרם נזק: {damage}, חיים נוכחיים: {currentHP}");  // הצגת נזק וחיים נוכחיים
    
        currentHP -= damage;  // הפחתת כמות החיים בהתאם לנזק
        if (currentHP < 0)
        {
            currentHP = 0;
        }
        
        // עדכון ה-HP של השחקן ב-PersistentObjectManager
        PersistentObjectManager.instance.SetPlayerHP(currentHP);
    
        UpdateHPUI();  // Make sure the health slider is updated after taking damage
        
        if (currentHP == 0)
        {
            Die();  // קריאה לפונקציית המוות אם ה-HP הגיע ל-0
        }
    }

	void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;  // עדכון ערך הסליידר ב-UI
        }
    }

    void Die()
    {
        Debug.Log("הדמות מתה!");   // ניתן להוסיף כאן לוגיקה למוות של הדמות
        // לדוגמה, להציג מסך "Game Over"
    }

	void UpdateDamageBasedOnWeapon()
    {
        // עדכון הנזק בהתאם לסוג הנשק
        if (weaponType == 0) // נשק רגיל
        {
            attackDamage = 20;
        }
        else if (weaponType == 1) // חרב
        {
            attackDamage = 70;
        }
        Debug.Log($"Updated damage: {attackDamage}, weaponType: {weaponType}");
    }
}