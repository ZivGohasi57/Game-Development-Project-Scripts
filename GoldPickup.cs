using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int goldAmount = 10;  // כמות הזהב שיינתן כשנאסף

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // בדיקה אם השחקן התקרב לזהב
        {
            GoldManager.Instance.AddGold(goldAmount);  // הוספת הזהב לניהול הזהב
            Debug.Log("אספת " + goldAmount + " זהב!");
            gameObject.SetActive(false);  // הסרת הזהב מהמשחק
        }
    }
}