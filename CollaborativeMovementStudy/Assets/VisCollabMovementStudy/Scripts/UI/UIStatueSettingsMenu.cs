using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStatueSettingsMenu : MonoBehaviour
{

    public CollaborationStudyManager CollaborationStudyManager;
    public NetworkedGameSettings GameSettings;
    public bool StartStudyOnToggleDoor = true;
    [Space]
    public Text Label_CurrentBuildModelIndex;
    public Text Label_CurrentBuildModelVariantName;
    public Button Button_SwitchToVariantA;
    public Button Button_SwitchToVariantB;
    public Button Button_SwitchToVariantC;
    public Button Button_ToggleDoor;

    public Button Button_OverrideSpawnAdditionalBox;

    // Start is called before the first frame update
    void Awake()
    {
        if (CollaborationStudyManager == null)
            throw new MissingComponentException("CollaborationStudyManager Component was not set.");
        if (GameSettings == null)
            throw new MissingComponentException("GameSettings Component was not set.");

        if (Label_CurrentBuildModelIndex == null)
            throw new MissingComponentException("Label_CurrentStatueTypeIndex Component was not set.");
        if (Label_CurrentBuildModelVariantName == null)
            throw new MissingComponentException("Label_CurrentBuildModelVariantName Component was not set.");

        if (Button_SwitchToVariantA == null)
            throw new MissingComponentException("Button_SwitchToVariantA Component was not set.");
        if (Button_SwitchToVariantB == null)
            throw new MissingComponentException("Button_SwitchToVariantB Component was not set.");
        if (Button_SwitchToVariantC == null)
            throw new MissingComponentException("Button_SwitchToVariantC Component was not set.");

        if (Button_ToggleDoor == null)
            throw new MissingComponentException("Button_ToggleDoor Component was not set.");

        if (Button_OverrideSpawnAdditionalBox == null)
            Debug.LogWarning("Button_OverrideSpawnAdditionalBox Component is not set.");
    }

    void Start()
    {
        Button_SwitchToVariantA.onClick.AddListener(SwitchToVariantA);
        Button_SwitchToVariantB.onClick.AddListener(SwitchToVariantB);
        Button_SwitchToVariantC.onClick.AddListener(SwitchToVariantC);
        Button_ToggleDoor.onClick.AddListener(ToggleOpenDoor);

        Button_OverrideSpawnAdditionalBox?.onClick.AddListener(OverrideAdvanceProgress);

        UpdateLabels(true);
    }

    private int lastPrintedBuildModelId = -1;

    // Update is called once per frame
    void Update()
    {
        UpdateLabels();
    }

    private void UpdateLabels(bool a_ignoreAlreadyPrintedCheck = false)
    {
        int currentbuildModelId = CollaborationStudyManager.ConstructionSite.CurrentBuildModelId;

        // keep updates to a minimum
        if (lastPrintedBuildModelId != currentbuildModelId || a_ignoreAlreadyPrintedCheck)
        {
            CollaborationStudyManager.Variant parsedVariant = CollaborationStudyManager.CurrentBuildModelIdToVariant();
            Label_CurrentBuildModelIndex.text = currentbuildModelId + ")";
            Label_CurrentBuildModelVariantName.text = parsedVariant.ToString();

            lastPrintedBuildModelId = currentbuildModelId;
        }
    }

    private void SwitchToVariantA()
    {
        CollaborationStudyManager.RequestOwnershipAndResetStudyToVariant(CollaborationStudyManager.Variant.VariantA);
    }

    private void SwitchToVariantB()
    {
        CollaborationStudyManager.RequestOwnershipAndResetStudyToVariant(CollaborationStudyManager.Variant.VariantB);
    }

    private void SwitchToVariantC()
    {
        CollaborationStudyManager.RequestOwnershipAndResetStudyToVariant(CollaborationStudyManager.Variant.VariantC);
    }

    private void ToggleOpenDoor()
    {
        if (StartStudyOnToggleDoor)
        {
            CollaborationStudyManager.BeginStudy();
        }
        GameSettings.SetDoorWorldOpen(!GameSettings.OpenStateDoorOutsideWorld);
    }

    private void OverrideAdvanceProgress()
    {
        CollaborationStudyManager.OverrideAdvanceProgress();
    }
}
