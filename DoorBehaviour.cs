using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    Animator animator;
    AudioSource sound;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        sound = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
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