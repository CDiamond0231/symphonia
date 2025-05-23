//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Button Manager
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
using UnityEditor;

[CustomEditor(typeof(ButtonManager))]
public class CustomEditor_ButtonManager : CustomEditor_Base<ButtonManager>
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Readers
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override string ScriptDescription
	{
		get
		{
			return "This script handles all of the input updates for all buttons in the game.\n" +
					"In this way, only one script is attempting to raycast input and map to\navailable buttons which will help to reduce input lag.\n\n" +
					"Additionally, as a manager, any script wishing to make changes to the\nbutton system must do so through this script/class.";
		}
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Debug Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawDebugOptions()
	{
		EditorGUILayout.LabelField(new GUIContent("Recent Touches:", "Shows you which Transform Components have been recently touched (Up to 100 Times)."));
		int i = 1;
		foreach(Transform t in ButtonManager.dm_lTriggeredObjects)
		{
			DrawNonImportantObjectOption("Recent Hit Area - " + (i < 100 ? "0" : "") + (i < 10 ? "0" : "") + i.ToString() + ":", t);
			++i;
		}

		AddSpaces(3);
		EditorGUILayout.LabelField(new GUIContent("Recent Calls to the ToggleButtons Method:", "Shows the StackTrack for everytime the \"ToggleAllButtons\" Method has been called (Up to 100 Times)"));
		i = 1;
		foreach(string trace in ButtonManager.dm_lToggleStackTrace)
		{
			EditorGUILayout.TextField("Call To ToggleAllButtons: " + (i < 100 ? "0" : "") + (i < 10 ? "0" : "") + i.ToString(), trace);
			++i; 
		}
	}
}
