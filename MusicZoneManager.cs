using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZoneManager : MonoBehaviour
{
    private AudioSource currentMusicSource;
    public float fadeDuration = 1f; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            AudioSource newMusicSource = GetComponent<AudioSource>();

            if (newMusicSource != currentMusicSource) 
            {
                StartCoroutine(SwitchMusic(newMusicSource));
            }
        }
    }

    private IEnumerator SwitchMusic(AudioSource newMusicSource)
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut(currentMusicSource, fadeDuration));
            currentMusicSource.Stop();
            currentMusicSource.volume = 0.25f; 
        }

        currentMusicSource = newMusicSource;
        if (currentMusicSource != null)
        {
            yield return StartCoroutine(FadeIn(currentMusicSource, fadeDuration));
        }
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume; 
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / duration); 
            yield return null;
        }

        audioSource.volume = 0; 
    }

    private IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        audioSource.volume = 0; 
        audioSource.Play();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, 0.25f, elapsedTime / duration); 
            yield return null;
        }

        audioSource.volume = 0.25f; 
    }
}
