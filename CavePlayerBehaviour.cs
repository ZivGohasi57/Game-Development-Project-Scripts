using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // לשימוש במעבר בין סצנות


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

    public LayerMask enemyLayer;  // שכבת האויבים
    public List<Collider> attackColliders; // רשימה של קוליידרים עבור התקפות שונות
    public float attackDamage = 0; // כמות הנזק שהשחקן נותן

    CharacterController controller;
    float speed = 10f;
    float runSpeed = 20f;
    float combatWalkSpeed = 5f; // מהירות הליכה במצב קרב
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

    public int weaponType; // סוג הנשק, 0 - נשק רגיל, 1 - חרב
    public float damage;  // משתנה לכמות הנזק
    public Image fadeImage;  // Image עבור אפקט fade
    public float fadeDuration = 1f;

    private string currentSceneName = "CaveScene";     // שם הסצנה הנוכחית

    public Image topEdge;
    public Image bottomEdge;
    public Image leftEdge;
    public Image rightEdge;
    public float lowHpThreshold = 40f;
    public float maxEdgeAlpha = 0.5f;
    private bool isBlinking = false;

    public enum WeaponType { None = -1, Fists = 0, Sword = 1 }

    public WeaponType currentWeapon = WeaponType.None; // שדה הפך ל-public כדי לאפשר גישה מבחוץ
    public bool hasFists = false; // שדה הפך ל-public כדי לאפשר גישה מבחוץ
    public bool hasSword = false; // שדה הפך ל-public כדי לאפשר גישה מבחוץ
    public bool isAttacking = false;


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
            sword_in_hand.SetActive(true);  // הצגת החרב ביד השחקן
        }
        else
        {
            sword_in_hand.SetActive(false); // כיבוי תצוגת החרב אם היא לא הנשק הנבחר
        }


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
        DisableAllAttackColliders(); // לוודא שכל הקוליידרים כבויים בהתחלה
		currentHP = maxHP;           // מתחילים עם כמות החיים המקסימלית
        UpdateHPUI();                // עדכון ה-UI בתחילת המשחק
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
        isAttacking = true; // לנעילת התנועה
        Debug.Log("Single Attack");
        animator.SetTrigger("SingleAttack");
        StartCoroutine(AttackAnimationLock(1f)); // מנעילת תנועה עד לסיום האנימציה
        StartCoroutine(ActivateAttackColliders()); // הפעלת כל הקוליידרים לזמן קצר כדי לפגוע באויב
		
		if (currentJar != null) 
		{ 
            Jar jarScript = currentJar.GetComponent<Jar>(); // בדיקה אם יש חבית בטווח
            if (jarScript != null)
            {
                jarScript.Break(); // קריאה לפונקציית השבירה של החבית
            }
		}

        // נבדוק אם יש אויב בטווח
        if (currentEnemy != null)
        {
            AttackEnemy(currentEnemy, attackDamage); // כאן השימוש בנזק שמתעדכן ב-SwitchWeapon
        }
    }

    void ExecuteComboAttack()
    {
        isAttacking = true; // לנעילת התנועה
        Debug.Log("Combo Attack");
        animator.SetTrigger("ComboAttack");
        StartCoroutine(AttackAnimationLock(1f));
        StartCoroutine(ActivateAttackColliders()); // הפעלת כל הקוליידרים לזמן קצר כדי לפגוע באויב
		if (currentJar != null) 
		{ 
			Jar jarScript = currentJar.GetComponent<Jar>(); // בדיקה אם יש חבית בטווח
            if (jarScript != null)
            {
                jarScript.Break(); // קריאה לפונקציית השבירה של החבית
            }
		}

        // נבדוק אם יש אויב בטווח
        if (currentEnemy != null)
        {
            AttackEnemy(currentEnemy, attackDamage); // כאן השימוש בנזק שמתעדכן ב-SwitchWeapon
        }
    }
    
    IEnumerator AttackAnimationLock(float extraWaitTime)
    {
        // המתן עד שהאנימציה של ההתקפה תסתיים
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + extraWaitTime);
        isAttacking = false; // שחרור הנעילה
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
        float currentSpeed;

        // קביעת מהירות לפי מצב הקרב
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
		AddWeapon("Sword");
		PersistentObjectManager.instance.SetHasSword(true);
		SwitchWeapon(WeaponType.Sword);

        int newWeaponType = 1; // עדכון סוג הנשק
        animator.SetInteger("WeaponType", newWeaponType);

        // עדכון PersistentObjectManager
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetWeaponType(newWeaponType);
        }

        pickText.gameObject.SetActive(false);

        // הוספת הקריאה ל-MissionManager להתקדמות המשימה
        MissionManager missionManager = FindObjectOfType<MissionManager>();
        if (missionManager != null)
        {
            missionManager.AdvanceMission();
            Debug.Log("המשימה התקדמה לאחר איסוף החרב!");
        }
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

        // עדכון ה-UI
        UpdateHPUI(); 
		UpdateEdgeEffect();

        // בדוק אם החיים הגיעו ל-0
        if (currentHP == 0)
        {
            Debug.Log("החיים של השחקן הגיעו ל-0, קריאה לפונקציית Die()");
            Die();  // קריאה לפונקציית המוות
        }
    }

    void Die()
    {
        Debug.Log("הדמות מתה!");

        // הפעלת אנימציית מוות
        animator.SetTrigger("Die"); // שינוי המשתנה הבוליאני ל-true
        Debug.Log("האנימציה של המוות הופעלה");

        // קריאה לפונקציה שממתינה לסיום האנימציה ואז טוענת את מסך המוות עם אפקט fade
        StartCoroutine(WaitForDeathAnimation());
    }

	void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;  // עדכון ערך הסליידר ב-UI
        }
    }

    

    IEnumerator WaitForDeathAnimation()
    {
        // זמן השהיה למשך זמן האנימציה (בהתאם לאורך האנימציה)
        float deathAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;
        
        // ממתין עד לסיום האנימציה
        yield return new WaitForSeconds(deathAnimationTime);

        // הפעלת אפקט fade
        yield return StartCoroutine(FadeOut(fadeDuration));

        // מעבר לסצנת מסך המוות
        SceneManager.LoadScene("DeathScreen");
    }

    IEnumerator FadeOut(float duration)
    {
        float currentTime = 0f;
        Color fadeColor = fadeImage.color;  // קבלת צבע ה-Image

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, currentTime / duration);  // העלאת השקיפות בהדרגה
            fadeImage.color = fadeColor;  // עדכון צבע ה-Image
            yield return null;
        }
    }

	


	IEnumerator BlinkEdgeEffect()
        {
        isBlinking = true;
        float blinkDuration = 0.5f;  // משך כל מעבר
        float minAlpha = 0f;
        float maxAlpha = maxEdgeAlpha;
        bool increasing = true;  // עוקב אחר כיוון השקיפות
    
        while (currentHP <= lowHpThreshold)  // ממשיך להבהב כל עוד ה-HP מתחת לסף
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
    
            increasing = !increasing;  // הפוך את הכיוון
        }
    
        SetEdgeEffect(0f);  // לאפס את האפקט כאשר יוצאים מהלולאה
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
            // כאשר ה-HP גבוה יותר מהסף, כבה את האפקט
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
        // חיבור החיים החדשים לערך הנוכחי עד לתקרה של maxHP
        currentHP = Mathf.Min(currentHP + healthToAdd, maxHP);
        
        // עדכון ה-HP של השחקן ב-PersistentObjectManager
        PersistentObjectManager.instance.SetPlayerHP(currentHP);
    
        // עדכון התצוגה על ה-UI
        UpdateHPUI();
    }

	public void AddWeapon(string weaponName)
    {
        if (weaponName == "Fists" && !hasFists && !hasSword) // Adding fists only if no sword or fists are added
        {
            hasFists = true;
            Debug.Log("נוסף נשק: אגרופים");
            SwitchWeapon(WeaponType.Fists);
            animator.SetInteger("WeaponType", (int)currentWeapon);
			PersistentObjectManager.instance.SetHasFists(true);
        }
        else if (weaponName == "Sword" && hasFists && !hasSword) // Adding sword only if fists are already there and sword is not
        {
            hasSword = true;
            Debug.Log("נוסף נשק: חרב");
            SwitchWeapon(WeaponType.Sword);
            animator.SetInteger("WeaponType", (int)currentWeapon);
			PersistentObjectManager.instance.SetHasSword(true);
        }
        else
        {
            Debug.Log($"נשק {weaponName} אינו זמין להוספה.");
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
    
        // אם הנשק הנוכחי הוא חרב, הצג את החרב ביד השחקן
        if (currentWeapon == WeaponType.Sword)
        {
            sword_in_hand.SetActive(true);  // הצגת החרב ביד
			attackDamage = 70f;
            Debug.Log("חרב ביד השחקן.");
        }
        else
        {
            sword_in_hand.SetActive(false);  // הסתרת החרב מהיד
			attackDamage = 20f;
            Debug.Log("חרב הוסרה מהיד.");
        }
    
        Debug.Log($"Picked weapon {currentWeapon} damage {attackDamage}");
    }
}
	
