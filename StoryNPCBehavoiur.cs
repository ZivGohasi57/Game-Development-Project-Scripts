using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNPCBehaviour : MonoBehaviour
{
    public int state = 0;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip interactionSound;
    public float interactionClipLength = 5f;
    public Transform playerTransform;

    private bool playerNearby = false;
    private bool hasToldStory = false;

    public PlayerBehaviour playerBehaviour;
    public GateController gateController; // הוספת חיבור ל-GateController

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
        }
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned!");
        }
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned!");
        }
        if (playerBehaviour == null)
        {
            Debug.LogError("PlayerBehaviour is not assigned!");
        }
        if (gateController == null)
        {
            Debug.LogError("GateController is not assigned!");
        }

        animator.SetInteger("state", state);
    }

    void Update()
    {
        if (playerNearby && state != 2)
        {
            LookAtPlayer();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasToldStory)
        {
            playerNearby = true;

            state = 1;
            animator.SetInteger("state", state);
            Debug.Log("State changed to 1");

            if (audioSource != null && interactionSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = interactionSound;
                audioSource.Play();
                Debug.Log("Playing interaction sound.");
            }

            StartCoroutine(CompleteStory());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            StopAllCoroutines();
            ResetToState2();
        }
    }

    private IEnumerator CompleteStory()
    {
        Debug.Log("CompleteStory started.");

        if (interactionSound != null && audioSource != null)
        {
            audioSource.clip = interactionSound;
            audioSource.Play();
            Debug.Log("Playing interaction sound.");
        }
        else
        {
            Debug.LogError("AudioSource or interactionSound is missing!");
        }

        float waitTime = interactionSound != null ? interactionSound.length : interactionClipLength;
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Interaction sound completed.");

        hasToldStory = true;

        if (playerBehaviour != null)
        {
            playerBehaviour.CompleteInteractionWithNPC();
            Debug.Log("Player interaction complete, boolean updated.");
            playerBehaviour.TriggerPlayerResponseAfterStory(); // Trigger the player response

            // עדכון GateController שהשחקן דיבר עם ה-NPC
            if (gateController != null)
            {
                gateController.SetHasTalkedToNPC(true);
                Debug.Log("GateController updated: hasTalkedToNPC set to true.");
            }
            else
            {
                Debug.LogError("GateController is not assigned or null!");
            }
        }
        else
        {
            Debug.LogError("PlayerBehaviour is not assigned or null!");
        }

        ResetToState2();
    }

    private void ResetToState2()
    {
        if (animator != null)
        {
            state = 2;
            animator.SetInteger("state", state);
            Debug.Log("State changed to 2.");
        }
        else
        {
            Debug.LogError("Animator is not assigned or missing!");
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);
    }
}