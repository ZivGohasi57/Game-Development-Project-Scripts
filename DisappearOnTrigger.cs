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
            if (audio != null)
            {
                audio.Play();
            }

            StartCoroutine(DisableAfterSound());
        }
    }

    private IEnumerator DisableAfterSound()
    {
        yield return new WaitForSeconds(audio.clip.length);

        gameObject.SetActive(false);
    }
}
