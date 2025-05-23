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

[CustomEditor(typeof(Button_BrowseForFile))]
public class CustomEditor_ButtonBrowseForFile : CustomEditor_ButtonBase<Button_BrowseForFile>
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*+ Attr_Readers
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


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
        Target.MusicRoadManager = DrawObjectOption("Music Road Manager:", Target.MusicRoadManager);
        Target.BrowsingForFileType = (Button_BrowseForFile.FileType)EditorGUILayout.EnumPopup("File Type To Browse For: ", Target.BrowsingForFileType);

        if (Target.BrowsingForFileType == Button_BrowseForFile.FileType.Midi)
        {
            Target.SelectedMidiFileTextRenderer = DrawObjectOption("Selected Midi File Text Renderer:", Target.SelectedMidiFileTextRenderer);
        }

        Target.SelectedOverlayTrackFileTextRenderer = DrawObjectOption("Selected Overlay File Text Renderer:", Target.SelectedOverlayTrackFileTextRenderer);
    }
}
