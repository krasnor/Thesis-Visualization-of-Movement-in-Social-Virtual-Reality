using UnityEngine;
using UnityEngine.InputSystem;

public class DebugOpenXRAction : MonoBehaviour
{
    public InputActionReference dbgInputActionReference;

    // Start is called before the first frame update
    void Start()
    {
        dbgInputActionReference.action.performed += InputActionPerformed;
        dbgInputActionReference.action.started += InputActionStarted;
        dbgInputActionReference.action.canceled += InputActionCancel;
    }
    // Update is called once per frame
    void Update()
    {
        // nothing to do
    }

    private void InputActionCancel(InputAction.CallbackContext obj)
    {
        Debug.Log("dbg InputActionCancel");
    }

    private void InputActionStarted(InputAction.CallbackContext obj)
    {
        Debug.Log("dbg InputActionStarted");
    }

    private void InputActionPerformed(InputAction.CallbackContext obj)
    {
        Debug.Log("dbg InputActionPerformed");
    }


}