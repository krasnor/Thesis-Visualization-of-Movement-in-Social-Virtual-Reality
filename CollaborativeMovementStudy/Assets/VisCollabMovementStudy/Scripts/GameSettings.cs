using UnityEngine;
using UnityEngine.Events;

public class GameSettings : MonoBehaviour
{

    #region Properties/Events
    //public enum TracedTeleportationProviderMode
    //{
    //    Instant,
    //    Trace_Line,
    //    Trace_Particles,
    //    Continuous,
    //}
    [SerializeField]
    TracedTeleportationProviderMode m_teleportationMode = TracedTeleportationProviderMode.Instant; // yes auto property would be shorter, but the fields would then not show up in Unity Editor so an Custom Drawer must then be written which is then more debugging work

    public TracedTeleportationProviderMode TeleportationMode
    {
        get => m_teleportationMode;
        private set => m_teleportationMode = value;
    }

    [SerializeField]
    bool m_openStateDoorRooms = false;
    public bool OpenStateDoorRooms
    {
        get => m_openStateDoorRooms;
        private set => m_openStateDoorRooms = value;
    }

    [SerializeField]
    bool m_openStateDoorOutsideWorld = false;
    public bool OpenStateDoorOutsideWorld
    {
        get => m_openStateDoorOutsideWorld;
        private set => m_openStateDoorOutsideWorld = value;
    }

    public static readonly string DefaultLoggingSessionId = "-999";
    [SerializeField]
    string m_loggingSessionId = DefaultLoggingSessionId;
    public string LoggingSessionId
    {
        get => m_loggingSessionId;
        private set => m_loggingSessionId = value;
    }

    [SerializeField]
    TracedTeleportationProvider TeleportationProvider;

    public bool RaiseTeleportationModeEventOnAppStart = true;
    public bool RaiseOpenStateDoorRoomsEvenOnAppStart = true;
    public bool RaiseOpenStateOutsideWorldEventOnAppStart = true;
    public bool RaiseLoggingSessionIdEventOnAppStart = false;

    [Space]

    public UnityEvent<TracedTeleportationProviderMode> OnGlobalTeleportationModeChanged = null;
    public UnityEvent<bool> OnOpenStateDoorRoomsChanged = null;
    public UnityEvent<bool> OnOpenStateDoorOutsideWorldChanged = null;
    public UnityEvent<string> OnLoggingSessionIdChanged = null;

    #endregion

    #region Modifiers

    protected void UpdateTeleportationMode(TracedTeleportationProviderMode a_newTeleportationMode)
    {
        if (m_teleportationMode == a_newTeleportationMode)
            return;

        TeleportationMode = a_newTeleportationMode;

        if (TeleportationProvider != null)
        {
            TeleportationProvider.ChangeTracedTeleportationProviderMode(a_newTeleportationMode);
            OnGlobalTeleportationModeChanged.Invoke(a_newTeleportationMode);
        }
    }

    protected void UpdateOpenStateDoorRooms(bool a_openState)
    {
        if (m_openStateDoorRooms == a_openState)
            return;

        OpenStateDoorRooms = a_openState;

        OnOpenStateDoorRoomsChanged.Invoke(OpenStateDoorRooms);
    }

    protected void UpdateOpenStateDoorOutsideWorld(bool a_openState)
    {
        if (m_openStateDoorOutsideWorld == a_openState)
            return;

        m_openStateDoorOutsideWorld = a_openState;

        OnOpenStateDoorOutsideWorldChanged.Invoke(OpenStateDoorOutsideWorld);
    }

    protected void UpdateLoggingSessionIdKey(string a_sessionId)
    {
        if (m_loggingSessionId == a_sessionId)
            return;

        m_loggingSessionId = a_sessionId;

        OnLoggingSessionIdChanged.Invoke(a_sessionId);
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (RaiseTeleportationModeEventOnAppStart)
        {
            OnGlobalTeleportationModeChanged.Invoke(TeleportationMode);
        }
        if (RaiseOpenStateDoorRoomsEvenOnAppStart)
        {
            OnOpenStateDoorRoomsChanged.Invoke(OpenStateDoorRooms);
        }
        if (RaiseOpenStateOutsideWorldEventOnAppStart)
        {
            OnOpenStateDoorOutsideWorldChanged.Invoke(OpenStateDoorOutsideWorld);
        }

        if (RaiseLoggingSessionIdEventOnAppStart)
        {
            OnLoggingSessionIdChanged.Invoke(LoggingSessionId);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
