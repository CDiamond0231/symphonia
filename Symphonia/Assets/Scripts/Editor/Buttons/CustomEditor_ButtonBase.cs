//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Button Base
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


public class CustomEditor_ButtonBase<K> : CustomEditor_Base<K>
							   where K: Button_Base
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw All Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawAllOptions(bool includeSpaces = true)
	{
		DrawNonSpriteOptions(includeSpaces);


        // If no Sprite Renderer Exists, asks for one
        SpriteRenderer sprRend = Target.gameObject.GetComponent<SpriteRenderer>();

        if (sprRend == null)
		{
			Target.m_srButtonRenderer = DrawObjectOption("Assigned Sprite Renderer: ", Target.m_srButtonRenderer, "No Sprite Renderer has been found attached. Is there a sprite Renderer that this button should pretend it belongs to?");
		}


		// We can't ask for sprites to render if there is a SpriteRenderer to actually render them
		if (sprRend != null || Target.m_srButtonRenderer != null)
		{
			DrawUnpressedSpriteOption(includeSpaces: false);
			DrawPressedSpriteOption(includeSpaces: false);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Non-Sprite Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawNonSpriteOptions(bool includeSpaces = true)
	{
		DrawButtonNameOption();
		DrawButtonTypeOption(includeSpaces: includeSpaces);
		DrawOnClickAudioOption();
		if(includeSpaces)
			AddSpaces(3);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Button Name Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawButtonNameOption()
	{
		UnityEngine.UI.Text textComponent = Target.TextRenderer;
		if (textComponent != null)
        {
			string name = EditorGUILayout.TextField("Button Label: ", textComponent.text);
            if (name != textComponent.text)
            {
                textComponent.text = name;
            }

            Target.m_cTextColourWhenHoveredOver = EditorGUILayout.ColorField("Text Colour WHen Hovered Over: ", Target.m_cTextColourWhenHoveredOver);
        }
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Button Type Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawButtonTypeOption(string label = "Button Type: ", bool includeSpaces = true, string tooltip = "Type of Button to be associated with. Certain buttons will be exclusively available and unavailable at certain times to prevent overlapping")
	{
		Target.m_eButtonType = (ButtonManager.ButtonType)EditorGUILayout.EnumPopup(new GUIContent(label, tooltip), Target.m_eButtonType);
		if (includeSpaces)
		{
			AddSpaces(3);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Unpressed Sprite Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawUnpressedSpriteOption(string label = "Unpressed Sprite: ", bool includeSpaces = true, string tooltip = "Sprite to show when this button is NOT pressed\n\nIf null/empty: The button sprite will not change when unpressed!")
	{
		Target.m_sprUnPressedSprite = DrawObjectOption(label, Target.m_sprUnPressedSprite, tooltip);
		if (includeSpaces)
		{
			AddSpaces(3);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Pressed Sprite Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawPressedSpriteOption(string label = "Pressed Sprite: ", bool includeSpaces = true, string tooltip = "Sprite to show when this button has been pressed\n\nIf null/empty: The button sprite will not change when pressed!")
	{
		Target.m_sprPressedSprite = DrawObjectOption(label, Target.m_sprPressedSprite, tooltip);
		if (includeSpaces)
		{
			AddSpaces(3);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw On Click Audio Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawOnClickAudioOption(string label = "Audio Clip To Play When Clicked: ", string tooltip = "Audio File to Play When this Button is Clicked On")
	{
		Target.m_acClipToPlay = DrawAudioClipOption(label, Target.m_acClipToPlay, tooltip);
	}
}
