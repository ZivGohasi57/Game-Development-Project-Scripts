using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance;  // Singleton של השחקן

    void Awake()
    {
        // בדיקה אם יש כבר עותק פעיל של השחקן
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // השמד עותק נוסף
            return;
        }

        // שמור את השחקן אם הוא מהסצנה השנייה
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Scene2")  // החלף בשם הסצנה השנייה
        {
            instance = this;  // שמור את העותק הנוכחי
            DontDestroyOnLoad(gameObject);  // הפוך את השחקן לעקשן
        }
    }
}
