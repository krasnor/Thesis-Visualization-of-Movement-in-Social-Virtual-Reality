using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkColorSync : MonoBehaviourPun
{
    private Color lastSyncedColor;
    private Renderer _renderer;
    public string[] ShaderColorPropertyNames = new string[0];

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            Color currentColor = _renderer.material.color;
            if (currentColor != lastSyncedColor)
            {
                lastSyncedColor = currentColor;
                photonView.RPC("RPC_SetNetworkPlayerColor", RpcTarget.All, new object[] { currentColor.r, currentColor.g, currentColor.b, currentColor.a }); // update local on all networkPlayers
            }
        }
    }

    [PunRPC]
    public void RPC_SetNetworkPlayerColor(float r, float g, float b, float a)
    {
        Debug.Log("NetworkColorSync.RPC_SetNetworkPlayerColor");
        Color tmp_color = new Color(r, g, b, a);

        SetColorConsideringShaderPropertyName(_renderer.material, ShaderColorPropertyNames, tmp_color);
    }

    private void SetColorConsideringShaderPropertyName(Material a_material, string[] a_propertiesToConsider, Color a_color)
    {
        if (a_propertiesToConsider.Length > 0)
        {
            foreach (string cPropname in a_propertiesToConsider)
            {
                a_material.SetColor(cPropname, a_color);
            }
        }
        else
        {
            a_material.color = a_color;
        }
    }
}
