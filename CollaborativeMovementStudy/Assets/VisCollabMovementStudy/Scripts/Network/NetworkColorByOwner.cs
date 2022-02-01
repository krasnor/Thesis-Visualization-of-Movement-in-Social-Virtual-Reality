using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkColorByOwner : MonoBehaviour
{
    public PhotonView PhotonViewToObserve;
    public Renderer RendererToColor;
    public Color DefaultColor = Color.magenta;
    private string m_customPlayerColorPropertyName = NetworkedPlayerSettings.PropertyKeyPlayerColor;

    private Color m_lastSyncedColor;
    private int m_lastSyncedOwnerActorNumber = -1;
    public Color LastSyncedColor { get { return m_lastSyncedColor; } }


    // Start is called before the first frame update
    void Awake()
    {
        if (PhotonViewToObserve == null)
            throw new MissingComponentException("PhotonViewToObserve Component not assigned.");
    }

    private void Start()
    {
        ColorObject(DefaultColor);
        m_lastSyncedColor = DefaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            var owner = PhotonViewToObserve.Owner;
            if (owner == null)
            {
                ColorObject(DefaultColor);
            }
            else
            {
                // try to color by custom the player property
                if (owner.ActorNumber != m_lastSyncedOwnerActorNumber)
                {
                    if(NetworkedPlayerSettings.TryGetColorOfPlayer(owner, out var newColor))
                    {
                        ColorObject(newColor);
                    }
                    else
                    {
                        ColorObject(DefaultColor);
                    }

                    m_lastSyncedColor = newColor;
                    m_lastSyncedOwnerActorNumber = owner.ActorNumber;
                }

            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }

    //bool TryGetPlayerColor(Player a_player, out Color a_color, string a_customPlayerColorPropertyName)
    //{
    //    try
    //    {
    //        if (a_player.CustomProperties.ContainsKey(a_customPlayerColorPropertyName))
    //        {

    //            Color newColor;
    //            if (ColorUtility.TryParseHtmlString("#" + a_player.CustomProperties[a_customPlayerColorPropertyName].ToString(), out newColor))
    //            {
    //                a_color = newColor;
    //                return true;
    //            }
    //        }
    //        a_color = DefaultColor;
    //        return false;
    //    }
    //    catch (System.Exception)
    //    {
    //        a_color = DefaultColor;
    //        return false;
    //    }
    //}

    void ColorObject(Color a_color)
    {
        if (RendererToColor != null && RendererToColor.material != null)
        {
            RendererToColor.material.color = a_color;
        }
    }
}
