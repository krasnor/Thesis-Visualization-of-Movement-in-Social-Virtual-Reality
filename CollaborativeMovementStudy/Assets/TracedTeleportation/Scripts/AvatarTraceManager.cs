using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AvatarTraceManager : MonoBehaviour
{
    /// <summary>
    /// Used to specify in the coordinate system of given positions.
    /// E.g. Are this positions relative to the avatars head or its Ground/Feet position?
    /// -> depending on coordinate system a different offset may be wished.
    /// </summary>
    public enum CoordinateType
    {
        GROUND,
        BODY,
        HEAD
    }

    public LineRenderer LineRenderer;

    private List<Vector3> m_trailPositions = new List<Vector3>();
    private List<float> m_trailPositionTimes = new List<float>();

    public float EndTime = 5;
    public Color TraceColor = Color.magenta;
    public float TraceWidth = 0.2f;

    [Space]
    public Vector3 TraceOffset_Ground = new Vector3(0, 0.11f, 0);
    public Vector3 TraceOffset_Body = new Vector3(0, 0, 0);
    public Vector3 TraceOffset_Head = new Vector3(0, 0, 0);

    public event LineSegmentAdded OnLineSegmentAdded;
    public delegate void LineSegmentAdded(object sender, LineSegmentAddedEventArgs e);
    public class LineSegmentAddedEventArgs
    {
        public LineSegmentAddedEventArgs(Vector3 a_from, Vector3 a_to, AvatarTraceManager.CoordinateType a_coordinateType, Vector3 a_offset) { PositionFrom = a_from; PositionTo = a_to; CoordinateType = a_coordinateType; Offset = a_offset; }
        public Vector3 PositionFrom;
        public Vector3 PositionTo;
        public AvatarTraceManager.CoordinateType CoordinateType;
        public Vector3 Offset;
    }

    public void SetTraceColor(Color a_traceColor)
    {
        TraceColor = a_traceColor;

        // ## Update Gradient
        Gradient current_gradient = LineRenderer.colorGradient;

        GradientColorKey[] currentColors = current_gradient.colorKeys;
        GradientColorKey[] changedColors = new GradientColorKey[currentColors.Length];
        for (int i = 0; i < current_gradient.colorKeys.Length; i++)
        {
            changedColors[i] = new GradientColorKey()
            {
                color = TraceColor,
                time = currentColors[i].time,
            };
        }
        current_gradient.SetKeys(changedColors, current_gradient.alphaKeys);
        LineRenderer.colorGradient = current_gradient;
    }

    public void SetDefaultGradient(Color a_traceColor)
    {
        TraceColor = a_traceColor;

        Gradient default_trace_gradient = new Gradient();

        default_trace_gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey()
                {
                    color = a_traceColor,
                    time = 0f,
                },
                 new GradientColorKey()
                {
                    color = a_traceColor,
                    time = 1f,
                },
            }, 
            new GradientAlphaKey[] {
                new GradientAlphaKey()
                {
                    alpha= 0f,
                    time = 0f,
                },
                 new GradientAlphaKey()
                {
                    alpha = 255f,
                    time = 27f,
                },
            });

        LineRenderer.colorGradient = default_trace_gradient;
    }

    private Vector3 GetTraceOffset(CoordinateType a_traceOffsetMode)
    {
        switch (a_traceOffsetMode)
        {
            case CoordinateType.GROUND:
                return TraceOffset_Ground;
            case CoordinateType.BODY:
                return TraceOffset_Body;
            case CoordinateType.HEAD:
                return TraceOffset_Head;
            default:
                Assert.IsTrue(false, $"Unhandled {nameof(CoordinateType)}={a_traceOffsetMode}.");
                break;
        }
        return new Vector3();
    }

    public void AddLineTrailSegment(Vector3 a_pos_from, Vector3 a_pos_to, CoordinateType a_coordinateType = CoordinateType.GROUND)
    {
        Vector3 offset = GetTraceOffset(a_coordinateType);
        a_pos_from += offset;
        a_pos_to += offset;

        // if no points exist yet, add line-point of start position
        if (LineRenderer.positionCount == 0)
        {
            LineRenderer.positionCount = 1;
        
            m_trailPositionTimes.Add(Time.time - EndTime);
            m_trailPositions.Add(a_pos_from);
            LineRenderer.SetPosition(0, a_pos_from);
        }

        // add line-point of target position
        int currPositionCount = LineRenderer.positionCount + 1;
        LineRenderer.positionCount = currPositionCount;

        m_trailPositionTimes.Add(Time.time);
        m_trailPositions.Add(a_pos_to);
        LineRenderer.SetPosition(currPositionCount - 1, a_pos_to);

        // send events
        OnLineSegmentAdded?.Invoke(this, new LineSegmentAddedEventArgs(a_pos_from, a_pos_to, a_coordinateType, offset));
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        LineRenderer = gameObject.GetComponent<LineRenderer>();
        if (LineRenderer == null)
            throw new MissingComponentException("LineRenderer Component is not assigned.");

        SetTraceColor(TraceColor);
        LineRenderer.startWidth = TraceWidth;
        LineRenderer.endWidth = TraceWidth;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        float currentTime = Time.time;

        // Fadeout of LineTrail
        for (int i = m_trailPositionTimes.Count - 1; i >= 0; i--)
        {
            // find first outdated Trail Point
            if (m_trailPositionTimes[i] + EndTime <= Time.time)
            {
                // this point is outdated -> handle point deletion
                // Debug.Log("Trail end found idx: " + i + " t:" + posTimes[i] + " CurrTime:" + curr_time);

                if (m_trailPositionTimes.Count == 1)
                {
                    // ## only one point remaining, remove last point
                    LineRenderer.positionCount = 0;
                    m_trailPositionTimes.RemoveRange(0, i + 1);
                    m_trailPositions.RemoveRange(0, i + 1);
                    break;
                }
                else if (m_trailPositions.Count == i + 1)
                {
                    // ## closest/youngest point to avatar is outdated
                    // -> clear list
                    m_trailPositions.Clear();
                    m_trailPositionTimes.Clear();
                    LineRenderer.positionCount = 0;

                    break;
                }
                else
                {
                    // calc new end
                    if (m_trailPositions.Count == i + 1)
                    {
                        Debug.Log("err");
                    }
                    Vector3 last_validPoint = m_trailPositions[i + 1];
                    Vector3 curr_EndPoint = m_trailPositions[i];

                    float last_validPoint_Time = m_trailPositionTimes[i + 1]; // 15 
                    float curr_EndPoint_Time = m_trailPositionTimes[i];  // 10

                    float delta_segment = last_validPoint_Time - curr_EndPoint_Time; // 15-10 = 5
                    float delta_segment_time = (currentTime - EndTime) - curr_EndPoint_Time; // 12-10 = 2
                    float delta_percent = delta_segment_time / delta_segment; // 2/5

                    Vector3 interpolated_new_end = Vector3.Lerp(curr_EndPoint, last_validPoint, delta_percent);

                    if (interpolated_new_end == last_validPoint)
                    {
                        //Debug.Log("reached end");
                    }

                    // apply changes
                    m_trailPositions[i] = interpolated_new_end;
                    m_trailPositionTimes[i] = currentTime - EndTime;

                    // cleanup
                    if (i > 0)
                    {
                        // delete all positions after this point (except this one)
                        m_trailPositionTimes.RemoveRange(0, i);
                        m_trailPositions.RemoveRange(0, i);
                    }

                    // update LineRenderer
                    LineRenderer.positionCount = m_trailPositions.Count;
                    LineRenderer.SetPositions(m_trailPositions.ToArray());
                    break;
                }
            }

        }
    }
}
