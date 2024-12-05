using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    public float punchRange = 2.5f; 
    public LayerMask breakableLayer; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Punch();
        }
    }

    void Punch()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, punchRange, breakableLayer))
        {
            BreakObject(hit.collider.gameObject); 
        }
    }

    void BreakObject(GameObject pot)
    {
        pot.SetActive(false);  
        Transform brokenPot = pot.transform.parent.Find("BrokenPot");
        if (brokenPot != null)
        {
            brokenPot.gameObject.SetActive(true);  
        }
    }
}
