//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Colour Animation Effect
//             Author: Christopher Allport
//             Date: December 10, 2015
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script contains the Colour Animation Effect. It's use is to transition
//		a sprite/text between two different colours. If there is anything more
//		complex that is required, use the 'AnimationEffect' class instead; which
//		also includes the ability to transition between colours as well as positions,
//		rotations, scale, etc.
//
//	  Note that this class only works with materials that contain a "_Colour" uniform
//		variable. This class does nothing otherwise.
//	  Essentially this class works with (currently) the SolidColour material in order
//		to show a highlighted outline.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;



[System.Serializable]
public class ColourAnimationEffect
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Public Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public float m_fTotalAnimationTime;
	public Color m_cStartingColour = Color.white;
	public Color m_cEndColour = Color.white;

#if UNITY_EDITOR
	public string m_sEffectName = "Colour Animation Effect";					// Give this animation effect a name? (Only useful within the Unity Editor)
	public bool m_bDisplayAnimationOptions = true;								// Display Animation Options in Unity Inspector? Is changed insided inspector itself. This is just a stored instance variable {since we can have many AnimationEffects in one script, one variable in the CustomEditor won't cut it}
	public float m_fCompletionRange = 0.0f;										// Completion Percentage, used with a slider to show the animation happening (Only useful within the Unity Editor)
	public static ColourAnimationEffect sm_rAnimationEffectInstance = null;		// Allows one Colour Animation Effect to be copied to other Colour Animation Effects (Only useful within the Unity Editor)
#endif
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*- Private Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private int m_iColourHashID;												// Quicker Access to shader values!
	private Material m_rTarget;													// Object To Apply Changes to
	private TimeTracker m_ttTimer;												// Animation Time Manager
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Acessors
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public Material Target
	{
		get { return m_rTarget; }
		set { Setup(value); }
	}

	public float CompletionPercentage
	{
		get { return m_ttTimer.GetCompletionPercentage(); }
		set { SetPosition(value); }
	}

	public Color CurrentColour
	{
		get			{ return m_rTarget.GetColor(m_iColourHashID); }
		private set { m_rTarget.SetColor(m_iColourHashID, value); }
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Setup
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void Setup(Material target)
	{
		m_iColourHashID = Shader.PropertyToID("_Colour");
		m_rTarget = target;
		m_ttTimer = new TimeTracker(m_fTotalAnimationTime);

		Reset();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool UpdateAnimation(float fAnimationSpeed = 1.0f)
	{
		if (!IsCompleted())
		{
			// If timer is finished. Finish Animation
			if (m_ttTimer.Update(fAnimationSpeed))
			{
				UpdateColour(m_cEndColour, m_cEndColour, 1.0f);
			}

			// Otherwise Update it according to Timer Completion Percentage
			else
			{
				UpdateColour(m_cStartingColour, m_cEndColour, m_ttTimer.GetCompletionPercentage());
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
			// If timer is finished. Finish Animation
			if (m_ttTimer.Update(fAnimationSpeed))
			{
				UpdateColour(m_cStartingColour, m_cStartingColour, 1.0f);
			}

			// Otherwise Update it according to Timer Completion Percentage
			else
			{
				UpdateColour(m_cEndColour, m_cStartingColour, m_ttTimer.GetCompletionPercentage());
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
		UpdateColour(m_cStartingColour, m_cEndColour, t);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Colour
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateColour(Color from, Color to, float t)
	{
		if (m_rTarget != null)
			CurrentColour = Color.Lerp(from, to, t);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Is Completed?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool IsCompleted()
	{
		return m_ttTimer.TimeUp();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Reset
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void Reset()
	{
		if (m_ttTimer != null)
		{
			m_ttTimer.Reset();
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
		UpdateColour(m_cStartingColour, m_cStartingColour, 1.0f);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show Last Frame
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ShowLastFrame()
	{
#if UNITY_EDITOR
		Reset();
#endif
		UpdateColour(m_cEndColour, m_cEndColour, 1.0f);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Force Finish
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ForceFinish(bool reversedFinish = false)
	{
		m_ttTimer.ForceTimerFinish();
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
	public ColourAnimationEffect Clone()
	{
		ColourAnimationEffect AE	= new ColourAnimationEffect();
		AE.m_fTotalAnimationTime	= this.m_fTotalAnimationTime;
		AE.m_cStartingColour		= this.m_cStartingColour;
		AE.m_cEndColour				= this.m_cEndColour;
		if (this.m_ttTimer != null)
		{
			AE.m_ttTimer			= new TimeTracker(this.m_ttTimer.FinishTime);
		}

#if UNITY_EDITOR
		AE.m_sEffectName			= this.m_sEffectName;
#endif

		return AE;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Resize Array
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public static ColourAnimationEffect[] ResizeArray(ColourAnimationEffect[] Original, int NewSize)
	{
		ColourAnimationEffect[] aeNewArray = new ColourAnimationEffect[NewSize];
		for (int i = 0; i < aeNewArray.Length; ++i)
		{
			if (i < Original.Length)
			{
				aeNewArray[i] = Original[i].Clone();
			}
			else
			{
				aeNewArray[i] = new ColourAnimationEffect();
			}
		}
		return aeNewArray;
	}
}
