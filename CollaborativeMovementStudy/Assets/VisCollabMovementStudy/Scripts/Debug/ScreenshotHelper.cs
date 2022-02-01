using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotHelper : MonoBehaviour
{
    public InputActionReference ScreenshotActionReferemce;
    public bool OnlyMasterCanMakeScreenShot = true;

    // Start is called before the first frame update
    void Start()
    {
        if (ScreenshotActionReferemce != null)
            ScreenshotActionReferemce.action.performed += MakeScreenShot;

    }

    private void MakeScreenShot(InputAction.CallbackContext obj)
    {
        if (enabled)
        {
            try
            {
                if (OnlyMasterCanMakeScreenShot && !PhotonNetwork.IsMasterClient)
                {
                    return;
                }
                string filename = "CollabStudy_" + DateTime.UtcNow.ToString(@"O") + ".png";
                filename = filename.Replace(":", "-");
                string fullPath = Application.persistentDataPath + "/" + filename;
                Debug.Log("Taking Screenshot: " + fullPath);
                ScreenCapture.CaptureScreenshot(fullPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error Occured while taking screenshot: " + ex);
            }
        }
    }
}
