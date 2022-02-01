using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualizationManager : MonoBehaviour
{
    [SerializeField]
    private DataQueryManager m_dataQueryManager;

    [SerializeField]
    private VisualizationManager m_visualizationManager;

    [SerializeField]
    private List<PrefabVisualizationMapping> m_prefabVisualizationMapping = new List<PrefabVisualizationMapping>();

    private readonly Dictionary<string, VisualizationRenderer> m_visualizationRenderers = new Dictionary<string, VisualizationRenderer>();



    public T CreateOrGetVisualization<T>(VisualizationType a_visualizationType, LoggingDataPointType a_loggingDataPointType) where T : VisualizationRenderer
    {
        var visualizationName = string.Format("{0} {1}", a_visualizationType.ToFriendlyString(), a_loggingDataPointType.ToString());

        if (m_visualizationRenderers.ContainsKey(visualizationName))
        {
            Debug.LogFormat("Visualization with name {0} already exists.", visualizationName);
        }
        else
        {
            T instantiatedRenderer = CreateVisualization<T>(a_visualizationType, visualizationName);
            m_visualizationRenderers[visualizationName] = instantiatedRenderer;
        }

        return (T)m_visualizationRenderers[visualizationName];
    }

    /// <summary>
    /// Get all renderers of the specified VisualizationType.
    /// </summary>
    /// <typeparam name="T">VisualizationRenderer</typeparam>
    /// <param name="a_visualizationType">type thats used to find visualizations.</param>
    /// <param name="a_renderers">Found renderers.</param>
    /// <returns>True if renderers were found for the specified VisualizationType.</returns>
    public bool TryGetVisualization<T>(VisualizationType a_visualizationType, out List<T> a_renderers) where T : VisualizationRenderer
    {
        a_renderers = new List<T>();

        foreach (var visualizationRenderersKvp in m_visualizationRenderers)
        {
            var renderer = visualizationRenderersKvp.Value;
            if (renderer.VisualizationType.Equals(a_visualizationType))
            {
                a_renderers.Add((T)renderer);
            }
        }

        if (a_renderers.Count == 0)
            return false;

        return true;
    }

    public void ShowVisualizations(List<VisualizationFilterSelection> a_visualizationFilterSelections, Dictionary<VisualizationType, GroupingCategory> a_groupingSelection)
    {
        foreach (var visualizationFilterSelection in a_visualizationFilterSelections)
        {
            var filterSelections = visualizationFilterSelection.FilterSelections;
            if (!a_groupingSelection.TryGetValue(visualizationFilterSelection.Visualization, out var grouping))
            {
                Debug.LogFormat("No grouping for visualization {0}.", visualizationFilterSelection.Visualization);
            }

            if (filterSelections.TryGetValue(FilterCategory.EventType, out var eventSelection))
            {
                List<LoggingDataPointType> selectedEvents = ParseToLoggingDataPointTypeCollection(eventSelection);
                SelectAndShowVisualizations(visualizationFilterSelection, filterSelections, grouping, selectedEvents);
            }
            else
            {
                Debug.LogError("No event to visualize selected.");
            }
        }
    }

    private void SelectAndShowVisualizations(VisualizationFilterSelection a_visualizationFilterSelection, Dictionary<FilterCategory, List<string>> a_filterSelections, GroupingCategory a_grouping, List<LoggingDataPointType> a_selectedEvents)
    {
        foreach (var selectedEventType in a_selectedEvents)
        {
            SelectAndShowVisualization(a_visualizationFilterSelection, a_filterSelections, a_grouping, selectedEventType);
        }
    }

    private void SelectAndShowVisualization(VisualizationFilterSelection a_visualizationFilterSelection, Dictionary<FilterCategory, List<string>> a_filterSelections, GroupingCategory a_grouping, LoggingDataPointType a_selectedEventType)
    {
        IEnumerable<SpatialLoggingDataPoint> ungroupedData;
        Dictionary<string, List<SpatialLoggingDataPoint>> groupedData;
        switch (a_visualizationFilterSelection.Visualization)
        {
            case VisualizationType.HeatMap:
                ungroupedData = m_dataQueryManager.GetSpatialDataPoints<SpatialLoggingDataPoint>(a_selectedEventType, a_filterSelections);
                var heatMapRenderer = m_visualizationManager.CreateOrGetVisualization<HeatMapRenderer>(VisualizationType.HeatMap, a_selectedEventType);
                heatMapRenderer.ShowHeatMaps(ungroupedData);
                break;
            case VisualizationType.PointCloud:
                groupedData = m_dataQueryManager.GetSpatialDataPoints<SpatialLoggingDataPoint>(a_selectedEventType, a_filterSelections, a_grouping);
                var pointCloud = m_visualizationManager.CreateOrGetVisualization<PointCloudRenderer>(VisualizationType.PointCloud, a_selectedEventType);
                pointCloud.ShowVisualization(groupedData, a_selectedEventType);
                break;
            case VisualizationType.LineGraph:
                groupedData = m_dataQueryManager.GetSpatialDataPoints<SpatialLoggingDataPoint>(a_selectedEventType, a_filterSelections, a_grouping);
                var lineGraph = m_visualizationManager.CreateOrGetVisualization<LineGraphRenderer>(VisualizationType.LineGraph, a_selectedEventType);
                lineGraph.ShowVisualization(groupedData, a_selectedEventType);
                break;
            case VisualizationType.None:
            default:
                Debug.LogError("No visualization selected to show.");
                break;
        }
    }

    private List<LoggingDataPointType> ParseToLoggingDataPointTypeCollection(List<string> a_eventSelection)
    {
        var selectedEvents = new List<LoggingDataPointType>();
        foreach (var selectedItem in a_eventSelection)
        {
            if (Enum.TryParse<LoggingDataPointType>(selectedItem, true, out var parsedSelectedItem))
            {
                selectedEvents.Add(parsedSelectedItem);
            }
        }

        return selectedEvents;
    }

    private T CreateVisualization<T>(VisualizationType a_visualizationType, string a_visualizationName) where T : VisualizationRenderer
    {
        var mapping = m_prefabVisualizationMapping.FirstOrDefault(mapping => mapping.VisualizationType.Equals(a_visualizationType));
        var instantiatedRenderer = Instantiate(mapping.Prefab, transform).GetComponent<T>();
        instantiatedRenderer.name = a_visualizationName;
        return instantiatedRenderer;
    }

}

[Serializable]
public class PrefabVisualizationMapping
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private VisualizationType visualizationType;

    public GameObject Prefab { get => prefab; set => prefab = value; }

    public VisualizationType VisualizationType { get => visualizationType; set => visualizationType = value; }
}
