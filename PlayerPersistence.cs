using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance; 

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Scene2")  
        {
            instance = this; 
            DontDestroyOnLoad(gameObject);  
        }
    }
}
