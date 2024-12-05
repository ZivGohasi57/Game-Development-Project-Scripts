using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HPManager : MonoBehaviour
{
    public Slider hpSlider;
    public TextMeshProUGUI hpText; 
    public float maxHP = 100f; 
    private float currentHP; 
    private float targetHP; 

    public float updateDelay = 0.5f; 
    private Coroutine healthRegenCoroutine; 
    private bool isInCombatMode = false; 

    public Color normalColor = Color.green; 
    public Color lowHpColor = Color.yellow; 
    public Color criticalHpColor = Color.red;

    void Start()
    {
        currentHP = maxHP; 
        targetHP = maxHP;
        UpdateHPUI(); 
    }

    void Update()
    {
        if (!isInCombatMode && currentHP < maxHP * 0.3f)
        {
            if (healthRegenCoroutine == null)
            {
                healthRegenCoroutine = StartCoroutine(RegenerateHealth());
            }
        }
        else if (isInCombatMode || currentHP >= maxHP * 0.3f)
        {
            if (healthRegenCoroutine != null)
            {
                StopCoroutine(healthRegenCoroutine);
                healthRegenCoroutine = null;
            }
        }
    }

    public void SetCombatMode(bool isInCombat)
    {
        isInCombatMode = isInCombat;
    }

    IEnumerator RegenerateHealth()
    {
        while (currentHP < maxHP * 0.7f && !isInCombatMode)
        {
            currentHP = Mathf.Min(currentHP + 1, maxHP * 0.3f);
            UpdateHPUI(); 
            yield return new WaitForSeconds(2f); 
        }
    }

    public void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
            
            Color newColor;
            if (currentHP / maxHP >= 0.4f)
            {
                newColor = normalColor;
            }
            else if (currentHP / maxHP >= 0.2f)
            {
                newColor = lowHpColor;
            }
            else
            {
                newColor = criticalHpColor;
            }
        
            hpSlider.fillRect.GetComponent<Image>().color = newColor;

            if (hpSlider.handleRect != null)
            {
                hpSlider.handleRect.GetComponent<Image>().color = newColor;
            }
        }

        if (hpText != null)
        {
            hpText.text = Mathf.RoundToInt(currentHP) + "/" + Mathf.RoundToInt(maxHP);
        }
    }

    public void SetHP(float newHP)
    {
        targetHP = newHP;
        StartCoroutine(UpdateHPWithDelay());
    }

    IEnumerator UpdateHPWithDelay()
    {
        float elapsedTime = 0;
        float startHP = currentHP;

        while (elapsedTime < updateDelay)
        {
            elapsedTime += Time.deltaTime;
            currentHP = Mathf.Lerp(startHP, targetHP, elapsedTime / updateDelay);
            UpdateHPUI();
            yield return null;
        }

        currentHP = targetHP;
        UpdateHPUI();
    }
}
