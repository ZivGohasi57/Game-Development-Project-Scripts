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
    public GameObject sword_in_hand; // חרב ביד השחקן

    private bool isInCombatMode = false; // מצב הקרב
    private int clickCount = 0;
    private float lastClickTime = 0;
    private float timeBetweenClicks = 0.3f;

    public bool InteractionCompleted { get { return interactionCompleted; } }

    public enum WeaponType { None = -1, Fists = 0, Sword = 1 } // הוספת enum לסוגי הנשקים
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
	private GameObject currentEnemy;  // לשמירת האויב בטווח
    public float maxHP = 100f;       // כמות ה-HP המקסימלית של השחקן
    public float currentHP;          // כמות ה-HP הנוכחית של השחקן
    public Slider hpSlider;          // סליידר המייצג את כמות ה-HP של השחקן
	public LayerMask enemyLayer;  // שכבת האויבים
    public List<Collider> attackColliders; // רשימה של קוליידרים עבור התקפות שונות
	public float attackDamage = 0; // כמות הנזק שהשחקן נותן
	public float fadeDuration = 1f;
    public Image fadeImage;  // Image עבור אפקט fade
	public bool isAttacking = false;
	float combatWalkSpeed = 5f; // מהירות הליכה במצב קרב

  
  void Start()
    {
		int savedWeaponType = PersistentObjectManager.instance.weaponType;
        currentWeapon = (WeaponType)savedWeaponType;
        animator.SetInteger("WeaponType", savedWeaponType);
		SwitchWeapon(currentWeapon);	
		if (currentWeapon == WeaponType.Sword && hasSword)
        {
            sword_in_hand.SetActive(true);  // הצגת החרב ביד השחקן
        }
        else
        {
            sword_in_hand.SetActive(false); // כיבוי תצוגת החרב אם היא לא הנשק הנבחר
        }

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
		
		hasFists = PersistentObjectManager.instance.hasFists;
        hasSword = PersistentObjectManager.instance.hasSword;
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
        HandleWeaponSwitch(); // פונקציה לניהול הלחצנים 1 ו-2
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
    	if (isAttacking) return; // חסימת תנועה אם הדמות במצב התקפה

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
            moveDirection.y = -1f; // משיכה קלה למטה כדי להיצמד לקרקע
    
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
        if (!(Mathf.Abs(dx) < 0.01f && Mathf.Abs(dz) < 0.01f)) // אם השחקן בתנועה
        {
            if (!footStepsAudioSource.isPlaying)
            {
                footStepsAudioSource.Play();
            }
    
            // התאמת מהירות הסאונד לסוג ההליכה
            if (isInCombatMode)
            {
                footStepsAudioSource.pitch = 0.75f; // מהירות סאונד נמוכה להליכה קרבית
            }
            else if (currentSpeed == runSpeed)
            {
                footStepsAudioSource.pitch = 1.5f; // מהירות סאונד גבוהה לריצה
            }
            else
            {
                footStepsAudioSource.pitch = 1.0f; // מהירות סאונד רגילה להליכה
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
    
            // וידוא של השחקן נמצא במצב לחימה
            if (Input.GetMouseButton(1))
            {
                EnterCombatMode();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                ExitCombatMode();
            }
    
            // ביצוע תקיפה רק אם השחקן במצב קרב ויש לו נשק מתאים
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
        isAttacking = true; // לנעילת התנועה
    	Debug.Log("Single Attack");
    	animator.SetTrigger("SingleAttack");
    	StartCoroutine(AttackAnimationLock(1f)); // מנעילת תנועה עד לסיום האנימציה
        StartCoroutine(ActivateAttackColliders()); // הפעלת כל הקוליידרים לזמן קצר כדי לפגוע באויב
		
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
        isAttacking = false;
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

    void HandleWeaponSwitch() // פונקציה לניהול מעבר נשקים עם לחצנים 1 ו-2
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
        // הצגת החרב ביד רק אם בחרנו בחרב
        if (currentWeapon == WeaponType.Sword)
        {
            sword_in_hand.SetActive(true);
        }
        else
        {
            sword_in_hand.SetActive(false);
        }

        Debug.Log($"החלפת נשק ל-{currentWeapon}");
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

	private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))  // נניח שלכל האויבים יש תגית "Enemy"
        {
            currentEnemy = other.gameObject;
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
}