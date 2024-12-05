using System.Collections;
using UnityEngine;

public class Jar : MonoBehaviour
{
    public GameObject brokenJar;  
    public GameObject hiddenItem; 
    public string jarId; 

    private bool isBroken = false;
    private void Start()
    {
        jarId = $"{gameObject.name}_{transform.position}";

      
        if (PersistentObjectManager.instance != null &&
            PersistentObjectManager.instance.IsContainerOpen(jarId))
        {
            ActivateBrokenJar(); 
        }
        else
        {
            brokenJar.SetActive(false);
            if (hiddenItem != null)
            {
                hiddenItem.SetActive(false);
            }
        }
    }

    public void Break()
    {
        if (!isBroken)
        {
            isBroken = true;
            Debug.Log("Jar is breaking...");
            StartCoroutine(BreakAfterDelay(0.8f)); 
        }
    }

    private IEnumerator BreakAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 

        if (hiddenItem != null)
        {
            hiddenItem.SetActive(true);
        }

        PersistentObjectManager.instance?.SetContainerOpen(jarId); 
        ActivateBrokenJar(); 
    }

    private void ActivateBrokenJar()
    {
        if (brokenJar != null)
        {
            brokenJar.SetActive(true); 

            AudioSource parentAudioSource = transform.parent.GetComponent<AudioSource>();
            if (parentAudioSource != null)
            {
                parentAudioSource.PlayOneShot(parentAudioSource.clip); 
            }
        }

        gameObject.SetActive(false);
    }
}
