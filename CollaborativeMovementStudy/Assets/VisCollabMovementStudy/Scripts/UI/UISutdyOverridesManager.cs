using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISutdyOverridesManager : MonoBehaviour
{
    public NetworkConnectionManager ConnectionManager;
    public Button ButtonKickPlayer;
    public InputField InputActorNumberKick;

    // Start is called before the first frame update
    void Start()
    {
        ButtonKickPlayer.onClick.AddListener(OnKickPressed);
    }

    private void Awake()
    {
        if (ConnectionManager == null)
            throw new MissingReferenceException("ConnectionManager component not assigned.");
        if (ButtonKickPlayer == null)
            throw new MissingReferenceException("ButtonKickPlayer component not assigned.");
        if (InputActorNumberKick == null)
            throw new MissingReferenceException("InputActorNumberKick component not assigned.");
    }

    private void OnKickPressed()
    {
        if (int.TryParse(InputActorNumberKick.text, out int actorId))
        {
            ConnectionManager?.TriggerRemoteCloseForPlayer(actorId);
        }
    }
}
