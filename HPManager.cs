using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HPManager : MonoBehaviour
{
    public Slider hpSlider;  // סרגל HP בקאנבס
    public TextMeshProUGUI hpText;  // טקסט של TextMeshPro שמראה את כמות ה-HP
    public float maxHP = 100f;  // כמות ה-HP המקסימלית
    private float currentHP;  // כמות ה-HP הנוכחית
    private float targetHP;  // כמות ה-HP אליה נרצה להגיע בעדכון הדרגתי

    public float updateDelay = 0.5f;  // זמן הדילאי לעדכון הסרגל
    private Coroutine healthRegenCoroutine;  // משתנה לשמירה על Coroutine של חידוש החיים
    private bool isInCombatMode = false;  // משתנה לבדוק אם השחקן במצב קרב

    public Color normalColor = Color.green;    // צבע ירוק למעל 40% חיים
    public Color lowHpColor = Color.yellow;    // צבע כתום בין 20% ל-40% חיים
    public Color criticalHpColor = Color.red;  // צבע אדום מתחת ל-20% חיים

    void Start()
    {
        currentHP = maxHP;  // התחלת ה-HP במקסימום
        targetHP = maxHP;
        UpdateHPUI();  // עדכון ה-UI בעת התחלת המשחק
    }

    void Update()
    {
        // חידוש אוטומטי של חיים אם ה-HP מתחת ל-30% והדמות לא במצב קרב
        if (!isInCombatMode && currentHP < maxHP * 0.3f)
        {
            if (healthRegenCoroutine == null)
            {
                healthRegenCoroutine = StartCoroutine(RegenerateHealth());
            }
        }
        else if (isInCombatMode || currentHP >= maxHP * 0.3f)
        {
            if (healthRegenCoroutine != null)
            {
                StopCoroutine(healthRegenCoroutine);
                healthRegenCoroutine = null;
            }
        }
    }

    // פונקציה להגדרת מצב הקרב מבחוץ
    public void SetCombatMode(bool isInCombat)
    {
        isInCombatMode = isInCombat;
    }

    // פונקציה לחידוש הדרגתי של החיים
    IEnumerator RegenerateHealth()
    {
        while (currentHP < maxHP * 0.7f && !isInCombatMode)
        {
            currentHP = Mathf.Min(currentHP + 1, maxHP * 0.3f);  // הוספה של 1 HP עד לתקרה של 30%
            UpdateHPUI();  // עדכון UI
            yield return new WaitForSeconds(2f);  // השהייה של 2 שניות לפני תוספת הבאה
        }
    }

    // פונקציה שמעדכנת את תצוגת ה-HP בקאנבס
    public void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;

            // שינוי צבע פס החיים וצבע Handle בהתאם למצב החיים
            Color newColor;
            if (currentHP / maxHP >= 0.4f)
            {
                newColor = normalColor;
            }
            else if (currentHP / maxHP >= 0.2f)
            {
                newColor = lowHpColor;
            }
            else
            {
                newColor = criticalHpColor;
            }
        
            // עדכון צבע ה-fill של הסליידר
            hpSlider.fillRect.GetComponent<Image>().color = newColor;

            // עדכון צבע ה-handle slide area
            if (hpSlider.handleRect != null)
            {
                hpSlider.handleRect.GetComponent<Image>().color = newColor;
            }
        }

        if (hpText != null)
        {
            hpText.text = Mathf.RoundToInt(currentHP) + "/" + Mathf.RoundToInt(maxHP);
        }
    }

    // פונקציה להגדרת ה-HP מבחוץ (משמש את PersistentObjectManager)
    public void SetHP(float newHP)
    {
        targetHP = newHP;
        StartCoroutine(UpdateHPWithDelay());
    }

    // פונקציה שמעדכנת את תצוגת ה-HP בקאנבס בהדרגה
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
}