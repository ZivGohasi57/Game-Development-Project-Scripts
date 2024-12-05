using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearAndDisappearOnTrigger : MonoBehaviour
{
    public GameObject objectToDisappear;
    public AudioSource appearSound; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           
            gameObject.SetActive(true);

            if (objectToDisappear != null)
            {
                objectToDisappear.SetActive(false);
            }

            if (appearSound != null)
            {
                appearSound.Play();
            }
        }
    }
}

