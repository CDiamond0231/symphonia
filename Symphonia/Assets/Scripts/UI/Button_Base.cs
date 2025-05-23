//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Button Base
//             Version: 1.0
//             Author: Christopher Allport
//             Date: September 4, 2014
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script is used as a base class to all the various button types used in 
//      the app. This includes generic buttons for opening up items and other button
//      types such as the ones which cause music to be played.
//
//    In addition this script comes equipped with an identifier for different 
//      button types. Which means that you can deactivate the usage of certain 
//      button types in favour of others. 
//          EG. When in a menu, you can disable all button types besides the menu
//              buttons, so as to stop input into other areas besides the menu. 
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Button_Base : MonoBehaviour
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Public Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public Sprite m_sprUnPressedSprite							= null;
	public Sprite m_sprPressedSprite							= null;
	public SpriteRenderer m_srButtonRenderer					= null;
	public AudioClip m_acClipToPlay								= null;
	public ButtonManager.ButtonType m_eButtonType				= ButtonManager.ButtonType.GAME;
    public Color m_cTextColourWhenHoveredOver                   = new Color(0.5f, 0.5f, 0.5f);
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*. Protected Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected bool m_bIsPressed									= false;
    protected Color m_cDefaultTexColour;
	
	protected LinkedList<int> m_lCurrentTouchIDs				= new LinkedList<int>();		// List of Input IDs which are currently touching this Button. Only Used if MultiTouch Button
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*{} Class Declarations
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public enum ButtonType
	{
		TITLE,
		GAME,
		SETTINGS,
		MENU,
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Readers
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected bool Triggered
	{
		get
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			return Input.GetMouseButtonDown(0);
#else
			return Input.GetTouch(0).phase == TouchPhase.Began;
#endif
		}
	}

	protected Vector3 TouchPosition
	{
		get
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			return Camera.main.ScreenToWorldPoint(Input.mousePosition);
#else
			return Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
#endif
		}
	}

	public SpriteRenderer SprRenderer 
	{ 
		get
		{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			return m_srButtonRenderer == null ? gameObject.GetComponent<SpriteRenderer>() : m_srButtonRenderer; 
#else
			return m_srButtonRenderer; 
#endif
		} 
	}

	// Used to inform ButtonManager to Keep Button atively watched, even if input has been taken away from button. ie. Finger can touch button; move off from butotn; but still act like it is touching;
	//		Best used for Button_Sliders
	public bool KeepPressedEvenWithNoContact
	{
		get;
		protected set;
	}

	public UnityEngine.UI.Text TextRenderer
	{
		get
        {
            UnityEngine.UI.Text txtRend = GetComponentInChildren<UnityEngine.UI.Text>(true);
            return txtRend;
        }
	}

	public Collider2D ButtonCollider
	{
		get { return this.gameObject.GetComponent<Collider2D>(); }
	}

	public string TextLabel
	{
		get
		{
			UnityEngine.UI.Text textComponent = TextRenderer;
			return (textComponent != null ? textComponent.name : "N/A");
		}
		set
		{
			UnityEngine.UI.Text textComponent = TextRenderer;
			if (textComponent != null)
				textComponent.text = value;
		}
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Awake
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void Awake()
	{
        if (m_srButtonRenderer == null)
			m_srButtonRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (TextRenderer != null)
            m_cDefaultTexColour = TextRenderer.color;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Start
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void Start()
	{
		ButtonManager.AssignButton(m_eButtonType, this);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void Update()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Push Input System
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	/// <summary>
	/// This method is called from the ButtonManager. It will provide information
	/// about all input IDs which have touched the Button on this frame. Effectively
	/// updating input on the button,
	/// </summary>
	/// <param name="lInputIDs">LinkedList of all Inputs which are currently pressing down on this button on this frame. If null: Button has just been released</param>
	/// <param name="eButtonState">Current Button State, depending on the State the button may call the "OnTrigger", "OnClick", "OnHeld", or "OnRelease" Methods</param>
	public void PushInputSystem(LinkedList<int> lInputIDs, ButtonManager.ButtonState eButtonState)
	{
		switch(eButtonState)
		{
			case ButtonManager.ButtonState.TRIGGERED:
			{
				if(m_bIsPressed)
					OnAdditionalTrigger();
				else
					OnTrigger();
				break;
			}

			case ButtonManager.ButtonState.CLICKED:
			{
				if(m_bIsPressed)
					OnAdditionalTouch();
				else
					OnTouch();
				break;
			}

			case ButtonManager.ButtonState.RELEASED:
			{
				OnRelease();
				break;
			}

			default:
			{
				if(m_bIsPressed)
					OnHeld();
				break;
			}
		}
		m_lCurrentTouchIDs = lInputIDs;
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Force Click
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ForceClick()
	{
		OnTrigger();
	}
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: On Hover
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public virtual void OnHover()
    {
        if (TextRenderer != null)
        {
            Color colour = m_cTextColourWhenHoveredOver;
            colour.a = TextRenderer.color.a;
            TextRenderer.color = colour;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: On Hover Removed
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public virtual void OnHoverRemoved()
    {
        if (TextRenderer != null)
        {
            TextRenderer.color = m_cDefaultTexColour;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: On Trigger
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    protected virtual void OnTrigger()
	{
		OnTouch();
		PlayButtonSound();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Additional Trigger
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// <summary>
	/// Called if a MultiTouch Button has been pressed by a new InputID whilst already have been
	///  clicked/held by another InputID. i.e. The user is touching this button with two fingers.
    ///   This would be the trigger for any additional touches after the 'initial' one.
    /// </summary>
	protected virtual void OnAdditionalTrigger()
	{
		OnAdditionalTouch();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Touch
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void OnTouch()
	{
		m_bIsPressed = true;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Additional Touch
	//----------------------------------------------------
	// Called if a MultiTouch Button has been pressed by 
	//	a different InputID whilst already have been 
	//  clicked/held by another InputID.
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void OnAdditionalTouch()
	{
		OnTouch();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Held
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void OnHeld()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Release
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void OnRelease()
	{
		m_bIsPressed = false;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Play Button Sound
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public virtual void PlayButtonSound()
	{
		if (m_acClipToPlay != null)
		{
			UnityAudioSourceManager.PlayAudioClip(m_acClipToPlay);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show Unpressed Sprite
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void ShowUnpressedSprite()
	{
		if (m_sprUnPressedSprite != null && SprRenderer != null)
		{
			SprRenderer.sprite = m_sprUnPressedSprite;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Show Pressed Sprite
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void ShowPressedSprite()
	{
		if (m_sprPressedSprite != null && SprRenderer != null)
		{
			SprRenderer.sprite = m_sprPressedSprite;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Activate Button
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ActivateButton()
	{
		OnButtonEnable(true);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Deactivate Button
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void DeactivateButton()
	{
		OnButtonDisable(true);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: On Enable
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void OnEnable()
	{
		OnButtonEnable(false);
	}

	protected virtual void OnButtonEnable(bool forceEvenIfLocked = false)
	{
        bool bAllowed = true;// (!ButtonManager.ButtonsLocked || forceEvenIfLocked);
		if(bAllowed)
		{
			this.enabled = true;
			m_bIsPressed = false;
			ShowUnpressedSprite();
			if(ButtonCollider != null)
				ButtonCollider.enabled = true;
		}
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: On Disable
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void OnDisable()
	{
		OnButtonDisable(false);
	}

	protected virtual void OnButtonDisable(bool forceEvenIfLocked = false)
	{
        bool bAllowed = true;// (!ButtonManager.ButtonsLocked || forceEvenIfLocked);
		if(bAllowed)
		{
			this.enabled = false;
			m_bIsPressed = false;
			if(m_bIsPressed)
			{
				OnRelease();
			}


			// See if we can disable collider. Saves processing time on checking RayCasts the less colliders that are available
			//		=> If this button is sharing a GameObject with other buttons. Make sure those other buttons are disabled before turning off the shared collider
			if(ButtonCollider != null)
			{
				Button_Base[] sharedButtons = this.gameObject.GetComponents<Button_Base>();
				bool canDisableCollider = true;
				for(int i = 0; i < sharedButtons.Length; ++i)
				{
					if(sharedButtons[i].enabled)
					{
						canDisableCollider = false;
						break;
					}
				}
				if(canDisableCollider)
					ButtonCollider.enabled = false;
			}
		}
	}
}
