using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoggingHelper : MonoBehaviour
{
    public NetworkedGameSettings NetworkedGameSettings;
    public NetworkedPlayerSettings NetworkedPlayerSettings;

    public Text Text_CurrentSessionId;
    [Space]
    public InputField InputField_SessionId;
    public Button Button_SetSessionId;

    [Space]
    public InputField InputField_ActorA;
    public InputField InputField_ActorAParticipantId;
    public Button Button_SetActorAParticipantId;

    [Space]
    public InputField InputField_ActorB;
    public InputField InputField_ActorBParticipantId;
    public Button Button_SetActorBParticipantId;
   

    void Awake()
    {
        if (NetworkedGameSettings == null)
            throw new MissingComponentException("NetworkedGameSettings component not assigned");
        if (NetworkedPlayerSettings == null)
            throw new MissingComponentException("NetworkedPlayerSettings component not assigned");

        if (InputField_SessionId == null)
            throw new MissingComponentException("InputField_SessionId component not assigned");
        if (Button_SetSessionId == null)
            throw new MissingComponentException("Button_SetSessionId component not assigned");

        if (InputField_ActorA == null)
            throw new MissingComponentException("InputField_ActorA component not assigned");
        if (InputField_ActorAParticipantId == null)
            throw new MissingComponentException("InputField_ActorAParticipantId component not assigned");
        if (Button_SetActorAParticipantId == null)
            throw new MissingComponentException("Button_SetActorAParticipantId component not assigned");

        if (InputField_ActorB == null)
            throw new MissingComponentException("InputField_ActorB component not assigned");
        if (InputField_ActorBParticipantId == null)
            throw new MissingComponentException("InputField_ActorBParticipantId component not assigned");
        if (Button_SetActorBParticipantId == null)
            throw new MissingComponentException("Button_SetActorBParticipantId component not assigned");
    }

    // Start is called before the first frame update
    void Start()
    {
        Button_SetSessionId.onClick.AddListener(OnSetSessionIdClick);
        Button_SetActorAParticipantId.onClick.AddListener(OnSetActorAPArticipantIdClick);
        Button_SetActorBParticipantId.onClick.AddListener(OnSetActorBPArticipantIdClick);
    }

    void Update()
    {
        string currentSessionId = NetworkedGameSettings.LoggingSessionId;
        if(Text_CurrentSessionId.text != currentSessionId)
        {
            Text_CurrentSessionId.text = currentSessionId;
        }    
    }

    public void OnSetSessionIdClick()
    {
        if (int.TryParse(InputField_SessionId.text, out int sessionId))
        {
            NetworkedGameSettings.SetLoggingSessionId(sessionId.ToString());
        }
    }

    public void OnSetActorAPArticipantIdClick()
    {
        if (int.TryParse(InputField_ActorA.text, out int actorAId))
        {
            if (int.TryParse(InputField_ActorAParticipantId.text, out int participantIdA))
            {
                NetworkedPlayerSettings.RequestParticipantIdChange(actorAId, participantIdA.ToString());
            }
        }
    }

    public void OnSetActorBPArticipantIdClick()
    {
        if (int.TryParse(InputField_ActorB.text, out int actorBId))
        {
            if (int.TryParse(InputField_ActorBParticipantId.text, out int participantIdB))
            {
                NetworkedPlayerSettings.RequestParticipantIdChange(actorBId, participantIdB.ToString());
            }
        }
    }
}
