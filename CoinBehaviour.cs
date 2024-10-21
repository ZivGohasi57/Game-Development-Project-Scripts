using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    public GameObject player; // הפניה לדמות השחקן
    public GameObject parent; // הפניה להורה של האובייקט הזה
    public MissionManager missionManager; // משתנה לחיבור ל-MissionManager

    private void OnTriggerEnter(Collider other)
    {
        // בדיקה אם השחקן נכנס לקוליידר
        if (other.gameObject == player)
        {
            gameObject.SetActive(false); // הפוך את המסמך ללא פעיל
            AudioSource sound = parent.GetComponent<AudioSource>();
            sound.Play();

            // גישה לסקריפט של השחקן והפעלת פונקציה להוספת מסמך
            PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
            if (playerBehaviour != null)
            {
                playerBehaviour.CollectDocument(); // קריאה לפונקציה לעדכון מספר המסמכים
                // עדכון המשימה לאחר איסוף המסמך
                if (missionManager != null)
                {
                    missionManager.TriggerNextMission(); // קידום למשימה הבאה
                    Debug.Log("Mission updated to the next one after collecting document.");
                }
            }
            else
            {
                Debug.LogError("PlayerBehaviour is not found!");
            }
        }
    }
}