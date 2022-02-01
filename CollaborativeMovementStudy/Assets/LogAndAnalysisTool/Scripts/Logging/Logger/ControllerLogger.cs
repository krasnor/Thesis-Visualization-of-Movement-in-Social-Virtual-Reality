using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class ControllerLogger : ParticipantLogger
{
    [SerializeField]
    protected Transform m_controllerTransform;

    [SerializeField]
    protected LoggingManager m_loggingManager;


    public abstract string ControllerName
    {
        get;
    }

    void Start()
    {
        if (m_controllerTransform == null)
            Debug.LogErrorFormat("Assign controller Transform for controller {0}.", ControllerName);

        //XRControllerRecording r = new XRControllerRecording();
        //r.name = "TestControllerRecording";
        //r.InitRecording();
        //r.AddRecordingFrame(new XRControllerState());
        //r.AddRecordingFrame(new XRControllerState());
        //r.SaveRecording();
        //Debug.Log("ControllerLogger save.");
    }

    protected (Vector3 position, Quaternion rotation) GetControllerData(UnityEngine.XR.InputDevice a_hand)
    {
        var allSuccess = false;
        Vector3 position = default;
        Quaternion rotation = default;

        if (a_hand.isValid)
        {
            allSuccess =
                a_hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out position)
                && a_hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out rotation);

        }

        if (!allSuccess)
            Debug.Log("Could not aquire all Controller Data Points for Controller: " + a_hand.name);

        return (position, rotation);
    }

}
