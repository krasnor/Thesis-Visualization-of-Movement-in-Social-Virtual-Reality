using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DebugControllerActionListener : MonoBehaviour
{

    public ActionBasedController ctrl;
    public InputActionProperty selectActionDbg;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EventFired()
    {
        Debug.Log("EventFired");
    }
}
