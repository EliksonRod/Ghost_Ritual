using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{

    [SerializeField] GameObject flashlightObject; // Reference to the flashlight GameObject
    [SerializeField] GameObject blacklightObject; // Reference to the blacklight GameObject
    bool flashlightOn = false; // Track the state of the flashlight
    bool blacklightOn = false; // Track the state of the blacklight
    ControllerForPlayer PlayerController; // Reference to the player's controller script

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerController = ControllerForPlayer.Instance; // Get the instance of the InputForPlayer script
    }

    void Update()
    {
        if (blacklightOn)
        {
            PlayerController.ChangeFov((PlayerController.originalFov * (70f / 100f))); // Change FOV to 40 when blacklight is active
        }
        else
        {
            PlayerController.ResetFov(); // Reset FOV to default when blacklight is not active
        }
    }

    public void ToggleFlashlight(InputAction.CallbackContext context)
    {
        if (blacklightObject.activeInHierarchy) return; // Prevent toggling flashlight if blacklight is active
        flashlightObject.SetActive(!flashlightObject.activeSelf);
    }

    public void UseBlacklight(InputAction.CallbackContext context)
    {
        blacklightOn = true;
        flashlightObject.SetActive(false);
        blacklightObject.SetActive(true);
    }

    public void StopBlacklight(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            blacklightOn = false;
            blacklightObject.SetActive(false); // Ensure blacklight is turned off when the input is released
            PlayerController.ResetFov(); // Reset FOV to default when blacklight is turned off
            flashlightObject.SetActive(true);
        }
    }
}
