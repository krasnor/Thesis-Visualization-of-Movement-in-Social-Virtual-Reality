using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPlayerEntryRow : MonoBehaviour
{
    public int ActorId;
    public string UserId;
    public bool IsMaster;
    public string customPropertyRole;
    public string customPropertyColor;
    public string customParticipantId;
    public string customHearingScope;
    public bool IsPlayerInvisible;

    public UnityEvent<int> OnMasterRequest;
    public UnityEvent<int, StudyPlayerRole> OnRoleChange;

    public Button Button_Master;
    public Button Button_Visitor;
    public Button Button_Guide;
    public Button Button_Supervisor;
    public Image PlayerColorImage;
    public Text Id;
    public Text Master;
    public Text Pid;
    public Text Role;
    public Text AudioHearingScope;
    public GameObject IsInvisibleIndicator;

    public void SetValues(int a_actorId, string a_userId, bool a_isMaster, string a_role, string a_color, string a_participantId, string a_hearingScope, bool a_isInvisible)
    {
        ActorId = a_actorId;
        UserId = a_userId;
        IsMaster = a_isMaster;
        customPropertyRole = a_role;
        customPropertyColor = a_color;
        customParticipantId = a_participantId;
        customHearingScope = a_hearingScope;
        IsPlayerInvisible = a_isInvisible;

        Id.text = $"{ActorId}";
        Master.text = IsMaster ? "M" : " ";
        Role.text = $"{customPropertyRole}";
        Pid.text = $"{customParticipantId}";
        AudioHearingScope.text = $"{customHearingScope}";
        IsInvisibleIndicator.gameObject.SetActive(IsPlayerInvisible);

        Color parsedColor;
        if (ColorUtility.TryParseHtmlString("#" + customPropertyColor.ToString(), out parsedColor))
        {
            PlayerColorImage.color = parsedColor;
        }
        else
        {
            PlayerColorImage.color = new Color(0, 0, 0, 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Button_Master.onClick.AddListener(OnMasterRequestButtonClick);
        Button_Visitor.onClick.AddListener(OnVisitorButtonClick);
        Button_Guide.onClick.AddListener(OnGuideButtonClick);
        Button_Supervisor.onClick.AddListener(OnSupervisorButtonClick);
    }

    public void OnVisitorButtonClick() => OnRoleButtonClick(StudyPlayerRole.VISITOR);
    public void OnGuideButtonClick() => OnRoleButtonClick(StudyPlayerRole.GUIDE);
    public void OnSupervisorButtonClick() => OnRoleButtonClick(StudyPlayerRole.SUPERVISOR);

    public void OnMasterRequestButtonClick()
    {
        OnMasterRequest.Invoke(ActorId);
    }

    public void OnRoleButtonClick(StudyPlayerRole a_role)
    {
        OnRoleChange.Invoke(ActorId, a_role);
    }
}
