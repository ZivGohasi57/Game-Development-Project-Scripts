using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarTenderBehaviour : MonoBehaviour
{
    public int State = 0; // 0: Moving, 1: Waiting, 2: Final Wait
    public GameObject[] waypoints; // List of waypoints (GameObjects)
    public UnityEngine.AI.NavMeshAgent agent;
    public Animator animator;
    public AudioSource audioSource;

    public float finalWaitTime = 30f; 
    private int currentWaypointIndex = 0;

    void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned!");
            return;
        }

        MoveToNextWaypoint();
    }

    void Update()
    {
        // Check if agent reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Rotate character to face negative X direction of the waypoint's local axis
            FaceTarget(waypoints[currentWaypointIndex].transform);

            if (currentWaypointIndex < waypoints.Length - 1)
            {
                // Reached one of the first waypoints
                if (State == 0)
                {
                    StartCoroutine(WaitAtWaypoint());
                }
            }
            else
            {
                // Reached the final waypoint
                if (State != 2)
                {
                    StartCoroutine(WaitAtFinalWaypoint());
                }
            }
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        State = 1; // Waiting state
        animator.SetInteger("State", State); // Update Animator state

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        yield return new WaitUntil(() => !audioSource.isPlaying);

        yield return new WaitForSeconds(1f);

        State = 0; // Return to moving state
        animator.SetInteger("State", State);

        MoveToNextWaypoint();
    }

    private IEnumerator WaitAtFinalWaypoint()
    {
        State = 2; // Final waiting state
        animator.SetInteger("State", State); // Update Animator state

        yield return new WaitForSeconds(finalWaitTime);

        State = 0; // Return to moving state after final wait
        animator.SetInteger("State", State);

        // Restart cycle
        currentWaypointIndex = 0;
        MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            currentWaypointIndex++;
        }
    }

    private void FaceTarget(Transform target)
    {
        // Get the direction to face the target
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Keep the rotation on the horizontal plane

        if (direction != Vector3.zero)
        {
            // Rotate towards the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}
