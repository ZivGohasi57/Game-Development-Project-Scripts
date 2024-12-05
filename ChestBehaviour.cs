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
        if (itemInsideChest != null)
        {
            itemInsideChest.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isChestOpened)
        {
            OpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void OpenChest()
    {
        if (chestTopAnimator != null)
        {
            chestTopAnimator.SetTrigger("Open");
            isChestOpened = true;

            if (itemInsideChest != null)
            {
                itemInsideChest.SetActive(true);
            }
        }
    }
}
