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
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        animator = GetComponent<Animator>();
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (!agent.isStopped && distance < 3)
        {
            if(target.transform.position.y<8)// move it to the second floor
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
            animator.SetInteger("State", 0); //animation idle

        }
        if (Input.GetKeyDown(KeyCode.Q))
        {

            // finds path to the target and starts moving towards the target
            // Uses A* pfathfinding algorithm
            if (agent.isStopped)
            {
                animator.SetInteger("State", 1); //walking
                agent.SetDestination(target.transform.position);
                agent.isStopped = false;
                // set length of array of corners/line and array of path corners itself
                line.positionCount = agent.path.corners.Length;
                line.SetPositions(agent.path.corners);
            }
        }
        // starts pathfinding with A* which is not efficient but
        // is necessary to dynamically change the path
        // 1. finds path to target
        // 2. starts moving to the target
        if(!agent.isStopped) { 
            agent.SetDestination(target.transform.position);
            agent.isStopped = false;
            // set length of array of corners/line and array of path corners itself
            line.positionCount = agent.path.corners.Length;
            line.SetPositions(agent.path.corners);

        }

    }
}
