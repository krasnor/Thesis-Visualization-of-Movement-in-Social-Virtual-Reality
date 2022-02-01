using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFollowTaskSettings : MonoBehaviour
{
    public Color NoPlayerAssingedColor = Color.magenta;
    public Dropdown DropdownRoute;

    [Space]
    public AvatarFollowStudyManager Room1_Manager;
    public Text Room1_Actor;
    public Text Room1_Route;
    public Text Room1_Stage;
    public Image Room1_ActorColor;

    [Space]
    public AvatarFollowStudyManager Room2_Manager;
    public Text Room2_Actor;
    public Text Room2_Route;
    public Text Room2_Stage;
    public Image Room2_ActorColor;

    [Space]
    public NetworkedGameSettings GameSettings;
    public Button ButtonOpenRooms;
    public Button ButtonAssingRoom1;
    public Button ButtonAssingRoom2;
    public InputField InputActorNumberRoom1;
    public InputField InputActorNumberRoom2;

    private void UpdateRoomInfo(Text a_labelActorId, Text a_labelRoute, Text a_labelStage, Image a_actorColor, Photon.Realtime.Player a_roomOwner, AvatarFollowStudyManager a_mngr)
    {
        a_labelRoute.text = "" + a_mngr.CurrentRouteIndex;
        a_labelStage.text = "" + a_mngr.CurrentStageIndex;
        if (a_roomOwner == null || a_roomOwner.ActorNumber == -1)
        {
            a_labelActorId.text = "-1";
            a_actorColor.color = NoPlayerAssingedColor;
        }
        else
        {
            if (a_roomOwner.ActorNumber.ToString() != a_labelActorId.text)
            {
                // prevent update every frame
                a_labelActorId.text = "" + a_roomOwner.ActorNumber;

                if (NetworkedPlayerSettings.TryGetColorOfPlayer(a_roomOwner, out var color))
                {
                    a_actorColor.color = color;
                }
            }
        }
    }



    private void Awake()
    {
        if (DropdownRoute == null)
            throw new MissingComponentException("DropdownRoute Component was not set");

        if (Room1_Manager == null)
            throw new MissingComponentException("Room1_Manager Component was not set");
        if (Room1_Actor == null)
            throw new MissingComponentException("Room1_Actor Component was not set");
        if (Room1_Route == null)
            throw new MissingComponentException("Room1_Route Component was not set");
        if (Room1_Stage == null)
            throw new MissingComponentException("Room1_Stage Component was not set");
        if (Room1_ActorColor == null)
            throw new MissingComponentException("Room1_ActorColor Component was not set");


        if (Room2_Manager == null)
            throw new MissingComponentException("Room2_Manager Component was not set");
        if (Room2_Actor == null)
            throw new MissingComponentException("Room2_Actor Component was not set");
        if (Room2_Route == null)
            throw new MissingComponentException("Room2_Route Component was not set");
        if (Room2_Stage == null)
            throw new MissingComponentException("Room2_Stage Component was not set");
        if (Room2_ActorColor == null)
            throw new MissingComponentException("Room2_ActorColor Component was not set");

        if (GameSettings == null)
            throw new MissingComponentException("GameSettings Component was not set");
        if (ButtonOpenRooms == null)
            throw new MissingComponentException("ButtonOpenRooms Component was not set");
        if (ButtonAssingRoom1 == null)
            throw new MissingComponentException("ButtonAssingRoom1 Component was not set");
        if (ButtonAssingRoom2 == null)
            throw new MissingComponentException("ButtonAssingRoom2 Component was not set");
        if (InputActorNumberRoom1 == null)
            throw new MissingComponentException("InputActorNumberRoom1 Component was not set");
        if (InputActorNumberRoom2 == null)
            throw new MissingComponentException("InputActorNumberRoom2 Component was not set");
    }
    // Start is called before the first frame update
    void Start()
    {
        ButtonOpenRooms.onClick.AddListener(OnToggleDoorsToRooms);
        ButtonAssingRoom1.onClick.AddListener(OnAssignRoom1Pressed);
        ButtonAssingRoom2.onClick.AddListener(OnAssignRoom2Pressed);
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled)
        {
            if (Room1_Manager != null)
            {
                UpdateRoomInfo(Room1_Actor, Room1_Route, Room1_Stage, Room1_ActorColor, Room1_Manager.photonView.Owner, Room1_Manager);
            }
            if (Room2_Manager != null)
            {
                UpdateRoomInfo(Room2_Actor, Room2_Route, Room2_Stage, Room2_ActorColor, Room2_Manager.photonView.Owner, Room2_Manager);
            }
        }
    }

    private void OnAssignRoom1Pressed()
    {
        int route = DropDownToRouteIndex();
        if (int.TryParse(InputActorNumberRoom1.text, out int actorNumber))
        {
            AssignPlayerToRoom(actorNumber, route, true);
        }
    }
    private void OnAssignRoom2Pressed()
    {
        int route = DropDownToRouteIndex();
        if (int.TryParse(InputActorNumberRoom2.text, out int actorNumber))
        {
            AssignPlayerToRoom(actorNumber, route, false);
        }
    }

    /// <summary>
    /// Parses Route Index. 
    /// Returns 0 on any conversion error. 
    /// </summary>
    /// <returns></returns>
    private int DropDownToRouteIndex()
    {
        //The Value is the index number of the current selection in the Dropdown. 0 is the first option in the Dropdown, 1 is the second, and so on.
        switch (DropdownRoute.value)
        {
            case 0:
                return 0;
            case 1:
                return 1;
            case 2:
                return 2;
            default:
                return 0;
        }
    }

    private void AssignPlayerToRoom(int a_ActorNumber, int a_routeIndex, bool Room1)
    {
        Debug.Log($"AssignPlayerToRoom. ActorNumber: {a_ActorNumber} RouteIndex: {a_routeIndex} Room1: {Room1}");

        AvatarFollowStudyManager manager = Room1 ? Room1_Manager : Room2_Manager;
        if (manager == null)
        {
            return;
        }
        manager.AssignPlayerStudyManagerAndReset(a_ActorNumber, a_routeIndex);
    }

    private void OnToggleDoorsToRooms()
    {
        GameSettings.SetDoorRoomsOpen(!GameSettings.OpenStateDoorRooms);
    }

}
