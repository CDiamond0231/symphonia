//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Animation Effect
//             Version: 1.0
//             Author: Christopher Allport
//             Date: November 24, 2014
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script is used as a base class to the animations found throughout
//	  the app. The animations used with this script are ones based upon time.
//		Most animations come from this script.
//
//	  All others continue to use Unity's in-built Animation System.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;


[System.Serializable]
public class AnimationEffect
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Public Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public float m_fTotalAnimationTime;
	public Vector3 m_vStartingPosition, m_vEndPosition;
	public Vector3 m_vStartingRotation, m_vEndRotation;
	public Vector3 m_vStartingScale, m_vEndScale;
	public Color m_cStartingColour = Color.white, m_cEndColour = Color.white;
	public Sprite m_sprNewSprite;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*- Private Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private TimeTracker m_ttTimer;																	// Animation Time Manager
	private bool m_bFirstUpdate = false;
	[SerializeField] private Transform m_tTarget;													// Object To Apply Changes to
	[SerializeField] private SpriteRenderer m_rSprRend;												// Object To Apply Changes to
	[SerializeField] private UnityEngine.UI.Text m_rText;											// Object To Apply Changes to
	[SerializeField] private UnityEngine.UI.Image m_rImage;                                         // Object To Apply Changes to
	[SerializeField] private bool m_bPositionChanging;                                              // - Booleans to help
	[SerializeField] private bool m_bRotationChanging;                                              //    speed up the animation
	[SerializeField] private bool m_bScaleChanging;                                                 //     process by not doing 
	[SerializeField] private bool m_bColourChanging;                                                //      things that aren't necessary
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*-- Debug Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#if UNITY_EDITOR
	public string m_sEffectName = "Animation Effect";												// Give this animation effect a name? (Only useful within the Unity Editor)
	public bool m_bDisplayAnimationOptions = true;													// Display Animation Options in Unity Inspector? Is changed insided inspector itself. This is just a stored instance variable {since we can have many AnimationEffects in one script, one variable in the CustomEditor won't cut it}
	public float m_fCompletionRange = 0.0f;															// Completion Percentage, used with a slider to show the animation happening (Only useful within the Unity Editor)
	public static AnimationEffect sm_rAnimationEffectInstance = null;								// Allows one Animation Effect to be copied to other Animation Effects (Only useful within the Unity Editor)
#endif
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Acessors
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public Transform Target
	{
		get { return m_tTarget;	}
		set { Setup(value); }
	}

	public float CompletionPercentage
	{
		get { return m_ttTimer.GetCompletionPercentage(); }
		set { SetPosition(value); }
	}


	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Setup
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void Setup(Transform target, SpriteRenderer spriteRenderer = null, UnityEngine.UI.Text textRenderer = null, UnityEngine.UI.Image imageRenderer = null)
	{
		m_tTarget	= target;
		m_ttTimer	= new TimeTracker(m_fTotalAnimationTime);
        m_rSprRend	= (spriteRenderer != null) ? spriteRenderer : Target.GetComponent<SpriteRenderer>();
		m_rText		= (textRenderer != null) ? textRenderer : Target.GetComponent<UnityEngine.UI.Text>();
		m_rImage	= (imageRenderer != null) ? imageRenderer : Target.GetComponent<UnityEngine.UI.Image>();

		Reset();
	}

	public void Setup(GameObject target, SpriteRenderer spriteRenderer = null)
	{
		Setup(target.transform, spriteRenderer);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool UpdateAnimation(float fAnimationSpeed = 1.0f)
	{
		if (!IsCompleted())
		{
			if (m_bFirstUpdate)
			{
				OnFirstUpdate();
			}
			
			
			// If timer is finished. Finish Animation
			if (m_ttTimer.Update(fAnimationSpeed))
			{
                if (m_bPositionChanging)	{ Target.localPosition = m_vEndPosition; }
                if (m_bRotationChanging)	{ Target.localRotation = Quaternion.Euler(m_vEndRotation); }
                if (m_bScaleChanging)		{ Target.localScale = m_vEndScale; }
				if (m_bColourChanging)		{ UpdateColour(m_cEndColour, m_cEndColour, 1.0f); }
			}

			// Otherwise Update it according to Timer Completion Percentage
			else
			{
				float t = m_ttTimer.GetCompletionPercentage();
                if (m_bPositionChanging)	{ Target.localPosition = Vector3.Lerp(m_vStartingPosition, m_vEndPosition, t); }
                if (m_bRotationChanging)	{ Target.localRotation = Quaternion.Euler(Vector3.Lerp(m_vStartingRotation, m_vEndRotation, t)); }
                if (m_bScaleChanging)		{ Target.localScale = Vector3.Lerp(m_vStartingScale, m_vEndScale, t); }
				if (m_bColourChanging)		{ UpdateColour(m_cStartingColour, m_cEndColour, t); }
			}
		}
		return IsCompleted();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Reverse Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool ReverseUpdate(float fAnimationSpeed = 1.0f)
	{
		if (!IsCompleted())
		{
			if (m_bFirstUpdate)
			{
				OnFirstUpdate();
			}


			// If timer is finished. Finish Animation
			if (m_ttTimer.Update(fAnimationSpeed))
			{
				if (m_bPositionChanging) { Target.localPosition = m_vStartingPosition; }
				if (m_bRotationChanging) { Target.localRotation = Quaternion.Euler(m_vStartingRotation); }
				if (m_bScaleChanging)	 { Target.localScale = m_vStartingScale; }
				if (m_bColourChanging)	 { UpdateColour(m_cStartingColour, m_cStartingColour, 1.0f); }
			}

			// Otherwise Update it according to Timer Completion Percentage
			else
			{
				float t = m_ttTimer.GetCompletionPercentage();
				if (m_bPositionChanging) { Target.localPosition = Vector3.Lerp(m_vEndPosition, m_vStartingPosition, t); }
				if (m_bRotationChanging) { Target.localRotation = Quaternion.Euler(Vector3.Lerp(m_vEndRotation, m_vStartingRotation, t)); }
				if (m_bScaleChanging)	 { Target.localScale = Vector3.Lerp(m_vEndScale, m_vStartingScale, t); }
				if (m_bColourChanging)	 { UpdateColour(m_cEndColour, m_cStartingColour, t); }
			}
		}
		return IsCompleted();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Set Animation Position
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void SetPosition(float t)
	{
		m_ttTimer.SetTimerCompletionPercentage(t);
		if (m_bPositionChanging)	{ Target.localPosition = Vector3.Lerp(m_vStartingPosition, m_vEndPosition, t); }
		if (m_bRotationChanging)	{ Target.localRotation = Quaternion.Euler(Vector3.Lerp(m_vStartingRotation, m_vEndRotation, t)); }
		if (m_bScaleChanging)		{ Target.localScale = Vector3.Lerp(m_vStartingScale, m_vEndScale, t); }
		if (m_bColourChanging)		{ UpdateColour(m_cStartingColour, m_cEndColour, t); }
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On First Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void OnFirstUpdate()
	{
		m_bFirstUpdate = false;

		// If there is a new sprite to be switching to on this animation, do it now
		if (m_rSprRend != null && m_sprNewSprite != null)
		{
			m_rSprRend.sprite = m_sprNewSprite;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Colour
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateColour(Color from, Color to, float t)
	{
		if (m_rSprRend != null) m_rSprRend.color = Color.Lerp(from, to, t);
		if (m_rText != null)	m_rText.color	 = Color.Lerp(from, to, t);
		if (m_rImage != null)	m_rImage.color	 = Color.Lerp(from, to, t);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Is Completed?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool IsCompleted()
	{
		return (m_ttTimer != null ? m_ttTimer.TimeUp() : false);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Reset
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void Reset()
	{
        if (Target != null)
		{
			m_bPositionChanging = (m_vStartingPosition != m_vEndPosition);
			m_bRotationChanging = (m_vStartingRotation != m_vEndRotation);
			m_bScaleChanging	= (m_vStartingScale != m_vEndScale);
			m_bColourChanging	= ((m_rSprRend != null || m_rText != null || m_rImage != null) && (m_cStartingColour != m_cEndColour));
			
			m_bFirstUpdate = true;
			
			if (m_ttTimer != null)
			{
				m_ttTimer.Reset();
			}
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show First Frame
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ShowFirstFrame()
	{
#if UNITY_EDITOR
		Reset();
#endif
		if (m_bPositionChanging)	{ Target.localPosition = m_vStartingPosition; }
		if (m_bRotationChanging)	{ Target.localRotation = Quaternion.Euler(m_vStartingRotation); }
		if (m_bScaleChanging)		{ Target.localScale = m_vStartingScale; }
		if (m_bColourChanging)		{ UpdateColour(m_cStartingColour, m_cStartingColour, 1.0f); }
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show Last Frame
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ShowLastFrame()
	{
#if UNITY_EDITOR
		Reset();
#endif
		if (m_bPositionChanging)	{ Target.localPosition = m_vEndPosition; }
		if (m_bRotationChanging)	{ Target.localRotation = Quaternion.Euler(m_vEndRotation); }
		if (m_bScaleChanging)		{ Target.localScale = m_vEndScale; }
		if (m_bColourChanging)		{ UpdateColour(m_cEndColour, m_cEndColour, 1.0f); }
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Force Finish
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ForceFinish(bool reversedFinish = false)
	{
		if (m_ttTimer != null) 
		{
			m_ttTimer.ForceTimerFinish ();
		}

		if (reversedFinish)
		{
			ShowFirstFrame();
		}
		else
		{
			ShowLastFrame();   
		}
	}







	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Clone
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public AnimationEffect Clone()
	{
		AnimationEffect AE			= new AnimationEffect();
		AE.m_fTotalAnimationTime	= this.m_fTotalAnimationTime;
		AE.m_vStartingPosition		= this.m_vStartingPosition;
		AE.m_vEndPosition			= this.m_vEndPosition;
		AE.m_vStartingRotation		= this.m_vStartingRotation;
		AE.m_vEndRotation			= this.m_vEndRotation;
		AE.m_vStartingScale			= this.m_vStartingScale;
		AE.m_vEndScale				= this.m_vEndScale;
		AE.m_cStartingColour		= this.m_cStartingColour;
		AE.m_cEndColour				= this.m_cEndColour;
		AE.m_sprNewSprite			= this.m_sprNewSprite;
		AE.m_tTarget				= this.m_tTarget;
		AE.m_bPositionChanging		= this.m_bPositionChanging;
		AE.m_bRotationChanging		= this.m_bRotationChanging;
		AE.m_bScaleChanging			= this.m_bScaleChanging;
		AE.m_bColourChanging		= this.m_bColourChanging;
		if (this.m_ttTimer != null)
		{
			AE.m_ttTimer = new TimeTracker(this.m_ttTimer.FinishTime);
		}

#if UNITY_EDITOR
		AE.m_sEffectName = this.m_sEffectName;
#endif
		
		return AE;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Resize Array
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public static AnimationEffect[] ResizeArray(AnimationEffect[] Original, int NewSize)
	{
		AnimationEffect[] aeNewArray = new AnimationEffect[NewSize];
		for (int i = 0; i < aeNewArray.Length; ++i)
		{
			if (i < Original.Length)
			{
				aeNewArray[i] = Original[i].Clone();
			}
			else
			{
				aeNewArray[i] = new AnimationEffect();
			}
		}
		return aeNewArray;
	}
	

#if UNITY_EDITOR
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update From Slider GUI
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void UpdateFromSliderGUI()
	{
        if (Target != null)
		{
			if (m_vStartingPosition != m_vEndPosition)	{ Target.localPosition = Vector3.Lerp(m_vStartingPosition, m_vEndPosition, m_fCompletionRange); }
			if (m_vStartingRotation != m_vEndRotation)	{ Target.localRotation = Quaternion.Euler(Vector3.Lerp(m_vStartingRotation, m_vEndRotation, m_fCompletionRange)); }
			if (m_vStartingScale != m_vEndScale)		{ Target.localScale = Vector3.Lerp(m_vStartingScale, m_vEndScale, m_fCompletionRange); }
			if (m_cStartingColour != m_cEndColour)		{ UpdateColour(m_cStartingColour, m_cEndColour, m_fCompletionRange); }
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Copy Transform To Begin
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void CopyTransformToBegin()
	{
        if (Target != null)
		{
            m_vStartingPosition = Target.localPosition;
            m_vStartingRotation = Target.localRotation.eulerAngles;
            m_vStartingScale = Target.localScale;

			if (m_rText != null)	m_cStartingColour = m_rText.color;
			if (m_rImage != null)	m_cStartingColour = m_rImage.color;
			if (m_rSprRend != null) m_cStartingColour = m_rSprRend.color;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Copy Transform To End
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void CopyTransformToEnd()
	{
        if (Target != null)
		{
            m_vEndPosition = Target.localPosition;
            m_vEndRotation = Target.localRotation.eulerAngles;
            m_vEndScale = Target.localScale;

			if (m_rText != null)	m_cEndColour = m_rText.color;
			if (m_rImage != null)	m_cEndColour = m_rImage.color;
			if (m_rSprRend != null) m_cEndColour = m_rSprRend.color;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show Begin Transform
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ShowBeginTransform()
	{
        if (Target != null)
		{
            if (m_vStartingPosition != m_vEndPosition)	Target.localPosition = m_vStartingPosition;
			if (m_vStartingRotation != m_vEndRotation)	Target.localRotation = Quaternion.Euler(m_vStartingRotation);
			if (m_vStartingScale != m_vEndScale)		Target.localScale = m_vStartingScale;
			if (m_cStartingColour != m_cEndColour)		UpdateColour(m_cStartingColour, m_cStartingColour, 1.0f);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show End Transform
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ShowEndTransform()
	{
        if (Target != null)
		{
			if (m_vStartingPosition != m_vEndPosition)	Target.localPosition = m_vEndPosition;
			if (m_vStartingRotation != m_vEndRotation)	Target.localRotation = Quaternion.Euler(m_vEndRotation);
			if (m_vStartingScale != m_vEndScale)		Target.localScale = m_vEndScale;
			if (m_cStartingColour != m_cEndColour)		UpdateColour(m_cEndColour, m_cEndColour, 1.0f);
		}
	}
#endif
}