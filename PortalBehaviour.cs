using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalBehaviour : MonoBehaviour
{
    public Image fadeImage;  // התמונה שתשמש לפייד
    public float fadeDuration = 1f;  // משך הזמן של הפייד

    private void Start()
    {
        
    
        // Ensure the fade is invisible at the start
        Color color = fadeImage.color;
        color.a = 0;  // Full transparency
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(true);  // Ensure the object is active
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // רק אם השחקן נכנס
        {
            StartCoroutine(TransitionToScene());
        }
    }

    private IEnumerator TransitionToScene()
    {
        // פייד אוט - הכנס את התמונה
        yield return Fade(1);  // פייד ל-1 (שחור)

        // טען את הסצנה המתאימה
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SceneManager.LoadScene(1);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SceneManager.LoadScene(0);
        }

        // פייד אין - הוצא את התמונה
        yield return Fade(0);  // פייד ל-0 (שקוף)
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
            yield return null;  // המתן עד לפריים הבא
        }

        // לוודא שהאלפא הוא בדיוק מה שרצינו בסוף
        Color finalColor = fadeImage.color;
        finalColor.a = targetAlpha;
        fadeImage.color = finalColor;
    }
}