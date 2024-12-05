using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathScreenManager : MonoBehaviour
{
    private string lastSceneName;
    public Button respown
    public Button exit;  
    public Image fadeImage;
    public float fadeDuration = 1f; 

    void Start()
    {
        fadeImage.raycastTarget = false;

        lastSceneName = PlayerPrefs.GetString("LastScene", "DefaultScene");

        respown.onClick.AddListener(() => StartFadeAndRespawn());
        exit.onClick.AddListener(() => StartFadeAndExitToOpeningScreen());
    }

    private void StartFadeAndRespawn()
    {
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeOutAndRespawn());
    }

    private IEnumerator FadeOutAndRespawn()
    {
        float currentTime = 0f;
        Color fadeColor = fadeImage.color;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, currentTime / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        lastSceneName = PersistentObjectManager.instance.GetLastScene();
        PersistentObjectManager.instance.RespawnLife();
        
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            SceneManager.LoadScene(lastSceneName);
        }
    }

    private void StartFadeAndExitToOpeningScreen()
    {
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeOutAndExitToOpeningScreen());
    }

    private IEnumerator FadeOutAndExitToOpeningScreen()
    {
        float currentTime = 0f;
        Color fadeColor = fadeImage.color;

        PersistentObjectManager.instance.ClearData();
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, currentTime / fadeDuration);
            fadeImage.color = fadeColor; 
            yield return null;
        }
        SceneManager.LoadScene("OpeningScreen");
    }
}
