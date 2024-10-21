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
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
		agent.isStopped = true;
		animator = GetComponent<Animator>();
		line = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
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
                animator.SetInteger("State", 1); //Walking	
                agent.SetDestination(target.transform.position);
                agent.isStopped = false;
                // set length of array of corners/line
                line.positionCount = agent.path.corners.Length;
                line.SetPositions(agent.path.corners);
                
			}
		}
		//starts pathfingidng with a* which is not efficient but is neccesary to dynamically change the path
		 if (!agent.isStopped)
		{// finds path to the target and start moving towards the target
            agent.SetDestination(target.transform.position);
            agent.isStopped = false;
            line.positionCount = agent.path.corners.Length;
            line.SetPositions(agent.path.corners);
        }
        
    }
}
