using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public GameObject player;
    public GameObject gateCollider; // הקוליידר של השער
    public bool canPassThrough = false; 
    public bool hasTalkedToNPC = false; 
    public AudioClip selfTalk3;
    public AudioClip selfTalk4;

    private PlayerBehaviour playerBehaviour;
    private MissionManager missionManager;
    private bool messageDisplayed = false;

    public AudioSource selfTalkAudioSource;

    void Start()
    {
        if (player != null)
        {
            playerBehaviour = player.GetComponent<PlayerBehaviour>();
        }

        missionManager = FindObjectOfType<MissionManager>();

        if (playerBehaviour == null || missionManager == null)
        {
            Debug.LogError("PlayerBehaviour or MissionManager not found!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // בדוק אם שני התנאים מתקיימים
            if (canPassThrough && hasTalkedToNPC)
            {
                missionManager.TriggerNextMission();
                Debug.Log("Player met the conditions. Mission updated to the next one.");
                
                // הסתר את הקוליידר
                if (gateCollider != null)
                {
                    gateCollider.SetActive(false);
                }
            }
            else if (!messageDisplayed)
            {
                if (!hasTalkedToNPC)
                {
                    Debug.Log("Player needs to talk to the NPC first.");
                    PlaySelfTalk3();
                }
                else if (!canPassThrough)
                {
                    Debug.Log("Player needs to change clothes before passing.");
                    PlaySelfTalk4();
                }
                messageDisplayed = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            messageDisplayed = false;
        }
    }

    // פונקציה להגדיר האם השחקן יכול לעבור דרך השער
    public void SetCanPassThrough(bool value)
    {
        canPassThrough = value;
        Debug.Log($"canPassThrough updated to: {canPassThrough}");
    }

    public void SetHasTalkedToNPC(bool value)
    {
        hasTalkedToNPC = value;
        Debug.Log($"hasTalkedToNPC updated to: {hasTalkedToNPC}");
    }

    void PlaySelfTalk3()
    {
        if (selfTalkAudioSource != null && selfTalk3 != null && !selfTalkAudioSource.isPlaying)
        {
            selfTalkAudioSource.PlayOneShot(selfTalk3);
        }
    }

    void PlaySelfTalk4()
    {
        if (selfTalkAudioSource != null && selfTalk4 != null && !selfTalkAudioSource.isPlaying)
        {
            selfTalkAudioSource.PlayOneShot(selfTalk4);
        }
    }

    void DebugState()
    {
        Debug.Log($"CanPassThrough: {canPassThrough}, HasTalkedToNPC: {hasTalkedToNPC}");
    }
}