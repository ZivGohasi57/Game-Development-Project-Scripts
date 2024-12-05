using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightBehaviour : MonoBehaviour
{
	LineRenderer line;
	NavMeshAgent agent;
	Animator animator;
	public GameObject target;
 
	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.isStopped = true;
		animator = GetComponent<Animator>();
		line = this.GetComponent<LineRenderer>();
	}

	void Update()
	{
		float distance = Vector3.Distance(target.transform.position,transform.position);
		if (!agent.isStopped && distance < 1)
		{
            		agent.isStopped = true;
			animator.SetInteger("State",0);
		}
    		if (Input.GetKeyDown(KeyCode.Q)) 
		{			
			if (agent.isStopped)
			{
                		animator.SetInteger("State", 1); 
                		agent.SetDestination(target.transform.position);
                		agent.isStopped = false;
                		line.positionCount = agent.path.corners.Length;
                		line.SetPositions(agent.path.corners);
                
			}
		 }
		 if (!agent.isStopped)
		 {
            		agent.SetDestination(target.transform.position);
            		agent.isStopped = false;
            		line.positionCount = agent.path.corners.Length;
            		line.SetPositions(agent.path.corners);
        	}
        
    }
}
