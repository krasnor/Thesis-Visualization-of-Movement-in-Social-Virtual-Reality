using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FadeOutAndDestroy : MonoBehaviour
{
    // TODO create seperate particle script

    public float FadeOutTime = 10;
    public bool DestroyAfterFadeOut = true;
    public Renderer[] affectedRenderers; // manually link for performance Reasons
    public ParticleSystem[] affectedParticleSystems; // manually link for performance Reasons

    [SerializeField] private float CurrentTimeLeft = 10;
    [SerializeField] private float CurrentTime = 0;

    //public AnimationCurve testCurve;

    public void SetTraceColorAndTime(Color traceColor, float fadeOutTime)
    {
        FadeOutTime = fadeOutTime;
        foreach (var ps in affectedParticleSystems)
        {
            ps.time = FadeOutTime;

            //ps.startLifetime = FadeOutTime;  // TODO update to new api
            //ps.startColor = traceColor; // TODO update to new api

            ParticleSystem.MainModule m = ps.main;
            m.startLifetime = FadeOutTime;
            m.startColor = traceColor;

        }
        foreach (Renderer r in affectedRenderers)
        {
            r.material.color = traceColor;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        CurrentTimeLeft = FadeOutTime;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        CurrentTimeLeft -= Time.deltaTime;

        if (CurrentTime >= FadeOutTime && DestroyAfterFadeOut)
        {
            // DestroyGameObject
            Destroy(gameObject);
        }

        // update Interpolation
        float interp = CurrentTime / FadeOutTime;
        float interp2 = 1 - (CurrentTimeLeft / FadeOutTime);

        UpdateFadingAnimation(interp);
    }

    void UpdateFadingAnimation(float interpolationStep)
    {
        //Renderer[] renderers = this.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in affectedRenderers)
        {
            Color objColor = r.material.color;
            objColor.a = 1 - Mathf.Clamp01(interpolationStep);

            //objColor.a = testCurve.Evaluate(interpolationStep);
            //objColor.a = 0;
            //Debug.Log("step: " + interpolationStep + "a: " + objColor.a);

            r.material.color = objColor;
            //this.GetComponent<Renderer>().material.color = new Color(objColor.r, objColor.g, objColor.b, 0);
        }

    }
}
