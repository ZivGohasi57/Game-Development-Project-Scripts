using UnityEngine;
using UnityEngine.UI;

public class WeaponUIManager : MonoBehaviour
{
    public RawImage fistsImage;      // UI image for fists
    public RawImage swordImage;      // UI image for sword
    public Text fistsText;           // Text below fists image
    public Text swordText;           // Text below sword image
    private CavePlayerBehaviour cavePlayer; // Reference to the CavePlayer script
    private PlayerBehaviour firstPlayer;    // Reference to the PlayerBehaviour script

    private void Start()
    {
        cavePlayer = FindObjectOfType<CavePlayerBehaviour>();
        firstPlayer = FindObjectOfType<PlayerBehaviour>();

        if (cavePlayer == null && firstPlayer == null)
        {
            Debug.LogError("No player script found!");
        }
        
        UpdateWeaponUI();
    }

    public void UpdateWeaponUI()
    {
        if (cavePlayer != null)
        {
            // Handle UI update for CavePlayerBehaviour
            fistsImage.enabled = cavePlayer.hasFists;
            swordImage.enabled = cavePlayer.hasSword;
            fistsText.enabled = cavePlayer.hasFists;
            swordText.enabled = cavePlayer.hasSword;

            // Adjust opacity of the icons and texts based on the current weapon
            UpdateWeaponOpacity(cavePlayer.currentWeapon);
        }
        else if (firstPlayer != null)
        {
            // Handle UI update for PlayerBehaviour
            fistsImage.enabled = firstPlayer.hasFists;
            swordImage.enabled = firstPlayer.hasSword;
            fistsText.enabled = firstPlayer.hasFists;
            swordText.enabled = firstPlayer.hasSword;

            // Adjust opacity of the icons and texts based on the current weapon
            UpdateWeaponOpacity(firstPlayer.currentWeapon);
        }
    }

    private void UpdateWeaponOpacity(CavePlayerBehaviour.WeaponType currentWeapon)
    {
        Color fullColor = new Color(1f, 1f, 1f, 1f);  // Full opacity
        Color fadedColor = new Color(1f, 1f, 1f, 0.5f); // Half opacity

        if (cavePlayer != null)
        {
            // If fists are collected and not current weapon, dim the fists icon and text
            if (cavePlayer.hasFists)
            {
                fistsImage.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
                fistsText.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
            }

            // If sword is collected and not current weapon, dim the sword icon and text
            if (cavePlayer.hasSword)
            {
                swordImage.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
                swordText.color = (currentWeapon == CavePlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
            }
        }
    }

    private void UpdateWeaponOpacity(PlayerBehaviour.WeaponType currentWeapon)
    {
        Color fullColor = new Color(1f, 1f, 1f, 1f);  // Full opacity
        Color fadedColor = new Color(1f, 1f, 1f, 0.5f); // Half opacity

        if (firstPlayer != null)
        {
            // If fists are collected and not current weapon, dim the fists icon and text
            if (firstPlayer.hasFists)
            {
                fistsImage.color = (currentWeapon == PlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
                fistsText.color = (currentWeapon == PlayerBehaviour.WeaponType.Fists) ? fullColor : fadedColor;
            }

            // If sword is collected and not current weapon, dim the sword icon and text
            if (firstPlayer.hasSword)
            {
                swordImage.color = (currentWeapon == PlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
                swordText.color = (currentWeapon == PlayerBehaviour.WeaponType.Sword) ? fullColor : fadedColor;
            }
        }
    }

    private void Update()
    {
        UpdateWeaponUI(); // Continuously update UI based on current weapon state
    }
}