using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErikaBehaviour : MonoBehaviour
{
    public GameObject player;
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if(distance <10)
        {
            if(animator.GetInteger("State")!=1 ) 
            animator.SetInteger("State", 1);
			
			Vector3 target = player.transform.position - transform.position;
			target.y = 0;
			Vector3 tmp_target = Vector3.RotateTowards(transform.forward, target, Time.deltaTime, 0);	
			transform.rotation = Quaternion.LookRotation(tmp_target);
			
        }
        else 
        {
            if(animator.GetInteger("State") !=0)
                animator.SetInteger("State", 0);
        }
    }
}
