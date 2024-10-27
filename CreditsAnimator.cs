using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsAnimator : MonoBehaviour
{
    public Animator creditsAnimator; // הפניה לאנימטור של הקרדיטים
    public string sceneToLoad; // שם הסצנה לעבור אליה בסיום האנימציה

    private void Start()
    {
        StartCoroutine(PlayCreditsAnimation());
    }

    private IEnumerator PlayCreditsAnimation()
    {
        // המתנה של 3 שניות לפני תחילת האנימציה
        yield return new WaitForSeconds(3f);

        // הפעלת האנימציה
        creditsAnimator.SetTrigger("StartAnimation");

        // המתנה ל-5 שניות אחרי תחילת האנימציה
        yield return new WaitForSeconds(15f);
        
        if (PersistentObjectManager.instance != null)
        {
            PersistentObjectManager.instance.ClearData();
        }
        // מעבר לסצנה המבוקשת
        SceneManager.LoadScene(sceneToLoad);
    }
}