using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWarningsMenu : MonoBehaviour
{
    public NetworkedGameSettings NetworkedGameSettings;
    public NetworkedPlayerSettings NetworkedPlayerSettings;
    [Space]
    public GameObject WarningSessionIdNotSet;
    public GameObject WarningParticipantIdNotSet;
    public GameObject WarningNotMasterClient;
    public GameObject WarningNotSupervisor;
    public GameObject WarningNotInRoom;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkedGameSettings == null)
            throw new MissingComponentException("NetworkedGameSettings component not assigned.");

        if (WarningSessionIdNotSet == null)
            throw new MissingComponentException("WarningSessionIdNotSet component not assigned.");
        if (WarningParticipantIdNotSet == null)
            throw new MissingComponentException("WarningParticipantIdNotSet component not assigned.");
        if (WarningNotMasterClient == null)
            throw new MissingComponentException("WarningNotMasterClient component not assigned.");
        if (WarningNotSupervisor == null)
            throw new MissingComponentException("WarningNotSupervisor component not assigned.");
        if (WarningNotInRoom == null)
            throw new MissingComponentException("WarningNotInRoom component not assigned.");
    }

    // Update is called once per frame
    void Update()
    {
        ToggleObjectActiveState(WarningSessionIdNotSet, CheckShouldShowWarningSessionIdNotSet());
        ToggleObjectActiveState(WarningParticipantIdNotSet, CheckShouldShowWarningParticipantIdsNotSet());

        ToggleObjectActiveState(WarningNotMasterClient, CheckShouldShowWarningNotMasterClient());
        ToggleObjectActiveState(WarningNotSupervisor, CheckShouldShowWarningNotSupervisor());
        ToggleObjectActiveState(WarningNotInRoom, CheckShouldShowWarningNotInRoom());

    }
    private void ToggleObjectActiveState(GameObject a_obj, bool a_shouldShow)
    {
        if (a_obj.activeSelf != a_shouldShow)
            a_obj.SetActive(a_shouldShow);
    }

    private bool CheckShouldShowWarningSessionIdNotSet()
    {
        if (NetworkedGameSettings.LoggingSessionId == NetworkedGameSettings.DefaultLoggingSessionId)
            return true;
        return false;
    }

    private bool CheckShouldShowWarningParticipantIdsNotSet()
    {
        string defaultPid = PlayerSettings.DefaultParticipantId;
        int unsetPids = 0;

        foreach (var p in PhotonNetwork.PlayerList)
        {

            if (NetworkedPlayerSettings.TryGetRoleOfPlayer(p, out var a_role))
            {
                if (a_role != StudyPlayerRole.SUPERVISOR)
                {
                    if (NetworkedPlayerSettings.TryGetParticipantIdOfPlayer(p, out var pid))
                    {
                        if (pid == NetworkedPlayerSettings.DefaultParticipantId)
                        {
                            // default pid
                            unsetPids++;
                        }
                    }
                    else
                    {
                        // no pid
                        unsetPids++;
                    }
                }
            }
            else
            {
                // no role
                unsetPids++;
            }
        }


        // unsetPids == 0  -> all have id -> dont show warning
        // unsetPids != 0  -> some need id -> show warning -> true
        return unsetPids != 0;
    }

    private bool CheckShouldShowWarningNotMasterClient()
    {
        return !PhotonNetwork.IsMasterClient;
    }
    private bool CheckShouldShowWarningNotInRoom()
    {
        return !PhotonNetwork.InRoom;
    }
    private bool CheckShouldShowWarningNotSupervisor()
    {
        if (PlayerSettings.Instance != null)
        {
            return PlayerSettings.Instance.StudyPlayerRole != StudyPlayerRole.SUPERVISOR;
        }
        return true;
    }
}
