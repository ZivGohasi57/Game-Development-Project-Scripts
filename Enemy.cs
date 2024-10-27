using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;   
using TMPro;


public class Enemy : MonoBehaviour
{
    public string enemyId;
    public float maxHP = 100f;
    public float currentHP;
    public float speed = 6f;
    public float attackRange = 1.5f;
    public float chaseRange = 10f;
    public Animator animator;
    public Transform player;
    public int punchVariations = 3;
    public Collider hitCollider;
    public Door door;
    public float attackDamage = 10f;
    public List<Collider> attackColliders;
    public float attackCooldown = 1f;

    public Slider hpSlider;
    public Canvas enemyCanvas;
    public float updateDelay = 0.5f;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool canAttack = true;
    private float targetHP;
    private float hitCooldown = 0.5f;
    private bool canBeHit = true;
	public Door taskDoor; // דלת שנפתחת לאחר מותו של האויב
    public bool isFinalBoss = false; // משתנה לסימון אם זה הבוס הסופי
	public AudioClip chaseSound;  // הסאונד שישמיע בזמן רדיפה
    private AudioSource audioSource;
	private bool isChasing = false;  // משתנה בוליאני לעקוב אחרי מצב רדיפה
	public bool needToMakeMeTalk;  // משתנה בו��י��ני להפ��י�� את הרדי��ה
	public Color normalColor = Color.green;   // צבע ברירת מחדל למעל 40% חיים
    public Color lowHpColor = Color.yellow;   // צבע כתום בין 20% ל-40%
    public Color criticalHpColor = Color.red; // צבע אדום מתחת ל-20%צבע אדום מתחת ל-
    public TMP_Text hpText;  // Reference to TextMeshPro text component for HP







    void Start()
    {
		audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null && chaseSound != null)
        {
            audioSource.clip = chaseSound;
            audioSource.loop = true;  // הסאונד יחזור על עצמו במהלך הרדיפה
        }
        // יצירת מזהה ייחודי אם enemyId ריק
        if (string.IsNullOrEmpty(enemyId))
        {
            // אם יש מזהה שמור ב-PlayerPrefs, טען אותו
            enemyId = PlayerPrefs.GetString(gameObject.name + "_enemyId", Guid.NewGuid().ToString());
            
            // שמירת המזהה ב-PlayerPrefs לשימוש עתידי
            PlayerPrefs.SetString(gameObject.name + "_enemyId", enemyId);
        }

        // בדוק אם האויב כבר מת לפי המזהה ב-PersistentObjectManager
        if (PersistentObjectManager.instance != null && PersistentObjectManager.instance.IsEnemyDead(enemyId))
        {
            // השבתת האובייקט אם הוא מסומן כמת
            gameObject.SetActive(false);
            return;
        }

        currentHP = maxHP;
        targetHP = maxHP;
        UpdateHPUI();
    }

    void Update()
    {
        if (isDead) return;

        // הפניית סרגל החיים לכיוון המצלמה
        if (enemyCanvas != null)
        {
            enemyCanvas.transform.LookAt(Camera.main.transform);
            enemyCanvas.transform.Rotate(0, 180, 0);  // סיבוב של 180 מעלות כך שהקאנבס יפנה למצלמה
        }

        if (!door.isUnlocked) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > attackRange && !isAttacking)
            {
				StartChase();
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer <= attackRange && canAttack)
            {
                StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            StopChase();
            StopChasing();
        }
    }

	void StartChase()
    {
        if (!isChasing)
        {
            isChasing = true;
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();  // התחלת הסאונד של הרדיפה
            }
        }
    }

    void StopChase()
    {
        if (isChasing)
        {
            isChasing = false;
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();  // עצירת הסאונד כאשר מפסיק לרדוף
            }
        }
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isPunching", false);

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        transform.position += direction * speed * Time.deltaTime;
    }

    void StopChasing()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
    }

    IEnumerator AttackPlayer()
    {
        
        canAttack = false;
        animator.SetBool("isWalking", false);
        isAttacking = true;
    
        int punch = UnityEngine.Random.Range(0, punchVariations);
        animator.SetInteger("punch", punch);
        animator.SetBool("isPunching", true);
    
        yield return new WaitForSeconds(1f); // השהייה קצרה עד שהמכה מתחילה
    
        EnableAttackColliders(); // הפעלת הקוליידרים בזמן המכה
    
        yield return new WaitForSeconds(1f); // משך הזמן שהקוליידרים פועלים בזמן המכה
    
        DisableAttackColliders(); // כיבוי הקוליידרים בסיום המכה
        isAttacking = false;
        animator.SetBool("isPunching", false); // חזרה למצב רגיל
        yield return new WaitForSeconds(attackCooldown); // השהייה לפני שמכה נוספת אפשרית
        canAttack = true;
    }

    void EnableAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = true;
        }
    }

    void DisableAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = false;
        }
    }

    public void TakeDamage(float damage)
    {
        if (canBeHit)
        {
            targetHP -= damage;

            if (targetHP < 0)
            {
                targetHP = 0;
            }

            StartCoroutine(UpdateHPWithDelay());

            if (targetHP == 0)
            {
                Die();
            }

            canBeHit = false;
            StartCoroutine(HitCooldownRoutine());
        }
    }

    void Die()
    {
        isDead = true;

        // שמירת מצב האויב כמת ב-PersistentObjectManager
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.SetEnemyDead(enemyId);
        }

        // הפעלת אנימציית מוות
        animator.SetTrigger("die");

        // כיבוי הקאנבס של האויב
        if (enemyCanvas != null)
        {
            enemyCanvas.enabled = false;
        }

        // השבתת הקוליידר לאחר מוות
        hitCollider.enabled = false;

		 if (taskDoor != null)
        {
            taskDoor.taskCompleted = true;
            taskDoor.TryOpenDoor();
            Debug.Log("האויב מת - פתיחת דלת המשימה.");
        }

        // אם זה הבוס הסופי, נקדם משימה
        if (isFinalBoss && PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.AdvanceMission();
            Debug.Log("הבוס הסופי מת - קידום המשימה.");
        }

    	StartCoroutine(GradualStopChase());
		if (isFinalBoss && needToMakeMeTalk)
        {
            CavePlayerBehaviour player = FindObjectOfType<CavePlayerBehaviour>();
            if (player != null)
            {
                player.VoiceForestTalk();
            }
        }
    }

	IEnumerator GradualStopChase()
    {
        float delayTime = 2f; // הזמן שנרצה עד לעצירה מלאה
        float initialVolume = audioSource.volume; // עוצמת המוזיקה הנוכחית
    
        float elapsedTime = 0;
        while (elapsedTime < delayTime)
        {
            elapsedTime += Time.deltaTime;
            // הנמכת המוזיקה בהדרגה
            audioSource.volume = Mathf.Lerp(initialVolume, 0, elapsedTime / delayTime);
            yield return null;
        }
    
        // הבטחה שהמוזיקה נעלמת
        audioSource.volume = 0;
        audioSource.Stop(); // עצירת המוזיקה באופן סופי
    
        StopChase(); // עצירה סופית של המרדף
    }
    

    IEnumerator UpdateHPWithDelay()
    {
        float elapsedTime = 0;
        float startHP = currentHP;

        while (elapsedTime < updateDelay)
        {
            elapsedTime += Time.deltaTime;
            currentHP = Mathf.Lerp(startHP, targetHP, elapsedTime / updateDelay);
            UpdateHPUI();
            yield return null;
        }

        currentHP = targetHP;
        UpdateHPUI();
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

        if (hpText != null)
        {
            hpText.text = Mathf.RoundToInt(currentHP) + "/" + Mathf.RoundToInt(maxHP); // Display HP as text
        }
    }
    

    IEnumerator HitCooldownRoutine()
    {
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            CavePlayerBehaviour player = other.GetComponentInParent<CavePlayerBehaviour>();
            if (player != null)
            {
                TakeDamage(player.attackDamage);
                Debug.Log("האויב נפגע! חיים נוכחיים: " + targetHP);
            }
        }
    }

	
}