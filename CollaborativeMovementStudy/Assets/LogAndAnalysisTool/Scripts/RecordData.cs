
/* IN-GAME REPLAY - @madebyfeno - <feno@ironequal.com>
* You can use it in commercial projects (and non-commercial project of course), modify it and share it.
* Do not resell the resources of this project as-is or even modified. 
* TL;DR: Do what the fuck you want but don't re-sell it
* 
* ironequal.com
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Edited
/// </summary>
[Serializable]
public class RecordData
{
    public TimelinedVector3 position;
    public TimelinedQuaternion rotation;
    public TimelinedVector3 scale;

    public void Add(Transform t, float time)
    {
        position.Add(t.position, time);
        rotation.Add(t.rotation, time);
        scale.Add(t.localScale, time);
    }

    public void Set(float _time, Transform _transform)
    {
        _transform.position = position.Get(_time);
        _transform.rotation = rotation.Get(_time);
        _transform.localScale = scale.Get(_time);
    }
}
/// <summary>
/// Edited
/// </summary>
[Serializable]
public class TimelinedVector3
{
    public AnimationCurve x;
    public AnimationCurve y;
    public AnimationCurve z;

    public void Add(Vector3 v, float time)
    {
        x.AddKey(time, v.x);
        y.AddKey(time, v.y);
        z.AddKey(time, v.z);
    }

    public Vector3 Get(float _time)
    {
        return new Vector3(x.Evaluate(_time), y.Evaluate(_time), z.Evaluate(_time));
    }
}
/// <summary>
/// Edited
/// </summary>
[Serializable]
public class TimelinedQuaternion
{
    public AnimationCurve x;
    public AnimationCurve y;
    public AnimationCurve z;
    public AnimationCurve w;

    public void Add(Quaternion v, float time)
    {
        x.AddKey(time, v.x);
        y.AddKey(time, v.y);
        z.AddKey(time, v.z);
        w.AddKey(time, v.w);
    }

    public Quaternion Get(float _time)
    {
        return new Quaternion(x.Evaluate(_time), y.Evaluate(_time), z.Evaluate(_time), w.Evaluate(_time));
    }
}



