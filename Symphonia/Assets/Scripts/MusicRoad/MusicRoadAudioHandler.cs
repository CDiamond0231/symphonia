using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicRoadAudioHandler : MonoBehaviour
{
    public enum AudioPlaybackType
    {
        MidiOnly,
        OverlayTrack,
    }



    ///////////////////////////// Non-Editor Variables /////////////////////////////
    public bool ReadyToPlay { get; protected set; }
    public bool IsPlaying { get; protected set; }
    public double OverlayTrackTempo { get; protected set; } = -1;

    protected MusicRoadManager m_musicRoadManager = null;

    protected NativeAudioHandler.SoundRef m_currentlyPlayingBGMAudioClipRef = null;
    protected NativeAudioHandler.SoundRef m_loadedBGMAudioClipRef = null;               // What to load next


    public void Setup(MusicRoadManager musicRoadManager, string defaultMidiLocation)
    {
        m_musicRoadManager = musicRoadManager;
        m_musicRoadManager.OnStartOfMusicTriggeredCallback += OnStartOfMusicTriggered;
        m_musicRoadManager.OnFirstNoteReachedKeyboardCallback += OnFirstNoteReachedKeyboard;
        m_musicRoadManager.OnEndOfMusicRoadReachedCallback += OnEndOfMusicRoadReached;

        string audioPath;
        if (m_musicRoadManager.AudioPlaybackMethod == AudioPlaybackType.OverlayTrack)
        {
            audioPath = HelperFunctions.GetAssociatedOverlayTrack(defaultMidiLocation);
        }
        else
        {
            audioPath = defaultMidiLocation;
        }

        LoadOverlayTrack(audioPath);
    }

    public float GetSoundPosition()
    {
        if (IsPlaying == false)
        {
            return 0.0f;
        }

        float position = NativeAudioHandler.GetSoundPosition(m_loadedBGMAudioClipRef);
        return position;
    }

    public bool LoadOverlayTrack(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            ReadyToPlay = false;
        }
        else if (LoadBGMViaFMOD(filePath))
        {
            Debug.Log($"Loaded \"{filePath}\" via FMOD successfully");
            ReadyToPlay = true;
        }
        else
        {
            // Fall back to Midi Output
            Debug.Log($"Failed to load \"{filePath}\" via FMOD");
            ReadyToPlay = false;
        }

        return ReadyToPlay;
    }

    public bool HasAudioStoppedEarly()
    {
        if (IsPlaying == false)
        {
            // Not even playing... So no
            return false;
        }
        if (m_currentlyPlayingBGMAudioClipRef != null)
        {
            bool isStillPlaying = NativeAudioHandler.IsPlayingSound(m_currentlyPlayingBGMAudioClipRef);
            return isStillPlaying == false;
        }

        return false;
    }

    public bool FadeinSound(float fadeDuration = 1.5f)
    {
        if (m_currentlyPlayingBGMAudioClipRef != null)
        {
            NativeAudioHandler.FadeinSound(m_currentlyPlayingBGMAudioClipRef, fadeDuration);
            return true;
        }

        return false;
    }

    public bool FadeoutSound(float fadeDuration = 1.5f)
    {
        if (m_currentlyPlayingBGMAudioClipRef != null)
        {
            NativeAudioHandler.FadeoutSound(m_currentlyPlayingBGMAudioClipRef, fadeDuration);
            m_currentlyPlayingBGMAudioClipRef = null;
            return true;
        }

        return false;
    }

    public bool PlayBGM()
    {
        if (m_loadedBGMAudioClipRef == null)
        {
            return false;
        }

        if (m_currentlyPlayingBGMAudioClipRef == m_loadedBGMAudioClipRef)
        {
            // Already playing
            return true;
        }
        if (NativeAudioHandler.IsPlayingSoundOnChannel(NativeAudioHandler.SoundType.BGM))
        {
            NativeAudioHandler.StopChannel(NativeAudioHandler.SoundType.BGM);
        }

        m_currentlyPlayingBGMAudioClipRef = null;
        
        IsPlaying = NativeAudioHandler.PlaySound(m_loadedBGMAudioClipRef);
        if (IsPlaying)
        {
            m_currentlyPlayingBGMAudioClipRef = m_loadedBGMAudioClipRef;
        }

        return IsPlaying;
    }

    public bool PlayBGM(string filePath)
    {
        if (LoadBGMViaFMOD(filePath) == false)
        {
            return false;
        }

        bool result = PlayBGM();
        return result;
    }

    public bool StopBGM()
    {
        if (m_currentlyPlayingBGMAudioClipRef == null)
        {
            return false;
        }

        bool soundHasStopped = NativeAudioHandler.StopSound(m_currentlyPlayingBGMAudioClipRef);
        if (soundHasStopped)
        {
            m_currentlyPlayingBGMAudioClipRef = null;
        }
        return soundHasStopped;
    }

    protected void OnStartOfMusicTriggered()
    {
        PlayBGM();
    }

    protected void OnFirstNoteReachedKeyboard()
    {
    }

    protected void OnEndOfMusicRoadReached()
    {
    }

    protected bool LoadBGMViaFMOD(string filePath)
    {
        m_loadedBGMAudioClipRef = NativeAudioHandler.ImportAudio(filePath, NativeAudioHandler.SoundType.BGM, false, false);
        return m_loadedBGMAudioClipRef != null;
    }

    // Start is called before the first frame update
    protected void Start()
    {
    }

    // Update is called once per frame
    protected void Update()
    {
    }

    protected void OnApplicationQuit()
    {
    }
}
