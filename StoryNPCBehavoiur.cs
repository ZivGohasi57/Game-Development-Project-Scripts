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
    public GateController gateController; 

    void Start()
    {
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

            if (audioSource != null && interactionSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = interactionSound;
                audioSource.Play();
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
        if (interactionSound != null && audioSource != null)
        {
            audioSource.clip = interactionSound;
            audioSource.Play();
        }

        float waitTime = interactionSound != null ? interactionSound.length : interactionClipLength;
        yield return new WaitForSeconds(waitTime);

        hasToldStory = true;

        if (playerBehaviour != null)
        {
            playerBehaviour.CompleteInteractionWithNPC();
            playerBehaviour.TriggerPlayerResponseAfterStory(); // Trigger the player response

            if (gateController != null)
            {
                gateController.SetHasTalkedToNPC(true);
            }

        }
        ResetToState2();
    }

    private void ResetToState2()
    {
        if (animator != null)
        {
            state = 2;
            animator.SetInteger("state", state);
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
