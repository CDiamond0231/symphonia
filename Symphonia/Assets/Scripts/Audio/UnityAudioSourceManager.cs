//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Audio Source Manager
//             Author: Christopher Allport
//             Date: December 11, 2014
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script manages all Audio Output for the App; including SFX and BGM.
//		Any script wishing to change Audio in some way should go through this
//		script; whether that's for fading, stopping or starting any audio.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections.Generic;



//========================================================
// ***				AudioSourceManager
//========================================================
public partial class UnityAudioSourceManager : MonoBehaviour
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*+ Public Instance Variables
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public int m_iTotalAudioSourcesCount = 10;
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*- Private Variables
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static int sm_iCurrentAudioSource = 0;
    private static bool sm_bMuteBGM = false;
    private static UnityAudioSourceManager sm_iInstance;
    private static List<AudioHandlerManager> sm_aAudioSource = new List<AudioHandlerManager>();
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*+ Attr_Readers
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static bool MuteBGMModified
    {
        get;
        private set;
    }

    public static bool MuteBGM
    {
        get
        {
            return sm_bMuteBGM;
        }
        set
        {
            if (sm_bMuteBGM != value)
            {
                sm_bMuteBGM = value;
                MuteBGMModified = true;
            }
        }
    }

    private static UnityAudioSourceManager Instance
    {
        get
        {
            return (sm_iInstance != null ? sm_iInstance : Camera.main.GetComponent<UnityAudioSourceManager>());
        }
        set
        {
            sm_iInstance = value;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*{} Class Declarations
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private enum FadeType
    {
        REGULAR_FADE,
        FADE_OUT,
        FADE_OUT_AND_BACK_IN,
        NO_FADE,
    }

    public enum AudioType
    {
        /// <summary>
        /// Background Music. Can be muted via user input if desired
        /// </summary>
        BGM,
        /// <summary>
        /// Sound Effect. Will not be muted. Best to be assigned to instrument or button sounds.
        /// </summary>
        SFX,
    }


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Derived Method: Start
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Start()
    {
        // Set DSP Buffer Size
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        AudioConfiguration audioSettings = AudioSettings.GetConfiguration();
		audioSettings.dspBufferSize /= 2;
		AudioSettings.Reset(audioSettings);
#endif
        Instance = this;

        // Grab all Audio Sources already existing on this object
        AudioSource[] acquiredAudioSources = this.gameObject.GetComponents<AudioSource>();
        if (acquiredAudioSources.Length > m_iTotalAudioSourcesCount)
        {
            m_iTotalAudioSourcesCount = acquiredAudioSources.Length;
        }

        // Add Existing To List
        for (int i = 0; i < acquiredAudioSources.Length; ++i)
        {
            sm_aAudioSource.Add(new AudioHandlerManager(acquiredAudioSources[i]));
        }

        // Create the rest of the AudioSources up until the desired amount exist. Or stop if we already have more than required.
        for (int i = sm_aAudioSource.Count; i < m_iTotalAudioSourcesCount; ++i)
        {
            sm_aAudioSource.Add(new AudioHandlerManager(CreateAudioSource()));
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* Derived Method: Update
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Update()
    {
        foreach (AudioHandlerManager handler in sm_aAudioSource)
        {
            handler.Update();
        }

        MuteBGMModified = false;
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Create Audio Source
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static AudioSource CreateAudioSource()
    {
        AudioSource newAS = Instance.gameObject.AddComponent<AudioSource>();
        newAS.clip = null;
        newAS.playOnAwake = false;
        newAS.loop = false;
        newAS.volume = 1.0f;
        return newAS;
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Get Available Audio Handler!
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static AudioHandlerManager GetAvailableAudioHandler()
    {
        // Check from Next Selected AudioSource until end of available list. Make sure the AudioSource's are not already playing something
        for (int i = sm_iCurrentAudioSource; i < sm_aAudioSource.Count; ++i)
        {
            if (sm_aAudioSource[i].IsAvailable)
            {
                sm_iCurrentAudioSource = i;
                return sm_aAudioSource[i];
            }
        }

        // Alright then. We'll start search from beginning of list until we hit where we started from...
        for (int i = 0; i < sm_iCurrentAudioSource; ++i)
        {
            if (sm_aAudioSource[i].IsAvailable)
            {
                sm_iCurrentAudioSource = i;
                return sm_aAudioSource[i];
            }
        }

        // Still no luck? ;_; Guess we'll just have to make a new one to take care of it
        sm_iCurrentAudioSource = sm_aAudioSource.Count;
        sm_aAudioSource.Add(new AudioHandlerManager(CreateAudioSource()));
        return sm_aAudioSource[sm_iCurrentAudioSource];
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Play Audio Clip
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// <summary>
    /// Will return the ID of the AudioSource being used to
    /// play the desired track/clip. Use this same ID to
    ///  stop playback.
    /// </summary>
    /// <param name="a_cClipToPlay">Which Audio Clip to play?</param>
    /// <param name="a_bLoop">Should the audio loop?</param>
    /// <param name="a_fVol">What volume should we play at?</param>
    /// <param name="a_eAudioType">Which AudioType is this? If BGM, sound may not actually play due to AudioSystem being muted.</param>
    /// <param name="a_iTrackStartPosition">Using PCM Samples... Where should the AudioTrack start from? If Unsure, leave as zero. Will play from Start of Audio Track</param>
    /// <returns>Audio ID informing which AudioHandler has been used. Keep hold of this Audio ID if you need to make modifications to the current track in future.</returns>
    public static int PlayAudioClip(AudioClip a_cClipToPlay, bool a_bLoop = false, float a_fVol = 1.0f, AudioType a_eAudioType = AudioType.SFX, int a_iTrackStartPosition = 0)
    {
        // Can't play non-existing audio
        if (a_cClipToPlay == null)
        {
            return -1;
        }

#if UNITY_EDITOR
        // Only in the Editor Can we somehow get here without AudioSources' assigned to the List. Make one if necessary
        if (sm_aAudioSource.Count == 0)
        {
            sm_aAudioSource.Add(new AudioHandlerManager(CreateAudioSource()));
        }
#endif

        // Increment which audioSource to use. Resetting if necessary. If selected AudioSource is busy, find a free one.
        sm_iCurrentAudioSource = (sm_iCurrentAudioSource + 1) % sm_aAudioSource.Count;
        AudioHandlerManager audHandler = sm_aAudioSource[sm_iCurrentAudioSource];
        if (audHandler == null || !audHandler.IsAvailable)
        {
            audHandler = GetAvailableAudioHandler();
        }

        // Play Audio!
        audHandler.PlaySound(a_cClipToPlay, a_bLoop, a_fVol, a_eAudioType, a_iTrackStartPosition);
        return sm_iCurrentAudioSource;
    }

    public static void PlayAudioClip(AudioHandlerInfo a_rAudioHandlerInfo)
    {
        int trackStartPosition;
        if (a_rAudioHandlerInfo.m_bRandomiseTrackStartPosition)
        {
            trackStartPosition = Random.Range(0, (a_rAudioHandlerInfo.m_acAudioToPlay.samples / a_rAudioHandlerInfo.m_acAudioToPlay.channels));
        }
        else
        {
            trackStartPosition = a_rAudioHandlerInfo.m_iStartTrackPosition;
        }

        int iAudioHandlerID = PlayAudioClip(a_rAudioHandlerInfo.m_acAudioToPlay, a_rAudioHandlerInfo.m_bLoopAudio, a_rAudioHandlerInfo.m_fMaxVolume, a_rAudioHandlerInfo.m_eAudioType, trackStartPosition);
        SetAudioHandler(a_rAudioHandlerInfo, iAudioHandlerID);
        if (IsValidAudioHandlerID(a_rAudioHandlerInfo.AudioHandlerID))
        {
            if (a_rAudioHandlerInfo.m_bFadeinAudioUponPlaying)
            {
                sm_aAudioSource[a_rAudioHandlerInfo.AudioHandlerID].Volume = 0.0f;
                FadeAudio(a_rAudioHandlerInfo);
            }
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Stop Audio Playback!
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void StopAudio(int a_iAudioHandlerID)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            sm_aAudioSource[a_iAudioHandlerID].StopSound();
        }
    }

    public static void StopAudio(AudioHandlerInfo a_rAudioHandlerInfo)
    {
        if (a_rAudioHandlerInfo != null && IsValidAudioHandlerID(a_rAudioHandlerInfo.AudioHandlerID))
        {
            sm_aAudioSource[a_rAudioHandlerInfo.AudioHandlerID].StopSound();
            SetAudioHandler(a_rAudioHandlerInfo, -1);
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Set Audio Handler ID
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static void SetAudioHandler(AudioHandlerInfo a_rAudioHandlerInfo, int a_iNewID)
    {
        a_rAudioHandlerInfo.GetType().GetField("m_iAudioHandlerID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(a_rAudioHandlerInfo, a_iNewID);
        if (a_iNewID > -1)
        {
            a_rAudioHandlerInfo.GetType().GetField("m_rAssignedAudioHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(a_rAudioHandlerInfo, sm_aAudioSource[a_iNewID]);
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Set Audio Volume
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void SetAudioVolume(int a_iAudioHandlerID, float a_fDesiredVolume)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            sm_aAudioSource[a_iAudioHandlerID].Volume = a_fDesiredVolume;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Set Audio Pitch
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void SetAudioPitch(int a_iAudioHandlerID, float a_fDesiredPitch)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            sm_aAudioSource[a_iAudioHandlerID].Pitch = a_fDesiredPitch;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Methods: Fade Audio
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void FadeAudio(AudioHandlerInfo a_rAudioHandlerInfo)
    {
        if (a_rAudioHandlerInfo != null && IsValidAudioHandlerID(a_rAudioHandlerInfo.AudioHandlerID))
        {
            FadeAudio(a_rAudioHandlerInfo.AudioHandlerID, sm_aAudioSource[a_rAudioHandlerInfo.AudioHandlerID].Volume, a_rAudioHandlerInfo.m_fMaxVolume, a_rAudioHandlerInfo.m_fFadeinAudioTime);
        }
    }

    public static void FadeAudio(int a_iAudioHandlerID, float a_fEndVolume, float a_fFadeTime)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            FadeAudio(a_iAudioHandlerID, sm_aAudioSource[a_iAudioHandlerID].Volume, a_fEndVolume, a_fFadeTime);
        }
    }

    public static void FadeAudio(int a_iAudioHandlerID, float a_fStartVolume, float a_fEndVolume, float a_fFadeTime)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            sm_aAudioSource[a_iAudioHandlerID].FadeAudio(a_fStartVolume, a_fEndVolume, a_fFadeTime, FadeType.REGULAR_FADE);
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Methods: Fadeout Audio
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void FadeoutAudio(AudioHandlerInfo a_rAudioHandlerInfo)
    {
        if (a_rAudioHandlerInfo != null && IsValidAudioHandlerID(a_rAudioHandlerInfo.AudioHandlerID))
        {
            FadeoutAudio(a_rAudioHandlerInfo.AudioHandlerID, sm_aAudioSource[a_rAudioHandlerInfo.AudioHandlerID].Volume, 0.0f, a_rAudioHandlerInfo.m_fFadeoutAudioTime);
            SetAudioHandler(a_rAudioHandlerInfo, -1);
        }
    }

    public static void FadeoutAudio(int a_iAudioHandlerID, float a_fFadeTime)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            FadeoutAudio(a_iAudioHandlerID, sm_aAudioSource[a_iAudioHandlerID].Volume, 0.0f, a_fFadeTime);
        }
    }

    public static void FadeoutAudio(int a_iAudioHandlerID, float a_fStartVolume, float a_fEndVolume, float a_fFadeTime)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            sm_aAudioSource[a_iAudioHandlerID].FadeAudio(a_fStartVolume, a_fEndVolume, a_fFadeTime, FadeType.FADE_OUT);
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Methods: Fadeout Audio THEN Fade Back In
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void FadeoutAudioThenFadeBackIn(AudioHandlerInfo a_rAudioHandlerInfo, float a_fFadeoutTime, float a_fFadeBackInWaitTime)
    {
        if (a_rAudioHandlerInfo != null && IsValidAudioHandlerID(a_rAudioHandlerInfo.AudioHandlerID))
        {
            FadeoutAudioThenFadeBackIn(a_rAudioHandlerInfo.AudioHandlerID, sm_aAudioSource[a_rAudioHandlerInfo.AudioHandlerID].Volume, a_rAudioHandlerInfo.m_fMaxVolume * 0.2f, a_rAudioHandlerInfo.m_fMaxVolume, a_fFadeoutTime, a_fFadeBackInWaitTime);
        }
    }

    public static void FadeoutAudioThenFadeBackIn(int a_iAudioHandlerID, float a_fEndVolume, float a_fFadeTime, float a_fFadeBackInWaitTime)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            FadeoutAudioThenFadeBackIn(a_iAudioHandlerID, sm_aAudioSource[a_iAudioHandlerID].Volume, a_fEndVolume, sm_aAudioSource[a_iAudioHandlerID].Volume, a_fFadeTime, a_fFadeBackInWaitTime);
        }
    }

    public static void FadeoutAudioThenFadeBackIn(int a_iAudioHandlerID, float a_fStartVolume, float a_fEndVolume, float a_fDefaultVolume, float a_fFadeTime, float a_fFadeBackInWaitTime)
    {
        if (IsValidAudioHandlerID(a_iAudioHandlerID))
        {
            sm_aAudioSource[a_iAudioHandlerID].FadeoutThenFadein(a_fStartVolume, a_fEndVolume, a_fDefaultVolume, a_fFadeTime, a_fFadeBackInWaitTime);
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Get Selected Audio Clip
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static AudioClip GetSelectedAudioClip(int a_iAudioHandlerID)
    {
        return (IsValidAudioHandlerID(a_iAudioHandlerID) ? sm_aAudioSource[a_iAudioHandlerID].Sound : null);
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Get Selected Audio Volume
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static float GetSelectedAudioVolume(int a_iAudioHandlerID)
    {
        return (IsValidAudioHandlerID(a_iAudioHandlerID) ? sm_aAudioSource[a_iAudioHandlerID].Volume : 0.0f);
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Methods: Is Playing Audio?
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static bool IsPlayingAudio(int a_iAudioHandlerID)
    {
        return (IsValidAudioHandlerID(a_iAudioHandlerID) ? sm_aAudioSource[a_iAudioHandlerID].IsPlaying : false);
    }

    public static bool IsPlayingAudio(int a_iAudioHandlerID, AudioClip a_rClip)
    {
        return ((IsValidAudioHandlerID(a_iAudioHandlerID) && sm_aAudioSource[a_iAudioHandlerID].Sound == a_rClip) ? sm_aAudioSource[a_iAudioHandlerID].IsPlaying : false);
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	* New Static Method: Is Valid AudioHandler ID?
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static bool IsValidAudioHandlerID(int a_iAudioHandlerID)
    {
        return (a_iAudioHandlerID > -1 && a_iAudioHandlerID < sm_aAudioSource.Count);
    }
}



//========================================================
// ***				AudioHandlerInfo
//========================================================
public partial class UnityAudioSourceManager
{
    [System.Serializable]
    public class AudioHandlerInfo
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	*+ Public Instance Variables
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public AudioClip m_acAudioToPlay;
        public AudioType m_eAudioType = AudioType.BGM;
        public bool m_bLoopAudio = false;
        public float m_fMaxVolume = 1.0f;

        public bool m_bRandomiseTrackStartPosition = false;
        public int m_iStartTrackPosition = 0;

        public bool m_bFadeinAudioUponPlaying = false;          // Fadein Audio when audio begins playing?
        public float m_fFadeinAudioTime = 3.0f;                 // How long will it take to fade in audio to Max Volume (in seconds)?
        public float m_fFadeoutAudioTime = 1.5f;                // How long will it take to fade out audio? Will only occur when called upon via code
                                                                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                                                                //	*- Private Instance Variables
                                                                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private int m_iAudioHandlerID = -1;
        private AudioHandlerManager m_rAssignedAudioHandler = null;
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	*+ Attr_Accessors
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public int AudioHandlerID { get { return m_iAudioHandlerID; } }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	*+ Set Methods~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void SetVolume(float desiredVolume, float transitionTime = 0.0f)
        {
            if (m_rAssignedAudioHandler != null)
            {
                if (transitionTime > 0.0f)
                {
                    m_rAssignedAudioHandler.FadeAudio(m_rAssignedAudioHandler.Volume, desiredVolume, transitionTime, FadeType.REGULAR_FADE);
                }
                else
                {
                    m_rAssignedAudioHandler.Volume = desiredVolume;
                }
            }
        }

        public void SetPitch(float desiredPitch)
        {
            if (m_rAssignedAudioHandler != null)
            {
                m_rAssignedAudioHandler.Pitch = desiredPitch;
            }
        }
    }
}



//========================================================
// ***				AudioHandlerManager
//========================================================
public partial class UnityAudioSourceManager
{
    private class AudioHandlerManager
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	*- Private Instance Variables
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private TimeTracker m_ttFadeTracker;            // TimeTracker Managing the FadeEffect
        private TimeTracker m_ttWaitTracker;            // TimeTracker Managing the WaitEffect
        private float m_fStartFadeVolume;           // When Lerping Volume: Starting Volume
        private float m_fEndFadeVolume;         // When Lerping Volume: Finishing Volume
        private float m_fDefaultVolume;         // When Lerping Volume: This will be the default Max Volume
        private FadeType m_eCurrentFadeType;            // Current FadeType: Set to NO_FADE automatically in Constructor to avoid needless processing
        private AudioType m_eAudioType;             // Current AudioType being played.
                                                    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                                                    //	*+ Attr_Accessors
                                                    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public AudioSource Source
        {
            get;
            private set;
        }

        public bool IsFading
        {
            get;
            private set;
        }

        public bool IsMuted
        {
            get
            {
                return m_eAudioType == AudioType.BGM && MuteBGM;
            }
        }

        public AudioClip Sound
        {
            get
            {
                return Source != null ? Source.clip : null;
            }
            private set
            {
                if (Source != null)
                {
                    Source.clip = value;
                }
            }
        }

        public float Volume
        {
            get
            {
                return Source != null ? Source.volume : 0.8f;
            }
            set
            {
                if (Source != null)
                {
                    Source.volume = value;
                }
            }
        }

        public float Pitch
        {
            get
            {
                return Source != null ? Source.pitch : 1.0f;
            }
            set
            {
                if (Source != null)
                {
                    Source.pitch = value;
                }
            }
        }
        public bool IsLooping
        {
            get
            {
                return Source != null ? Source.loop : true;
            }
            set
            {
                if (Source != null)
                {
                    Source.loop = value;
                }
            }
        }

        public bool IsAvailable
        {
            get
            {
                return IsPlaying == false;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return Source != null ? Source.isPlaying : false;
            }
        }



        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	** Constructor
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public AudioHandlerManager(AudioSource assignedAudioSource)
        {
            Source = assignedAudioSource;
            m_ttFadeTracker = new TimeTracker(1.0f);
            m_ttWaitTracker = new TimeTracker(1.0f);
            m_eCurrentFadeType = FadeType.NO_FADE;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Update
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void Update()
        {
            if (IsPlaying == false)
            {
                return;
            }

            if (MuteBGMModified && m_eAudioType == AudioType.BGM)
            {
                if (MuteBGM)
                {
                    Volume = 0.0f;
                }
                else
                {
                    Volume = m_fDefaultVolume;
                }
            }

            else
            {
                switch (m_eCurrentFadeType)
                {
                    case FadeType.REGULAR_FADE:
                        UpdateRegularFade();
                        break;
                    case FadeType.FADE_OUT:
                        UpdateFadeout();
                        break;
                    case FadeType.FADE_OUT_AND_BACK_IN:
                        UpdateFadeoutAndBackIn();
                        break;
                    default:
                        break;
                }
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Update Regular Fade
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void UpdateRegularFade()
        {
            if (m_ttFadeTracker.Update())
            {
                IsFading = false;
                m_eCurrentFadeType = FadeType.NO_FADE;
                Volume = m_fEndFadeVolume;
            }
            else
            {
                Volume = Mathf.Lerp(m_fStartFadeVolume, m_fEndFadeVolume, m_ttFadeTracker.GetCompletionPercentage());
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Update Fadeout
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void UpdateFadeout()
        {
            if (m_ttFadeTracker.Update())
            {
                IsFading = false;
                m_eCurrentFadeType = FadeType.NO_FADE;
                StopSound();
            }
            else
            {
                Volume = Mathf.Lerp(m_fStartFadeVolume, m_fEndFadeVolume, m_ttFadeTracker.GetCompletionPercentage());
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Update Fadeout and FadeBackIn
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void UpdateFadeoutAndBackIn()
        {
            if (m_ttFadeTracker.Update())
            {
                Volume = m_fEndFadeVolume;
                if (m_ttWaitTracker.Update())
                {
                    FadeAudio(Volume, m_fDefaultVolume, (m_ttFadeTracker.FinishTime < 0.25f ? 1.0f : m_ttFadeTracker.FinishTime));
                }
            }
            else
            {
                Volume = Mathf.Lerp(m_fStartFadeVolume, m_fEndFadeVolume, m_ttFadeTracker.GetCompletionPercentage());
            }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Methods: Play Sound
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void PlaySound(AudioClip desiredSoundToPlay, bool looping, float desiredVolume, AudioType audType, int trackStartPosition = 0)
        {
            Sound = desiredSoundToPlay;
            Source.loop = looping;
            m_eAudioType = audType;
            m_fDefaultVolume = desiredVolume;
            Volume = (IsMuted == false ? desiredVolume : 0.0f);
            Pitch = 1.0f;
            Source.timeSamples = 0;
            Source.Play();
            Source.timeSamples = (trackStartPosition < (desiredSoundToPlay.samples / desiredSoundToPlay.channels) ? trackStartPosition : 0);

            IsFading = false;
            m_eCurrentFadeType = FadeType.NO_FADE;
        }

        public void PlaySound(AudioClip desiredSoundToPlay, bool looping, float desiredVolume, AudioType audType, float fadeinTime)
        {
            PlaySound(desiredSoundToPlay, looping, desiredVolume, audType);
            FadeAudio(0.0f, desiredVolume, fadeinTime);
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Method: Stop Sound
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public void StopSound()
        {
            Source.Stop();
            IsFading = false;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //	* New Methods: Fade Audio
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private void FadeAudio(float fromVolume, float toVolume, float fadeTime)
        {
            if (IsMuted == false)
            {
                IsFading = true;
                m_eCurrentFadeType = FadeType.REGULAR_FADE;
                Volume = fromVolume;
                m_fStartFadeVolume = fromVolume;
                m_fEndFadeVolume = toVolume;

                m_ttFadeTracker.FinishTime = fadeTime;
                m_ttFadeTracker.Reset();
            }
        }

        public void FadeAudio(float fromVolume, float toVolume, float fadeTime, FadeType eFadeType)
        {
            if (IsMuted == false)
            {
                FadeAudio(fromVolume, toVolume, fadeTime);
                m_eCurrentFadeType = eFadeType;
            }
            else if (eFadeType == FadeType.FADE_OUT)
            {
                StopSound();
            }
        }

        public void FadeoutThenFadein(float fromVolume, float toVolume, float defaultVolume, float fadeTime, float waitTime)
        {
            if (IsMuted == false)
            {
                m_fDefaultVolume = defaultVolume;
                FadeAudio(fromVolume, toVolume, fadeTime);
                m_eCurrentFadeType = FadeType.FADE_OUT_AND_BACK_IN;

                m_ttWaitTracker.FinishTime = waitTime;
                m_ttWaitTracker.Reset();
            }
        }
    }
}