using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZoneManager : MonoBehaviour
{
    private AudioSource currentMusicSource; // המוזיקה שמתנגנת כרגע
    public float fadeDuration = 1f; // זמן המעבר בין קטעי המוזיקה

    // פונקציה להפעלת המוזיקה כשנכנסים לאזור חדש
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // בדוק אם השחקן נכנס לאזור
        {
            AudioSource newMusicSource = GetComponent<AudioSource>(); // מקבל את ה-AudioSource של האזור הנוכחי

            if (newMusicSource != currentMusicSource) // אם נכנסים לאזור עם מוזיקה אחרת
            {
                // התחלת תהליך של החלפת מוזיקה
                StartCoroutine(SwitchMusic(newMusicSource));
            }
        }
    }

    // פונקציה להחלפת מוזיקה עם Fade Out ו-Fade In
    private IEnumerator SwitchMusic(AudioSource newMusicSource)
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying) // אם יש מוזיקה שמתנגנת כרגע
        {
            yield return StartCoroutine(FadeOut(currentMusicSource, fadeDuration)); // ביצוע Fade Out למוזיקה הישנה
            currentMusicSource.Stop(); // ודא שהמוזיקה הקודמת נעצרת לחלוטין
            currentMusicSource.volume = 0.25f; // איפוס עוצמת המוזיקה לקודמת (במידה ונרצה להשמיע שוב בעתיד)
        }

        currentMusicSource = newMusicSource; // עדכון ה- AudioSource הנוכחי
        if (currentMusicSource != null)
        {
            yield return StartCoroutine(FadeIn(currentMusicSource, fadeDuration)); // ביצוע Fade In למוזיקה החדשה
        }
    }

    // פונקציה להנמכת עוצמת מוזיקה (Fade Out) בטווח של 0 ל-0.25
    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume; // שמירת הווליום ההתחלתי (0.25)
        float elapsedTime = 0f; // אתחול הזמן שחלף

        // לולאה להנמכת עוצמת השמע לאורך זמן
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / duration); // הנמכה הדרגתית של הווליום מ-0.25 ל-0
            yield return null;
        }

        audioSource.volume = 0; // בסיום הווליום יהיה 0
    }

    // פונקציה להעלאת עוצמת מוזיקה (Fade In) בטווח של 0 ל-0.25
    private IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        audioSource.volume = 0; // התחל מעוצמה 0
        audioSource.Play(); // הפעל את האודיו
        float elapsedTime = 0f; // אתחול הזמן שחלף

        // לולאה להעלאת עוצמת השמע לאורך זמן
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, 0.25f, elapsedTime / duration); // העלאה הדרגתית של הווליום מ-0 ל-0.25
            yield return null;
        }

        audioSource.volume = 0.25f; // בסיום הווליום יהיה 0.25
    }
}