//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Animation Effect Designer
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

[CustomEditor(typeof(AnimationEffectDesigner))]
public class CustomEditor_AnimationEffectDesigner : CustomEditor_Base<AnimationEffectDesigner>
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Readers
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override string ScriptDescription
	{
		get
		{
			return "This script is used as a manager for an array of Animation_Effects'.\n" +
					"Animations will occur consistently in a loop";
		}
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Inspector Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawInspectorOptions()
	{
		Target.m_rAdditionalSpriteRendererComponent = DrawNonImportantObjectOption("Target Sprite Renderer: ", Target.m_rAdditionalSpriteRendererComponent, "The Targetted Sprite Renderer which will have modifications done to it");
		Target.m_rAdditionalTextComponent			= DrawNonImportantObjectOption("Target Text Renderer: ", Target.m_rAdditionalTextComponent, "The Targetted Text Renderer which will have modifications done to it");
		Target.m_rAdditionalImageComponent			= DrawNonImportantObjectOption("Target Image Renderer: ", Target.m_rAdditionalImageComponent, "The Targetted Image Renderer which will have modifications done to it");

		if(Target.m_rAdditionalSpriteRendererComponent == null)
			Target.m_rAdditionalSpriteRendererComponent = Target.gameObject.GetComponent<SpriteRenderer>();
		if(Target.m_rAdditionalTextComponent == null)
			Target.m_rAdditionalTextComponent = Target.gameObject.GetComponent<UnityEngine.UI.Text>();
		if(Target.m_rAdditionalImageComponent == null)
			Target.m_rAdditionalImageComponent = Target.gameObject.GetComponent<UnityEngine.UI.Image>();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: Draw Animation Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected override void DrawAnimationOptions()
	{
		DrawAnimationEffectOptions(ref Target.m_aAnimationEffect, Target.transform);
	}
}
