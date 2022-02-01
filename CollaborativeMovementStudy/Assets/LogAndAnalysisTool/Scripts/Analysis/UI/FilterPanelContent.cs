using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FilterPanelContent : MonoBehaviour
{
    [SerializeField]
    private GameObject m_scrollViewCollectionContainerPrefab;

    [SerializeField]
    private GameObject m_filterCategoryPrefab;

    public Dictionary<VisualizationType, UIScrollViewCollectionContainer> FilterCategoriesCollection { get; private set; } = new Dictionary<VisualizationType, UIScrollViewCollectionContainer>();

    public void AddFilterCategories(VisualizationType a_visualization, Dictionary<FilterCategory, List<string>> a_filterSelections)
    {
        if (!FilterCategoriesCollection.ContainsKey(a_visualization))
        {
            UIScrollViewCollectionContainer uiFilterCategories = CreateUIFilterCategories(a_visualization);
            foreach (var filterSelectionKvp in a_filterSelections)
            {
                AddFilterCategory(filterSelectionKvp, uiFilterCategories.ScrollViewCollectionContainer.transform);
            }
            Debug.LogFormat("Filter categories for visualization {0} does not already exist. Create new filter categories object.", a_visualization);
        }
        else
        {
            FilterCategoriesCollection[a_visualization].gameObject.SetActive(true);
            Debug.LogFormat("Filter categories for visualization {0} already exist. Set visible.", a_visualization);
        }
    }

    public void RemoveFilterCategories(VisualizationType a_visualization)
    {
        if (FilterCategoriesCollection.TryGetValue(a_visualization, out var filterCategories))
        {
            filterCategories.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogFormat("Filter categories for visualization {0} already not visible.", a_visualization);
        }
    }

    public void RemoveAllFilterCategories()
    {
        foreach (var filterCategoriesCollectionKvp in FilterCategoriesCollection)
        {
            filterCategoriesCollectionKvp.Value.gameObject.SetActive(false);
        }
    }

    public List<VisualizationFilterSelection> GetVisualizationFilterSelection()
    {
        var visualizationFilterSelection = new List<VisualizationFilterSelection>();
        foreach (var filterCategoriesCollectionKvp in FilterCategoriesCollection)
        {
            if (filterCategoriesCollectionKvp.Value.gameObject.activeSelf)
            {
                Debug.LogFormat("Include {0} in selection.", filterCategoriesCollectionKvp.Key);
                var uiFilterCategories = filterCategoriesCollectionKvp.Value.ScrollViewCollectionContainer.GetComponentsInChildren<UIFilterCategory>();
                if (uiFilterCategories != null)
                {
                    var filterSelections = new Dictionary<FilterCategory, List<string>>();
                    foreach (var uiFilterCategory in uiFilterCategories)
                    {
                        var selection = uiFilterCategory.GetSelection();
                        filterSelections[selection.title] = selection.selectedItems;
                    }
                    visualizationFilterSelection.Add(new VisualizationFilterSelection(filterCategoriesCollectionKvp.Key, filterSelections));
                }
                else
                {
                    Debug.LogError("No UIFilterCategory scripts attached to children.");
                }
            }
        }
        return visualizationFilterSelection;
    }

    private void AddFilterCategory(KeyValuePair<FilterCategory, List<string>> a_filterSelection, Transform a_parent)
    {
        var uiFilterCategory = Instantiate(m_filterCategoryPrefab, a_parent).GetComponent<UIFilterCategory>();
        uiFilterCategory.SetContent(a_filterSelection.Key, a_filterSelection.Value);
    }

    private UIScrollViewCollectionContainer CreateUIFilterCategories(VisualizationType a_visualization)
    {
        var uiFilterCategoriesObject = Instantiate(m_scrollViewCollectionContainerPrefab, transform);
        var uiFilterCategories = uiFilterCategoriesObject.GetComponent<UIScrollViewCollectionContainer>();
        uiFilterCategories.SetHeader(a_visualization.ToFriendlyString());
        FilterCategoriesCollection[a_visualization] = uiFilterCategories;
        return uiFilterCategories;
    }

}
