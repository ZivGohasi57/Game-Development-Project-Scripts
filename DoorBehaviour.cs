using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    Animator animator;
    AudioSource sound;
    
    void Start()
    {
        animator = this.GetComponent<Animator>();
        sound = this.GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        animator.SetBool("DoorOpens", true);
        sound.PlayDelayed(0.4f);
    }
    
    private void OnTriggerExit(Collider other)
    {
        animator.SetBool("DoorOpens", false);
        sound.PlayDelayed(1f);
    }
}
