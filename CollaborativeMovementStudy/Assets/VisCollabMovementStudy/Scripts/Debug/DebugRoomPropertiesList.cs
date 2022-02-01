using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class DebugRoomPropertiesList : MonoBehaviourPunCallbacks
{
    public Text playerlistText;

    private void Awake()
    {
        if (playerlistText == null)
            throw new MissingComponentException("playerlistText Component was not set");
    }

    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        //Debug.Log($"Debug: {propertiesThatChanged.Count} Room Properties Changed");
        string text = "";
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        foreach (System.Collections.DictionaryEntry item in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            //Debug.Log($"k: {item.Key.ToString()} v: {item.Value.ToString()}");
            text += $"{item.Key.ToString()}    {item.Value.ToString()} \n";
        }

        playerlistText.text = text;
    }



}
