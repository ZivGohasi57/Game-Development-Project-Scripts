using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightBehaviourScript : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public GameObject target;
    LineRenderer line;
    public GameObject Point1;
    public GameObject Point2;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        animator = GetComponent<Animator>();
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (!agent.isStopped && distance < 3)
        {
            if(target.transform.position.y<8)
            {
                target.transform.position = Point2.transform.position;  
            }
            else
            {
                target.transform.position = Point1.transform.position;

            }
        }


        if (!agent.isStopped && distance < 2)
        {
                agent.isStopped = true;
                animator.SetInteger("State", 0);

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

        if(!agent.isStopped) 
        { 
            agent.SetDestination(target.transform.position);
            agent.isStopped = false;
            line.positionCount = agent.path.corners.Length;
            line.SetPositions(agent.path.corners);

        }

    }
}
