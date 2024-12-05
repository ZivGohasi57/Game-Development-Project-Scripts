using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public Transform Point1;
    public Transform Point2; 
    private Transform currentTarget;
    private bool isWaiting = false; 
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentTarget = Point1;
        MoveToNextPoint();
        animator.SetInteger("State", 0);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(WaitBeforeSwitching());
        }
    }

    IEnumerator WaitBeforeSwitching()
    {
        isWaiting = true;
        animator.SetInteger("State", 1);
        yield return new WaitForSeconds(4);
        SwitchTarget();
        MoveToNextPoint();
        isWaiting = false;
        animator.SetInteger("State", 0);
    }

    void MoveToNextPoint()
    {
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
            animator.SetInteger("State", 0);
        }
    }

    void SwitchTarget()
    {
        currentTarget = (currentTarget == Point1) ? Point2 : Point1;
    }
}
