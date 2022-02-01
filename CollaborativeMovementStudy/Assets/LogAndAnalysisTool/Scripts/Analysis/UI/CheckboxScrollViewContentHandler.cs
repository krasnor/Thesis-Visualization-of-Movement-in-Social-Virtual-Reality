using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Handling collection of checkboxes inside a scrollview. 
/// Not using ToggleGroups because of reset issues.
/// (see https://forum.unity.com/threads/why-does-the-toggle-group-always-reset-my-default-value.681454/)
/// </summary>
public class CheckboxScrollViewContentHandler : MonoBehaviour
{
    /// <summary>
    /// Checkbox prfab to instantiate.
    /// </summary>
    [SerializeField]
    private GameObject m_checkboxPrefab;

    /// <summary>
    /// Only one toggle is active at all times.
    /// </summary>
    [SerializeField]
    private bool onlyOneToggle;

    /// <summary>
    /// Instantiated Checkboxes.
    /// </summary>
    private readonly List<Toggle> m_instantiatedCheckboxes = new List<Toggle>();

    /// <summary>
    /// Only one toggle is active at all times.
    /// </summary>
    public bool OnlyOneToggle { get => onlyOneToggle; set => onlyOneToggle = value; }

    /// <summary>
    /// Add names to checkbox container if they do not already exist.
    /// </summary>
    /// <param name="a_namesToShow">Names to fill into checkbox container.</param>
    public void Fill(List<string> a_namesToShow)
    {
        foreach (var name in a_namesToShow)
        {
            var exists = m_instantiatedCheckboxes.Exists(instantiatedCheckBox => instantiatedCheckBox.GetComponentInChildren<Text>().text == name);
            if (!exists)
            {
                var checkBox = Instantiate(m_checkboxPrefab, transform);
                var textComponent = checkBox.GetComponentInChildren<Text>();
                textComponent.text = name;
                var toggle = checkBox.GetComponent<Toggle>();
                m_instantiatedCheckboxes.Add(toggle);
                toggle.onValueChanged.AddListener(on => OnToggleValueChanged(toggle, on));
            }
        }
        if (OnlyOneToggle)
        {
            DisableAllExceptOne();
        }
    }


    /// <summary>
    /// Destroy all toggle game objects.
    /// </summary>
    public void Clear()
    {
        foreach (var checkbox in m_instantiatedCheckboxes)
        {
            Destroy(checkbox.gameObject);
        }
        m_instantiatedCheckboxes.Clear();
    }

    /// <summary>
    /// Get Text value of selected toggle items.
    /// </summary>
    /// <returns>List of selected toggle items text.</returns>
    public List<string> GetSelectedItems()
    {
        List<string> selectedItems = new List<string>();
        foreach (var instantiatedCheckBox in m_instantiatedCheckboxes)
        {
            var textComponent = instantiatedCheckBox.GetComponentInChildren<Text>();
            if (instantiatedCheckBox.GetComponent<Toggle>().isOn)
                selectedItems.Add(textComponent.text);
        }

        return selectedItems;
    }

    /// <summary>
    /// Get one selected toggles text from the checkbox collection.
    /// </summary>
    /// <returns>Selected toggle text.</returns>
    public string GetSelectedItem()
    {
        var firstOnToggle = m_instantiatedCheckboxes.FirstOrDefault(instantiatedCheckbox => instantiatedCheckbox.isOn);
        if (firstOnToggle.Equals(default))
        {
            Debug.LogError("No item selected.");
            return "";
        }

        return firstOnToggle.GetComponentInChildren<Text>().text;
    }

    /// <summary>
    /// If OneToggle Mode is active:
    /// - Disable other checkboxes if value changed to on. 
    /// - Re-enable toggle if set to false and no other toggle is set to on.
    /// </summary>
    /// <param name="a_toggle">Toggle throwing event.</param>
    /// <param name="a_on">New toggle State.</param>
    private void OnToggleValueChanged(Toggle a_toggle, bool a_on)
    {
        if (OnlyOneToggle)
        {
            if (a_on)
            {
                foreach (var instantiatedCheckBox in m_instantiatedCheckboxes)
                {
                    if (!instantiatedCheckBox.Equals(a_toggle))
                    {
                        instantiatedCheckBox.isOn = false;
                    }
                }
            }
            else
            {
                var checkboxSelected = m_instantiatedCheckboxes.Exists(toggle => toggle.isOn);
                if (!checkboxSelected)
                    a_toggle.isOn = true;
            }
        }
    }

    /// <summary>
    /// Disable all checkboxes except one.
    /// </summary>
    private void DisableAllExceptOne()
    {
        var oneOn = false;
        foreach (var instantiatedCheckbox in m_instantiatedCheckboxes)
        {
            if (instantiatedCheckbox.isOn)
            {
                if (oneOn)
                    instantiatedCheckbox.isOn = false;

                oneOn = true;
            }
        }
    }

}
