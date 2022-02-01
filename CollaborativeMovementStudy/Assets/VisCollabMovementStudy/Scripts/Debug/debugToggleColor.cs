using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debugToggleColor : MonoBehaviour
{

    public bool InBaseState = true;
    public Material baseColor;
    public Material activeColor;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetState(bool baseColorState)
    {
        InBaseState = baseColorState;
        if (InBaseState)
        {
            GetComponent<Renderer>().material = baseColor;
        }
        else
        {
            GetComponent<Renderer>().material = activeColor;
        }
    }

    public void ToggleColor()
    {
        SetState(!InBaseState);
    }

}
