using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolBehaviour1 : MonoBehaviour
{
    public GameObject[] waypoints;    
    public Animator animator;           
    public NavMeshAgent agent;         
    public float[] waitTimes;         
    public int[] stateAtWaypoint;       
    public float waypointOffset = 1.5f; 

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;    
    private bool isInCollider = false;   

    void Start()
    {
        if (waypoints.Length > 0 && agent != null && animator != null && waitTimes.Length == waypoints.Length)
        {
            SetRandomDestination(); 
        }
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.1f && !isInCollider)
        {
            HandleWaypointReached();  
        }
    }

    private IEnumerator WaitAndMoveToNextWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimes[currentWaypointIndex]);
        SetRandomDestination(); 
        SetAnimationState(stateAtWaypoint[currentWaypointIndex]); 
        isWaiting = false;
    }

    private void SetRandomDestination()
    {
        Vector3 randomOffset = new Vector3(Random.Range(-waypointOffset, waypointOffset), 0, Random.Range(-waypointOffset, waypointOffset));
        Vector3 targetPosition = waypoints[currentWaypointIndex].transform.position + randomOffset;
        agent.SetDestination(targetPosition);
    }

    private void HandleWaypointReached()
    {
        FaceWaypoint(waypoints[currentWaypointIndex]);
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0; 
        }
        StartCoroutine(WaitAndMoveToNextWaypoint());
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
        else
        {
            Debug.LogError("Animator is not set.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == waypoints[currentWaypointIndex])
        {
            isInCollider = true;
            HandleWaypointReached(); 
            isInCollider = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == waypoints[currentWaypointIndex])
        {
            isInCollider = false;
        }
    }
}
