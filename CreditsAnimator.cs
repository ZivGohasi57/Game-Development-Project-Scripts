using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsAnimator : MonoBehaviour
{
    public Animator creditsAnimator;
    public string sceneToLoad;

    private void Start()
    {
        StartCoroutine(PlayCreditsAnimation());
    }

    private IEnumerator PlayCreditsAnimation()
    {
        yield return new WaitForSeconds(3f);

        creditsAnimator.SetTrigger("StartAnimation");

        yield return new WaitForSeconds(15f);
        
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.ClearData();
        }
        SceneManager.LoadScene(sceneToLoad);
    }
}
