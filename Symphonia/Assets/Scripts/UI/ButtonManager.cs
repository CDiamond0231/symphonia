//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Button Manager
//             Author: Christopher Allport
//             Date: January 09, 2016
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script handles all of the input updates for all buttons in the game.
//		In this way, only one script is attempting to raycast input and map to
//		available buttons which will help to reduce input lag.
//
//	  Additionally, as a manager, any script wishing to make changes to the
//		button system must do so through this script/class.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections.Generic;




//========================================================
// ***				ButtonManager
//========================================================
public partial class ButtonManager : MonoBehaviour
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*- Private Static Variables
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static LinkedList<Button_Base> sm_lTitleButtonsList = new LinkedList<Button_Base>();
    private static LinkedList<Button_Base> sm_lGameButtonsList = new LinkedList<Button_Base>();
    private static LinkedList<Button_Base> sm_lSettingsButtonsList = new LinkedList<Button_Base>();
    private static LinkedList<Button_Base> sm_lMenuButtonsList = new LinkedList<Button_Base>();
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Static Attr_Accessors
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static bool ButtonsLocked { get; private set; }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*- Private Instance Variables
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private InputList m_lPreviousInput = new InputList();
    private InputList m_lCurrentInput = new InputList();
    private Transform m_tPreviousObjectHoveredOver = null;
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*= Debug Variables
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#if UNITY_EDITOR
    public static LinkedList<Transform> dm_lTriggeredObjects = new LinkedList<Transform>(); // Shows up in the CustomButtonManagerInspector which Transforms have been pressed by the User.
    public static LinkedList<string> dm_lToggleStackTrace = new LinkedList<string>();   // Shows up in the CustomButtonManagerInspector the StackTrace for calling the "ToggleAllButtons" Method.
#endif
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

    public enum ButtonState
    {
        TRIGGERED,
        CLICKED,
        RELEASED,
        NO_CHANGE,
    }



    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Derived Method: Awake
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        sm_lTitleButtonsList.Clear();
        sm_lGameButtonsList.Clear();
        sm_lSettingsButtonsList.Clear();
        sm_lMenuButtonsList.Clear();
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Derived Method: Update
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Update()
    {
        m_lPreviousInput = m_lCurrentInput;
        m_lCurrentInput = new InputList();
        LinkedList<FrameInputInfo> lInputInfo = GetUpdatedInputInfo();

        foreach (FrameInputInfo input in lInputInfo)
        {
            if (input.hitObject != null)
            {
                ActiveInputButtons activeButton = m_lCurrentInput.GetOrNew(input.hitObject);
                activeButton.touchIDs.AddLast(input.inputID);
                if (input.triggered)
                    activeButton.inputTriggered = true;
            }
        }

        UpdateReleasedButtons();
        UpdateCurrentlyActiveButtons();
        UpdateHoveredObject();
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: Update Released Buttons
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void UpdateReleasedButtons()
    {
        foreach (ActiveInputButtons previousFrameButton in m_lPreviousInput)
        {
            if (m_lCurrentInput.GetExisting(previousFrameButton.buttonObject) == null)
            {
                Button_Base[] releasedButtons = previousFrameButton.buttonObject.GetComponents<Button_Base>();
                for (int i = 0; i < releasedButtons.Length; ++i)
                {
                    if (releasedButtons[i].enabled)
                    {
                        if (releasedButtons[i].KeepPressedEvenWithNoContact)
                        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                            if (Input.GetMouseButton(previousFrameButton.touchIDs.First.Value))
#else
							if(Input.touchCount > previousFrameButton.touchIDs.First.Value)
#endif
                            {
                                previousFrameButton.inputTriggered = false;
                                m_lCurrentInput.AddLast(previousFrameButton);
                            }
                            else
                            {
                                releasedButtons[i].PushInputSystem(null, ButtonState.RELEASED);
                            }
                        }
                        else
                        {
                            releasedButtons[i].PushInputSystem(null, ButtonState.RELEASED);
                        }
                    }
                }
            }
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: Update Currently Active Buttons
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void UpdateCurrentlyActiveButtons()
    {
        foreach (ActiveInputButtons activeInput in m_lCurrentInput)
        {
            Button_Base[] activeButtons = activeInput.buttonObject.GetComponents<Button_Base>();
            for (int i = 0; i < activeButtons.Length; ++i)
            {
                if (activeButtons[i].enabled)
                {
                    ButtonState eButtonState = ButtonState.NO_CHANGE;
                    if (activeInput.inputTriggered)
                    {
                        eButtonState = ButtonState.TRIGGERED;
                    }
                    else
                    {
                        ActiveInputButtons previousFrameInfo = m_lPreviousInput.GetExisting(activeInput.buttonObject);
                        if (previousFrameInfo != null)
                        {
                            if (previousFrameInfo.touchIDs.Count < activeInput.touchIDs.Count)
                                eButtonState = ButtonState.CLICKED;
                        }
                        else
                        {
                            eButtonState = ButtonState.CLICKED;
                        }
                    }

                    activeButtons[i].PushInputSystem(activeInput.touchIDs, eButtonState);
                }
            }
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: Update Object Mouse Is Hovering Over
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void UpdateHoveredObject()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Transform hitObject = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).transform;
        if (hitObject != null && m_tPreviousObjectHoveredOver != hitObject)
        {
            ActiveInputButtons currentFrameInfo = m_lCurrentInput.GetExisting(hitObject);
            if (currentFrameInfo == null || currentFrameInfo.touchIDs.Count == 0)
            {
                // Can hover. Otherwise if Touch Count is greater than zero, it means this button is clicked. In which case we shouldn't say it is being hovered over.
                Button_Base[] activeButtons = hitObject.GetComponents<Button_Base>();
                for (int i = 0; i < activeButtons.Length; ++i)
                {
                    activeButtons[i].OnHover();
                }

                m_tPreviousObjectHoveredOver = hitObject;
            }
        }

        if (m_tPreviousObjectHoveredOver != null && m_tPreviousObjectHoveredOver != hitObject)
        {
            Button_Base[] activeButtons = m_tPreviousObjectHoveredOver.GetComponents<Button_Base>();
            for (int i = 0; i < activeButtons.Length; ++i)
            {
                activeButtons[i].OnHoverRemoved();
            }

            m_tPreviousObjectHoveredOver = hitObject;
        }
#endif
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: Get Updated Input Info
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private LinkedList<FrameInputInfo> GetUpdatedInputInfo()
    {
        LinkedList<FrameInputInfo> lInputList = new LinkedList<FrameInputInfo>();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        for (int i = 0; i < 2; ++i)
        {
            if (Input.GetMouseButton(i))
            {
                FrameInputInfo info = new FrameInputInfo();
                info.hitObject = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).transform;
                info.inputID = i;
                info.triggered = Input.GetMouseButtonDown(i);
                lInputList.AddLast(info);

#if UNITY_EDITOR
                // DEBUG ONLY~ Update InputTrigger List. In the CustomEditor This will tell the developer what has most recently been touched.
                //	=> Additionally, whenever the List gets above 100, reduce it back down to 100 (or below) by chopping off the first value in the list.
                if (info.triggered && info.hitObject != null)
                {
                    while (dm_lTriggeredObjects.Count > 99)
                    {
                        dm_lTriggeredObjects.RemoveFirst();
                    }
                    dm_lTriggeredObjects.AddLast(info.hitObject);
                }
#endif
            }
        }
#else
		for (int i = 0; i < Input.touchCount; ++i)
		{
			FrameInputInfo info	= new FrameInputInfo();
			info.hitObject		= Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), Vector2.zero).transform;
			info.inputID		= i;
			info.triggered		= Input.GetTouch(i).phase == TouchPhase.Began;
			lInputList.AddLast(info);
		}
#endif
        return lInputList;
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Assign Button
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void AssignButton(ButtonType eButtonType, Button_Base button)
    {
        if (button != null)
        {
            bool bDeactivatebutton = false;
            switch (eButtonType)
            {
                case ButtonType.TITLE: { sm_lTitleButtonsList.AddLast(button); bDeactivatebutton = !sm_lTitleButtonsList.First.Value.enabled; break; }
                case ButtonType.GAME: { sm_lGameButtonsList.AddLast(button); bDeactivatebutton = !sm_lGameButtonsList.First.Value.enabled; break; }
                case ButtonType.MENU: { sm_lMenuButtonsList.AddLast(button); bDeactivatebutton = !sm_lMenuButtonsList.First.Value.enabled; break; }
                default: { sm_lSettingsButtonsList.AddLast(button); bDeactivatebutton = !sm_lSettingsButtonsList.First.Value.enabled; break; }
            }

            // A button may become enabled for the first time after the ButtonManager has locked all ButtonInputs. If this is the case, disable the newly enabled button!
            if (bDeactivatebutton)
                button.GetType().GetMethod("OnButtonDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(button, new object[] { true });
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Lock Buttons
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// <summary>
    /// This method locks all buttons in their current state. This means that scripts cannot Toggle all buttons to be active/inactive, etc.
    /// It's primary purpose is to lock player input during the tutorial.
    /// </summary>
    public static void LockButtons()
    {
        ButtonsLocked = true;
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Unlock Buttons
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// <summary>
    /// This method unlocks all buttons and allows their activity to be modified.
    /// It's primary purpose is to unlock player input after completing the tutorial.
    /// </summary>
    public static void UnlockButtons()
    {
        ButtonsLocked = false;
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Methods: Toggle All Buttons
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static void ToggleAllButtons(LinkedList<Button_Base> lButtons, bool enabled)
    {
        if (!ButtonsLocked)
        {
            if (enabled)
            {
                foreach (Button_Base button in lButtons)
                {
                    if (button != null)
                    {
                        button.GetType().GetMethod("OnButtonEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).
                                            Invoke(button, new object[] { false });
                    }
                }
            }
            else
            {
                foreach (Button_Base button in lButtons)
                {
                    if (button != null)
                    {
                        button.GetType().GetMethod("OnButtonDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).
                                            Invoke(button, new object[] { false });
                    }
                }
            }


            // ~~~~~ UNIT TESTING: Find out what has been screwing with the Button System! ~~~~~~
#if UNITY_EDITOR
            while (dm_lToggleStackTrace.Count > 99)
            {
                dm_lToggleStackTrace.RemoveFirst();
            }
            dm_lToggleStackTrace.AddLast(StackTraceUtility.ExtractStackTrace());
#endif
        }
    }

    public static void ToggleAllButtonsExcept(ButtonType eButtonType, bool enabled)
    {
        ToggleAllButtons(sm_lTitleButtonsList, (eButtonType == ButtonType.TITLE ? !enabled : enabled));
        ToggleAllButtons(sm_lGameButtonsList, (eButtonType == ButtonType.GAME ? !enabled : enabled));
        ToggleAllButtons(sm_lSettingsButtonsList, (eButtonType == ButtonType.SETTINGS ? !enabled : enabled));
        ToggleAllButtons(sm_lMenuButtonsList, (eButtonType == ButtonType.MENU ? !enabled : enabled));
    }

    public static void ToggleAllButtons(ButtonType eButtonType, bool enabled)
    {
        switch (eButtonType)
        {
            case ButtonType.TITLE: { ToggleAllButtons(sm_lTitleButtonsList, enabled); break; }
            case ButtonType.GAME: { ToggleAllButtons(sm_lGameButtonsList, enabled); break; }
            case ButtonType.MENU: { ToggleAllButtons(sm_lMenuButtonsList, enabled); break; }
            default:    /*SYSTEM:*/	{ ToggleAllButtons(sm_lSettingsButtonsList, enabled); break; }
        }
    }

    public static void ToggleAllButtons(bool enabled)
    {
        ToggleAllButtons(sm_lTitleButtonsList, enabled);
        ToggleAllButtons(sm_lGameButtonsList, enabled);
        ToggleAllButtons(sm_lMenuButtonsList, enabled);
        ToggleAllButtons(sm_lSettingsButtonsList, enabled);
    }
}


//========================================================
// ***				   InputList
//========================================================
public partial class ButtonManager
{
    private class InputList : LinkedList<ActiveInputButtons>
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Contains (Specific Transform)?
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Checks if this InputList contains a specific Transform?
        /// </summary>
        /// <param name="value">The Transform that is to be checked for</param>
        /// <returns></returns>
        public bool Contains(Transform value)
        {
            foreach (ActiveInputButtons button in this)
                if (button.buttonObject == value)
                    return true;
            return false;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Get Existing (Transform)
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Returns an instance of "ActiveInputButons" found in the LinkedList that
        /// contains the specified Transform. If none are found, returns null.
        /// </summary>
        /// <param name="value">The Transform that is to be checked for</param>
        /// <returns></returns>
        public ActiveInputButtons GetExisting(Transform value)
        {
            foreach (ActiveInputButtons button in this)
                if (button.buttonObject == value)
                    return button;
            return null;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Get Existing or New (Transform)
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Returns an instance of "ActiveInputButtons" found in the LinkedList that
        /// contains the specified Transform. If no instances are found, creates a new
        ///	one and add it to the end of the LinkedList.
        /// </summary>
        /// <param name="value">The Transform that is to be checked for</param>
        /// <returns></returns>
        public ActiveInputButtons GetOrNew(Transform value)
        {
            // Get if in list already
            foreach (ActiveInputButtons button in this)
                if (button.buttonObject == value)
                    return button;

            // Or Create New
            ActiveInputButtons newButton = new ActiveInputButtons();
            newButton.buttonObject = value;
            this.AddLast(newButton);
            return newButton;
        }
    }
}



//========================================================
// ***				  FrameInputInfo
//========================================================
public partial class ButtonManager
{
    private struct FrameInputInfo
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	*+ Public Instance Variables
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public Transform hitObject;
        public int inputID;
        public bool triggered;
    }
}



//========================================================
// ***				ActiveInputButtons
//========================================================
public partial class ButtonManager
{
    private class ActiveInputButtons
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	*+ Public Instance Variables
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public Transform buttonObject;
        public LinkedList<int> touchIDs = new LinkedList<int>();
        public bool inputTriggered = false;
    }
}