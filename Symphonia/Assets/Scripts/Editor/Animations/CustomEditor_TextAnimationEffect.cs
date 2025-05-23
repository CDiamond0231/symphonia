//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Text Animation Effect
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

[CustomEditor(typeof(TextAnimationEffect))]
public class CustomEditor_TextAnimationEffect : CustomEditor_Base<TextAnimationEffect>
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Readers
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override string ScriptDescription
	{
		get
		{
			return "This class animates the text on display. Default presets allow you to show\n" +
					"text one character/line at a time; fading in the next sequence as needed.";
		}
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Editable Values Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawInspectorOptions()
	{
		Target.m_rTextRenderer = DrawImportantObjectOption("Text Component Ref: ", Target.m_rTextRenderer, "Reference to the TextRenderer which will have it's text animated");
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Editable Values Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawEditableValuesOptions()
	{
		Target.m_eDesiredFadeinAnimationType = (TextAnimationEffect.FadeinAnimationType)EditorGUILayout.EnumPopup(new GUIContent("Desired Fade-in Animation Style:", "Which animation style will be used to fade-in?"), Target.m_eDesiredFadeinAnimationType);
		Target.m_eDesiredIdleAnimationType = (TextAnimationEffect.IdleAnimationType)EditorGUILayout.EnumPopup(new GUIContent("Desired Idle Animation Style:", "Which animation style will be used whilst idle"), Target.m_eDesiredIdleAnimationType);

		AddSpaces(2);

		if(Target.m_eDesiredIdleAnimationType != TextAnimationEffect.IdleAnimationType.NONE)
			Target.m_fHighlightEffectWaitTime = DrawFloatField("Highlight Effect Wait Time:", Target.m_fHighlightEffectWaitTime, "How long will the effect wait between each successful animation before redoing it?");

		if(Target.m_eDesiredIdleAnimationType != TextAnimationEffect.IdleAnimationType.NONE || Target.m_eDesiredFadeinAnimationType == TextAnimationEffect.FadeinAnimationType.SINGLE_LINE_CHARACTERS_FADEIN || Target.m_eDesiredFadeinAnimationType == TextAnimationEffect.FadeinAnimationType.ALL_LINES_CHARACTER_FADEIN)
			Target.m_fTextHighlightFadeTime = DrawFloatField("Highlight Effect Character Fade Time:", Target.m_fTextHighlightFadeTime, "How long will the effect take to fade-in each character in the displayed text?");
	}
}
