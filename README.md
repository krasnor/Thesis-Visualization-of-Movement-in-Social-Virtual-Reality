# Overview

This project is an updated version of the Collaborative Movement Study, which I conducted as part of my Master's thesis "Visualization of Movement in Social Virtual Reality".

![impl_move_trace](https://user-images.githubusercontent.com/10096384/151816110-864e49c5-14c9-4366-9481-d9317309c2d0.png)


This project implements three movement methods:
 - Teleport (instant movement to target location)
 - Teleport with a trace (additionally a fading trace is left behind)
 - Continuous Movement (user is moved at constant speed to target location)

For evaluation two tasks were implemented. In Task 1 (Follow Task) the idea is to avoid as much influences as possible (e.g. social interactions). In this task a participant is guided by a scripted avatar in virtual a museum to ten different paintings. At each painting the participant should stand in a circle on the ground and look at the painting until it changes its colour (so every user has roughly the same position and orientation). After the interaction the scripted avatar guides the participant to the next painting. This repeats until the participant was guided to ten different paintings.

In Task 2 (Collaboration Task), the goal is to test the movement methods in a collaborative scenario. Two participants should move together through an environment, find and interact with excavation sites to obtain boxes and bring those boxes back to a construction site to construct a statue.

In the final study the movement techniques were evaluated using only the Follow Task. 
Questionnaires were filled out outside of VR.

In addition to my own logging solution, the *LogAndAnalysisTool* (see https://github.com/torantie/Master-Thesis-Visualization-of-Human-Behaviour-in-VR) was integrated in the hope that additional data could be useful for later work and more in-depth evaluations.
Since the evaluation of the data gathered by the *LogAndAnalysisTool* did go beyond the scope of the thesis, none of this additional data was evaluated.

The logging data is logged to `Application.persistentDataPath` (see https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html).


# General Requirements 
Unity version: 2021.1.10f1

Unity XR compatible Hardware.

# Setup
To setup this project you will need to open this project in Safe-Mode (should happen automatically) and import all the assets and dependencies (see section `Project Dependencies`).
To avoid a naming conflict with the `PlayerSettings`-class do not import the demos of the Photon Voice 2 Asset or alternatively delete the conflicting Photon samples after import. 
For some project dependencies, such as the XR Interaction Toolkit, it may be necessary to enable pre-release packages in the settings.
As OpenXR is still under development, it is likely that there will be breaking changes in future revisions of XR-related assets.
In `/VisCollabMovementStudy/Scenes/` you will find the study scene.
You will need to *setup Photon* to be able to change all settings (some settings are only applied while connected to a server/room).

If you are only interested in testing out the movement methods, then you only need to import the folders `TracedTeleportation/` and `XR/`.
In `/TracedTeleportation/Scenes/` you will find a sample scene, that implements the movement methods.

# Project Structure Overview
This diagram provides an overview of all the assets, what they are used for and along with additional comments.

```
/CollaborativeMovementStudy/
 |- Packages				
 |- ProjectSettings			
 |- Assets				(code and models)
	|- AdjustedPrefabs/		contains adjustments (e.g. additional colliders etc.) of third party assets
	|- AlignedGames/		third party 3D Assets
	|- CSVSerializer/		* needed only for LogAndAnalysisTool
	|- EmaceArt/			third party 3D Assets
	|- Heatmap/			* needed only for LogAndAnalysisTool
	|- LogAndAnalysisTool/		* core implementation of LogAndAnalysisTool
	|- Oculus/			SampleFramework by Oculus. Only CustomHands was imported.
	|- Photon/			Photon Voice 2 (includes Photon Unity Networking)
	|- Samples/			sample assets by unity XR Interaction Toolkit
	|- SavWav/			* needed only for LogAndAnalysisTool
	|- TextMesh Pro/		* needed only for LogAndAnalysisTool
	|- TracedTeleportation/		implementation of the movement methods (Teleport, Trace and Continuous), if you want to incorporate the movement techniques in your study, you will find everything necessary in this folder.
	|- VisCollabMovementStudy/	contains all code and assets for the study
	|- XR/				Unity OpenXR
```
# Project Dependencies

Unity Editor version: 2021.1.10f1

## Assets needed to be imported

| Asset Folder | Name | Version | Comment | Unity Asset Store link |
| :----------- | :---: | ------- | ------- | -------------- |
| Aligned Games | Polygonal Foliage Asset Package | v1.1 | assets used in Task 2 | https://assetstore.unity.com/packages/3d/environments/polygonal-foliage-asset-package-133037 |
| CSVSerializer | CSV Serialize | v1.0 | used by LogAndAnalysisTool | https://assetstore.unity.com/packages/tools/integration/csv-serialize-135763 |
| EmaceArt      | Free Fantasy Medieval Houses and Props Pack | v1.3 | assets used in Task 2 | https://assetstore.unity.com/packages/3d/environments/fantasy/free-fantasy-medieval-houses-and-props-pack-167010 |
| Oculus        | Oculus Integration | v31.0 | only Hand models needed `SampleFramework/Core/CustomHands/` | https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022 |
| Photon        | Photon Voice 2 | v2.26.3 | do not import PhotonVoice demos (causes class naming conflict) | https://assetstore.unity.com/packages/tools/audio/photon-voice-2-130518 |

## Assets already integrated or loaded with project
This projects VR implementation is based on a beta version of the OpenXR Plugin and XR Interaction Toolkit. 
Future revisions of these assets may introduce breaking changes for this project.
It may be necessary to enable pre-release packages in the settings to import the XR Interaction Toolkit.

| Folder | Version | Comment | |
| ------ | ------- | ------- | --- |
| Heatmap       |   | used by LogAndAnalysisTool | based on https://unitycodemonkey.com/video.php?v=mZzZXfySeFQ |
| LogAndAnalysisTool  | v1.0 | core of implementation of LogAndAnalysisTool | https://github.com/torantie/Master-Thesis-Visualization-of-Human-Behaviour-in-VR |
| Samples       | XR Interaction Toolkit  1.0.0-pre.5 | XR Input and Eventhandling | https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@1.0/manual/index.html |
| SavWav        |   | used by LogAndAnalysisTool | based on https://gist.github.com/darktable/2317063#file-savwav-cs |
| TextMesh Pro  | v3.0.6 | used by LogAndAnalysisTool | https://docs.unity3d.com/Manual/com.unity.textmeshpro.html |
| XR            | OpenXRPlugin v1.2.8 | OpenXR functionality | https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.2/manual/index.html |
|             | XR Plugin Management v4.0.7 | OpenXR functionality | https://docs.unity3d.com/Packages/com.unity.xr.management@4.0/license/LICENSE.html |



# FAQ
 - 	Q: Is HMD xyz compatible?
	
    A: We tested the following HMDs: HTC Vive (Pro), Valve Index and Oculus Quest 2.
	   Generally the implementation is based on the OpenXR standard,
	   thus this application should be compatible with all HMDs that are compatible to this standard.
	   For some newer or upcoming HMDs it may be required to import controller profiles.
	   As the Unity OpenXR implementation is still ongoing, future patches may introduce breaking changes and incompatiblites.

 - 	Q: Where can I find the implementation of the study?

    A: `/Assets/VisCollabMovementStudy/`
	
 - 	Q: Where is the study scene?: 

	A: `/Assets/VisCollabMovementStudy/Scenes/CollabMovementStudyScene.unity`
 
 - 	Q: Where can I find the implementation of the three movement methods?

    A: `/Assets/TracedTeleportation/`
	  
 - 	Q: Lag: When running from Editor there is some lag (e.g. avatar or other things are lagging)

	A: We found multiple common problems:

		1. Are in objects selected in Editor or visible in Inspector?
		   Often when an frequently updated object is seleceted (e.g. XR_Rig, Avatar-Hand, etc.) in the editor, 
           the UI updates (e.g. Tranfsorm) slow the rendering down. 
           Try deselecting the object.
		2. Is the HMDs tracking-system calibrated?
		3. Bad/Slow internet connection
		4. Graphics card can't keep up

 - 	Q: Oculus Quest - Audio: I cannot hear other VR users and/or they cannot hear me.
  
	A: This might be a permission problem

		- Is microphone or audio muted?
		- have a audio recording permissions been granted?
		- on some devices the permissions dialog might not show during VR - try restarting the application
	
 - 	Q: PC - Audio: I cannot hear other VR users and/or they cannot hear me.
  
	A: The application uses per default the "default Audio Speaker/Microphone":

		- Is microphone or audio muted?
		- Is desired speaker/microphone set up as "default Audio Speaker/Microphone"
		- Don't change speaker while application is running, changes might not be always detected reliable.
		- Restart the application
		
 - 	Q: Connection - I cannot connect to the study room / cannot connect with other users / settings are not applied.
 
	A: Networking is based on the Photon (Voice) 2 plugin.
		There could be multiple problems:
        
		- Photon has not yet been configured
		- Bad/Slow/No internet connection
		- Photon servers are down
		If this is run in a few years:
		- Photon Plugin may no longer supported -> update Photon Plugin
