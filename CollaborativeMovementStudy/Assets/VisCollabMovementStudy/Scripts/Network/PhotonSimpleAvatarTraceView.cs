using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Naive Sync of an AvatarTraceManager's Line Renderer Points.
/// </summary>
[RequireComponent(typeof(AvatarTraceManager))]
[RequireComponent(typeof(LineRenderer))]
public class PhotonSimpleAvatarTraceView : MonoBehaviourPun, IPunObservable
{
    private AvatarTraceManager m_traceManager;
    private LineRenderer m_lineRenderer;

    private Vector3[] r_linePoints;
    private bool m_newData;

    void Start()
    {
        this.m_traceManager = GetComponent<AvatarTraceManager>();
        this.m_lineRenderer = GetComponent<LineRenderer>();

        m_newData = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.photonView.IsMine)
        {
            // only if not owned

            if (m_newData)
            {
                m_newData = false;
                m_lineRenderer.positionCount = r_linePoints.Length;
                m_lineRenderer.SetPositions(r_linePoints);
            }
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Vector3[] positions = new Vector3[m_lineRenderer.positionCount];
            m_lineRenderer.GetPositions(positions);
            stream.SendNext(positions);
        }
        else
        {
            r_linePoints = (Vector3[])stream.ReceiveNext();
            m_newData = true;
        }
    }
}
