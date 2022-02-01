using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualizationPanel : MenuPanel
{
    [SerializeField]
    private VisualizationManager m_visualizationManager;

    [SerializeField]
    private FilterPanel m_filterPanel;

    [SerializeField]
    private GroupingPanel m_groupingPanel;

    [SerializeField]
    private CheckboxScrollViewContentHandler m_visualizationSelection;

    [SerializeField]
    private GameObject m_content;

    [SerializeField]
    private GameObject m_textScrollViewPrefab;

    private Dictionary<VisualizationType, GameObject> m_legends = new Dictionary<VisualizationType, GameObject>();

    private List<VisualizationType> m_prevSelectedVisualizations = new List<VisualizationType>();

    private void OnEnable()
    {
        FillVisualizationSelection();
    }

    /// <summary>
    /// Fill the options of visualizations.
    /// </summary>
    public void FillVisualizationSelection()
    {
        List<string> visualizationTypeNames = new List<string>();
        foreach (VisualizationType visualizationType in Enum.GetValues(typeof(VisualizationType)))
        {
            if (!visualizationType.Equals(VisualizationType.None))
                visualizationTypeNames.Add(visualizationType.ToFriendlyString());
        }
        m_visualizationSelection.Fill(visualizationTypeNames);
    }

    public List<VisualizationType> GetVisualizationSelection()
    {
        List<VisualizationType> visualizationTypes = new List<VisualizationType>();
        foreach (var selectedItem in m_visualizationSelection.GetSelectedItems())
        {
            visualizationTypes.Add(selectedItem.ParseToVisualizationType());
        }
        return visualizationTypes;
    }

    /// <summary>
    /// Show Visualizations based on the filter selections.
    /// </summary>
    public void OnShowVisualization()
    {
        var filterSelections = m_filterPanel.GetSelection();
        var groupingSelection = m_groupingPanel.GetSelection();
        m_visualizationManager.ShowVisualizations(filterSelections, groupingSelection);
        foreach (var filterSelection in filterSelections)
        {
            switch (filterSelection.Visualization)
            {
                case VisualizationType.None:
                    break;
                case VisualizationType.HeatMap:
                    if (m_visualizationManager.TryGetVisualization<HeatMapRenderer>(filterSelection.Visualization, out var renderers))
                    {

                    }
                    break;
                case VisualizationType.PointCloud:
                    if (m_visualizationManager.TryGetVisualization<PointCloudRenderer>(filterSelection.Visualization, out var pointCloudRenderers))
                    {
                        foreach (var pointCloudRenderer in pointCloudRenderers)
                        {
                            var buttonScrollView = Instantiate(m_textScrollViewPrefab, m_content.transform);
                            buttonScrollView.GetComponentInChildren<TextScrollViewContentHandler>().Fill(pointCloudRenderer.ColorMappings);
                            m_legends[filterSelection.Visualization] = buttonScrollView;
                        }
                    }
                    break;
                case VisualizationType.LineGraph:
                    break;
                default:
                    break;
            }
        }
    }


    // Todo Remove
    /// <summary>
    /// Remove and/or add filter categories to the filter panel depending on the visualization selection.
    /// </summary>
    public void OnVisualizationSelectionChanged()
    {
        foreach (VisualizationType visualizationType in Enum.GetValues(typeof(VisualizationType)))
        {
            var exists = m_prevSelectedVisualizations.Exists((selectedItem) => selectedItem.Equals(visualizationType));

            if (!exists)
                m_filterPanel.RemoveCategoryObjectsContent(visualizationType);
        }

        foreach (var currentSelection in m_visualizationSelection.GetSelectedItems())
        {
            var currentSelectionItem = currentSelection.ParseToVisualizationType();
            var exists = m_prevSelectedVisualizations.Exists((selectedItem) => selectedItem.Equals(currentSelectionItem));

            if (!exists)
                m_filterPanel.FillCategoryObjectsContent(currentSelectionItem);
        }
    }
}
