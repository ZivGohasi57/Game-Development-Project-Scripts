using UnityEngine;
using System.Collections;


public class FenceBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            // קבלת המצב של השחקן
            PlayerBehaviour player = other.GetComponentInParent<PlayerBehaviour>();
            if (player != null && player.isAttacking) // בדיקה אם השחקן במצב התקפה
            {
                StartCoroutine(DestroyWithDelay(0.5f));  // התחלת coroutine עם עיכוב של 0.5 שניות
            }
        }
    }

    private IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // המתנה של 0.5 שניות
        Destroy(gameObject);  // השמדת הגדר לאחר ההמתנה
    }
}