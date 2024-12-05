using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalBehaviour : MonoBehaviour
{
    public string targetSceneName; 
    public Image fadeImage;
    public float fadeDuration = 1f; 

    private void Start()
    {
        Color color = fadeImage.color;
        color.a = 0; 
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(true); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PersistentObjectManager.instance.GetLastScene();
            SceneManager.LoadScene(targetSceneName); 
            MissionManager missionManager = FindObjectOfType<MissionManager>();

            if (missionManager != null)
            {
                missionManager.TriggerNextMission();
            }

            StartCoroutine(TransitionToScene());
        }
    }

    private IEnumerator TransitionToScene()
    {
        yield return Fade(1); 

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SceneManager.LoadScene(1);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SceneManager.LoadScene(0);
        }

        yield return Fade(0);
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
            yield return null; 
        }

        Color finalColor = fadeImage.color;
        finalColor.a = targetAlpha;
        fadeImage.color = finalColor;
    }
}
