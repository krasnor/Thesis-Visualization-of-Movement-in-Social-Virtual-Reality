using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointCloudRenderer : GroupedVisualizationRenderer
{
    [SerializeField]
    private AnalysisManager m_analysisManager;

    private List<Transform> m_visualizations = new List<Transform>();

    [SerializeField, Range(0.1f, 1f)]
    private float m_cubeSize = 0.3f;

    public float CubeSize
    {
        get => m_cubeSize;
        set
        {
            var oldCubeSize = m_cubeSize;
            m_cubeSize = value;

            if (oldCubeSize != m_cubeSize)
                UpdateVisualization();
        }
    }

    public Dictionary<string, List<GameObject>> VisualizedPointsCollections { get; private set; } = new Dictionary<string, List<GameObject>>();

    public override VisualizationType VisualizationType => VisualizationType.PointCloud;

    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_pointCollections"></param>
    /// <param name="a_labelLetter"></param>
    public override void ShowVisualization<T>(Dictionary<string, List<T>> a_pointCollections, LoggingDataPointType a_loggingDataPointType)
    {
        //ClearAllPoints();

        ColorGenerator colorGenerator = new ColorGenerator(a_pointCollections.Count);
        char lableLetter = GetLableLetter(a_loggingDataPointType);
        switch (a_loggingDataPointType)
        {
            case LoggingDataPointType.SpatialAudioEvent:
            case LoggingDataPointType.OfflineSpatialAudioEvent:
                ShowAudioDataPoints(a_pointCollections, colorGenerator, lableLetter);
                break;
            case LoggingDataPointType.ParticipantTrackingData:
            case LoggingDataPointType.InputActionEvent:
            case LoggingDataPointType.CustomSpatialEvent:
            case LoggingDataPointType.TransformTrackingData:
            default:
                ShowSpatialDataPoints(a_pointCollections, colorGenerator, lableLetter);
                break;
        }
    }

    private void ShowSpatialDataPoints<T>(Dictionary<string, List<T>> a_pointCollections, ColorGenerator a_colorGenerator, char a_lableLetter) where T : SpatialLoggingDataPoint
    {
        foreach (var pointCollection in a_pointCollections)
        {
            ShowPoints(pointCollection.Key, pointCollection.Value, a_colorGenerator, a_lableLetter);
        }
    }

    private void ShowAudioDataPoints<T>(Dictionary<string, List<T>> a_pointCollections, ColorGenerator a_colorGenerator, char a_lableLetter) where T : SpatialLoggingDataPoint
    {
        foreach (var pointCollection in a_pointCollections)
        {
            var key = pointCollection.Key;
            var color = a_colorGenerator.GetColor();
            color.a = Alpha;

            var cubeCollection = new List<GameObject>();

            foreach (var point in pointCollection.Value)
            {
                var pointObject = CreatePoint(key, a_lableLetter, color, point.position.StringToVector3(), point);
                pointObject.AddComponent<AudioSource>();
                pointObject.AddComponent<AudioPlayer>();
                cubeCollection.Add(pointObject);
            }

            ColorMappings[key] = color;
            VisualizedPointsCollections[key] = cubeCollection;
        }
    }


    private void ShowPoints<T>(string a_id, List<T> a_points, ColorGenerator a_colorGenerator, char a_labelLetter = default) where T : SpatialLoggingDataPoint
    {
        var color = a_colorGenerator.GetColor();
        color.a = Alpha;

        var cubeCollection = new List<GameObject>();

        foreach (var point in a_points)
        {
            var pointObject = CreatePoint(a_id, a_labelLetter, color, point.position.StringToVector3(), point);
            cubeCollection.Add(pointObject);
        }

        ColorMappings[a_id] = color;
        VisualizedPointsCollections[a_id] = cubeCollection;
    }

    private GameObject CreatePoint(string a_id, char a_labelLetter, Color a_color, Vector3 a_location, LoggingDataPoint a_loggingDataPoint)
    {
        var cube = Instantiate(Prefab, new Vector3(0, 0, 0), Quaternion.identity);
        cube.transform.position = a_location;
        cube.transform.localScale = new Vector3(CubeSize, CubeSize, CubeSize);
        cube.GetComponent<Renderer>().material.color = a_color;
        if (a_labelLetter != default)
            ShowLabels(cube, a_labelLetter);

        cube.name = GetRenderedEntityName(a_id);
        var visualDataPoint = cube.GetComponent<VisualDataPoint>();
        visualDataPoint.Id = a_id;
        visualDataPoint.LoggingDataPoint = a_loggingDataPoint;

        return cube;
    }

    public override void ClearVisualization()
    {
        ClearAllPoints();
    }

    public override void UpdateVisualization()
    {
        Debug.Log("UpdateVisualization m_cubeCollections.Count: " + VisualizedPointsCollections.Count);

        foreach (var cubeCollection in VisualizedPointsCollections)
        {
            foreach (var cube in cubeCollection.Value)
            {
                cube.transform.localScale = new Vector3(CubeSize, CubeSize, CubeSize);

                var renderer = cube.GetComponent<Renderer>();
                var color = renderer.material.color;
                renderer.material.color = new Color(color.r, color.g, color.b, Alpha);
            }
        }
    }

    private void ClearAllPoints()
    {
        foreach (var visualizedPointsCollection in VisualizedPointsCollections)
        {
            foreach (var point in visualizedPointsCollection.Value)
            {
                Destroy(point);
            }
        }
    }


}
