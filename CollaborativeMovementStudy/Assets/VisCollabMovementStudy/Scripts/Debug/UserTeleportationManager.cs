using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class UserTeleportationManager : MonoBehaviour
{

    [SerializeField] private InputActionAsset actionAsset;

    public TeleportationProvider teleportationProvider;

    public InputAction actionTeleportStart;     // btn press
    public InputAction actionTeleportPerform;   // btn release
    public InputAction actionTeleportCancel;    // grip button

    public XRRayInteractor rayInteractor;

    // Start is called before the first frame update
    void Start()
    {
        rayInteractor.enabled = false;

        actionTeleportStart.Enable();
        actionTeleportPerform.Enable();
        actionTeleportCancel.Enable();

        actionTeleportStart.performed += OnTeleportStart;      
        actionTeleportPerform.performed += OnTeleportPerform;   
        actionTeleportCancel.performed += OnTeleportCancel;     
    }

    private void OnTeleportStart(InputAction.CallbackContext obj)
    {
        // begin teleport "planning"
        rayInteractor.enabled = true;
    }

    private void OnTeleportPerform(InputAction.CallbackContext obj)
    {
        // TODO do teleport
        if(!rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            rayInteractor.enabled = false;
            return;
        }

        TeleportRequest tp_request = new TeleportRequest()
        {
            destinationPosition = hit.point,
            //destinationRotation = ,
            requestTime = Time.time,
            //matchOrientation,
        };

        teleportationProvider.QueueTeleportRequest(tp_request);
        rayInteractor.enabled = false;
    }



    private void OnTeleportCancel(InputAction.CallbackContext obj)
    {
        rayInteractor.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
