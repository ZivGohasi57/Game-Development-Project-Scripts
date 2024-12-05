using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNPCbehaviour : MonoBehaviour
{
    public float moveSpeed = 3.0f; 
    public float changeDirectionTime = 2.0f;
    public float minX, maxX, minZ, maxZ;

    private Vector3 randomDirection;
    private float timer;

    void Start()
    {
        ChangeDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            ChangeDirection();
        }

        transform.Translate(randomDirection * moveSpeed * Time.deltaTime);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    void ChangeDirection()
    {
        randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        timer = changeDirectionTime;
    }
}
