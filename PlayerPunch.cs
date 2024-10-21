using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    public float punchRange = 2.5f;  // הטווח של האגרוף
    public LayerMask breakableLayer;  // שכבת אובייקטים שניתן לשבור (כגון הכד)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))  // לחץ על Q לאגרוף
        {
            Punch();
        }
    }

    void Punch()
    {
        // שליחת קרן קדימה מהשחקן לזיהוי התנגשות
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, punchRange, breakableLayer))
        {
            Debug.Log("פגעת באובייקט שניתן לשבור!");
            BreakObject(hit.collider.gameObject);  // קריאה לפונקציה ששוברת את האובייקט
        }
        else
        {
            Debug.Log("לא פגעת בכלום.");
        }
    }

    void BreakObject(GameObject pot)
    {
        // מחליף את הכד השלם בכד השבור
        pot.SetActive(false);  // הסתרת הכד השלם
        Transform brokenPot = pot.transform.parent.Find("BrokenPot");
        if (brokenPot != null)
        {
            brokenPot.gameObject.SetActive(true);  // הצגת הכד השבור
        }
    }
}
