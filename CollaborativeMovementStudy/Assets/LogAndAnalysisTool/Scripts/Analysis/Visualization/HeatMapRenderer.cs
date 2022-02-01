using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HeatMapRenderer : VisualizationRenderer
{
    [SerializeField]
    private List<MeshRenderer> m_meshesToOverlay = new List<MeshRenderer>();

    private readonly List<HeatMapVisual> m_heatMaps = new List<HeatMapVisual>();

    public List<MeshRenderer> MeshesToOverlay { get => m_meshesToOverlay; private set => m_meshesToOverlay = value; }

    public override VisualizationType VisualizationType => VisualizationType.HeatMap;

    public void ShowHeatMaps<T>(IEnumerable<T> a_spatialLoggingDataPoints) where T : SpatialLoggingDataPoint
    {
        if (MeshesToOverlay == null || MeshesToOverlay.Count == 0)
            FindMeshesToOverlay();

        foreach (var heatMap in m_heatMaps)
        {
            Destroy(heatMap.gameObject);
        }

        m_heatMaps.Clear();

        foreach (var meshToOverlay in MeshesToOverlay)
        {
            var gameObject = Instantiate(Prefab, transform);
            var heatMapVisual = gameObject.GetComponent<HeatMapVisual>();
            heatMapVisual.ShowHeatmap(meshToOverlay, a_spatialLoggingDataPoints);
            m_heatMaps.Add(heatMapVisual);
        }
    }

    private void FindMeshesToOverlay()
    {
        foreach (var teleportationArea in FindObjectsOfType<TeleportationArea>())
        {
            if (teleportationArea.TryGetComponent<MeshRenderer>(out var meshRenderer) && !MeshesToOverlay.Contains(meshRenderer))
            {
                MeshesToOverlay.Add(meshRenderer);
            }
        }

        foreach (var teleportationAnchor in FindObjectsOfType<TeleportationAnchor>())
        {
            if (teleportationAnchor.TryGetComponent<MeshRenderer>(out var meshRenderer) && !MeshesToOverlay.Contains(meshRenderer))
            {
                MeshesToOverlay.Add(meshRenderer);
            }
        }
    }

    public override void UpdateVisualization()
    {
        foreach (var heatmap in m_heatMaps)
        {
            var renderer = heatmap.GetComponent<Renderer>();
            var color = renderer.material.color;
            renderer.material.color = new Color(color.r, color.g, color.b, Alpha);
        }
    }

    public override void ClearVisualization()
    {
        foreach (var heatMap in m_heatMaps)
        {
            Destroy(heatMap.gameObject);
        }
        m_heatMaps.Clear();
    }
}
