using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMenuController : MonoBehaviour
{
    public Text centerText;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Image fadeImage; 
    public float fadeDuration = 1f;

    private void Start()
    {
        fadeImage.raycastTarget = false;

        level1Button.onClick.AddListener(() => StartFadeAndLoadScene("SampleScene"));
        level2Button.onClick.AddListener(() => StartFadeAndLoadScene("SampleScene"));
        level3Button.onClick.AddListener(() => StartFadeAndLoadScene("SampleScene"));

        level1Button.gameObject.AddComponent<ButtonEvents>().onHover += () => UpdateCenterText("A calm adventure awaits. No intense challenges, just a journey for those who wish to experience the world and its tales. Perfect for explorers and those who seek to immerse themselves without the pressure of combat.");
        level2Button.gameObject.AddComponent<ButtonEvents>().onHover += () => UpdateCenterText("A balance of adventure and challenge. Step into a world where your wits and your blade are equally important. For those who enjoy a mix of story-driven exploration and thrilling encounters.");
        level3Button.gameObject.AddComponent<ButtonEvents>().onHover += () => UpdateCenterText("A relentless gauntlet of danger. Only the brave and the bold will survive this brutal challenge. Every step could be your last. Are you ready to face a world where death lurks in every shadow?");

    }

    private void UpdateCenterText(string newText)
    {
        centerText.text = newText;
    }

    private void StartFadeAndLoadScene(string sceneName)
    {
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    IEnumerator FadeOutAndLoadScene(string sceneName)
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

        SceneManager.LoadScene(sceneName);
    }
}
