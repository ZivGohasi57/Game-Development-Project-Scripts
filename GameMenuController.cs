using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMenuController : MonoBehaviour
{
    public Text centerText; // טקסט מרכזי
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Image fadeImage; // Image עבור ה-fade
    public float fadeDuration = 1f; // משך זמן ה-fade

    private void Start()
    {
        // ביטול היכולת של ה-Image לחסום לחיצות על כפתורים
        fadeImage.raycastTarget = false;

        // מחברים את הפונקציות לאירועים של העכבר עבור כל כפתור
        level1Button.onClick.AddListener(() => StartFadeAndLoadScene("SampleScene"));
        level2Button.onClick.AddListener(() => StartFadeAndLoadScene("SampleScene"));
        level3Button.onClick.AddListener(() => StartFadeAndLoadScene("SampleScene"));

        // מוסיפים Listeners שמקשיבים לאירועי העכבר (Mouse Enter/Exit)
        level1Button.gameObject.AddComponent<ButtonEvents>().onHover += () => UpdateCenterText("A calm adventure awaits. No intense challenges, just a journey for those who wish to experience the world and its tales. Perfect for explorers and those who seek to immerse themselves without the pressure of combat.");
        level2Button.gameObject.AddComponent<ButtonEvents>().onHover += () => UpdateCenterText("A balance of adventure and challenge. Step into a world where your wits and your blade are equally important. For those who enjoy a mix of story-driven exploration and thrilling encounters.");
        level3Button.gameObject.AddComponent<ButtonEvents>().onHover += () => UpdateCenterText("A relentless gauntlet of danger. Only the brave and the bold will survive this brutal challenge. Every step could be your last. Are you ready to face a world where death lurks in every shadow?");

    }

    private void UpdateCenterText(string newText)
    {
        centerText.text = newText;
    }

    // פונקציה שמתחילה את אפקט ה-fade ואז טוענת את הסצנה
    private void StartFadeAndLoadScene(string sceneName)
    {
        // הפעלת raycast target כדי לחסום לחיצות במהלך ה-fade
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // קורוטינה עבור אפקט fade ולאחריו טעינת הסצנה
    IEnumerator FadeOutAndLoadScene(string sceneName)
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

        // טען את הסצנה לאחר שה-fade מסתיים
        SceneManager.LoadScene(sceneName);
    }
}