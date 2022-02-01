using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// This script is intended to be used to toggle between two Game Contoller Objects.
/// This is especially needed when two InputAction Shemes Should be mapped based on different Buttons.
/// 
/// If one does want to grab a Object the "Select Action" of the XRController must triggered.
/// If one does want to teleport the "Select Action" must also be triggered.
/// => Problem: only one InputAction can be bound to the "Select Action" of the XRContoller
/// => we cannot differentiate between the intended action teleport or grab
/// 
/// When different buttons for each action should be used, we need a way to switch between these input shemes.
/// 
/// E.g. a dedicated Grab Button and a dedicated Teleport button.
/// 
/// </summary>
public class TeleportationManager : MonoBehaviour
{
    public InputActionReference teleportActivationReference;

    public UnityEvent onTeleportActivate;
    public UnityEvent onTeleportCancel;

    // Start is called before the first frame update
    void Start()
    {
        teleportActivationReference.action.performed += TeleportModeActivated;
        teleportActivationReference.action.started += TeleportModeStarted;
        teleportActivationReference.action.canceled += TeleportModeCancelled;
    }


    private void TeleportModeStarted(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Started");
    }

    private void TeleportModeCancelled(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Cancelled");
        //Debug.Log("TeleportModeCancelled");
        Invoke("DeactivateTeleporter", .1f);
    }

    private void DeactivateTeleporter()
    {
        //Debug.Log("DeactivateTeleporter");
        onTeleportCancel.Invoke();

    }

    private void TeleportModeActivated(InputAction.CallbackContext obj)
    {
        //Debug.Log("teleportActivateionReference Activated");
        onTeleportActivate.Invoke();
    }
}
