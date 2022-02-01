using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleDoor : MonoBehaviour
{
    public GameObject DoorParent;
    public GameObject DoorBase;
    public GameObject DoorHandle;

    [SerializeField]
    private bool m_openState;
    public bool OpenState
    {
        get { return m_openState; }
    }

    public bool FireEventsOnStart = true;
    public UnityEvent OnDoorOpen;
    public UnityEvent OnDoorClose;

    private void Awake()
    {
        if (DoorParent == null)
            throw new MissingComponentException("DoorParent component not set");
        if (DoorBase == null)
            throw new MissingComponentException("DoorBase component not set");
        if (DoorHandle == null)
            throw new MissingComponentException("DoorHandle component not set");
    }

    public void SetOpenState(bool a_openState)
    {
        if (m_openState != a_openState)
        {
            m_openState = a_openState;
            UpdateDoorState();

            FireDoorEvent();
        }
    }

    private void FireDoorEvent()
    {
        if (m_openState)
        {
            OnDoorOpen.Invoke();
        }
        else
        {
            OnDoorClose.Invoke();
        }
    }

    private void UpdateDoorState()
    {
        DoorParent.SetActive(!m_openState);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateDoorState();
        FireDoorEvent();
    }
}
