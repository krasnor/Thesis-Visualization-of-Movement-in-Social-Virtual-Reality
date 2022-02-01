using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationManagager2 : MonoBehaviour
{
    public InputActionReference teleportActivationReferenceL;
    public InputActionReference teleportActivationReferenceR;

    public XRRayInteractor rayinteractorL;
    public XRRayInteractor rayinteractorR;

    public ActionBasedController actionBasedControllerL;
    public ActionBasedController actionBasedControllerR;


    //public UnityEvent onTeleportActivate;
    //public UnityEvent onTeleportCancel;

    // Start is called before the first frame update
    void Start()
    {
        teleportActivationReferenceL.action.performed += TeleportModeActivatedL;
        teleportActivationReferenceL.action.started += TeleportModeStartedL;
        teleportActivationReferenceL.action.canceled += TeleportModeCancelledL;

        teleportActivationReferenceR.action.performed += TeleportModeActivatedR;
        teleportActivationReferenceR.action.started += TeleportModeStartedR;
        teleportActivationReferenceR.action.canceled += TeleportModeCancelledR;

    }

    private void EnableLeftControllerRay()
    {
        // disable right side, as only one ray should be active at the same time
        DisableRightControllerRay();

        actionBasedControllerL.enableInputActions = true;
        rayinteractorL.enabled = true;
    }

    private void EnableRightControllerRay()
    {
        // disable left side, as only one ray should be active at the same time
        DisableLeftControllerRay();

        actionBasedControllerR.enableInputActions = true;
        rayinteractorR.enabled = true;
    }

    private void DisableLeftControllerRay()
    {
        actionBasedControllerL.enableInputActions = false;
        rayinteractorL.enabled = false;
    }

    private void DisableRightControllerRay()
    {
        actionBasedControllerR.enableInputActions = false;
        rayinteractorR.enabled = false;
    }


    private void TeleportModeStartedL(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Started");
    }

    private void TeleportModeCancelledL(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Cancelled");
        //Debug.Log("TeleportModeCancelled");
        //Invoke("DeactivateTeleporterL", .1f);
        Invoke("DeactivateTeleporterL", .025f);
    }

    private void DeactivateTeleporterL()
    {
        //Debug.Log("DeactivateTeleporter");
        //onTeleportCancel.Invoke();
        DisableLeftControllerRay();
    }

    private void TeleportModeActivatedL(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Activated");
        //onTeleportActivate.Invoke();
        EnableLeftControllerRay();
    }




    private void TeleportModeStartedR(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Started");
    }

    private void TeleportModeCancelledR(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Cancelled");
        //Debug.Log("TeleportModeCancelled");
        //Invoke("DeactivateTeleporterR", .1f);
        Invoke("DeactivateTeleporterR", .025f); // ~25ms
    }

    private void DeactivateTeleporterR()
    {
        //Debug.Log("DeactivateTeleporter");
        //onTeleportCancel.Invoke();
        DisableRightControllerRay();
    }

    private void TeleportModeActivatedR(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Activated");
        //onTeleportActivate.Invoke();
        EnableRightControllerRay();
    }
}
