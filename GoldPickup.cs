using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int goldAmount = 10; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GoldManager.Instance.AddGold(goldAmount);  
            gameObject.SetActive(false); 
        }
    }
}
