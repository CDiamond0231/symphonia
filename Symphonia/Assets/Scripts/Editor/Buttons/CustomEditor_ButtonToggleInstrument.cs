//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Button - Toggle Instrument
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

[CustomEditor(typeof(Button_InstrumentToggle))]
public class CustomEditor_ButtonToggleInstrument : CustomEditor_ButtonBase<Button_InstrumentToggle>
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*+ Attr_Readers
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public Vector3 HiddenTitleCardPosition = new Vector3(-1189.0f, 37.0f, 0.0f);
    public Vector3 VisibleTitleCardPosition = new Vector3(-703.0f, 37.0f, 0.0f);
    public float TransitionDuration = 0.3f;

    public Rect HiddenTitleCardCollisionData = new Rect(0.0f, 0.0f, 11.28f, 7.74f);
    public Rect VisibleTitleCardCollisionData = new Rect(0.0f, 0.0f, 11.28f, 7.74f);


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Overwritten Method: Draw Inspector Options
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected override void DrawInspectorOptions()
    {
        DrawNonSpriteOptions(false);
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Overwritten Method: Draw Editable Values Options
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected override void DrawEditableValuesOptions()
    {
        Target.InstrumentsManager = DrawObjectOption("Instruments Manager:", Target.InstrumentsManager);
        Target.ChannelID = DrawIntField("Channel ID:", Target.ChannelID);

        if (GUI.changed 
            && Target.InstrumentsManager != null 
            && Target.InstrumentsManager.MusicRoadManager != null 
            && Target.ChannelID >= 0 
            && Target.ChannelID < MidiFileConstants.NumberOfChannels)
        {
            SpriteRenderer sprRenderer = Target.GetComponent<SpriteRenderer>();
            if (sprRenderer != null)
            {
                sprRenderer.color = Target.InstrumentsManager.MusicRoadManager.ChannelNotesColours[Target.ChannelID];
                EditorUtility.SetDirty(sprRenderer);
            }
        }
    }
}