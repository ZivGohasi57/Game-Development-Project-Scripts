using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolBehaviour1 : MonoBehaviour
{
    public GameObject[] waypoints;       // מערך הנקודות שהדמות צריכה לעבור
    public Animator animator;            // רפרנס לאנימטור של הדמות
    public NavMeshAgent agent;           // ה-AI של הדמות
    public float[] waitTimes;            // מערך של זמני המתנה עבור כל נקודת דרך
    public int[] stateAtWaypoint;        // מערך של מצבים (סטייטים) לכל נקודת דרך
    public float waypointOffset = 1.5f;  // סטייה רנדומלית מסביב לנקודת הדרך

    private int currentWaypointIndex = 0; // אינדקס של הנקודה הנוכחית
    private bool isWaiting = false;       // משתנה שמסמן אם הדמות במצב המתנה
    private bool isInCollider = false;    // משתנה שמסמן אם הדמות נכנסה לקוליידר

    void Start()
    {
        if (waypoints.Length > 0 && agent != null && animator != null && waitTimes.Length == waypoints.Length)
        {
            SetRandomDestination();  // קבע יעד ראשוני עם סטייה רנדומלית
        }
        else
        {
            Debug.LogError("One of the components (agent, animator, waypoints, or waitTimes) is not set correctly.");
        }
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.1f && !isInCollider)
        {
            HandleWaypointReached();  // אם אין קוליידר פעיל, עבור לנקודה הבאה כשהדמות מגיעה לאובייקט
        }
    }

    private IEnumerator WaitAndMoveToNextWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimes[currentWaypointIndex]);  // זמן המתנה לפני תנועה לנקודה הבאה
        SetRandomDestination();  // קבע יעד עם סטייה רנדומלית סביב הנקודה הבאה
        SetAnimationState(stateAtWaypoint[currentWaypointIndex]);  // שינוי הסטייט בהתאם לנקודת הדרך
        isWaiting = false;
    }

    private void SetRandomDestination()
    {
        // בוחר נקודת יעד עם סטייה רנדומלית סביב נקודת היעד הנוכחית
        Vector3 randomOffset = new Vector3(
            Random.Range(-waypointOffset, waypointOffset), 0, 
            Random.Range(-waypointOffset, waypointOffset));

        Vector3 targetPosition = waypoints[currentWaypointIndex].transform.position + randomOffset;
        agent.SetDestination(targetPosition);
    }

    private void HandleWaypointReached()
    {
        // פעולה המתבצעת כאשר הדמות מגיעה לנקודת היעד
        FaceWaypoint(waypoints[currentWaypointIndex]);  // הפנה את הדמות לכיוון הנקודה
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;  // חזרה לנקודה הראשונה
        }
        StartCoroutine(WaitAndMoveToNextWaypoint()); // המתן לפני תנועה לנקודה הבאה
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

    void OnTriggerEnter(Collider other)
    {
        // בודק אם הדמות נכנסה לקוליידר של נקודת היעד הנוכחית
        if (other.gameObject == waypoints[currentWaypointIndex])
        {
            isInCollider = true;
            HandleWaypointReached();  // עבור לנקודה הבאה
            isInCollider = false;  // אפס את המשתנה כדי לאפשר לדמות להמשיך לנקודות נוספות
        }
    }

    void OnTriggerExit(Collider other)
    {
        // כאשר הדמות יוצאת מהקוליידר, עדכן את המשתנה
        if (other.gameObject == waypoints[currentWaypointIndex])
        {
            isInCollider = false;
        }
    }
}