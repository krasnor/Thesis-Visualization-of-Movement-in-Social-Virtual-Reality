using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuTabsUI : MonoBehaviour
{
    [SerializeField]
    private MenuContent m_menuContent;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var tabButton in GetComponentsInChildren<Button>())
        {
            tabButton.onClick.AddListener(OnTabButtonClicked);
        }
    }

    private void OnTabButtonClicked()
    {
        //m_menuContent.SetTabsPosition();
    }
}
