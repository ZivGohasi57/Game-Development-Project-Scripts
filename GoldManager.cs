using UnityEngine;
using UnityEngine.UI;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;  // Singleton Instance

    public Text goldText;  // טקסט המציג את כמות הזהב
    private int currentGold = 0;  // כמות הזהב שהשחקן אסף

    private void Awake()
    {
        // יצירת Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // שמור את האובייקט בין סצנות
        }
        else
        {
            Destroy(gameObject);  // אם כבר קיים, מחק את האובייקט החדש
        }
    }

    private void Start()
    {
        UpdateGoldUI();  // עדכון ממשק המשתמש בהתחלה
    }

    public void AddGold(int amount)
    {
        currentGold += amount;  // הוספת זהב
        UpdateGoldUI();  // עדכון הטקסט בקנבס
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + currentGold;  // הצגת הכמות העדכנית
        }
        else
        {
            Debug.LogError("Gold Text לא מחובר ב-Inspector.");
        }
    }
}