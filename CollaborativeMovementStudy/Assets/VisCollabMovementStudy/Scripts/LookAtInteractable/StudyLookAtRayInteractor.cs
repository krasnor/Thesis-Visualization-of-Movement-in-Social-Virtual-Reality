using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StudyLookAtRayInteractor : MonoBehaviour
{
    private LineRenderer m_lineRenderer;
    public bool debug_showGaze = false;
    public LayerMask RaycastMask = ~0;
    public int MaxDistance = 20;

    [Space]
    [SerializeField]
    private StudyLookAtInteractable m_lastValidTarget;
    [SerializeField]
    private GameObject m_lastTarget;

    // Start is called before the first frame update
    void Start()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            Vector3 rayDir = transform.forward;
            if (m_lineRenderer != null)
            {
                if (debug_showGaze)
                {
                    m_lineRenderer.enabled = true;
                    m_lineRenderer.positionCount = 2;
                    m_lineRenderer.SetPosition(0, gameObject.transform.position);
                    m_lineRenderer.SetPosition(1, gameObject.transform.position + (rayDir * MaxDistance));
                }
                else
                {
                    m_lineRenderer.enabled = false;
                }
            }

            //Debug.Log("0: " + gameObject.transform.position + "  -- 1: " + (gameObject.transform.position + (rayDir * 2)));
            m_lastTarget = null;
            RaycastHit hit;
            bool validHit = false;
            if (Physics.Raycast(transform.position, rayDir, out hit, MaxDistance, RaycastMask))
            {
                m_lastTarget = hit.transform.gameObject;
                StudyLookAtInteractable triggerObj = hit.transform.gameObject.GetComponent<StudyLookAtInteractable>();
                //Debug.Log("triggerObj: " + triggerObj);
                if (triggerObj != null)
                {
                    validHit = true;
                    if (m_lastValidTarget != null && m_lastValidTarget.GetInstanceID() != triggerObj.GetInstanceID())
                    {
                        // within one update frame: switched from one object to another
                        m_lastValidTarget.OnGazeExit();
                        m_lastValidTarget = null;
                    }
                    if (m_lastValidTarget == null)
                    {
                        // gaze enter
                        triggerObj.OnGazeEnter();
                    }
                    triggerObj.OnGazeHover();
                    m_lastValidTarget = triggerObj;
                }
            }

            if (!validHit)
            {
                // raycast either hit nothing or another object
                if (m_lastValidTarget != null)
                {
                    // gaze exit
                    m_lastValidTarget.OnGazeExit();
                    m_lastValidTarget = null;
                }
            }
        }
    }
}