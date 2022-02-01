using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructedBuildItem : MonoBehaviour
{
    public int BuildModelId = 0;
    public int BuildPartId = 0;

    public bool Visible = false;
    private Renderer[] m_renderer;

    public void ResetConstructedBuildItem()
    {
        Visible = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_renderer = gameObject.GetComponentsInChildren<Renderer>();
        if (m_renderer != null)
        {
            foreach (var r in m_renderer)
            {
                r.enabled = Visible;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_renderer != null)
        {
            foreach (var r in m_renderer)
            {
                r.enabled = Visible;
            }
        }
    }
}
