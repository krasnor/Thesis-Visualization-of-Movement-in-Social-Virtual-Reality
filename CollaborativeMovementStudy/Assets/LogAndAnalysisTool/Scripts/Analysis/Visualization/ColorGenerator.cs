using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGenerator
{
    private const float MaxHueDegree = 1;

    private float CurrentHueDegree { get; set; } = 0;

    private float HueDegreeStepSize { get; set; } = 0.1f;

    private int NumberOfObjectGroups { get; set; }

    public ColorGenerator(int a_numberOfObjectGroups)
    {
        NumberOfObjectGroups = a_numberOfObjectGroups;
        HueDegreeStepSize = MaxHueDegree / NumberOfObjectGroups;
    }

    public Color GetColor()
    {
        var hueDegree = GetCurrentHueDegree();
        return Color.HSVToRGB(hueDegree, 1, 1);
    }

    public void Reset(int a_numberOfObjectGroups)
    {
        NumberOfObjectGroups = a_numberOfObjectGroups;
        HueDegreeStepSize = MaxHueDegree / NumberOfObjectGroups;
        ResetCurrentHueDegree();
    }

    private void ResetCurrentHueDegree()
    {
        CurrentHueDegree = 0;
    }

    private float GetCurrentHueDegree()
    {
        var currentHueDegree = CurrentHueDegree;

        if (CurrentHueDegree != MaxHueDegree)
            CurrentHueDegree += HueDegreeStepSize;
        else
            CurrentHueDegree = MaxHueDegree;

        Debug.Log(string.Format("currentHueDegree: {0} HueDegreeStepSize: {1}", currentHueDegree, HueDegreeStepSize));

        return currentHueDegree;
    }

}
