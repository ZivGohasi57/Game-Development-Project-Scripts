using UnityEngine;
using UnityEngine.UI;

public class WeaponUIManager : MonoBehaviour
{
    public RawImage fistsImage;   
    public RawImage swordImage;  
    public Text fistsText;   
    public Text swordText;   
    private CavePlayerBehaviour cavePlayer; 
    private PlayerBehaviour firstPlayer;  
    
    private void Start()
    {
        cavePlayer = FindObjectOfType<CavePlayerBehaviour>();
        firstPlayer = FindObjectOfType<PlayerBehaviour>();
        UpdateWeaponUI();
    }

    public void UpdateWeaponUI()
    {
        if (cavePlayer != null)
        {
            fistsImage.enabled = cavePlayer.hasFists;
            swordImage.enabled = cavePlayer.hasSword;
            fistsText.enabled = cavePlayer.hasFists;
            swordText.enabled = cavePlayer.hasSword;

            UpdateWeaponOpacity(cavePlayer.currentWeapon);
        }
        else if (firstPlayer != null)
        {
            fistsImage.enabled = firstPlayer.hasFists;
            swordImage.enabled = firstPlayer.hasSword;
            fistsText.enabled = firstPlayer.hasFists;
            swordText.enabled = firstPlayer.hasSword;

            UpdateWeaponOpacity(firstPlayer.currentWeapon);
        }
    }

    private void UpdateWeaponOpacity(CavePlayerBehaviour.WeaponType currentWeapon)
    {
        Color fullColor = new Color(1f, 1f, 1f, 1f);  
        Color fadedColor = new Color(1f, 1f, 1f, 0.5f); 

        if (cavePlayer != null)
        {
            if (cavePlayer.hasFists)
            {
                fistsImage.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
                fistsText.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
            }

            if (cavePlayer.hasSword)
            {
                swordImage.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
                swordText.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
            }
        }
    }

    private void UpdateWeaponOpacity(PlayerBehaviour.WeaponType currentWeapon)
    {
        Color fullColor = new Color(1f, 1f, 1f, 1f); 
        Color fadedColor = new Color(1f, 1f, 1f, 0.5f); 

        if (firstPlayer != null)
        {
            if (firstPlayer.hasFists)
            {
                fistsImage.color = (currentWeapon == PlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
                fistsText.color = (currentWeapon == PlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
            }

            if (firstPlayer.hasSword)
            {
                swordImage.color = (currentWeapon == PlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
                swordText.color = (currentWeapon == PlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
            }
        }
    }

    private void Update()
    {
        UpdateWeaponUI(); 
    }
}
