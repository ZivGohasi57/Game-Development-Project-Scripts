using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearOnTrigger : MonoBehaviour
{
    public AudioSource audio;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // נגן את הצליל לפני שהאובייקט נעלם
            if (audio != null)
            {
                audio.Play();
            }

            // הפוך את האובייקט לבלתי נראה אחרי שהשמע התחיל
            StartCoroutine(DisableAfterSound());
        }
    }

    // קורוטינה שמחכה עד שהשמע ינוגן לפני שהאובייקט נעלם
    private IEnumerator DisableAfterSound()
    {
        // חכה למשך הזמן של הצליל
        yield return new WaitForSeconds(audio.clip.length);

        // האובייקט נעלם
        gameObject.SetActive(false);
    }
}