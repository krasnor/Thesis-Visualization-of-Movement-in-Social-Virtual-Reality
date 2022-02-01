using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitledCheckBoxScrollView<T> : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_headline;

    [SerializeField]
    private CheckboxScrollViewContentHandler m_contentFiller;

    public T Title { get; private set; }

    public CheckboxScrollViewContentHandler ContentFiller { get => m_contentFiller; private set => m_contentFiller = value; }

    public void SetContent(T a_title, List<string> a_itemNames, bool a_onlyOneToggle = false)
    {
        Title = a_title;
        ContentFiller.OnlyOneToggle = a_onlyOneToggle;
        ContentFiller.Fill(a_itemNames);
        SetHeader(Title);
    }

    public (T title, List<string> selectedItems) GetSelection()
    {
        return (Title, ContentFiller.GetSelectedItems());
    }

    public (T title, string selectedItem) GetSingleSelection()
    {
        return (Title, ContentFiller.GetSelectedItem());
    }

    protected virtual void SetHeader(T a_title)
    {
        m_headline.text = a_title.ToString();
    }
}
