//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Custom Editor: Base Class
//             Author: Christopher Allport
//             Date: November 11th, 2014 
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//		This Script is a base class for all New Custom Editors. It adds some 
//			additional functionality that should be readily available in all
//			Custom Editor scripts.
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
using System.Reflection;
using UnityEditor;

public class CustomEditor_Base<K> : Editor			// K for Klass
						 where K: MonoBehaviour
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* CONST VALUES
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    const string PRIOR_APP_NAME = "Instinct";
    const string CURRENT_APP_NAME = "Symphonia";

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*. Protected Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected static string sm_sOpenPathDirectory = @"C:\";
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*- Private Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private bool m_bShowDescription = false;

	private static string m_sArabicText;
	private static string m_sArabicRTLText;
	private static string m_sArabicFixText;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Readers
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected K							Target						 { get { return target as K; } }
	protected Transform					transform					 { get { return Target.transform; } }
	protected virtual string			ScriptDescription			 { get { return ""; } }
	protected virtual bool				HasInspectorOptions			 { get { return false; } }
	protected virtual bool				HasEditableOptions			 { get { return false; } }
	protected virtual bool				HasAnimationOptions			 { get { return false; } }
	protected virtual bool				HasAnimatorOptions			 { get { return false; } }
	protected virtual bool				HasMultiLanguageTextOptions	 { get { return false; } }
	protected virtual bool				HasDebugOptions				 { get { return false; } }
	protected virtual InspectorRegion[] AdditionalRegions			 { get { return null; } }
	protected Rect						RectPos						 { get { return new Rect(232, GUILayoutUtility.GetLastRect().y, GUILayoutUtility.GetLastRect().width - 232, GUILayoutUtility.GetLastRect().height); } }

	protected virtual Color				MainFontColour				 { get { return new Color32(61, 84, 47, 255); } }
	protected virtual Color				SecondaryFontColour			 { get { return new Color32(137, 107, 47, 255); } }

	protected Color						NormalFontColour			 { get { return new Color(0.000f, 0.000f, 0.000f, 1.000f); } }	// Everything is fine			=> BLACK COLOUR
	protected Color				ImportantObjectMissingFontColour	 { get { return new Color(1.000f, 0.000f, 0.000f, 0.750f); } }	// Missing Object is Required	=> RED COLOUR
	protected Color						ObjectMissingFontColour		 { get { return new Color(0.627f, 0.121f, 0.729f, 0.750f); } }	// Missing Object				=> PURPLE COLOUR
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*{} Class Declarations
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected delegate void CallbackMethod();

	protected struct InspectorRegion
	{
		public string label;
		public CallbackMethod representingDrawMethod;
	}

	protected enum BooleanState
	{
		TRUE,
		FALSE
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Overwritten Method: On Inspector GUI
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public override void OnInspectorGUI()
	{
		// Get it away from the top of the inspector so it's easier to read!
		AddSpaces(2);

		// Make sure we are using up-to-date assets!
		CheckForOldAppSprite();

		// Draw Script Description, if it exists
		DrawScriptDescription();

		// Draw Predefined Regions
		if (DoesInspectorRegionExist(nameof(DrawInspectorOptions)))
        {
            DrawNewInspectorRegion("~Inspector Options~ (Applying References To The Script)", DrawInspectorOptions);
        }
		if (DoesInspectorRegionExist(nameof(DrawEditableValuesOptions)))
        {
            DrawNewInspectorRegion("~Editable Script Values~ (Editable Values In The Script)",DrawEditableValuesOptions);
        }

		// Draw Additional Regions... If any... (probably won't happen often!)
		InspectorRegion[] additionalRegions = AdditionalRegions;
		if (additionalRegions != null)
			for (int i = 0; i < additionalRegions.Length; ++i)
				DrawNewInspectorRegion(additionalRegions[i]);

		// Draw Remaining Predefined Regions.
		if (DoesInspectorRegionExist(nameof(DrawAudioHandlerInfoOptions)))
        {
            DrawNewInspectorRegion("~Audio Options~ (Edit Audio Information)", DrawAudioHandlerInfoOptions);
        }
		if (DoesInspectorRegionExist(nameof(DrawAnimationOptions)))
        {
            DrawNewInspectorRegion("~Animation Options~ (Shows Simple Animation Options)", DrawAnimationOptions);
        }
        if (DoesInspectorRegionExist(nameof(DrawAnimatorOptions)))
        {
            DrawNewInspectorRegion("~Animator Options~ (To Be Used With Unity's 2D Animation System)", DrawAnimatorOptions);
        }
		if (DoesInspectorRegionExist(nameof(DrawVignetteInfoOptions)))
        {
            DrawNewInspectorRegion("~Vignette Text Options~ (Apply a Background Vignette)", DrawVignetteInfoOptions);
        }
		if (DoesInspectorRegionExist(nameof(DrawMultiLanguageTextOptions)))
        {
            DrawNewInspectorRegion("~Multi-Language Text Options~ (Applying Values to Account for Other Languages)", DrawMultiLanguageTextOptions);
        }
		if (DoesInspectorRegionExist(nameof(DrawDebugOptions)))
        {
            DrawNewInspectorRegion("~Debug Options~ (To Help With Testing)", DrawDebugOptions);
        }

		// Reserialise Script Instance if things have been changed.
		if (GUI.changed)
		{
			EditorUtility.SetDirty(Target);
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
	}

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Does Inspector Region Exist?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected bool DoesInspectorRegionExist(string methodName)
	{
		MethodInfo methodInfo = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		return (methodInfo != null ? methodInfo.DeclaringType != typeof(CustomEditor_Base<K>) : false);
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Draw New Inspector Region
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawNewInspectorRegion(string label, CallbackMethod InspectorDrawMethod)
	{
		EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
		EditorGUI.indentLevel += 1;
		{
			InspectorDrawMethod();
			AddSpaces(3);
		}
		EditorGUI.indentLevel -= 1;
	}

	protected void DrawNewInspectorRegion(InspectorRegion newInspectorRegion)
	{
		DrawNewInspectorRegion(newInspectorRegion.label, newInspectorRegion.representingDrawMethod);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Script Description
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void DrawScriptDescription()
	{
		if (ScriptDescription != "")
		{
			if (DrawFoldoutOption("Description", ref m_bShowDescription, "Reveal description of this script"))
			{
				DrawSplitter();
				string[] textlines = ScriptDescription.Split(new char[] { '\n', });
				GUIStyle s = new GUIStyle();
				s.alignment = TextAnchor.UpperCenter;
				s.normal.textColor = new Color32(36, 68, 196, 255);
				foreach(string line in textlines)
				{
					AddSpaces(2);
					Rect pos = GetScaledRect();
					EditorGUI.LabelField(pos, line, s);
				}
				AddSpaces(2);
				DrawSplitter();
			}
			else
			{
				AddSpaces(1);
			}
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Inpector Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawInspectorOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Editable Values Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawEditableValuesOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Animation Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawAnimationOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Animator Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawAnimatorOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Audio Handler Info Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawAudioHandlerInfoOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Vignette Info Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawVignetteInfoOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Multi-Language Text Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawMultiLanguageTextOptions()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Debug Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void DrawDebugOptions()
	{
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Add Spaces
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void AddSpaces(int count = 3)
	{
		for (int i = 0; i < count; ++i)
		{
			EditorGUILayout.Space();
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Is a Secondary Component?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	/// <summary>
	/// Determines whether or not this component is a secondary component.
	/// Basically if this component is listed with an ID of 2, 4, 6, 8, 10, etc. it 
	/// will be considered a secondary component. It doesn't mean anything; it's just
	/// a way of identifying whether or not the colours of the font/labels in this component
	/// should change colours
	/// </summary>
	/// <returns>True if secondary component</returns>
	protected bool IsSecondaryComponent()
	{
		Component[] components = Target.gameObject.GetComponents<Component>();
		for (int i = 0; i < components.Length; ++i)
		{
			if (components[i] == Target)
				return (i % 2 != 0);
		}
		return false;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Check For Old App Sprite?!
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void CheckForOldAppSprite()
	{
		SpriteRenderer sprRend = Target.GetComponent<SpriteRenderer>();
		if(sprRend != null && CheckForOldAppSprite(sprRend.sprite))
		{
			GUIStyle s = new GUIStyle(EditorStyles.largeLabel);
			s.normal.textColor = new Color32(36, 124, 134, 255);
			EditorGUILayout.LabelField("This image appears to be from an older app!", s);
		}
	}

	protected bool CheckForOldAppSprite(Sprite input)
	{
		return (input != null ? input.texture.name.Contains(PRIOR_APP_NAME) : false);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Get Scaled Rect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected static Rect GetScaledRect()
	{
		float y = GUILayoutUtility.GetLastRect().y;
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		Rect scale = GUILayoutUtility.GetLastRect();
		scale.y = y;
		scale.height = 15;
		return scale;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Foldout Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected bool DrawFoldoutOption(string label, ref bool showFoldout, string tooltip = "")
	{
		showFoldout = EditorGUILayout.Foldout(showFoldout, (tooltip != "" ? new GUIContent(label, tooltip) : new GUIContent(label)), EditorStyles.foldout);
		return showFoldout;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Splitter
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawSplitter()
	{
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Int Field
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected int DrawIntField(string name, int currentValue, string tooltip = "")
	{
		GUIContent Label = (tooltip != "" ? new GUIContent(name, tooltip) : new GUIContent(name));
		return EditorGUILayout.IntField(Label, currentValue);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Float Field
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected float DrawFloatField(string name, float currentValue, string tooltip = "")
	{
		GUIContent Label = (tooltip != "" ? new GUIContent(name, tooltip) : new GUIContent(name));
		return EditorGUILayout.FloatField(Label, currentValue);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Toggle Field
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected bool DrawToggleField(string name, bool currentValue, string tooltip = "", bool useEnumPopup = true)
	{
		GUIContent Label = (tooltip != "" ? new GUIContent(name, tooltip) : new GUIContent(name));
		if (useEnumPopup)
			return ((BooleanState)EditorGUILayout.EnumPopup(Label, (currentValue ? BooleanState.TRUE : BooleanState.FALSE)) == BooleanState.TRUE);
		else
			return EditorGUILayout.Toggle(Label, currentValue);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Draw Object Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected T DrawObjectOption<T>(string name, T obj, string tooltip = "", bool showMissingObjectTextColourIfNull = true)  where T: UnityEngine.Object
	{
		if(showMissingObjectTextColourIfNull)
			return DrawImportantObjectOption(name, obj, tooltip);
		else
			return DrawNonImportantObjectOption(name, obj, tooltip);
    }

	protected T DrawNonImportantObjectOption<T>(string name, T obj, string tooltipWhenAssignedAnObject = "", string tooltipWhenNoObjectIsAssigned = "") where T : UnityEngine.Object
	{
		if(obj != null)
		{
			GUIContent label = (tooltipWhenAssignedAnObject != "" ? new GUIContent(name, tooltipWhenAssignedAnObject) : new GUIContent(name));
			return DrawObjectOption(label, obj, NormalFontColour, ObjectMissingFontColour);
		}
		else
		{
			GUIContent label = (tooltipWhenNoObjectIsAssigned != "" ? new GUIContent(name, tooltipWhenNoObjectIsAssigned) : (tooltipWhenAssignedAnObject != "" ? new GUIContent(name, tooltipWhenAssignedAnObject + "\n\nAre you sure you wish to leave this unassigned?") : new GUIContent(name)));
			return DrawObjectOption(label, obj, NormalFontColour, ObjectMissingFontColour);
		}
	}

	protected T DrawImportantObjectOption<T>(string name, T obj, string tooltipWhenAssignedAnObject = "", string tooltipWhenNoObjectIsAssigned = "") where T : UnityEngine.Object
	{
		if(obj != null)
		{
			GUIContent label = (tooltipWhenAssignedAnObject != "" ? new GUIContent(name, tooltipWhenAssignedAnObject) : new GUIContent(name));
			return DrawObjectOption(label, obj, NormalFontColour, ImportantObjectMissingFontColour);
		}
		else
		{
			GUIContent label = (tooltipWhenNoObjectIsAssigned != "" ? new GUIContent(name, tooltipWhenNoObjectIsAssigned) : (tooltipWhenAssignedAnObject != "" ? new GUIContent(name, tooltipWhenAssignedAnObject + "\n\nThis object is required. You must assign something here!") : new GUIContent(name)));
			return DrawObjectOption(label, obj, NormalFontColour, ImportantObjectMissingFontColour);
		}
	}

	protected T DrawObjectOption<T>(GUIContent display, T obj, Color normalColour, Color missingObjectColour) where T : UnityEngine.Object
	{
		GUIStyle s = new GUIStyle();
		s.normal.textColor = (obj == null ? missingObjectColour : normalColour);

		// Can't do Template Specialization in C#. So have to do this hacky copy-paste job instead. If it's a Sprite show all text on the same line as Sprite
		//  Since Unity now shows the sprites in the inspector rather than the object box (llike it used to).
		if(typeof(T) == typeof(Sprite))
		{
			Sprite input = (obj as Sprite);
			T val = (T)EditorGUILayout.ObjectField(" ", obj, typeof(T), true);
			Rect drawPosition = new Rect(15 * EditorGUI.indentLevel, GUILayoutUtility.GetLastRect().y + 22, 300, GUILayoutUtility.GetLastRect().height);
			EditorGUI.LabelField(drawPosition, display, s);
			if(CheckForOldAppSprite(input))
			{
				drawPosition.y += 15;
				s.normal.textColor = new Color32(36, 124, 134, 255);
				EditorGUI.LabelField(drawPosition, "This image appears to be from an older app!", s);
			}
			return val;
		}
		else if(obj == null)
		{
			T val = (T)EditorGUILayout.ObjectField(" ", obj, typeof(T), true);
			EditorGUI.LabelField(new Rect(15 * EditorGUI.indentLevel, GUILayoutUtility.GetLastRect().y, 300, GUILayoutUtility.GetLastRect().height), display, s);
			return val;
		}
		else
		{
			return (T)EditorGUILayout.ObjectField(display, obj, typeof(T), true);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Audio Clip Option
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected AudioClip DrawAudioClipOption(string name, AudioClip audioClip, string tooltip = "")
	{
		AudioClip returningValue = DrawObjectOption(name, audioClip, tooltip);
		if(returningValue != null)
		{
			Rect pos = new Rect(GUILayoutUtility.GetLastRect().width / 2, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height + 2, GUILayoutUtility.GetLastRect().width / 2, GUILayoutUtility.GetLastRect().height);
			if(GUI.Button(pos, new GUIContent("Play Sound", "Plays the associated sound")))
			{
				Camera.main.GetComponent<AudioSource>().PlayOneShot(returningValue);
			}
			AddSpaces(3);
		}
		return returningValue;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Label
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawLabel(string text, bool bUseCustomColour = false)
	{
		if (bUseCustomColour)
			DrawLabel(text, IsSecondaryComponent() ? SecondaryFontColour : MainFontColour);
		else 
			EditorGUILayout.LabelField(text);
	}

	protected void DrawLabel(string text, GUIStyle s)
	{
		EditorGUILayout.LabelField(text, s);
	}

	protected void DrawLabel(string text, Color colour)
	{
		GUIStyle s = new GUIStyle();
		s.normal.textColor = colour;
		DrawLabel(text, s);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Changeable Number Option (INT)
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected int DrawChangeableIntegerOption(string label, int currentNumber, int changingAmount = 1, string tooltip = "", bool indentLabel = true)
	{
		if(label != "")
		{
			if(indentLabel) { EditorGUI.indentLevel += 1; }
			EditorGUILayout.LabelField(new GUIContent(label, tooltip));
			if(indentLabel) { EditorGUI.indentLevel -= 1; }
		}
		return DrawChangeableIntegerOption(GetScaledRect(), "", currentNumber, changingAmount, "", false);
	}

    protected int DrawChangeableIntegerOption(Rect drawPos, string label, int curentNumber, int changingAmount = 1, string tooltip = "", bool indentLabel = true)
	{
		if (label != "")
		{
			if (indentLabel) { EditorGUI.indentLevel += 1; }
			EditorGUILayout.LabelField(new GUIContent(label, tooltip));
			if (indentLabel) { EditorGUI.indentLevel -= 1; }
		}
		float bw = 20; // Button Width
		float tw = 30; // Text Box width
		Rect pos = drawPos;
		pos.x = ((pos.x + pos.width) - (50.0f + tw));
		pos.width = bw;
		if (GUI.Button(pos, "<"))
		{
			curentNumber -= changingAmount;
		}

		pos.x += pos.width + 5;
		pos.width = tw;
		int currentEditorIndent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		curentNumber = int.Parse(EditorGUI.TextField(pos, curentNumber.ToString()));
		EditorGUI.indentLevel = currentEditorIndent;

		pos.x += pos.width + 5;
		pos.width = bw;
		if (GUI.Button(pos, ">"))
		{
			curentNumber += changingAmount;
		}
		return curentNumber;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Draw Changeable Number Option (FLOAT)
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected float DrawChangeableFloatOption(string label, float currentNumber, float changingAmount = 0.1f, string tooltip = "", bool indentLabel = true)
	{
		if(label != "")
		{
			if(indentLabel) { EditorGUI.indentLevel += 1; }
			EditorGUILayout.LabelField(new GUIContent(label, tooltip));
			if(indentLabel) { EditorGUI.indentLevel -= 1; }
		}
		return DrawChangeableFloatOption(GetScaledRect(), "", currentNumber, changingAmount, "", false);
	}

	protected float DrawChangeableFloatOption(Rect drawPos, string label, float curentNumber, float changingAmount = 0.1f, string tooltip = "", bool indentLabel = true)
	{
		if (label != "")
		{
			if (indentLabel) { EditorGUI.indentLevel += 1; }
			EditorGUILayout.LabelField(new GUIContent(label, tooltip));
			if (indentLabel) { EditorGUI.indentLevel -= 1; }
		}
		float bw = 20; // Button Width
		float tw = 50; // Text Box width
		Rect pos = drawPos;
		pos.x = ((pos.x + pos.width) - (50.0f + tw));
		pos.width = bw;
		if (GUI.Button(pos, "<"))
		{
			curentNumber -= changingAmount;
		}

		pos.x += pos.width + 5;
		pos.width = tw;
		int currentEditorIndent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		curentNumber = float.Parse(EditorGUI.TextField(pos, curentNumber.ToString()));
		EditorGUI.indentLevel = currentEditorIndent;

		pos.x += pos.width + 5;
		pos.width = bw;
		if (GUI.Button(pos, ">"))
		{
			curentNumber += changingAmount;
		}
		return curentNumber;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Audio Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawAudioOptions(UnityAudioSourceManager.AudioHandlerInfo audioInfo)
	{
		audioInfo.m_acAudioToPlay = DrawObjectOption("AudioClip To Play:", audioInfo.m_acAudioToPlay, "The Desired AudioClip To be played, whether that be BGM or SFX");
		if(audioInfo.m_acAudioToPlay != null)
		{
			AudioSource rAudioSource = ((Camera.main != null && Camera.main.GetComponent<AudioSource>() != null) ? Camera.main.GetComponent<AudioSource>() : null);
            EditorGUI.indentLevel += 1;
			{
				bool bNewIsLooping = DrawToggleField("Loop Audio:", audioInfo.m_bLoopAudio, "Will the Audio Loop?", true);
				if(bNewIsLooping != audioInfo.m_bLoopAudio)
				{
					audioInfo.m_bLoopAudio = bNewIsLooping;
					if(rAudioSource != null && rAudioSource.clip == audioInfo.m_acAudioToPlay)
						rAudioSource.loop = bNewIsLooping;
				}
				float fNewVolume = EditorGUILayout.Slider(new GUIContent("Play Volume:", "Max volume of the AudioClip whilst playing"), audioInfo.m_fMaxVolume, 0.05f, 1.0f);
				if(fNewVolume != audioInfo.m_fMaxVolume)
				{
					audioInfo.m_fMaxVolume = fNewVolume;
					if(rAudioSource != null && rAudioSource.clip == audioInfo.m_acAudioToPlay)
						rAudioSource.volume = fNewVolume;
                }
				AddSpaces(2);
				audioInfo.m_bRandomiseTrackStartPosition = DrawToggleField("Randomise Track Start Position: ", audioInfo.m_bRandomiseTrackStartPosition, "Should the Track Start from a Random Point when played?", true);
				if(!audioInfo.m_bRandomiseTrackStartPosition)
				{
					EditorGUI.indentLevel += 1;
						audioInfo.m_iStartTrackPosition = EditorGUILayout.IntSlider(new GUIContent("Audio Start Position:", "using PCM Samples, The Audio Track will start from the Specified Position. The value is clamped to be within range of the Specified Audio Clip"), audioInfo.m_iStartTrackPosition, 0, audioInfo.m_acAudioToPlay.samples - 1);// Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Audio Start Position:", "using PCM Samples, The Audio Track will start from the Specified Position. The value is clamped to be within range of the Specified Audio Clip"), audioInfo.m_iStartTrackPosition), 0, audioInfo.m_acAudioToPlay.samples);
					EditorGUI.indentLevel -= 1;
				}
				audioInfo.m_bFadeinAudioUponPlaying = DrawToggleField("Fade-in Audio When Playing:", audioInfo.m_bFadeinAudioUponPlaying, "When the call is made to begin playing this audio, shall we fade-in the Audio \n(From Vol: 0.00 : To Vol: " + audioInfo.m_fMaxVolume.ToString("0.00") + ")?");
				audioInfo.m_fFadeinAudioTime = EditorGUILayout.FloatField(new GUIContent("Fade-in Time:", "How long will it take to fade-in the Audio, assuming we ever do so"), audioInfo.m_fFadeinAudioTime);
				audioInfo.m_fFadeoutAudioTime = EditorGUILayout.FloatField(new GUIContent("Fade-out Time:", "How long will it take to fade-out the Audio, assuming we ever do so"), audioInfo.m_fFadeoutAudioTime);

				if(rAudioSource != null)
				{
                    AddSpaces(1);
					Rect drawPos = GetScaledRect();
					float buttonWidth = 150.0f;
					drawPos.x += (drawPos.width - buttonWidth);
					drawPos.width = buttonWidth;
					drawPos.height = 18.0f;
					if(GUI.Button(drawPos, new GUIContent("Play Audio", "Plays the audio using the values above")))
					{
						rAudioSource.Stop();
						rAudioSource.clip = audioInfo.m_acAudioToPlay;
						rAudioSource.loop = audioInfo.m_bLoopAudio;
						rAudioSource.volume = audioInfo.m_fMaxVolume;
						rAudioSource.Play();
						int trackStartPosition = (audioInfo.m_bRandomiseTrackStartPosition ? Random.Range(0, (audioInfo.m_acAudioToPlay.samples / audioInfo.m_acAudioToPlay.channels)) : audioInfo.m_iStartTrackPosition);
						rAudioSource.timeSamples = trackStartPosition;
					}
					if(rAudioSource.isPlaying)
					{
						drawPos.x -= (10.0f + buttonWidth);
						if(GUI.Button(drawPos, new GUIContent("Stop Audio", "Stops playing whatever audio is currently playing in the Editor")))
						{
							rAudioSource.Stop();
                        }
					}
					AddSpaces(2);
				}
	        }
			EditorGUI.indentLevel -= 1;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Serialized Object Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawSerialisedObjectOptions(string name, string variableName, string tooltip = "")
	{
		serializedObject.Update();
		SerializedProperty property = serializedObject.FindProperty(variableName);
		EditorGUI.BeginChangeCheck();
		GUIContent label = (tooltip != "" ? new GUIContent(name, tooltip) : new GUIContent(name));
		EditorGUILayout.PropertyField(property, label, true);
		if(EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Draw Array Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void DrawArrayOptions(string name, string arrayVariableName, string tooltip = "")
	{
		serializedObject.Update();
		SerializedProperty property = serializedObject.FindProperty(arrayVariableName);
		EditorGUI.BeginChangeCheck();
		GUIContent label = (tooltip != "" ? new GUIContent(name, tooltip) : new GUIContent(name));
		EditorGUILayout.PropertyField(property, label, true);
		if(EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
			OnArrayModification(arrayVariableName);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Resize Array
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected static void ResizeArray<T>(ref T[] arrayField, int newSize)
	{
		if(typeof(T).IsClass)
		{
			Debug.LogError("SCRIPTABLE OBJECT DETECTED, USE 'ResizeReferenceArray' Instead");
		}

		T[] newArray = new T[newSize];
		for(int i = 0; i < newSize; ++i)
		{
			if(arrayField.Length > i)
			{
				OnArrayVariableModification(ref newArray[i], ref arrayField[i]);
			}
		}
		arrayField = newArray;
	}

	protected static void ResizeReferenceArray<T>(ref T[] arrayField, int newSize) where T: class
	{
		T[] newArray = new T[newSize];
		for (int i = 0; i < newSize; ++i)
		{
			if (arrayField.Length > i)
			{
				OnArrayVariableModification(ref newArray[i], ref arrayField[i]);
			}
			else
			{
				OnArrayVariableModification(ref newArray[i]); 
			}
		}

		arrayField = newArray;
		OnArrayModification(ref arrayField);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Prepend Array
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected static void PrependArray<T>(ref T[] original, ref T[] prependingArray)
	{
		T[] newArray = prependingArray.Clone() as T[];
		AppendArray(ref newArray, ref original);
		original = newArray;
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Append Array
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected static void AppendArray<T>(ref T[] original, ref T[] appendingArray)
	{
		int originalLength = (original != null ? original.Length : 0);
		int newSize = originalLength + appendingArray.Length;
        T[] newArray = new T[newSize];

		int appendingIndex = 0;
		for(int i = 0; i < newSize; ++i)
		{
			if(originalLength > i)
			{
				OnArrayVariableModification(ref newArray[i], ref original[i]);
			}
			else
			{
				newArray[i] = appendingArray[appendingIndex++];
            }
		}

		original = newArray;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Callback Methods: On Array Modification
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected static void OnArrayVariableModification<T>(ref T destination) where T: class
	{
		// MonoBehaviour is not allow to be instantiated in Code without using the Instantiate Method. This will cause a warning... Hence the check for 'MonoBehaviour'
		if(typeof(T) != typeof(MonoBehaviour))
			destination = System.Activator.CreateInstance(typeof(T), true) as T;
	}

	protected static void OnArrayVariableModification<T>(ref T destination, ref T source)
	{
		destination = source;
	}

	protected static void OnArrayModification(string whichArray)
	{
	}

	protected static void OnArrayModification<T>(ref T[] arrayField)
	{
	}





	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Draw Colour Animation Effect Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void DrawColourAnimationEffectOptions(ref ColourAnimationEffect[] rAnimationEffects, Material mTarget, SpriteRenderer sprTarget)
	{
		int iTotalSize = DrawIntField("Total Animation Frames: ", rAnimationEffects.Length);
		if (iTotalSize != rAnimationEffects.Length)
		{
			rAnimationEffects = ColourAnimationEffect.ResizeArray(rAnimationEffects, iTotalSize);
		}

		for (int i = 0; i < rAnimationEffects.Length; ++i)
		{
			// Assign new instance if not already existing
			if (rAnimationEffects[i] == null)
				rAnimationEffects[i] = new ColourAnimationEffect();

			DrawColourAnimationEffectOptions(ref rAnimationEffects[i], mTarget, sprTarget);
			if (rAnimationEffects[i].m_bDisplayAnimationOptions)
				AddSpaces(3);
		}
	}

	public void DrawColourAnimationEffectOptions(ref ColourAnimationEffect rAnimationEffect, Material mTarget, SpriteRenderer sprTarget)
	{
		rAnimationEffect.Target = mTarget;
		if (mTarget == null)
			return;

		EditorGUI.indentLevel += 1;
		{
			//~~~ Draw Name of Animation Effect Option and Target Transform Object ~~~
			{
				AddSpaces(1);
				EditorGUI.ObjectField(new Rect(320, GUILayoutUtility.GetLastRect().y, GUILayoutUtility.GetLastRect().width - 320, 16), sprTarget.transform, typeof(Transform), true);
				rAnimationEffect.m_sEffectName = EditorGUI.TextArea(new Rect(50, GUILayoutUtility.GetLastRect().y, 275, 16), rAnimationEffect.m_sEffectName);
				rAnimationEffect.m_bDisplayAnimationOptions = EditorGUI.Foldout(new Rect(0, GUILayoutUtility.GetLastRect().y, 100, 16), rAnimationEffect.m_bDisplayAnimationOptions, rAnimationEffect.m_bDisplayAnimationOptions ? "Hide" : "Show", true, EditorStyles.foldout);
				AddSpaces(2);
			}

			if (rAnimationEffect.m_bDisplayAnimationOptions)
			{
				// Draw Global Copy/Paste Options
				bool showClipboardOptions = true;
				if (showClipboardOptions)
				{
					AddSpaces(1);
					int draw_xPos = EditorGUI.indentLevel * 20;
					if (GUI.Button(new Rect(draw_xPos, GUILayoutUtility.GetLastRect().y, (GUILayoutUtility.GetLastRect().width / 2) - (10 + (draw_xPos / 2)), 16), new GUIContent("Copy Animation Effect Values", "Copies the values of this Animation Effect into a global clipboard which can then be  used to paste these values into another Animation Effect")))
					{
						ColourAnimationEffect.sm_rAnimationEffectInstance = rAnimationEffect;
					}
					if (GUI.Button(new Rect((draw_xPos / 2) + ((GUILayoutUtility.GetLastRect().width / 2) - 5), GUILayoutUtility.GetLastRect().y, (GUILayoutUtility.GetLastRect().width / 2), 16), new GUIContent("Paste Copied Animation Effect Values", "Paste the values of the Animation Effect which has been copied into the global clipboard (if any exist)")))
					{
						if (AnimationEffect.sm_rAnimationEffectInstance == null)
						{
							EditorUtility.DisplayDialog("Error!", "There is no Colour Animation Effect to copy!", "Okay");
						}
						else
						{
							string values = "  Start Colour:  \t" + ColourAnimationEffect.sm_rAnimationEffectInstance.m_cStartingColour.ToString() + "\n" +
											"  End Colour:    \t" + ColourAnimationEffect.sm_rAnimationEffectInstance.m_cEndColour.ToString();
							if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you wish to replace the current Animation Effect (" + rAnimationEffect.m_sEffectName + ") with the copied one (" + AnimationEffect.sm_rAnimationEffectInstance.m_sEffectName + ")?  \n\nContaining Values:\n" + values, "Confirm", "Deny"))
							{
								rAnimationEffect = ColourAnimationEffect.sm_rAnimationEffectInstance.Clone();
							}
						}
					}
					AddSpaces(2);
				}


				EditorGUI.indentLevel += 1;
				{
					rAnimationEffect.m_fTotalAnimationTime = EditorGUILayout.FloatField("Total Animation Time: ", rAnimationEffect.m_fTotalAnimationTime);

					//~~~ Starting Colour ~~~
					Color newColourValue = EditorGUILayout.ColorField("Starting Colour: ", rAnimationEffect.m_cStartingColour);
					if (newColourValue != rAnimationEffect.m_cStartingColour)
					{
						rAnimationEffect.m_cStartingColour = newColourValue;
						sprTarget.color = newColourValue;
						sprTarget.sharedMaterial.SetColor("_Colour", sprTarget.color);
					}
					//~~~ Ending Colour ~~~
					newColourValue = EditorGUILayout.ColorField("Ending Colour: ", rAnimationEffect.m_cEndColour);
					if (newColourValue != rAnimationEffect.m_cEndColour)
					{
						rAnimationEffect.m_cEndColour = newColourValue;
						sprTarget.color = newColourValue;
						sprTarget.sharedMaterial.SetColor("_Colour", sprTarget.color);
					}

					//~~~ GUI Slider Movement ~~~
					float percentage = EditorGUILayout.Slider("Reveal: ", rAnimationEffect.m_fCompletionRange, 0.0f, 1.0f);
					if (rAnimationEffect.m_fCompletionRange != percentage)
					{
						rAnimationEffect.m_fCompletionRange = percentage;
						sprTarget.color = Color.Lerp(rAnimationEffect.m_cStartingColour, rAnimationEffect.m_cEndColour, percentage);
						sprTarget.sharedMaterial.SetColor("_Colour", sprTarget.color);
					}


					//~~~ Copy/Paste Button Options ~~~
					EditorGUI.indentLevel += 1;
					{
						AddSpaces(1);
						EditorGUILayout.LabelField(new GUIContent("Start Options: "));
						float x = 195;
						float w = 105;
						float ew = 180;
						Rect Pos = new Rect(x, GUILayoutUtility.GetLastRect().y, w, 20);
						if (GUI.Button(Pos, "Copy Colour"))
						{
							rAnimationEffect.m_cStartingColour = sprTarget.color;
						}
						Pos.x += Pos.width + 10;
						Pos.width = ew;
						if (GUI.Button(Pos, "Show Colour"))
						{
							sprTarget.color = rAnimationEffect.m_cStartingColour;
							sprTarget.sharedMaterial.SetColor("_Colour", sprTarget.color);
						}
						EditorGUILayout.LabelField(new GUIContent("End Options: "));
						Pos.x = x;
						Pos.width = w;
						Pos.y += Pos.height;
						AddSpaces(1);
						if (GUI.Button(Pos, "Copy Colour"))
						{
							rAnimationEffect.m_cEndColour = sprTarget.color;
						}
						Pos.x += Pos.width + 10;
						Pos.width = ew;
						if (GUI.Button(Pos, "Show Colour"))
						{
							sprTarget.color = rAnimationEffect.m_cEndColour;
							sprTarget.sharedMaterial.SetColor("_Colour", sprTarget.color);
						}
						AddSpaces(1);
					}
					EditorGUI.indentLevel -= 1;
				}
				EditorGUI.indentLevel -= 1;
			}
		}
		EditorGUI.indentLevel -= 1;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Draw Animation Effect Options
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void DrawAnimationEffectOptions(ref AnimationEffect[] rAnimationEffects, Transform TrTarget = null)
	{
		int iTotalSize = DrawIntField("Total Animation Frames: ", rAnimationEffects.Length);
		if (iTotalSize != rAnimationEffects.Length)
		{
			rAnimationEffects = AnimationEffect.ResizeArray(rAnimationEffects, iTotalSize);
		}

		for (int i = 0; i < rAnimationEffects.Length; ++i)
		{
			// Assign new instance if not already existing
			if (rAnimationEffects[i] == null)
				rAnimationEffects[i] = new AnimationEffect();

			DrawAnimationEffectOptions(ref rAnimationEffects[i], TrTarget);
			if (rAnimationEffects[i].m_bDisplayAnimationOptions)
				AddSpaces(3);
		}
	}

	public void DrawAnimationEffectOptions(ref AnimationEffect rAnimationEffect, Transform TrTarget)
	{
		rAnimationEffect.Target = TrTarget;

		EditorGUI.indentLevel += 1;
		{
			if(rAnimationEffect.m_fTotalAnimationTime < 0.01f)
			{
				DrawLabel("ANIMATION EFFECT HAS SOME INVALID OPTIONS!", Color.red);
			}

			//~~~ Draw Name of Animation Effect Option and Target Transform Object ~~~
			{
				AddSpaces(1);
				EditorGUI.ObjectField(new Rect(320, GUILayoutUtility.GetLastRect().y, GUILayoutUtility.GetLastRect().width - 320, 16), TrTarget, typeof(Transform), true);
				rAnimationEffect.m_sEffectName = EditorGUI.TextArea(new Rect(50, GUILayoutUtility.GetLastRect().y, 275, 16), rAnimationEffect.m_sEffectName);
				rAnimationEffect.m_bDisplayAnimationOptions = EditorGUI.Foldout(new Rect(0, GUILayoutUtility.GetLastRect().y, 100, 16), rAnimationEffect.m_bDisplayAnimationOptions, rAnimationEffect.m_bDisplayAnimationOptions ? "Hide" : "Show", true, EditorStyles.foldout);
				AddSpaces(2);
			}

			if (rAnimationEffect.m_bDisplayAnimationOptions)
			{
				// Draw Global Copy/Paste Options
				bool showClipboardOptions = true;
				if (showClipboardOptions)
				{
					AddSpaces(1);
					int draw_xPos = EditorGUI.indentLevel * 20;
					if (GUI.Button(new Rect(draw_xPos, GUILayoutUtility.GetLastRect().y, (GUILayoutUtility.GetLastRect().width / 2) - (10 + (draw_xPos / 2)), 16), new GUIContent("Copy Animation Effect Values", "Copies the values of this Animation Effect into a global clipboard which can then be  used to paste these values into another Animation Effect")))
					{
						AnimationEffect.sm_rAnimationEffectInstance = rAnimationEffect;
					}
					if (GUI.Button(new Rect((draw_xPos / 2) + ((GUILayoutUtility.GetLastRect().width / 2) - 5), GUILayoutUtility.GetLastRect().y, (GUILayoutUtility.GetLastRect().width / 2), 16), new GUIContent("Paste Copied Animation Effect Values", "Paste the values of the Animation Effect which has been copied into the global clipboard (if any exist)")))
					{
						if (AnimationEffect.sm_rAnimationEffectInstance == null)
						{
							EditorUtility.DisplayDialog("Error!", "There is no Animation Effect to copy!", "Okay");
						}
						else
						{
							string values = "  Start Position:\t" + AnimationEffect.sm_rAnimationEffectInstance.m_vStartingPosition.ToString() + "\n" +
											"  End Position:  \t" + AnimationEffect.sm_rAnimationEffectInstance.m_vEndPosition.ToString() + "\n" +
											"  Start Rotation:\t" + AnimationEffect.sm_rAnimationEffectInstance.m_vStartingRotation.ToString() + "\n" +
											"  End Rotation   \t" + AnimationEffect.sm_rAnimationEffectInstance.m_vEndRotation.ToString() + "\n" +
											"  Start Scale:   \t" + AnimationEffect.sm_rAnimationEffectInstance.m_vStartingScale.ToString() + "\n" +
											"  End Scale:     \t" + AnimationEffect.sm_rAnimationEffectInstance.m_vEndScale.ToString() + "\n" +
											"  Start Colour:  \t" + AnimationEffect.sm_rAnimationEffectInstance.m_cStartingColour.ToString() + "\n" +
											"  End Colour:    \t" + AnimationEffect.sm_rAnimationEffectInstance.m_cEndColour.ToString() + "\n" +
											(AnimationEffect.sm_rAnimationEffectInstance.m_sprNewSprite != null ? "  New Sprite:    \t" + AnimationEffect.sm_rAnimationEffectInstance.m_sprNewSprite.name : "");
							if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you wish to replace the current Animation Effect (" + rAnimationEffect.m_sEffectName + ") with the copied one (" + AnimationEffect.sm_rAnimationEffectInstance.m_sEffectName + ")?  \n\nContaining Values:\n" + values, "Confirm", "Deny"))
							{
								rAnimationEffect = AnimationEffect.sm_rAnimationEffectInstance.Clone();
							}
						}
					}
					AddSpaces(2);
				}


				EditorGUI.indentLevel += 1;
				{
					if(rAnimationEffect.m_fTotalAnimationTime < 0.01f)
					{
						GUIStyle s = new GUIStyle();
						s.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 0.75f);
						rAnimationEffect.m_fTotalAnimationTime = EditorGUILayout.FloatField(" ", rAnimationEffect.m_fTotalAnimationTime);
						EditorGUI.LabelField(new Rect(15 * EditorGUI.indentLevel, GUILayoutUtility.GetLastRect().y, 300, GUILayoutUtility.GetLastRect().height), "Total Animation Time: ", s);
					}
					else
					{
						rAnimationEffect.m_fTotalAnimationTime = EditorGUILayout.FloatField("Total Animation Time: ", rAnimationEffect.m_fTotalAnimationTime);
					}

					//~~~ Starting Position ~~~
					Vector3 newVectorValue = EditorGUILayout.Vector3Field("Starting Position: ", rAnimationEffect.m_vStartingPosition);
					if (newVectorValue != rAnimationEffect.m_vStartingPosition)
					{
						rAnimationEffect.m_vStartingPosition = newVectorValue;
						TrTarget.localPosition = newVectorValue;
					}
					//~~~ Ending Position ~~~
					if (rAnimationEffect.m_vStartingPosition != rAnimationEffect.m_vEndPosition)
					{
						newVectorValue = EditorGUILayout.Vector3Field("Ending Position: ", rAnimationEffect.m_vEndPosition);
						if (newVectorValue != rAnimationEffect.m_vEndPosition)
						{
							rAnimationEffect.m_vEndPosition = newVectorValue;
							TrTarget.localPosition = newVectorValue;
						}
					}

					//~~~ Starting Rotation ~~~
					newVectorValue = EditorGUILayout.Vector3Field("Starting Rotation: ", rAnimationEffect.m_vStartingRotation);
					if (newVectorValue != rAnimationEffect.m_vStartingRotation)
					{
						rAnimationEffect.m_vStartingRotation = newVectorValue;
						TrTarget.localRotation = Quaternion.Euler(newVectorValue);
					}
					//~~~ Ending Rotation ~~~
					if (rAnimationEffect.m_vStartingRotation != rAnimationEffect.m_vEndRotation)
					{
						newVectorValue = EditorGUILayout.Vector3Field("Ending Rotation: ", rAnimationEffect.m_vEndRotation);
						if (newVectorValue != rAnimationEffect.m_vEndRotation)
						{
							rAnimationEffect.m_vEndRotation = newVectorValue;
							TrTarget.localRotation = Quaternion.Euler(newVectorValue);
						}
					}

					//~~~ Starting Scale ~~~
					newVectorValue = EditorGUILayout.Vector3Field("Starting Scale: ", rAnimationEffect.m_vStartingScale);
					if (newVectorValue != rAnimationEffect.m_vStartingScale)
					{
						rAnimationEffect.m_vStartingScale = newVectorValue;
						TrTarget.localScale = newVectorValue;
					}
					//~~~ Ending Scale ~~~
					if (rAnimationEffect.m_vStartingScale != rAnimationEffect.m_vEndScale)
					{
						newVectorValue = EditorGUILayout.Vector3Field("Ending Scale: ", rAnimationEffect.m_vEndScale);
						if (newVectorValue != rAnimationEffect.m_vEndScale)
						{
							rAnimationEffect.m_vEndScale = newVectorValue;
							TrTarget.localScale = newVectorValue;
						}
					}

					//~~~ Starting Colour ~~~
					Color newColourValue = EditorGUILayout.ColorField("Starting Colour: ", rAnimationEffect.m_cStartingColour);
					if (newColourValue != rAnimationEffect.m_cStartingColour)
					{
						rAnimationEffect.m_cStartingColour = newColourValue;
						if (TrTarget.GetComponent<SpriteRenderer>() != null) TrTarget.GetComponent<SpriteRenderer>().color = newColourValue;
						else if (TrTarget.GetComponent<UnityEngine.UI.Image>() != null) TrTarget.GetComponent<UnityEngine.UI.Image>().color = newColourValue;
						else if (TrTarget.GetComponent<UnityEngine.UI.Text>() != null) TrTarget.GetComponent<UnityEngine.UI.Text>().color = newColourValue;
					}
					//~~~ Ending Colour ~~~
					newColourValue = EditorGUILayout.ColorField("Ending Colour: ", rAnimationEffect.m_cEndColour);
					if (newColourValue != rAnimationEffect.m_cEndColour)
					{
						rAnimationEffect.m_cEndColour = newColourValue;
						if (TrTarget.GetComponent<SpriteRenderer>() != null) TrTarget.GetComponent<SpriteRenderer>().color = newColourValue;
						else if (TrTarget.GetComponent<UnityEngine.UI.Image>() != null) TrTarget.GetComponent<UnityEngine.UI.Image>().color = newColourValue;
						else if (TrTarget.GetComponent<UnityEngine.UI.Text>() != null) TrTarget.GetComponent<UnityEngine.UI.Text>().color = newColourValue;
					}

					//~~~ First Frame - New Sprite ~~~
					rAnimationEffect.m_sprNewSprite = (Sprite)EditorGUILayout.ObjectField("New Sprite: ", rAnimationEffect.m_sprNewSprite, typeof(Sprite), true);


					//~~~ GUI Slider Movement ~~~
					float percentage = rAnimationEffect.m_fCompletionRange;
					rAnimationEffect.m_fCompletionRange = EditorGUILayout.Slider("Reveal: ", rAnimationEffect.m_fCompletionRange, 0.0f, 1.0f);
					if (rAnimationEffect.m_fCompletionRange != percentage)
					{
						rAnimationEffect.UpdateFromSliderGUI();
					}


					//~~~ Copy/Paste Button Options ~~~
					EditorGUI.indentLevel += 1;
					{
						AddSpaces(1);
						EditorGUILayout.LabelField(new GUIContent("Start Options: "));
						float x = 195;
						float w = 105;
						float ew = 180;
						Rect Pos = new Rect(x, GUILayoutUtility.GetLastRect().y, w, 20);
						if (GUI.Button(Pos, "Copy Transform"))
						{
							rAnimationEffect.CopyTransformToBegin();
						}
						Pos.x += Pos.width + 10;
						Pos.width = ew;
						if (GUI.Button(Pos, "Show Animation Transform"))
						{
							rAnimationEffect.ShowBeginTransform();
						}
						EditorGUILayout.LabelField(new GUIContent("End Options: "));
						Pos.x = x;
						Pos.width = w;
						Pos.y += Pos.height;
						AddSpaces(1);
						if (GUI.Button(Pos, "Copy Transform"))
						{
							rAnimationEffect.CopyTransformToEnd();
						}
						Pos.x += Pos.width + 10;
						Pos.width = ew;
						if (GUI.Button(Pos, "Show Animation Transform"))
						{
							rAnimationEffect.ShowEndTransform();
						}
						AddSpaces(1);
					}
					EditorGUI.indentLevel -= 1;
				}
				EditorGUI.indentLevel -= 1;
			} // ~~~ if(rAnimationEffect.m_bDisplayAnimationOptions)
		}
		EditorGUI.indentLevel -= 1;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show "Open File Dialogue"
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	/// <summary>
	/// Opens up the "Open File Dialog" Window on both Mac & PC. Will allow you to select what you want to have selected
	/// </summary>
	/// <param name="filters">Pass in the extensions you wish to allow the user (ie. you) to be able to select. EG: "txt". Pass in with a separating semicolon
	/// for multiple extension types (EG: "ogg;mp3;wav")</param>
	/// <returns>The absolute path to the desired file. Will return an empty string if the user chooses to exit the window without selecting a file</returns>
	protected string ShowOpenFileDialogueWindow(string filters)
	{
		string openFilename = EditorUtility.OpenFilePanel("Open File", sm_sOpenPathDirectory, filters);
		if(openFilename != "")
		{
			sm_sOpenPathDirectory = System.IO.Path.GetDirectoryName(openFilename) + '/';
		}
		return openFilename;
	}
}
