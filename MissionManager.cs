using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public TextMeshProUGUI missionText;
    public RectTransform missionPanel;
    public float animationDuration = 0.5f;
    public float slideDistance = 200f;
    private Vector2 originalPosition;

    private int currentMissionIndex = 0;
    private List<string> missions;

    private bool isMissionLocked = false;

    public bool IsMissionLocked
    {
        get { return isMissionLocked; }
        private set { isMissionLocked = value; }
    }

    private void Start()
    {
        missions = new List<string>
        {
            "Go to the bar and meet one of the castle's guards.",
            "Go to your house with the purple roof and disguise yourself as one of the castle's guards.",
            "Go to the castle.",
            "Search for the treasure notes 0 of 4.",
            "Search for the treasure notes 1 of 4.",
            "Search for the treasure notes 2 of 4.",
            "Search for the treasure notes 3 of 4.",
            "Find the well around the lake and follow its path.",
            "Find the weapon (The Blue Way!)",
			"You must fight the enemy's to get the clue about the tressure from the big boss",
			"Come back to the forbidden forest near to the castle and search for the treasure"
        };

        if (missionPanel != null)
        {
            originalPosition = missionPanel.anchoredPosition;
        }

        UpdateMission(missions[currentMissionIndex]);
    }

    public void UpdateMissionWithDelay(int missionIndex, float delay)
    {
        if (!IsMissionLocked)
        {
            StartCoroutine(UpdateMissionAfterDelay(missionIndex, delay));
        }
        else
        {
            Debug.LogWarning("Mission is currently locked. Cannot update mission with delay.");
        }
    }

    private IEnumerator UpdateMissionAfterDelay(int missionIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        UpdateMissionByIndex(missionIndex);
    }

    public void UpdateMissionByIndex(int missionIndex)
    {
        if (!IsMissionLocked && missionIndex >= 0 && missionIndex < missions.Count)
        {
            LockMission();
            currentMissionIndex = missionIndex;
            UpdateMission(missions[currentMissionIndex]);
        }
        else if (IsMissionLocked)
        {
            Debug.LogWarning("Mission is currently locked. Cannot update mission by index.");
        }
        else
        {
            Debug.LogWarning("Invalid mission index.");
        }
    }

    public void UpdateMission(string newMission)
    {
        if (missionText != null)
        {
            StartCoroutine(AnimateMissionUpdate(newMission));
        }
    }

    private IEnumerator AnimateMissionUpdate(string newMission)
    {
        yield return StartCoroutine(SlideOut());

        missionText.text = $"{newMission}";

        yield return StartCoroutine(SlideIn());

        UnlockMission();
    }

    private IEnumerator SlideOut()
    {
        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            float newX = Mathf.Lerp(originalPosition.x, originalPosition.x - slideDistance, elapsedTime / animationDuration);
            missionPanel.anchoredPosition = new Vector2(newX, originalPosition.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        missionPanel.anchoredPosition = new Vector2(originalPosition.x - slideDistance, originalPosition.y);
    }

    private IEnumerator SlideIn()
    {
        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            float newX = Mathf.Lerp(originalPosition.x - slideDistance, originalPosition.x, elapsedTime / animationDuration);
            missionPanel.anchoredPosition = new Vector2(newX, originalPosition.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        missionPanel.anchoredPosition = originalPosition;
    }

    public void LockMission()
    {
        IsMissionLocked = true;
    }

    public void UnlockMission()
    {
        IsMissionLocked = false;
    }

    public int GetCurrentMissionIndex()
    {
        return currentMissionIndex;
    }

    // New method to trigger mission update after sword is picked up
    public void TriggerNextMissionOnSwordPickup()
    {
        if (!IsMissionLocked && PersistentObjectManager.instance.hasSwordInHand)
        {
            TriggerNextMission();  // Advance mission
        }
        else
        {
            Debug.LogWarning("Sword not in hand, mission will not advance.");
        }
    }

    // Existing method to trigger the next mission
    public void TriggerNextMission()
    {
        if (!IsMissionLocked && currentMissionIndex + 1 < missions.Count)
        {
            currentMissionIndex++;
            UpdateMissionByIndex(currentMissionIndex);
            Debug.Log($"Mission updated to: {missions[currentMissionIndex]}");
        }
        else
        {
            Debug.LogWarning("Mission cannot be updated. Either it's locked or it's the last mission.");
        }
    }


    // פונקציה להתקדמות במשימות
    public void AdvanceMission()
    {
        if (currentMissionIndex < missions.Count - 1)
        {
            currentMissionIndex++;
            Debug.Log("משימה התקדמה: " + missions[currentMissionIndex]);
            UpdateMissionUI(); // עדכן את ה-UI או כל מה שצריך במשחק שלך
        }
        else
        {
            Debug.Log("כל המשימות הושלמו!");
        }
    }

    // פונקציה לעדכון ה-UI (או מה שנדרש במשחק שלך)
    private void UpdateMissionUI()
    {
        // כאן ייכנס הקוד לעדכון ה-UI של המשימה הנוכחית
        Debug.Log("עדכון משימה ל-UI: " + missions[currentMissionIndex]);
    }
}
