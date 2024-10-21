using System.Collections;
using UnityEngine;

public class Jar : MonoBehaviour
{
    public GameObject brokenJar;  // חבית שבורה שתופעל לאחר השבירה
    public GameObject hiddenItem;  // האובייקט הנסתר (מפתח/זהב)
    public string jarId;  // מזהה ייחודי לחבית

    private bool isBroken = false;  // מניעת שבירה כפולה

    private void Start()
    {
        jarId = $"{gameObject.name}_{transform.position}";

        // בדיקה אם החבית כבר נשברה בעבר
        if (PersistentObjectManager.instance != null &&
            PersistentObjectManager.instance.IsContainerOpen(jarId))
        {
            ActivateBrokenJar();  // הפעלת החבית השבורה עם האובייקט הנסתר
        }
        else
        {
            brokenJar.SetActive(false);  // הסתרת החבית השבורה בהתחלה
            if (hiddenItem != null)
            {
                hiddenItem.SetActive(false);  // הסתרת הפריט הנסתר
            }
        }
    }

    public void Break()
    {
        if (!isBroken)
        {
            isBroken = true;
            Debug.Log("Jar is breaking...");  // דיבאג - שבירת החבית
            StartCoroutine(BreakAfterDelay(0.8f));  // השהיה לפני השבירה
        }
    }

    private IEnumerator BreakAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // השהיה

        if (hiddenItem != null)
        {
            hiddenItem.SetActive(true);  // הפעלת הפריט הנסתר
            Debug.Log($"Activating {hiddenItem.name}...");  // דיבאג
        }
        else
        {
            Debug.LogError("No hidden item assigned to the jar!");
        }

        PersistentObjectManager.instance?.SetContainerOpen(jarId);  // שמירת מצב החבית כשבורה
        ActivateBrokenJar();  // הפעלת החבית השבורה
    }

    private void ActivateBrokenJar()
    {
        if (brokenJar != null)
        {
            brokenJar.SetActive(true);  // הפעלת החבית השבורה
            Debug.Log("Broken jar activated.");  // דיבאג

            // קבלת רכיב AudioSource מההורה
            AudioSource parentAudioSource = transform.parent.GetComponent<AudioSource>();
            if (parentAudioSource != null)
            {
                // השמעת הסאונד של השבר
                parentAudioSource.PlayOneShot(parentAudioSource.clip); // או כאן תשים את הקליפ המתאים
            }
            else
            {
                Debug.LogError("No AudioSource found on the parent object!"); // דיבאג
            }
        }

        gameObject.SetActive(false);  // הסתרת החבית השלמה
    }
}