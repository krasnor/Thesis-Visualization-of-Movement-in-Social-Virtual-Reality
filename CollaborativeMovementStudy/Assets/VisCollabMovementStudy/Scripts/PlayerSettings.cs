using UnityEngine;
using UnityEngine.Events;

//public class PlayerRoleChangeEventArgs
//{
//    public PlayerRoleChangeEventArgs(StudyPlayerRole a_newRole)
//    {
//        NewRole = a_newRole;
//    }
//    public StudyPlayerRole NewRole;
//}
//public sealed class PlayerRoleChangeEvent : UnityEvent<PlayerRoleChangeEventArgs> {}

public class PlayerSettings : MonoBehaviour
{
    #region Singelton
    // https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern

    private static PlayerSettings _instance;

    public static PlayerSettings Instance { get { return _instance; } }


    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("PlayerSettings Instance already instantiated. Destroying this object. To avoid having two PlayerSettings Instances.", this.gameObject);
            Destroy(this.gameObject);
        }
        else
        {
            //Debug.LogWarning("Creating PlayerSetting Instance of type: " + this.GetType(), this.gameObject);
            _instance = this;
        }

        if (TryLoadParticipantId(out var loadedPid))
        {
            m_loggingParticipantId = loadedPid; // don't use update as we don't want to throw events in Awake()
        }
    }

    #endregion
    public bool DoPersistParticipantId = true;

    private bool TryLoadParticipantId(out string a_pid)
    {
        a_pid = DefaultParticipantId;
        if (PlayerPrefs.HasKey(PersistedParticipantKey))
        {
            a_pid = PlayerPrefs.GetString(PersistedParticipantKey, DefaultParticipantId);
            return true;
        }
        return false;
    }

    private void SaveParticipantId(string a_pid)
    {
        PlayerPrefs.SetString(PersistedParticipantKey, a_pid);
        PlayerPrefs.Save();
    }
    public void DeleteSavedParticipantId()
    {
        if (PlayerPrefs.HasKey(PersistedParticipantKey))
        {
            PlayerPrefs.DeleteKey(PersistedParticipantKey);
            PlayerPrefs.Save();
        }
    }


    #region Properties/Events
    [SerializeField]
    StudyPlayerRole m_studyPlayerRole = StudyPlayerRole.VISITOR;

    public StudyPlayerRole StudyPlayerRole // yes auto property would be shorter, but the fields would then not show up in Unity Editor so an Custom Drawer must then be written which is then more debugging work
    {
        get => m_studyPlayerRole;
        private set => m_studyPlayerRole = value;
    }

    [SerializeField]
    Color m_studyPlayerColor = Color.green;
    public Color StudyPlayerColor
    {
        get => m_studyPlayerColor;
        private set => m_studyPlayerColor = value;
    }

    [SerializeField]
    bool m_isPlayerInvisible = false;
    public bool IsPlayerInvisible
    {
        get => m_isPlayerInvisible;
        private set => m_isPlayerInvisible = value;
    }


    [SerializeField]
    PlayerAudioHearingScope m_studyPlayerHearingScope = PlayerAudioHearingScope.Everyone;
    public PlayerAudioHearingScope StudyPlayerHearingScope
    {
        get => m_studyPlayerHearingScope;
        private set => m_studyPlayerHearingScope = value;
    }

    [SerializeField]
    public static readonly string PersistedParticipantKey = "Pid";
    public static readonly string DefaultParticipantId = "-1";
    string m_loggingParticipantId = DefaultParticipantId;
    public string LoggingParticipantId
    {
        get => m_loggingParticipantId;
        private set => m_loggingParticipantId = value;
    }

    public bool RaiseColorEventOnAppStart = true;
    public bool RaiseIsInvisibleEventOnAppStart = true;
    public bool RaiseRoleEventOnAppStart = true;
    public bool RaiseHearingScopeEventOnAppStart = true;

    public bool RaiseLoggingParticipantIdEventOnAppStart = false;

    [Space]

    public UnityEvent<StudyPlayerRole> OnLocalPlayerRoleChange = null;
    public UnityEvent<Color> OnLocalPlayerColorChange = null;
    public UnityEvent<bool> OnLocalPlayerInvisibilityChange = null;
    public UnityEvent<PlayerAudioHearingScope> OnLocalPlayerHearingScopeChange = null;

    public UnityEvent<string> OnLoggingParticipantIdChange = null;
    public UnityEvent<string> OnLoggingSessionIdChange = null;

    #endregion

    #region Modifiers

    /// <summary>
    /// Should only be called from Network Synchronizer! (thus protected)
    /// An Event is only sent if new value is different.
    /// </summary>
    /// <param name="a_newRole"></param>
    protected void UpdateStudyPlayerRole(StudyPlayerRole a_newRole)
    {
        if (m_studyPlayerRole == a_newRole)
            return;

        StudyPlayerRole = a_newRole;
        OnLocalPlayerRoleChange.Invoke(a_newRole);
    }

    /// <summary>
    /// Should only be called from Network Synchronizer! (thus protected)
    /// An Event is only sent if new value is different.
    /// </summary>
    protected void UpdateStudyPlayerColor(Color a_newColor)
    {
        if (m_studyPlayerColor == a_newColor)
            return;

        StudyPlayerColor = a_newColor;
        OnLocalPlayerColorChange.Invoke(a_newColor);
    }

    protected void UpdateStudyPlayerInvisibilityState(bool a_newInvisibilityState)
    {
        if (m_isPlayerInvisible == a_newInvisibilityState)
            return;

        IsPlayerInvisible = a_newInvisibilityState;
        OnLocalPlayerInvisibilityChange.Invoke(a_newInvisibilityState);
    }

    protected void UpdateStudyPlayerHearingScope(PlayerAudioHearingScope a_hearScope)
    {
        if (m_studyPlayerHearingScope == a_hearScope)
            return;

        StudyPlayerHearingScope = a_hearScope;
        OnLocalPlayerHearingScopeChange.Invoke(a_hearScope);
    }


    protected void UpdateLoggingParticipantId(string a_participantId)
    {
        if (m_loggingParticipantId == a_participantId)
            return;

        LoggingParticipantId = a_participantId;
        if (DoPersistParticipantId)
        {
            SaveParticipantId(LoggingParticipantId);
        }
        OnLoggingParticipantIdChange.Invoke(a_participantId);
    }

    #endregion



    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (RaiseColorEventOnAppStart)
        {
            OnLocalPlayerColorChange.Invoke(StudyPlayerColor);
        }
        if (RaiseIsInvisibleEventOnAppStart)
        {
            OnLocalPlayerInvisibilityChange.Invoke(IsPlayerInvisible);
        }
        if (RaiseRoleEventOnAppStart)
        {
            OnLocalPlayerRoleChange.Invoke(StudyPlayerRole);
        }
        if (RaiseHearingScopeEventOnAppStart)
        {
            OnLocalPlayerHearingScopeChange.Invoke(StudyPlayerHearingScope);
        }

        if (RaiseLoggingParticipantIdEventOnAppStart)
        {
            OnLoggingParticipantIdChange.Invoke(LoggingParticipantId);
        }
    }

}
