using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestBehaviour : MonoBehaviour
{
    private bool isPlayerInRange = false;
    public Animator chestTopAnimator;
    public GameObject itemInsideChest;
    private bool isChestOpened = false;

    void Start()
    {
        if (chestTopAnimator == null)
        {
            Debug.LogError("Animator for chest top is not assigned!");
        }

        if (itemInsideChest != null)
        {
            itemInsideChest.SetActive(false);
        }
    }

    void Update()
    {
        // בודקים אם השחקן נמצא בטווח ולוחץ על E
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isChestOpened)
        {
            Debug.Log("Player pressed E and is in range. Opening chest...");
            OpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered the range of the chest.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left the range of the chest.");
        }
    }

    private void OpenChest()
    {
        if (chestTopAnimator != null)
        {
            Debug.Log("Setting Trigger 'Open' in Animator.");
            chestTopAnimator.SetTrigger("Open");
            isChestOpened = true;

            if (itemInsideChest != null)
            {
                itemInsideChest.SetActive(true);
                Debug.Log("Item inside the chest is now visible.");
            }
        }
        else
        {
            Debug.LogError("No Animator found for the chest top.");
        }
    }
}