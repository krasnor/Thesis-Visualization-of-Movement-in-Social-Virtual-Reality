using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ReplayManager : MonoBehaviour
{
    [SerializeField]
    private DataQueryManager m_dataQueryManager;

    [SerializeField]
    private Transform m_camerarig;

    [SerializeField]
    private Transform m_left;

    [SerializeField]
    private Transform m_right;

    private const string AnimationName = "Replay";

    private readonly List<Animation> m_animations = new List<Animation>();

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Play()
    {
        if (m_animations.Count != 0)
        {
            foreach (var animation in m_animations)
            {
                animation.Play();
            }
        }
        else
        {

            if (m_dataQueryManager.TryGetParticipantData(k => k.participantId, out var groupedCollection) && m_camerarig != null)
            {
                foreach (var participantDataKvp in groupedCollection)
                {
                    var animationClip = new AnimationClip();
                    var participantDataPoints = participantDataKvp.Value;
                    var start = participantDataPoints[0].pointInTimeUtc;
                    var curvePx = new AnimationCurve();
                    var curvePy = new AnimationCurve();
                    var curvePz = new AnimationCurve();
                    var curveRx = new AnimationCurve();
                    var curveRy = new AnimationCurve();
                    var curveRz = new AnimationCurve();
                    var curveRw = new AnimationCurve();
                    foreach (var participantDataPoint in participantDataPoints)
                    {
                        var pointInTime = participantDataPoint.pointInTimeUtc;
                        var timeFromStart = pointInTime - start;
                        var time = (float)timeFromStart.TotalSeconds + ((float)timeFromStart.Milliseconds / 1000);
                        var position = participantDataPoint.position.StringToVector3();
                        curvePx.AddKey(new Keyframe(time, position.x, 0, float.PositiveInfinity));
                        curvePy.AddKey(new Keyframe(time, position.y, 0, float.PositiveInfinity));
                        curvePz.AddKey(new Keyframe(time, position.z, 0, float.PositiveInfinity));

                        var rotation = participantDataPoint.hmdTransformRotation.StringToQuaternion();
                        curveRx.AddKey(new Keyframe(time, rotation.x, 0, float.PositiveInfinity));
                        curveRy.AddKey(new Keyframe(time, rotation.y, 0, float.PositiveInfinity));
                        curveRz.AddKey(new Keyframe(time, rotation.z, 0, float.PositiveInfinity));
                        curveRw.AddKey(new Keyframe(time, rotation.w, 0, float.PositiveInfinity));
                    }
                    animationClip.SetCurve("", typeof(Transform), "localPosition.x", curvePx);
                    animationClip.SetCurve("", typeof(Transform), "localPosition.y", curvePy);
                    animationClip.SetCurve("", typeof(Transform), "localPosition.z", curvePz);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.x", curveRx);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.y", curveRy);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.z", curveRz);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.w", curveRw);
                    animationClip.name = AnimationName;
                    animationClip.legacy = true;

                    var animation = m_camerarig.gameObject.AddComponent<Animation>();
                    animation.clip = animationClip;
                    animation.AddClip(animationClip, animationClip.name);
                    animation[AnimationName].time = 0f;
                    animation.Play();
                    m_animations.Add(animation);
                }
            }

            if (m_dataQueryManager.TryGetTransformsData(k => k.transformId, out var groupedCollection2))
            {
                foreach (var trackableTransform in FindObjectsOfType<TrackableTransform>())
                {
                    if (!groupedCollection2.ContainsKey(trackableTransform.TransformId))
                    {
                        Debug.LogWarningFormat("No data for key: {0}", trackableTransform.TransformId);
                        continue;
                    }
                    var animationClip = new AnimationClip();
                    var transformDataPoints = groupedCollection2[trackableTransform.TransformId];
                    var start = transformDataPoints[0].pointInTimeUtc;
                    var end = transformDataPoints[transformDataPoints.Count - 1].pointInTimeUtc;
                    var duration = start - end;

                    var curvePx = new AnimationCurve();
                    var curvePy = new AnimationCurve();
                    var curvePz = new AnimationCurve();
                    var curveRx = new AnimationCurve();
                    var curveRy = new AnimationCurve();
                    var curveRz = new AnimationCurve();
                    var curveRw = new AnimationCurve();
                    var curveSx = new AnimationCurve();
                    var curveSy = new AnimationCurve();
                    var curveSz = new AnimationCurve();
                    foreach (var transformDataPoint in transformDataPoints)
                    {
                        var pointInTime = transformDataPoint.pointInTimeUtc;
                        var timeFromStart = pointInTime - start;
                        var time = (float)timeFromStart.TotalSeconds + ((float)timeFromStart.Milliseconds / 1000);
                        var position = transformDataPoint.position.StringToVector3();
                        //in tangent 0 out tangent float.PositiveInfinity to achieve´tangent mode constant (no interpolation between frames)
                        curvePx.AddKey(new Keyframe(time, position.x, 0, float.PositiveInfinity));
                        curvePy.AddKey(new Keyframe(time, position.y, 0, float.PositiveInfinity));
                        curvePz.AddKey(new Keyframe(time, position.z, 0, float.PositiveInfinity));

                        var rotation = transformDataPoint.rotation.StringToQuaternion();
                        curveRx.AddKey(new Keyframe(time, rotation.x, 0, float.PositiveInfinity));
                        curveRy.AddKey(new Keyframe(time, rotation.y, 0, float.PositiveInfinity));
                        curveRz.AddKey(new Keyframe(time, rotation.z, 0, float.PositiveInfinity));
                        curveRw.AddKey(new Keyframe(time, rotation.w, 0, float.PositiveInfinity));

                        var scale = transformDataPoint.scale.StringToVector3();
                        curveSx.AddKey(new Keyframe(time, scale.x, 0, float.PositiveInfinity));
                        curveSy.AddKey(new Keyframe(time, scale.y, 0, float.PositiveInfinity));
                        curveSz.AddKey(new Keyframe(time, scale.z, 0, float.PositiveInfinity));
                    }

                    animationClip.SetCurve("", typeof(Transform), "localPosition.x", curvePx);
                    animationClip.SetCurve("", typeof(Transform), "localPosition.y", curvePy);
                    animationClip.SetCurve("", typeof(Transform), "localPosition.z", curvePz);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.x", curveRx);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.y", curveRy);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.z", curveRz);
                    animationClip.SetCurve("", typeof(Transform), "localRotation.w", curveRw);
                    animationClip.SetCurve("", typeof(Transform), "localScale.x", curveSx);
                    animationClip.SetCurve("", typeof(Transform), "localScale.y", curveSy);
                    animationClip.SetCurve("", typeof(Transform), "localScale.z", curveSz);
                    animationClip.name = AnimationName;
                    animationClip.legacy = true;
                    var animation = trackableTransform.gameObject.AddComponent<Animation>();
                    animation.clip = animationClip;
                    animation.AddClip(animationClip, animationClip.name);
                    animation[AnimationName].time = 0f;
                    animation.Play();
                    m_animations.Add(animation);

                    //var animator = trackableTransform.gameObject.AddComponent<Animator>();
                    //var animatorOverrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
                    //if (animatorOverrideController == null)
                    //{
                    //    animatorOverrideController = new AnimatorOverrideController();
                    //    animator.runtimeAnimatorController = animatorOverrideController;
                    //}
                    //animatorOverrideController[animationClip.name] = animationClip;
                    //animator.Play(animationClip.name);
                }

            }
        }
    }

    public void Pause()
    {
        foreach (var animation in m_animations)
        {
            animation.Stop();
        }
    }

    public void TimeLineValueChanged(float a_changedValue)
    {
        foreach (var animation in m_animations)
        {
            var newPlayTime = animation[AnimationName].length * a_changedValue;
            animation[AnimationName].time = newPlayTime;
        }
    }

    private IEnumerator TimeOfAnimations()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            foreach (var animation in m_animations)
            {
                Debug.LogFormat("Animation object: {0}, time={1}",
                    animation.GetComponent<TrackableTransform>(), animation["Replay"].time);
            }
        }
    }

    private static string GetTransformPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    // Update is called once per frame
    void Update()
    {

    }
}

/// <summary>
/// Todo
/// https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
/// </summary>
public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}
