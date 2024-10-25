using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;   

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

    void Start()
    {
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
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer <= attackRange && canAttack)
            {
                StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            StopChasing();
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

        EnableAttackColliders();

        yield return new WaitForSeconds(0.5f);

        DisableAttackColliders();
        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
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