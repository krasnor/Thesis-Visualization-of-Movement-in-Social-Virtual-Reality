using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextScrollViewContentHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject m_textPrefab;

    private readonly List<TMP_Text> m_instantiatedTexts = new List<TMP_Text>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fill(List<string> a_namesToShow)
    {
        foreach (var name in a_namesToShow)
        {
            var exists = m_instantiatedTexts.Exists(instantiatedCheckBox => instantiatedCheckBox.GetComponentInChildren<Text>().text == name);
            if (!exists)
            {
                var text = Instantiate(m_textPrefab, transform);
                var textComponent = text.GetComponent<TMP_Text>();
                textComponent.text = name;
                m_instantiatedTexts.Add(textComponent);
            }
        }
    }

    public void Fill(Dictionary<string, Color> a_colorMappings)
    {
        foreach (var colorMapping in a_colorMappings)
        {
            var name = colorMapping.Key;
            var color = colorMapping.Value;
            var exists = m_instantiatedTexts.Exists(instantiatedCheckBox => instantiatedCheckBox.GetComponentInChildren<Text>().text == name);
            if (!exists)
            {
                var checkBox = Instantiate(m_textPrefab, transform);
                var textComponent = checkBox.GetComponent<TMP_Text>();
                textComponent.text = name;
                textComponent.color = color;
                m_instantiatedTexts.Add(textComponent);
            }
        }
    }

    /// <summary>
    /// Destroy all text game objects.
    /// </summary>
    public void Clear()
    {
        foreach (var text in m_instantiatedTexts)
        {
            Destroy(text.gameObject);
        }
        m_instantiatedTexts.Clear();
    }
}
