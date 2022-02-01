using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features;

public class GazeLogger : ParticipantLogger
{
    [SerializeField]
    private Text m_EyePositon;

    [SerializeField]
    private Text m_EyeRotation;

    public override void FillDataPoint(ParticipantTrackingDataPoint a_continuousDataPoint)
    {
        //TODO
    }

    bool TryGetCenterEyeFeature(out Quaternion rotation, out Vector3 position)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.centerEyeRotation, out rotation) && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.centerEyePosition, out position))
                return true;
        }
        // This is the fail case, where there was no center eye was available.
        rotation = Quaternion.identity;
        position = default;
        return false;
    }

    bool TryGetEyes(out Eyes eyes)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        if (device.isValid)
        {
            Debug.Log(string.Format("name:{0}, role:{1}, manufacturer:{2}, characteristics:{3}, serialNumber:{4},",
                device.name, device.role, device.manufacturer, device.characteristics, device.serialNumber));
            if (device.TryGetFeatureValue(CommonUsages.eyesData, out eyes))
                return true;
        }

        // This is the fail case, where there was no center eye was available.
        eyes = default;
        return false;
    }

    public void EyePositonCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector3>();

        if (m_EyePositon != null)
        {
            m_EyePositon.text = "EyePositonCallback " + value;
        }
        
        Debug.Log("EyePositonCallback " + context.valueType + " value: " + value);
    }

    public void EyeRotationCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Quaternion>();

        if (m_EyePositon != null)
        {
            m_EyePositon.text = "EyeRotationCallback " + value;
        }

        Debug.Log("EyeRotationCallback " + context.valueType + " value: " + value);
    }

}
