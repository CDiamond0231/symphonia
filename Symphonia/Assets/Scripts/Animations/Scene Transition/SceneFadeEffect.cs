//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Scene Fade Effect
//             Version: 1.0
//             Author: Christopher Allport
//             Date: September 18, 2014
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script is used to fadein/fadeout a scene. It goes through all available
//	  sprite renderers on initialisation then fades out/in all of those sprites
//	  when the appropriate function is called.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections.Generic;
using System;

public class SceneFadeEffect : MonoBehaviour
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Public Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public float m_fTransitionTime = 3.0f;
	public AudioSource m_asSoundPlayer;

#if UNITY_EDITOR
	public SceneFadeEffect m_rConnectingScene;	// Scene we are connected to!
#endif
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*- Private Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private TimeTracker m_ttTransitionTimer;
    private List<FadeTarget<SpriteRenderer>> m_lSprRends = new List<FadeTarget<SpriteRenderer>>();
    private List<FadeTarget<UnityEngine.UI.Text>> m_lTextRends = new List<FadeTarget<UnityEngine.UI.Text>>();
	private List<FadeTarget<UnityEngine.UI.Image>> m_lImgRends = new List<FadeTarget<UnityEngine.UI.Image>>();
	private TransitionState m_eTransitionState = TransitionState.IDLE;
    private OnFadeCompleted m_onFadeCompleted = null;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Reader
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool IsCompleted { get; private set; }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*{} Class Declarations
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public delegate void OnFadeCompleted(TransitionState fadeType);

	public enum TransitionState
	{
		IDLE,
		FADEIN,
		FADEOUT,
	}

    private class FadeTarget<T>
    {
        public T target;
        public float originalOpacity;
    }


	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Awake
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	void Awake()
	{
		IsCompleted = true;

		m_ttTransitionTimer = new TimeTracker(m_fTransitionTime);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	void Update()
	{
		if (m_eTransitionState != TransitionState.IDLE)
		{
			UpdateFadeEffect();
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Fade Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateFadeEffect()
	{
		// If the fadein timer has completed (or I got impatient on the PC); Show the Scene Fully Opaque / Fully Transparent
#if UNITY_EDITOR
		if (m_ttTransitionTimer.Update() || Input.GetKeyDown(KeyCode.Space))
#else
		if(m_ttTransitionTimer.Update())
#endif
		{
			if (m_eTransitionState == TransitionState.FADEIN)
			{
				OnFadeIn();
			}
			else
			{
				OnFadeout();
			}
			m_eTransitionState = TransitionState.IDLE;
		}

		// Otherwise Fade in/out scene according to the completion percentage of the timer.
		else
		{
			float t = Mathf.Lerp(0.0f, 1.0f, (m_eTransitionState == TransitionState.FADEIN ? m_ttTransitionTimer.GetCompletionPercentage() : 1.0f - m_ttTransitionTimer.GetCompletionPercentage()));
            SetFade(t);
        }
	}
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: Set Fade
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void SetFade(float t)
    {
        foreach (FadeTarget<SpriteRenderer> sr in m_lSprRends)
        {
            // Make Sure the Sprite Renderer is still valid
            if (sr != null)
            {
                Color colour = sr.target.color;
                float targetOpacity = sr.originalOpacity == 0.0f ? 1.0f : sr.originalOpacity;
                colour.a = Mathf.Lerp(0.0f, targetOpacity, t);
                sr.target.color = colour;
            }
        }
        foreach (FadeTarget<UnityEngine.UI.Text> tr in m_lTextRends)
        {
            if (tr != null)
            {
                Color colour = tr.target.color;
                float targetOpacity = tr.originalOpacity == 0.0f ? 1.0f : tr.originalOpacity;
                colour.a = Mathf.Lerp(0.0f, targetOpacity, t);
                tr.target.color = colour;
            }
        }
        foreach (FadeTarget<UnityEngine.UI.Image> ir in m_lImgRends)
        {
            if (ir != null)
            {
                Color colour = ir.target.color;
                float targetOpacity = ir.originalOpacity == 0.0f ? 1.0f : ir.originalOpacity;
                colour.a = Mathf.Lerp(0.0f, targetOpacity, t);
                ir.target.color = colour;
            }
        }

        // If there is a Sound Player for BGM, fade in/out the audio volume also
        if (m_asSoundPlayer != null)
        {
            m_asSoundPlayer.volume = Mathf.Lerp(0.0f, 1.0f, t);
        }
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Cycle Through Children (Get Sprite Renderers)
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void CycleThroughChildren(Transform Parent)
	{
		// Recursively go through ALL children in the Scene
		foreach (Transform child in Parent)
		{
			CycleThroughChildren(child);
		}

		// If this particular object (Remember it's a recursive function) has a sprite renderer, add it to the sprite renderer list for Fadeout/Fadein.
		SpriteRenderer sr = Parent.GetComponent<SpriteRenderer>();
		if (sr != null)
		{
            FadeTarget<SpriteRenderer> targetInfo = new FadeTarget<SpriteRenderer>();
            targetInfo.target = sr;
            targetInfo.originalOpacity = targetInfo.target.color.a;

            m_lSprRends.Add(targetInfo);
		}

		UnityEngine.UI.Text tr = Parent.GetComponent<UnityEngine.UI.Text>();
		if (tr != null)
		{
            FadeTarget<UnityEngine.UI.Text> targetInfo = new FadeTarget<UnityEngine.UI.Text>();
            targetInfo.target = tr;
            targetInfo.originalOpacity = targetInfo.target.color.a;

            m_lTextRends.Add(targetInfo);
		}

		UnityEngine.UI.Image ir = Parent.GetComponent<UnityEngine.UI.Image>();
		if (ir != null)
		{
            FadeTarget<UnityEngine.UI.Image> targetInfo = new FadeTarget<UnityEngine.UI.Image>();
            targetInfo.target = ir;
            targetInfo.originalOpacity = targetInfo.target.color.a;

            m_lImgRends.Add(targetInfo);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Initiate FadeIn
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void InitiateFadeIn(bool enableObject = true, OnFadeCompleted whenFinishedCallback = null)
	{
        CycleThroughChildren(transform);

		m_eTransitionState = TransitionState.FADEIN;
		ButtonManager.ToggleAllButtons(false);
		IsCompleted = false;

        if (whenFinishedCallback != null)
        {
            m_onFadeCompleted = whenFinishedCallback;
        }

        if (enableObject)
        {
            gameObject.SetActive(true);
            SetFade(0.0f);
        }
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Initiate FadeOut
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void InitiateFadeOut(bool disableObject = true, OnFadeCompleted whenFinishedCallback = null)
	{
        CycleThroughChildren(transform);

        m_eTransitionState = TransitionState.FADEOUT;
		ButtonManager.ToggleAllButtons(false);
		IsCompleted = false;

        if (whenFinishedCallback != null)
        {
            m_onFadeCompleted = whenFinishedCallback;
        }
        if (disableObject)
        {
            m_onFadeCompleted += DisableFadedOutObject;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: On FadeIn
    //---------------------------------------------------
    //	: Is also called by the Unity Inspector Button
    //	  for fading in a scene.
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void OnFadeIn()
    {
#if UNITY_EDITOR
        if (m_lSprRends.Count == 0 || m_ttTransitionTimer == null)
            Awake(); // In case we call this from the Editor whilst creating the game; We must make sure that the start function has done its job. This isn't a concern when the game is released (or not in Debug Mode)
#endif

        // We've completed, so make sure everyone knows about it. Oh and Giving the Player the ability to press things might be a good idea too...
        IsCompleted = true;
        ButtonManager.ToggleAllButtons(true);

        // Reset Fade Effect
        m_ttTransitionTimer.Reset();
        m_eTransitionState = TransitionState.IDLE;

        // Show all SpriteRenders with Full Opacity
        foreach (FadeTarget<SpriteRenderer> sr in m_lSprRends)
        {
            // Make Sure the Sprite Renderer is still valid
            if (sr != null)
            {
                Color colour = sr.target.color;
                colour.a = sr.originalOpacity == 0.0f ? 1.0f : sr.originalOpacity;
                sr.target.color = colour;
            }
        }
        foreach (FadeTarget<UnityEngine.UI.Text> tr in m_lTextRends)
        {
            if (tr != null)
            {
                Color colour = tr.target.color;
                colour.a = tr.originalOpacity == 0.0f ? 1.0f : tr.originalOpacity;
                tr.target.color = colour;
            }
        }
        foreach (FadeTarget<UnityEngine.UI.Image> ir in m_lImgRends)
        {
            if (ir != null)
            {
                Color colour = ir.target.color;
                colour.a = ir.originalOpacity == 0.0f ? 1.0f : ir.originalOpacity;
                ir.target.color = colour;
            }
        }

        // And AudioPlayer as well (Full Volume)
        if (m_asSoundPlayer != null)
        {
            m_asSoundPlayer.volume = 1.0f;
        }

        if (m_onFadeCompleted != null)
        {
            m_onFadeCompleted.Invoke(TransitionState.FADEIN);
            m_onFadeCompleted = null;
        }
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Fadeout
	//---------------------------------------------------
	//	: Is also called by the Unity Inspector Button
	//	  for fading out a scene.
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void OnFadeout()
	{
#if UNITY_EDITOR
		if (m_lSprRends.Count == 0 || m_ttTransitionTimer == null)
			Awake();    // In case we call this from the Editor whilst creating the game; We must make sure that the start function has done its job. This isn't a concern when the game is released (or not in Debug Mode)
#endif

		// We've completed, so make sure everyone knows about it. Oh and Giving the Player the ability to press things might be a good idea too...
		IsCompleted = true;
		ButtonManager.ToggleAllButtons(true);

		// Reset Fade Effect
		m_ttTransitionTimer.Reset();
		m_eTransitionState = TransitionState.IDLE;

		// Show all SpriteRenders with Full Transparency
		foreach (FadeTarget<SpriteRenderer> sr in m_lSprRends)
		{
			// Make Sure the Sprite Renderer is still valid
			if (sr != null)
			{
				Color colour = sr.target.color;
				colour.a = sr.originalOpacity;
				sr.target.color = colour;
			}
		}
		foreach (FadeTarget<UnityEngine.UI.Text> tr in m_lTextRends)
		{
			if (tr != null)
			{
				Color colour = tr.target.color;
				colour.a = tr.originalOpacity;
				tr.target.color = colour;
			}
		}
		foreach (FadeTarget<UnityEngine.UI.Image> ir in m_lImgRends)
		{
			if (ir != null)
			{
				Color colour = ir.target.color;
				colour.a = ir.originalOpacity;
				ir.target.color = colour;
			}
		}

		// And AudioPlayer as well (Muted)
		if (m_asSoundPlayer != null)
		{
			m_asSoundPlayer.volume = 0.0f;
		}

		gameObject.SetActive(false);

        if (m_onFadeCompleted != null)
        {
            m_onFadeCompleted.Invoke(TransitionState.FADEOUT);
            m_onFadeCompleted = null;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Method: Disable Faded Out Object
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void DisableFadedOutObject(TransitionState fadeType)
    {
        gameObject.SetActive(false);
    }
}
