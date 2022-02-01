using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarColorManager : MonoBehaviour
{
    public Color InitalColor = Color.gray;
    public Color CurrentAvatarColor;
    public bool IsInvisible;

    [Space]

    public GameObject Head;
    public GameObject Torso;
    public GameObject HandLeft;
    public GameObject HandRight;
    public AvatarTraceManager TraceManager;

    /// <summary>
    /// Some Shaders do not have set a Main Color -> thus we need to know the the color Property name to set a color correctly.
    /// If this array is Empty it will be attempted to set the color via the Main Color (material.color or "_Color")
    /// </summary>
    public string[] HandShaderColorPropertyNames = new string[0];

    private void Awake()
    {
        CurrentAvatarColor = InitalColor;
        if (Head == null)
            Debug.LogWarning("Head component not assigned. Will not be able to adjust color of this part.", this.gameObject);
        if (Torso == null)
            Debug.LogWarning("Torso component not assigned. Will not be able to adjust color of this part.", this.gameObject);
        if (HandLeft == null)
            Debug.LogWarning("HandLeft component not assigned. Will not be able to adjust color of this part.", this.gameObject);
        if (HandRight == null)
            Debug.LogWarning("HandRight component not assigned. Will not be able to adjust color of this part.", this.gameObject);
        if (TraceManager == null)
            Debug.LogWarning("TraceManager component not assigned. Will not be able to adjust color of this part.", this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateAvatarColor(CurrentAvatarColor);
    }

    public void SetInvisibilityState(bool a_IsInvisible)
    {
        IsInvisible = a_IsInvisible;
        UpdateAvatarColor(CurrentAvatarColor);
    }

    public void UpdateAvatarColor(Color a_newColor)
    {
        CurrentAvatarColor = a_newColor;

        //if (!IsInvisible)
        {
            // Update Colors
            Torso.GetComponent<Renderer>().material.color = CurrentAvatarColor;
            if (HandLeft != null)
            {
                Material material_leftHand = HandLeft.GetComponent<Renderer>()?.material;
                SetColorConsideringShaderPropertyName(material_leftHand, HandShaderColorPropertyNames, CurrentAvatarColor);
            }
            if (HandRight != null)
            {
                Material material_rightHand = HandRight.GetComponent<Renderer>()?.material;
                SetColorConsideringShaderPropertyName(material_rightHand, HandShaderColorPropertyNames, CurrentAvatarColor);
            }

            TraceManager.SetTraceColor(CurrentAvatarColor);
        }

        // player should be rendered as invisible
        if (Head != null)
            Head.GetComponent<Renderer>().enabled = !IsInvisible;
        if (Torso != null)
            Torso.GetComponent<Renderer>().enabled = !IsInvisible;
        if (HandLeft != null)
            HandLeft.GetComponent<Renderer>().enabled = !IsInvisible;
        if (HandRight != null)
            HandRight.GetComponent<Renderer>().enabled = !IsInvisible;
        if (TraceManager != null)
            TraceManager.LineRenderer.enabled = !IsInvisible;
    }

    private void SetColorConsideringShaderPropertyName(Material a_material, string[] a_propertiesToConsider, Color a_color)
    {
        if (a_material == null)
            return;

        if (a_propertiesToConsider.Length > 0)
        {
            foreach (string cPropname in a_propertiesToConsider)
            {
                //material_leftHand.SetColor("_InnerColor", CurrentAvatarColor);
                //material_rightHand.SetColor("_RimColor", CurrentAvatarColor);
                a_material.SetColor(cPropname, a_color);
            }
        }
        else
        {
            a_material.color = a_color;
        }
    }
}
