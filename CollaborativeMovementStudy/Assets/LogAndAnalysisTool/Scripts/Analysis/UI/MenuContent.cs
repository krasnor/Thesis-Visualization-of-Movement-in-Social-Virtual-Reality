using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuContent : MonoBehaviour
{
    [SerializeField]
    private SelectionPanel m_selectionPanel;

    [SerializeField]
    private RectTransform m_tabs;

    private MenuPanel[] m_panels;

    private Vector2 m_tabsPosition;

    private void Awake()
    {
        m_panels = GetComponentsInChildren<MenuPanel>(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_tabsPosition = m_tabs.anchoredPosition;
        //SetTabsPosition();
    }

    public void SetAllPanels(bool a_active)
    {
        foreach (var panel in m_panels)
        {
            panel.gameObject.SetActive(a_active);
        }
    }

    public void SetSelectionPanel(VisualDataPoint a_visualDataPoint)
    {
        //SetTabsPosition();
        m_selectionPanel.gameObject.SetActive(true);
        m_selectionPanel.SetContent(a_visualDataPoint);
    }

    public void SetTabsPosition()
    {
        foreach (var panel in m_panels)
        {
            if (panel.gameObject.activeSelf)
            {
                m_tabs.anchoredPosition = m_tabsPosition;
                return;
            }
        }

        m_tabs.anchoredPosition = new Vector2(0, 0);
    }
}
