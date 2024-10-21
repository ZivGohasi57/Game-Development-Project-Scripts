using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public Transform Point1;  // First point
    public Transform Point2;  // Second point
    private Transform currentTarget; // Current destination target
    private bool isWaiting = false; // To check if the NPC is waiting

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentTarget = Point1; // Start at Point1
        MoveToNextPoint();
        animator.SetInteger("State", 0); // Initialize State to 0 in Animator
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the NPC has reached the current target and is not waiting
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            // Start waiting before switching to the next target
            StartCoroutine(WaitBeforeSwitching());
        }
    }

    // Coroutine to handle waiting before switching targets
    IEnumerator WaitBeforeSwitching()
    {
        isWaiting = true;
        animator.SetInteger("State", 1); // Set State to 1 when waiting
        yield return new WaitForSeconds(4); // Wait for 4 seconds
        SwitchTarget();
        MoveToNextPoint();
        isWaiting = false;
        animator.SetInteger("State", 0); // Set State back to 0 when moving
    }

    // Moves the agent to the current target point
    void MoveToNextPoint()
    {
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
            animator.SetInteger("State", 0); // Ensure State is set to 0 when starting to move
        }
    }

    // Switches the target between Point1 and Point2
    void SwitchTarget()
    {
        currentTarget = (currentTarget == Point1) ? Point2 : Point1;
    }
}