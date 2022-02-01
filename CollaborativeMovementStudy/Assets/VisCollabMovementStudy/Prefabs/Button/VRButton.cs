using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class VRButton : XRBaseInteractable
{
    private XRBaseInteractor hoverInteractor = null;
    private float previousHandHeight = 0.0f;

    public float threshold = 0.01f;
    private float yMin = 0.0f;
    private float yMax = 0.0f;
    private bool pressedLastFrameState = false;

    public UnityEvent OnPress = null;

    protected override void Awake()
    {
        base.Awake();
        hoverEntered.AddListener(StartPress);
        hoverExited.AddListener(EndPress);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        hoverEntered.RemoveListener(StartPress);
        hoverExited.RemoveListener(EndPress);
    }


    private void StartPress(HoverEnterEventArgs args)
    {
        //Debug.Log("Start Press");
        hoverInteractor = args.interactor;
        previousHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
    }

    private void EndPress(HoverExitEventArgs args)
    {
        //Debug.Log("End Press");
        hoverInteractor = null;
        previousHandHeight = 0.0f;

        pressedLastFrameState = false;
        SetYPosition(yMax);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetMinMax();
    }

    private void SetMinMax()
    {
        Collider collider = GetComponent<Collider>();
        yMin = transform.localPosition.y - (collider.bounds.size.y * 0.5f);
        yMax = transform.localPosition.y;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (hoverInteractor)
        {
            float newHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
            float handDifference = previousHandHeight - newHandHeight;

            previousHandHeight = newHandHeight;

            float newPosition = transform.localPosition.y - handDifference;
            SetYPosition(newPosition);

            CheckPress();
        }
    }

    private float GetLocalYPosition(Vector3 position)
    {
        Vector3 localPosition = transform.root.InverseTransformPoint(position);
        return localPosition.y;
    }

    private void SetYPosition(float pos)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.y = Mathf.Clamp(pos, yMin, yMax);

        transform.localPosition = newPosition;
    }

    private void CheckPress()
    {
        bool inPos = InPosition();

        if (inPos && inPos != pressedLastFrameState)
        {
            OnPress.Invoke();
        }

        pressedLastFrameState = inPos;
    }

    private bool InPosition()
    {
        float inRange = Mathf.Clamp(transform.localPosition.y, yMin, yMin + threshold);

        return transform.localPosition.y == inRange; // in range will only be different when it was clamped -> not in range
    }

}
