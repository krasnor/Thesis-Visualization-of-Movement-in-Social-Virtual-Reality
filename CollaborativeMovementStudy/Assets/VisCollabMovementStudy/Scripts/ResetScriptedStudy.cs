using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScriptedStudy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        if (ScriptedAvatar == null)
            throw new MissingComponentException("Scripted Avatar Compnent was not set");
    }


    public ScriptedVRAvatar ScriptedAvatar;
    public List<StudyLookAtInteractable> Interactables = new List<StudyLookAtInteractable>();


    public void DoResetStudy()
    {
        ScriptedAvatar.ResetAvatar();
        foreach (var interactable in Interactables)
        {
            interactable.ResetInteractable();
        }
    }

    public void DoResetStudyAndTakeOwnership()
    {
        ScriptedAvatar.GetComponent<PhotonView>().RequestOwnership();
        ScriptedAvatar.ResetAvatar();
        foreach (var interactable in Interactables)
        {
            interactable.ResetInteractable();
        }
    }
}
