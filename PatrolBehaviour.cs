using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolBehaviour : MonoBehaviour
{
    public GameObject[] waypoints;   
    public Animator animator;       
    public NavMeshAgent agent;   
    public float[] waitTimes;   
    public int[] stateAtWaypoint;    
    
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        if (waypoints.Length > 0 && agent != null && animator != null && waitTimes.Length == waypoints.Length)
        {
            StartCoroutine(WaitAndMoveToNextWaypoint());
        }
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.1f)
        {
            FaceWaypoint(waypoints[currentWaypointIndex]); 
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
            StartCoroutine(WaitAndMoveToNextWaypoint());
        }
    }

    private IEnumerator WaitAndMoveToNextWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimes[currentWaypointIndex]);
        agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        SetAnimationState(stateAtWaypoint[currentWaypointIndex]);
        isWaiting = false;
    }

    void FaceWaypoint(GameObject waypoint)
    {
        Vector3 targetDirection = -waypoint.transform.right;
        targetDirection.y = 0; 

        Quaternion rotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
    }

    void SetAnimationState(int state)
    {
        if (animator != null)
        {
            animator.SetInteger("State", state);
        }
    }
}
