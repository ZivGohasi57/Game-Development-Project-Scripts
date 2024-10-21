using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearAndDisappearOnTrigger : MonoBehaviour
{
    public GameObject objectToDisappear; // האובייקט שצריך להיעלם
    public AudioSource appearSound; // מקור השמע עבור הצליל שיופיע

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // האובייקט הנוכחי יופיע (מופעל מחדש)
            gameObject.SetActive(true);

            // האובייקט האחר ייעלם
            if (objectToDisappear != null)
            {
                objectToDisappear.SetActive(false);
            }

            // נגן את צליל ההופעה
            if (appearSound != null)
            {
                appearSound.Play();
            }
        }
    }
}

