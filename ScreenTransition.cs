using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenTransition : MonoBehaviour
{
    public GameObject player;
    public bool canTrigger = true;
    public Image fadeImage;
    public GameObject newPlayerModel;
    public GameObject currentPlayerModel;
    public Animator newPlayerAnimator;
    public AudioClip selfTalk2;

    private PlayerBehaviour playerBehaviour;
    private GateController gateController;
    private MissionManager missionManager;
    private bool isTransitioning = false;

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        if (player != null)
        {
            playerBehaviour = player.GetComponent<PlayerBehaviour>();
        }

        gateController = FindObjectOfType<GateController>();
        missionManager = FindObjectOfType<MissionManager>();

        if (playerBehaviour == null || gateController == null || missionManager == null)
        {
            Debug.LogError("PlayerBehaviour, GateController or MissionManager not found!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player && canTrigger && !isTransitioning && playerBehaviour != null && playerBehaviour.InteractionCompleted)
        {
            StartCoroutine(FadeTransition());
        }
    }

    private IEnumerator FadeTransition()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeToBlack());

        currentPlayerModel.SetActive(false);
        newPlayerModel.SetActive(true);

        if (newPlayerAnimator != null)
        {
            playerBehaviour.animator = newPlayerAnimator;
        }

        playerBehaviour.HasChangedClothes = true;
        Debug.Log("Player has changed clothes. HasChangedClothes is now true.");

        if (gateController != null)
        {
            gateController.SetCanPassThrough(true);
            Debug.Log("GateController updated: canPassThrough set to true.");
        }

        yield return StartCoroutine(FadeToClear());

        StartCoroutine(PlaySelfTalkAfterDelay());
    }

    private IEnumerator PlaySelfTalkAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (playerBehaviour.selfTalkAudioSource != null && selfTalk2 != null)
        {
            playerBehaviour.selfTalkAudioSource.PlayOneShot(selfTalk2);
            Debug.Log("Playing SelfTalk2 audio.");
            yield return new WaitForSeconds(selfTalk2.length);
            
            // עדכון המשימה למשימה הבאה
            if (missionManager != null)
            {
                missionManager.TriggerNextMission();
                Debug.Log("Mission updated after SelfTalk2.");
            }
        }

        isTransitioning = false;
    }

    private IEnumerator FadeToBlack()
    {
        float duration = 1f;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(currentTime / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeToClear()
    {
        float duration = 1f;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (currentTime / duration));
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}