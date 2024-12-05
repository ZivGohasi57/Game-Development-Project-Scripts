using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public GameObject player;
    public GameObject gateCollider;
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canPassThrough && hasTalkedToNPC)
            {
                missionManager.TriggerNextMission();
                
                if (gateCollider != null)
                {
                    gateCollider.SetActive(false);
                }
            }
            else if (!messageDisplayed)
            {
                if (!hasTalkedToNPC)
                {
                    PlaySelfTalk3();
                }
                else if (!canPassThrough)
                {
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

    public void SetCanPassThrough(bool value)
    {
        canPassThrough = value;
    }

    public void SetHasTalkedToNPC(bool value)
    {
        hasTalkedToNPC = value;
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
}
