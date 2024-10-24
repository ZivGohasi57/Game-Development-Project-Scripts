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

    void Start()
    {
        currentHP = maxHP;  // התחלת ה-HP במקסימום
        targetHP = maxHP;
        UpdateHPUI();  // עדכון ה-UI בעת התחלת המשחק
    }

    // פונקציה להגדרת ה-HP מבחוץ (משמש את PersistentObjectManager)
    public void SetHP(float newHP)
    {
        targetHP = newHP;
        StartCoroutine(UpdateHPWithDelay());  // קריאה לעדכון הדרגתי
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
            UpdateHPUI();  // עדכון ה-UI בזמן אמת
            yield return null;  // להמתין פריים אחד לפני המשך הלולאה
        }

        currentHP = targetHP;  // לוודא שהערך הסופי מעודכן בצורה מדויקת
        UpdateHPUI();
    }

    // פונקציה שמעדכנת את תצוגת ה-HP בקאנבס
    public void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            Debug.Log($"Updating HP Slider: {currentHP}/{maxHP}");
            hpSlider.value = currentHP / maxHP;  // עדכון ערך הסליידר בהתאם לערכי ה-HP
        }
        else
        {
            Debug.LogError("HP Slider reference is missing!");
        }

        if (hpText != null)
        {
            hpText.text = Mathf.RoundToInt(currentHP) + "/" + Mathf.RoundToInt(maxHP);  // עדכון ערך הטקסט בהתאם לערכי ה-HP
        }
    }
}