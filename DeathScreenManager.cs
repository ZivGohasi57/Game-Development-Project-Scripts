using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // ייבוא ה-namespace עבור IEnumerator

public class DeathScreenManager : MonoBehaviour
{
    private string lastSceneName;
    public Button respown;  // הגדרת כפתור Respawn
    public Button exit;     // הגדרת כפתור Exit
    public Image fadeImage; // Image עבור אפקט fade
    public float fadeDuration = 1f; // משך זמן ה-fade

    void Start()
    {
        // ביטול היכולת של ה-Image לחסום לחיצות על כפתורים בהתחלה
        fadeImage.raycastTarget = false;

        // קבל את שם הסצנה האחרונה מתוך PlayerPrefs
        lastSceneName = PlayerPrefs.GetString("LastScene", "DefaultScene");

        // הוספת Listeners לכפתורים
        respown.onClick.AddListener(() => StartFadeAndRespawn());
        exit.onClick.AddListener(() => StartFadeAndExitToOpeningScreen());
    }

    // פונקציה עבור כפתור Respawn
    private void StartFadeAndRespawn()
    {
        // הפעלת raycast target כדי לחסום לחיצות במהלך ה-fade
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeOutAndRespawn());
    }

    private IEnumerator FadeOutAndRespawn()
    {
        float currentTime = 0f;
        Color fadeColor = fadeImage.color;

        // העלאת השקיפות בהדרגה
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, currentTime / fadeDuration); // עדכון השקיפות
            fadeImage.color = fadeColor; // הגדרת הצבע של ה-Image
            yield return null;
        }

        // קבלת שם הסצנה האחרונה וטעינתה
        lastSceneName = PersistentObjectManager.instance.GetLastScene();
        PersistentObjectManager.instance.RespawnLife();
        
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            SceneManager.LoadScene(lastSceneName); // טעינת הסצנה האחרונה
        }
        else
        {
            Debug.LogError("No saved scene found!");
        }
    }

    // פונקציה עבור כפתור Exit
    private void StartFadeAndExitToOpeningScreen()
    {
        // הפעלת raycast target כדי לחסום לחיצות במהלך ה-fade
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeOutAndExitToOpeningScreen());
    }

    private IEnumerator FadeOutAndExitToOpeningScreen()
    {
        float currentTime = 0f;
        Color fadeColor = fadeImage.color;

        PersistentObjectManager.instance.ClearData();
        // העלאת השקיפות בהדרגה
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, currentTime / fadeDuration); // עדכון השקיפות
            fadeImage.color = fadeColor; // הגדרת הצבע של ה-Image
            yield return null;
        }

        // טעינת סצנת הפתיחה
        SceneManager.LoadScene("OpeningScreen");
    }
}