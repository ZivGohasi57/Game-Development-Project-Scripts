using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public Door linkedDoor;  

    private bool isInRange = false;

    void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            linkedDoor.hasKey = true; 
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }
}
