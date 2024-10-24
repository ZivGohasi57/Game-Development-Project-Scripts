using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float maxHP = 100f;      // נקודות חיים של האויב
    public float currentHP;         // נקודות חיים עכשוויות
    public float speed = 6f;        // מהירות התקרבות לשחקן
    public float attackRange = 1.5f; // מרחק התקפה
    public float chaseRange = 10f;  // טווח המרדף (המרחק בו האויב יתחיל לרדוף)
    public Animator animator;       // רפרנס לאנימטור
    public Transform player;        // רפרנס לדמות השחקן
    public int punchVariations = 3; // מספר האנימציות של מכות
    public Collider hitCollider;    // הקוליידר של האויב
    public Door door;               // רפרנס לדלת
    public float attackDamage = 10f;   // כמות הנזק שהאויב גורם
    public List<Collider> attackColliders; // רשימת קוליידרים להתקפה
    public float attackCooldown = 1f;  // זמן ההמתנה בין התקפות

    public Slider hpSlider;         // סליידר המייצג את ה-HP מעל הראש של האויב
    public Canvas enemyCanvas;      // הקאנבס של ה-HP מעל ראש האויב
    public float updateDelay = 0.5f;  // משך הזמן שבו נעדכן את ה-HP בצורה הדרגתית

    private bool isDead = false;    // האם האויב מת
    private bool isAttacking = false; // האם האויב בהתקפה
    private bool canAttack = true;  // משתנה לשליטה על יכולת ההתקפה
    private float targetHP;         // יעד ה-HP לעדכון הדרגתי
    private float hitCooldown = 0.5f; // זמן מינימום בין מכות
    private bool canBeHit = true;    // משתנה לשליטה בקבלת נזק

    void Start()
    {
        currentHP = maxHP;
        targetHP = maxHP;

        // ודא שהסליידר מותאם למצב ההתחלתי
        UpdateHPUI();
    }

    void Update()
    {
        // הקאנבס יפנה תמיד לכיוון המצלמה (לתצוגה ברורה)
        if (enemyCanvas != null)
        {
            enemyCanvas.transform.LookAt(Camera.main.transform);
            enemyCanvas.transform.Rotate(0, 180, 0);  // סיבוב של 180 מעלות כך שהקאנבס יפנה למצלמה
        }

        // בדיקה אם הדלת פתוחה
        if (!door.isUnlocked) return; // אם הדלת סגורה, לא עושים כלום

        if (isDead) return; // אם האויב מת, לא עושים כלום

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // בדיקה אם השחקן בטווח המרדף
        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > attackRange && !isAttacking)
            {
                // האויב מתקרב לשחקן
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer <= attackRange && canAttack)
            {
                // האויב מתחיל לתקוף
                StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            // האויב מפסיק לרדוף אם השחקן מחוץ לטווח המרדף וחוזר לאנימציית idle
            StopChasing();
        }
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true); // שינוי לאנימציה של הליכה
        animator.SetBool("isPunching", false);

        Vector3 direction = (player.position - transform.position).normalized;
    
        // סיבוב הגוף לכיוון השחקן
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // סיבוב חלק

        // התקדמות לכיוון השחקן
        transform.position += direction * speed * Time.deltaTime;
    }

    void StopChasing()
    {
        animator.SetBool("isWalking", false); // עצירת אנימציית הליכה
        animator.SetBool("isIdle", true);     // מעבר לאנימציית idle
    }

    IEnumerator AttackPlayer()
    {
        canAttack = false;  // למנוע התקפה נוספת עד סיום ההתקפה
        animator.SetBool("isWalking", false); // עצירה של אנימציית הליכה
        isAttacking = true;

        int punch = Random.Range(0, punchVariations); // בחירת אנימציית מכה רנדומלית
        animator.SetInteger("punch", punch);
        animator.SetBool("isPunching", true); // שינוי לאנימציית מכות

        EnableAttackColliders(); // הפעלת הקוליידרים כדי שהשחקן ייפגע

        yield return new WaitForSeconds(0.5f); // זמן המכה

        DisableAttackColliders(); // כיבוי הקוליידרים לאחר ההתקפה

        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);  // המתנה בין התקפות
        canAttack = true;
    }

    void EnableAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = true;  // הפעלת הקוליידרים להתקפה
        }
    }

    void DisableAttackColliders()
    {
        foreach (var collider in attackColliders)
        {
            collider.enabled = false;  // כיבוי הקוליידרים לאחר התקפה
        }
    }

    public void TakeDamage(float damage)
    {
        if (canBeHit) // רק אם ניתן להיפגע
        {
            targetHP -= damage;

            if (targetHP < 0)
            {
                targetHP = 0;
            }

            StartCoroutine(UpdateHPWithDelay()); // קריאה לעדכון הדרגתי של HP

            if (targetHP == 0)
            {
                Die();
            }

            canBeHit = false; // מניעת פגיעה נוספת למשך זמן קצר
            StartCoroutine(HitCooldownRoutine()); // התחלת ה-cooldown
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("die"); // הפעלת אנימציית מוות
        hitCollider.enabled = false; // השבתת הקוליידר לאחר מוות
    }

    // עדכון הסליידר של HP בהדרגה
    IEnumerator UpdateHPWithDelay()
    {
        float elapsedTime = 0;
        float startHP = currentHP;

        while (elapsedTime < updateDelay)
        {
            elapsedTime += Time.deltaTime;
            currentHP = Mathf.Lerp(startHP, targetHP, elapsedTime / updateDelay);
            UpdateHPUI();
            yield return null; // המתן פריים אחד לפני המשך הלולאה
        }

        currentHP = targetHP;  // לוודא שה-HP מתעדכן בצורה מדויקת בסוף
        UpdateHPUI();
    }

    void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;  // עדכון הסליידר בהתאם לאחוזי ה-HP
        }
    }

    // פונקציה שמוסיפה זמן מינימלי בין מכות
    IEnumerator HitCooldownRoutine()
    {
        yield return new WaitForSeconds(hitCooldown); // המתן זמן קבוע לפני שניתן להיפגע שוב
        canBeHit = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))  // בדיקה אם הפגיעה באה מהשחקן
        {
            // קבל את הנזק מהשחקן על ידי גישה לסקריפט 'CavePlayerBehaviour'
            CavePlayerBehaviour player = other.GetComponentInParent<CavePlayerBehaviour>();
            if (player != null)
            {
                TakeDamage(player.attackDamage);  // הורדת כמות חיים בהתאם לנזק של השחקן
                Debug.Log("האויב נפגע! חיים נוכחיים: " + targetHP);
            }
        }
    }
}