using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNPCbehaviour : MonoBehaviour
{
    public float moveSpeed = 3.0f; // מהירות התנועה של ה-NPC
    public float changeDirectionTime = 2.0f; // כל כמה זמן לשנות כיוון (בשניות)
    public float minX, maxX, minZ, maxZ; // הגבולות של המפה (x ו-z)

    private Vector3 randomDirection;
    private float timer;

    void Start()
    {
        ChangeDirection(); // לקבוע כיוון רנדומלי בהתחלה
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // אם הזמן לשנות כיוון נגמר
        if (timer <= 0)
        {
            ChangeDirection();
        }

        // תנועה בכיוון הנוכחי
        transform.Translate(randomDirection * moveSpeed * Time.deltaTime);

        // למנוע יציאה מהגבולות
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    void ChangeDirection()
    {
        // בחר כיוון רנדומלי בתלת ממד
        randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        timer = changeDirectionTime;
    }
}