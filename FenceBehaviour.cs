using UnityEngine;
using System.Collections;


public class FenceBehaviour : MonoBehaviour
{
		
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            PlayerBehaviour player = other.GetComponentInParent<PlayerBehaviour>();
            if (player != null && player.isAttacking)
            {
			
                StartCoroutine(DestroyWithDelay(0.5f)); 
            }
        }
    }

    private IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 
        Destroy(gameObject);  
    }
}
