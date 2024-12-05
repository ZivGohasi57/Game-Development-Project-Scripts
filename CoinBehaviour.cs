using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    public GameObject player;
    public GameObject parent;
    public MissionManager missionManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            gameObject.SetActive(false); 
            AudioSource sound = parent.GetComponent<AudioSource>();
            sound.Play();

            PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
            if (playerBehaviour != null)
            {
                playerBehaviour.CollectDocument(); 
                if (missionManager != null)
                {
                    missionManager.TriggerNextMission();
                    Debug.Log("Mission updated to the next one after collecting document.");
                }
            }
            else
            {
                Debug.LogError("PlayerBehaviour is not found!");
            }
        }
    }
}
