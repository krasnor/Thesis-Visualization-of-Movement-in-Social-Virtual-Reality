using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class UIRoomSettingsMenu : MonoBehaviour
{

    public Text Label_CurrentMode;
    public NetworkedGameSettings GameSettings;

    //public Dropdown Dropdown_TeleportMode;
    public Button Button_TeleportModeInstant;
    public Button Button_TeleportModeTrace;
    public Button Button_TeleportModeContinuous;



    private void Awake()
    {
        if (Label_CurrentMode == null)
            throw new MissingComponentException("Label_CurrentMode Component was not set");
        if (GameSettings == null)
            throw new MissingComponentException("GameSettings Component was not set");
        if (Button_TeleportModeInstant == null)
            throw new MissingComponentException("Button_TeleportModeInstant Component was not set");
        if (Button_TeleportModeTrace == null)
            throw new MissingComponentException("Button_TeleportModeTrace Component was not set");
        if (Button_TeleportModeContinuous == null)
            throw new MissingComponentException("Button_TeleportModeContinuous Component was not set");
    }

    // Start is called before the first frame update
    void Start()
    {
        //Dropdown_TeleportMode.ClearOptions();
        //List<OptionData> dropdownOptions = new List<OptionData>();
        //TracedTeleportationProviderMode currentOption = GameSettings.TeleportationMode;

        //int initialValue = 0;
        //TracedTeleportationProviderMode[] enumVals = (TracedTeleportationProviderMode[])Enum.GetValues(typeof(TracedTeleportationProviderMode));
        //for (int i = 0; i < enumVals.Length; i++)
        //{
        //    if (enumVals[i] == currentOption)
        //        initialValue = i;

        //    dropdownOptions.Add(new OptionData(enumVals[i].ToString()));
        //}
        //Dropdown_TeleportMode.AddOptions(dropdownOptions);
        //Dropdown_TeleportMode.SetValueWithoutNotify(initialValue);

        //Dropdown_TeleportMode.onValueChanged.AddListener(OnTPModeChanged);
        Button_TeleportModeInstant.onClick.AddListener(SetToInstantTP);
        Button_TeleportModeTrace.onClick.AddListener(SetToTracedTP);
        Button_TeleportModeContinuous.onClick.AddListener(SetToContinuousTP);

    }
    public void SetToInstantTP()
    {
        GameSettings.SetRoomTeleportationMode(TracedTeleportationProviderMode.Instant);
    }

    public void SetToTracedTP()
    {
        GameSettings.SetRoomTeleportationMode(TracedTeleportationProviderMode.Trace_Line);

    }
    public void SetToContinuousTP()
    {
        GameSettings.SetRoomTeleportationMode(TracedTeleportationProviderMode.Continuous);
    }

    //private void OnTPModeChanged(int i)
    //{
    //    TracedTeleportationProviderMode newTPMode = (TracedTeleportationProviderMode)Enum.Parse(typeof(TracedTeleportationProviderMode), Dropdown_TeleportMode.options[i].text);

    //    Debug.Log("OnTPModeChanged: entry " + i + " parsed: " + newTPMode);
    //    GameSettings.SetRoomTeleportationMode(newTPMode);
    //}

    // Update is called once per frame
    void Update()
    {
        Label_CurrentMode.text = GameSettings.TeleportationMode.ToString();
    }
}
