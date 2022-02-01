using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarStudyRoleManager : MonoBehaviour
{
    public Camera playerCamera;
    public LayerMask LayerMask_role_visitor_and_default = ~0;
    public LayerMask LayerMask_role_guide = ~0;
    public LayerMask LayerMask_role_supervisor = ~0;

    void Awake()
    {
        if (playerCamera == null)
        {
            throw new MissingComponentException("Camera Component not assigned to Script");
        }
    }

    public void SetStudyPlayerRole(StudyPlayerRole a_role)
    {
        switch (a_role)
        {
            case StudyPlayerRole.GUIDE:
                playerCamera.cullingMask = LayerMask_role_guide;
                break;
            case StudyPlayerRole.SUPERVISOR:
                playerCamera.cullingMask = LayerMask_role_supervisor;
                break;
            case StudyPlayerRole.VISITOR:
            default:
                playerCamera.cullingMask = LayerMask_role_visitor_and_default;
                break;
        }
    }
}
