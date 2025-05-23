//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Button - Toggle Title Card Visibility
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

[CustomEditor(typeof(Button_ToggleTitleCardVisibility))]
public class CustomEditor_ButtonToggleTitleCardVisibility : CustomEditor_ButtonBase<Button_ToggleTitleCardVisibility>
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*+ Attr_Readers
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected override string ScriptDescription
    {
        get
        {
            return "This button simply plays a Sound Effect when pressed. It can also prevent\n" +
                    "itself from playing multiple times at once if desired.";
        }
    }


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
        Target.HiddenTitleCardPosition = EditorGUILayout.Vector3Field(new GUIContent("Hidden Title Card Position: "), Target.HiddenTitleCardPosition);
        Target.VisibleTitleCardPosition = EditorGUILayout.Vector3Field(new GUIContent("Visible Title Card Position: "), Target.VisibleTitleCardPosition);
        AddSpaces(2);
        Target.TransitionDuration = EditorGUILayout.FloatField(new GUIContent("Transition Duration: "), Target.TransitionDuration);
        AddSpaces(2);
        Target.HiddenTitleCardCollisionData = EditorGUILayout.RectField(new GUIContent("Hidden Title Card Collision Data: "), Target.HiddenTitleCardCollisionData);
        Target.VisibleTitleCardCollisionData = EditorGUILayout.RectField(new GUIContent("Visible Title Card Collision Data: "), Target.VisibleTitleCardCollisionData);
    }
}
