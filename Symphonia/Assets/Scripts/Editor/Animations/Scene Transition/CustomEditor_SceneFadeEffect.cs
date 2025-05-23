//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Scene Fade Effect
//             Author: Christopher Allport
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    A custom editor is used to add additional functionality to the Unity 
//		inspector when dealing with the aforementioned class data.
//
//	  This includes the addition of adding in buttons or calling a method when a 
//		value is changed.
//	  Most importantly, a custom editor is used to make the inspector more 
//		readable and easier to edit.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SceneFadeEffect))]
public class CustomEditor_SceneFadeEffect : CustomEditor_Base<SceneFadeEffect> 
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Readers
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override string ScriptDescription
	{
		get
		{
			return "This script is used to fadein/fadeout a scene. It goes through all available\n" +
					"sprite renderers on initialisation then fades out/in all of those sprites\n" +
					"when the appropriate function is called.";
		}
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Inspector Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawInspectorOptions()
	{
		Target.m_asSoundPlayer = DrawNonImportantObjectOption("Audio Player (if any): ", Target.m_asSoundPlayer, "Audio Player to fade in/out when transitioning. Leave empty if none exist that require fading");
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Editable Values Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawEditableValuesOptions()
	{
		Target.m_fTransitionTime = EditorGUILayout.FloatField(new GUIContent("Fade Transition Time: ", "How long will it take to fadein/fadeout the scene (in seconds)"), Target.m_fTransitionTime);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Debug Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawDebugOptions()
	{
		Target.m_rConnectingScene = DrawImportantObjectOption("Connecting Scene: ", Target.m_rConnectingScene, "Scene which will be faded out/faded in in the opposite order whenever this scene does. i.e Other scene fades out whenever this scene fades in...");
		AddSpaces(2);
		if (GUILayout.Button(new GUIContent("Fade In", "Disables other (connecting) scene and shows this scene as though it was fully opaque")))
		{
			Target.gameObject.SetActive(true);
			Target.OnFadeIn();
			if (Target.m_rConnectingScene != null)
			{
				Target.m_rConnectingScene.gameObject.SetActive(false);
			}
		}

		if (GUILayout.Button(new GUIContent("Fade Out", "Disables this scene and shows the other (connecting) scene as though it was fully opaque")))
		{
			Target.OnFadeout();
			if (Target.m_rConnectingScene != null)
			{
				Target.m_rConnectingScene.gameObject.SetActive(true);
			}
		}
	}
}
