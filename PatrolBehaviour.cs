using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolBehaviour : MonoBehaviour
{
    public GameObject[] waypoints;       // מערך הנקודות שהדמות צריכה לעבור
    public Animator animator;            // רפרנס לאנימטור של הדמות
    public NavMeshAgent agent;           // ה-AI של הדמות
    public float[] waitTimes;            // מערך של זמני המתנה עבור כל נקודת דרך
    public int[] stateAtWaypoint;        // מערך של מצבים (סטייטים) לכל נקודת דרך
    
    private int currentWaypointIndex = 0; // אינדקס של הנקודה הנוכחית
    private bool isWaiting = false;

    void Start()
    {
        if (waypoints.Length > 0 && agent != null && animator != null && waitTimes.Length == waypoints.Length)
        {
            StartCoroutine(WaitAndMoveToNextWaypoint());  // מתחיל תנועה לנקודה הראשונה אחרי זמן המתנה
        }
        else
        {
            Debug.LogError("One of the components (agent, animator, waypoints, or waitTimes) is not set correctly.");
        }
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.1f)
        {
            // כשהדמות מגיעה לנקודה הנוכחית
            FaceWaypoint(waypoints[currentWaypointIndex]);  // הפנה את הדמות לכיוון השלילי של ציר ה-x של הנקודה
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;  // חזרה לנקודה הראשונה
            }
            StartCoroutine(WaitAndMoveToNextWaypoint()); // המתן לפני תנועה לנקודה הבאה
        }
    }

    private IEnumerator WaitAndMoveToNextWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimes[currentWaypointIndex]);  // זמן המתנה לפני התנועה לנקודה הבאה
        agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        SetAnimationState(stateAtWaypoint[currentWaypointIndex]);  // שינוי הסטייט בהתאם לנקודת הדרך
        isWaiting = false;
    }

    void FaceWaypoint(GameObject waypoint)
    {
        // מחשב את הכיוון השלילי של ציר ה-x של הנקודה
        Vector3 targetDirection = -waypoint.transform.right;
        targetDirection.y = 0;  // שומר על כיוון ציר ה-y של הדמות

        // מסובב את הדמות לכיוון המטרה
        Quaternion rotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
    }

    void SetAnimationState(int state)
    {
        if (animator != null)
        {
            animator.SetInteger("State", state);  // שינוי הסטייט לפי המספר
        }
        else
        {
            Debug.LogError("Animator is not set.");
        }
    }
}