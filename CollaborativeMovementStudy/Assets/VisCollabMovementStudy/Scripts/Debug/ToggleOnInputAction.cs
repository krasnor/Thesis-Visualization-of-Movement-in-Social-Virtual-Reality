using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ToggleOnInputAction : MonoBehaviour
{
    public InputActionReference ToggleActionReference;

    [Tooltip("Invokes the OnToggle-Event when Start() is called. Useful to make sure state is in sync from the beginning of the game.")]
    public bool FireEventOnStart = true;
    public bool CurrentState = true;
    public UnityEvent<bool> OnToggle = null;

    private void Awake()
    {
        if(ToggleActionReference == null)
        {
            Debug.LogWarning("ToggleOnInputAction: no InputAction Referenced. Will toggle on input.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ToggleActionReference != null)
        {
            ToggleActionReference.action.performed += InputActionPerformed;
            ToggleActionReference.action.started += InputActionStarted;
            ToggleActionReference.action.canceled += InputActionCancel;

            if (FireEventOnStart)
            {
                OnToggle.Invoke(CurrentState);
            }
        }
    }

    private void InputActionCancel(InputAction.CallbackContext obj)
    {
    }

    private void InputActionStarted(InputAction.CallbackContext obj)
    {
    }

    private void InputActionPerformed(InputAction.CallbackContext obj)
    {
        CurrentState = !CurrentState;
        OnToggle.Invoke(CurrentState);
    }

}
