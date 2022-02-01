using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private MenuContent m_uiContent;

    public void ShowSelection(VisualDataPoint a_visualDataPoint)
    {
        m_uiContent.SetSelectionPanel(a_visualDataPoint);
    }
}
