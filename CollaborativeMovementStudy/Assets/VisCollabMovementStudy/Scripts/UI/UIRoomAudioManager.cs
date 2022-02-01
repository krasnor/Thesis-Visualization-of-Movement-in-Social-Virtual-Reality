using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomAudioManager : MonoBehaviour
{
    public NetworkedPlayerSettings PlayerSettings;
    public Button Button_Everyone;
    public Button Button_Nobody;
    public Button Button_OnlySupervisor;
    public GameObject Text_NotSupervisorWarningLabel;
    public GameObject Text_SupervisorDescription;

    // Start is called before the first frame update
    void Awake()
    {
        if (PlayerSettings == null)
            throw new MissingComponentException("PlayerSettings Component was not set");

        if (Text_NotSupervisorWarningLabel == null)
            throw new MissingComponentException("Text_NotSupervisorWarningLabel Component was not set");
        if (Text_SupervisorDescription == null)
            throw new MissingComponentException("Text_SupervisorDescription Component was not set");
        if (Button_Everyone == null)
            throw new MissingComponentException("Button_Everyone Component was not set");
        if (Button_Nobody == null)
            throw new MissingComponentException("Button_Nobody Component was not set");
        if (Button_OnlySupervisor == null)
            throw new MissingComponentException("Button_OnlySupervisor Component was not set");
    }

    // Update is called once per frame
    void Start()
    {
        Button_Everyone.onClick.AddListener(OnScopeToAudioEveryone);
        Button_Nobody.onClick.AddListener(OnScopeToAudioNobody);
        Button_OnlySupervisor.onClick.AddListener(OnScopeToAudioSupervisor);
        Text_NotSupervisorWarningLabel.SetActive(false);
        Text_NotSupervisorWarningLabel.SetActive(false);
    }

    private void Update()
    {
        bool isNotSupervisor = false;
        if (PlayerSettings.StudyPlayerRole != StudyPlayerRole.SUPERVISOR)
        {
            isNotSupervisor = true;
        }
        if (Text_NotSupervisorWarningLabel.activeSelf != isNotSupervisor)
        {
            Text_NotSupervisorWarningLabel.SetActive(isNotSupervisor);
            Text_SupervisorDescription.SetActive(!isNotSupervisor);
        }
    }

    private void OnScopeToAudioEveryone()
    {
        SetAudioHearingScopeForEveryBody(PlayerAudioHearingScope.Everyone);
    }

    private void OnScopeToAudioNobody()
    {
        SetAudioHearingScopeForEveryBody(PlayerAudioHearingScope.Nobody);
    }

    private void OnScopeToAudioSupervisor()
    {
        SetAudioHearingScopeForEveryBody(PlayerAudioHearingScope.SupervisorOnly);
    }

    private void SetAudioHearingScopeForEveryBody(PlayerAudioHearingScope a_hearingScope)
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            PlayerSettings.RequestAudioHearingScopeChange(p.ActorNumber, a_hearingScope);
        }
    }
}
