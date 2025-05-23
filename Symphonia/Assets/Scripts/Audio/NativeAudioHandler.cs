using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeAudioHandler : MonoBehaviour
{
    public enum FMOD_RESULT
    {
        FMOD_OK,                        /* No errors. */
        FMOD_ERR_ALREADYLOCKED,         /* Tried to call lock a second time before unlock was called. */
        FMOD_ERR_BADCOMMAND,            /* Tried to call a function on a data type that does not allow this type of functionality (ie calling Sound::lock on a streaming sound). */
        FMOD_ERR_CDDA_DRIVERS,          /* Neither NTSCSI nor ASPI could be initialised. */
        FMOD_ERR_CDDA_INIT,             /* An error occurred while initialising the CDDA subsystem. */
        FMOD_ERR_CDDA_INVALID_DEVICE,   /* Couldn't find the specified device. */
        FMOD_ERR_CDDA_NOAUDIO,          /* No audio tracks on the specified disc. */
        FMOD_ERR_CDDA_NODEVICES,        /* No CD/DVD devices were found. */
        FMOD_ERR_CDDA_NODISC,           /* No disc present in the specified drive. */
        FMOD_ERR_CDDA_READ,             /* A CDDA read error occurred. */
        FMOD_ERR_CHANNEL_ALLOC,         /* Error trying to allocate a channel. */
        FMOD_ERR_CHANNEL_STOLEN,        /* The specified channel has been reused to play another sound. */
        FMOD_ERR_COM,                   /* A Win32 COM related error occured. COM failed to initialize or a QueryInterface failed meaning a Windows codec or driver was not installed properly. */
        FMOD_ERR_DMA,                   /* DMA Failure.  See debug output for more information. */
        FMOD_ERR_DSP_CONNECTION,        /* DSP connection error.  Connection possibly caused a cyclic dependancy.  Or tried to connect a tree too many units deep (more than 128). */
        FMOD_ERR_DSP_FORMAT,            /* DSP Format error.  A DSP unit may have attempted to connect to this network with the wrong format. */
        FMOD_ERR_DSP_NOTFOUND,          /* DSP connection error.  Couldn't find the DSP unit specified. */
        FMOD_ERR_DSP_RUNNING,           /* DSP error.  Cannot perform this operation while the network is in the middle of running.  This will most likely happen if a connection or disconnection is attempted in a DSP callback. */
        FMOD_ERR_DSP_TOOMANYCONNECTIONS,/* DSP connection error.  The unit being connected to or disconnected should only have 1 input or output. */
        FMOD_ERR_FILE_BAD,              /* Error loading file. */
        FMOD_ERR_FILE_COULDNOTSEEK,     /* Couldn't perform seek operation.  This is a limitation of the medium (ie netstreams) or the file format. */
        FMOD_ERR_FILE_DISKEJECTED,      /* Media was ejected while reading. */
        FMOD_ERR_FILE_EOF,              /* End of file unexpectedly reached while trying to read essential data (truncated data?). */
        FMOD_ERR_FILE_NOTFOUND,         /* File not found. */
        FMOD_ERR_FILE_UNWANTED,         /* Unwanted file access occured. */
        FMOD_ERR_FORMAT,                /* Unsupported file or audio format. */
        FMOD_ERR_HTTP,                  /* A HTTP error occurred. This is a catch-all for HTTP errors not listed elsewhere. */
        FMOD_ERR_HTTP_ACCESS,           /* The specified resource requires authentication or is forbidden. */
        FMOD_ERR_HTTP_PROXY_AUTH,       /* Proxy authentication is required to access the specified resource. */
        FMOD_ERR_HTTP_SERVER_ERROR,     /* A HTTP server error occurred. */
        FMOD_ERR_HTTP_TIMEOUT,          /* The HTTP request timed out. */
        FMOD_ERR_INITIALIZATION,        /* FMOD was not initialized correctly to support this function. */
        FMOD_ERR_INITIALIZED,           /* Cannot call this command after System::init. */
        FMOD_ERR_INTERNAL,              /* An error occured that wasn't supposed to.  Contact support. */
        FMOD_ERR_INVALID_ADDRESS,       /* On Xbox 360, this memory address passed to FMOD must be physical, (ie allocated with XPhysicalAlloc.) */
        FMOD_ERR_INVALID_FLOAT,         /* Value passed in was a NaN, Inf or denormalized float. */
        FMOD_ERR_INVALID_HANDLE,        /* An invalid object handle was used. */
        FMOD_ERR_INVALID_PARAM,         /* An invalid parameter was passed to this function. */
        FMOD_ERR_INVALID_POSITION,      /* An invalid seek position was passed to this function. */
        FMOD_ERR_INVALID_SPEAKER,       /* An invalid speaker was passed to this function based on the current speaker mode. */
        FMOD_ERR_INVALID_SYNCPOINT,     /* The syncpoint did not come from this sound handle. */
        FMOD_ERR_INVALID_VECTOR,        /* The vectors passed in are not unit length, or perpendicular. */
        FMOD_ERR_MAXAUDIBLE,            /* Reached maximum audible playback count for this sound's soundgroup. */
        FMOD_ERR_MEMORY,                /* Not enough memory or resources. */
        FMOD_ERR_MEMORY_CANTPOINT,      /* Can't use FMOD_OPENMEMORY_POINT on non PCM source data, or non mp3/xma/adpcm data if FMOD_CREATECOMPRESSEDSAMPLE was used. */
        FMOD_ERR_MEMORY_SRAM,           /* Not enough memory or resources on console sound ram. */
        FMOD_ERR_NEEDS2D,               /* Tried to call a command on a 3d sound when the command was meant for 2d sound. */
        FMOD_ERR_NEEDS3D,               /* Tried to call a command on a 2d sound when the command was meant for 3d sound. */
        FMOD_ERR_NEEDSHARDWARE,         /* Tried to use a feature that requires hardware support.  (ie trying to play a GCADPCM compressed sound in software on Wii). */
        FMOD_ERR_NEEDSSOFTWARE,         /* Tried to use a feature that requires the software engine.  Software engine has either been turned off, or command was executed on a hardware channel which does not support this feature. */
        FMOD_ERR_NET_CONNECT,           /* Couldn't connect to the specified host. */
        FMOD_ERR_NET_SOCKET_ERROR,      /* A socket error occurred.  This is a catch-all for socket-related errors not listed elsewhere. */
        FMOD_ERR_NET_URL,               /* The specified URL couldn't be resolved. */
        FMOD_ERR_NET_WOULD_BLOCK,       /* Operation on a non-blocking socket could not complete immediately. */
        FMOD_ERR_NOTREADY,              /* Operation could not be performed because specified sound/DSP connection is not ready. */
        FMOD_ERR_OUTPUT_ALLOCATED,      /* Error initializing output device, but more specifically, the output device is already in use and cannot be reused. */
        FMOD_ERR_OUTPUT_CREATEBUFFER,   /* Error creating hardware sound buffer. */
        FMOD_ERR_OUTPUT_DRIVERCALL,     /* A call to a standard soundcard driver failed, which could possibly mean a bug in the driver or resources were missing or exhausted. */
        FMOD_ERR_OUTPUT_ENUMERATION,    /* Error enumerating the available driver list. List may be inconsistent due to a recent device addition or removal. */
        FMOD_ERR_OUTPUT_FORMAT,         /* Soundcard does not support the minimum features needed for this soundsystem (16bit stereo output). */
        FMOD_ERR_OUTPUT_INIT,           /* Error initializing output device. */
        FMOD_ERR_OUTPUT_NOHARDWARE,     /* FMOD_HARDWARE was specified but the sound card does not have the resources necessary to play it. */
        FMOD_ERR_OUTPUT_NOSOFTWARE,     /* Attempted to create a software sound but no software channels were specified in System::init. */
        FMOD_ERR_PAN,                   /* Panning only works with mono or stereo sound sources. */
        FMOD_ERR_PLUGIN,                /* An unspecified error has been returned from a 3rd party plugin. */
        FMOD_ERR_PLUGIN_INSTANCES,      /* The number of allowed instances of a plugin has been exceeded. */
        FMOD_ERR_PLUGIN_MISSING,        /* A requested output, dsp unit type or codec was not available. */
        FMOD_ERR_PLUGIN_RESOURCE,       /* A resource that the plugin requires cannot be found. (ie the DLS file for MIDI playback) */
        FMOD_ERR_PRELOADED,             /* The specified sound is still in use by the event system, call EventSystem::unloadFSB before trying to release it. */
        FMOD_ERR_PROGRAMMERSOUND,       /* The specified sound is still in use by the event system, wait for the event which is using it finish with it. */
        FMOD_ERR_RECORD,                /* An error occured trying to initialize the recording device. */
        FMOD_ERR_REVERB_INSTANCE,       /* Specified instance in FMOD_REVERB_PROPERTIES couldn't be set. Most likely because it is an invalid instance number or the reverb doesnt exist. */
        FMOD_ERR_SUBSOUND_ALLOCATED,    /* This subsound is already being used by another sound, you cannot have more than one parent to a sound.  Null out the other parent's entry first. */
        FMOD_ERR_SUBSOUND_CANTMOVE,     /* Shared subsounds cannot be replaced or moved from their parent stream, such as when the parent stream is an FSB file. */
        FMOD_ERR_SUBSOUND_MODE,         /* The subsound's mode bits do not match with the parent sound's mode bits.  See documentation for function that it was called with. */
        FMOD_ERR_SUBSOUNDS,             /* The error occured because the sound referenced contains subsounds when it shouldn't have, or it doesn't contain subsounds when it should have.  The operation may also not be able to be performed on a parent sound, or a parent sound was played without setting up a sentence first. */
        FMOD_ERR_TAGNOTFOUND,           /* The specified tag could not be found or there are no tags. */
        FMOD_ERR_TOOMANYCHANNELS,       /* The sound created exceeds the allowable input channel count.  This can be increased using the maxinputchannels parameter in System::setSoftwareFormat. */
        FMOD_ERR_UNIMPLEMENTED,         /* Something in FMOD hasn't been implemented when it should be! contact support! */
        FMOD_ERR_UNINITIALIZED,         /* This command failed because System::init or System::setDriver was not called. */
        FMOD_ERR_UNSUPPORTED,           /* A command issued was not supported by this object.  Possibly a plugin without certain callbacks specified. */
        FMOD_ERR_UPDATE,                /* An error caused by System::update occured. */
        FMOD_ERR_VERSION,               /* The version number of this file format is not supported. */

        FMOD_ERR_EVENT_FAILED,          /* An Event failed to be retrieved, most likely due to 'just fail' being specified as the max playbacks behavior. */
        FMOD_ERR_EVENT_INFOONLY,        /* Can't execute this command on an EVENT_INFOONLY event. */
        FMOD_ERR_EVENT_INTERNAL,        /* An error occured that wasn't supposed to.  See debug log for reason. */
        FMOD_ERR_EVENT_MAXSTREAMS,      /* Event failed because 'Max streams' was hit when FMOD_EVENT_INIT_FAIL_ON_MAXSTREAMS was specified. */
        FMOD_ERR_EVENT_MISMATCH,        /* FSB mismatches the FEV it was compiled with, the stream/sample mode it was meant to be created with was different, or the FEV was built for a different platform. */
        FMOD_ERR_EVENT_NAMECONFLICT,    /* A category with the same name already exists. */
        FMOD_ERR_EVENT_NOTFOUND,        /* The requested event, event group, event category or event property could not be found. */
        FMOD_ERR_EVENT_NEEDSSIMPLE,     /* Tried to call a function on a complex event that's only supported by simple events. */
        FMOD_ERR_EVENT_GUIDCONFLICT,    /* An event with the same GUID already exists. */
        FMOD_ERR_EVENT_ALREADY_LOADED,  /* The specified project or bank has already been loaded. Having multiple copies of the same project loaded simultaneously is forbidden. */

        FMOD_ERR_MUSIC_UNINITIALIZED,   /* Music system is not initialized probably because no music data is loaded. */
        FMOD_ERR_MUSIC_NOTFOUND,        /* The requested music entity could not be found. */
        FMOD_ERR_MUSIC_NOCALLBACK,      /* The music callback is required, but it has not been set. */

        FMOD_RESULT_FORCEINT = 65536    /* Makes sure this enum is signed 32bit. */
    }

    //// Declarations /////////////////////////////////////////////////
    public class SoundRef
    {
        public SoundType SoundType { get; protected set; }
        public uint SoundID { get; protected set; }
        public float PlayDuration { get; protected set; }

        public SoundRef(uint soundID, SoundType soundType, float playDuration)
        {
            this.SoundID = soundID;
            this.SoundType = soundType;
            this.PlayDuration = PlayDuration;
        }
    }

    public enum SoundType : int
    {
        BGM = 0,
        BGS = 1,
        SE = 2,
        COUNT
    }

    //// Static Variables /////////////////////////////////////////////////
    private static List<SoundRef> s_importedSounds = new List<SoundRef>();
    
    //// DLL Import /////////////////////////////////////////////////
    [DllImport("FMODAudioPlayer", EntryPoint = "InitialiseSoundSystem", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_InitialiseSoundSystem();

    [DllImport("FMODAudioPlayer", EntryPoint = "UpdateSoundSystem", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_UpdateSoundSystem(float deltaTime);

    [DllImport("FMODAudioPlayer", EntryPoint = "GetFMODResult", CallingConvention = CallingConvention.StdCall)]
    private static extern int _Dll_GetFMODResult();

    [DllImport("FMODAudioPlayer", EntryPoint = "IsPlayingSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_IsPlayingSound(int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "IsPlayingSpecifiedSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_IsPlayingSpecifiedSound(uint soundId, int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "GetSoundPositionSeconds", CallingConvention = CallingConvention.StdCall)]
    private static extern float _Dll_GetSoundPositionSeconds(int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "GetSoundLengthSeconds", CallingConvention = CallingConvention.StdCall)]
    private static extern float _Dll_GetSoundLengthSeconds(uint soundId, int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "GetSoundProgress", CallingConvention = CallingConvention.StdCall)]
    private static extern float _Dll_GetSoundProgress(int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "ImportAudio", CallingConvention = CallingConvention.StdCall)]
    private static extern uint _Dll_ImportAudio(IntPtr filePath, int soundType, bool loopAudio, bool removeSilenceFromAudio, uint volume = 100, uint speed = 100);

    [DllImport("FMODAudioPlayer", EntryPoint = "PlaySound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_PlaySound(uint soundId, int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "PauseSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_PauseSound(int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "UnpauseSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_UnpauseSound(int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "StopSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_StopSound(int soundType);

    [DllImport("FMODAudioPlayer", EntryPoint = "FadeinSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_FadeinSound(int soundType, float fadeInDuration);

    [DllImport("FMODAudioPlayer", EntryPoint = "FadeoutSound", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_FadeoutSound(int soundType, float fadeInDuration);

    [DllImport("FMODAudioPlayer", EntryPoint = "TerminateSoundSystem", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_TerminateSoundSystem();
    /////////////////////////////////////////////////////////////////


    //////////////////// Static Functions /////////////////////////////////////
    public static FMOD_RESULT GetFMODResult()
    {
        int result = _Dll_GetFMODResult();
        FMOD_RESULT fmodResult = (FMOD_RESULT)result;
        return fmodResult;
    }

    public static bool IsPlayingSoundOnChannel(SoundType soundType)
    {
        bool isPlaying = _Dll_IsPlayingSound((int)soundType);
        return isPlaying;
    }

    public static bool IsPlayingSound(SoundRef sound)
    {
        bool isPlaying = _Dll_IsPlayingSpecifiedSound(sound.SoundID, (int)sound.SoundType);
        return isPlaying;
    }

    public static float GetSoundPosition(SoundRef sound)
    {
        float posInSeconds = _Dll_GetSoundPositionSeconds((int)sound.SoundType);
        return posInSeconds;
    }

    public static float GetSoundProgress(SoundRef sound)
    {
        float progress = _Dll_GetSoundProgress((int)sound.SoundType);
        return progress;
    }

    public static SoundRef ImportAudio(string filePath, SoundType soundType, bool loopAudio, bool removeSilenceFromAudio = false, uint volume = 100, uint speed = 100)
    {
        IntPtr asCStyleString = IntPtr.Zero;
        asCStyleString = Marshal.StringToHGlobalUni(filePath);

        uint soundId = _Dll_ImportAudio(asCStyleString, (int)soundType, loopAudio, removeSilenceFromAudio, volume, speed);
        Marshal.FreeHGlobal(asCStyleString);

        if (soundId == 0)
        {
            // ID of 0 means an error occured.
            return null;
        }

        int indexID = (int)soundId - 1;
        if (indexID < s_importedSounds.Count)
        {
            // Already imported.
            return s_importedSounds[indexID];
        }

        float playDuration = _Dll_GetSoundLengthSeconds(soundId, (int)soundType);

        SoundRef soundRef = new SoundRef(soundId, soundType, playDuration);
        return soundRef;
    }

    public static SoundRef ImportAudioAndPlayAudio(string filePath, SoundType soundType, bool loopAudio, bool removeSilenceFromAudio = false, uint volume = 100, uint speed = 100)
    {
        SoundRef soundRef = ImportAudio(filePath, soundType, loopAudio, removeSilenceFromAudio, volume, speed);
        if (soundRef == null)
        {
            return null;
        }

        PlaySound(soundRef);
        return soundRef;
    }

    public static bool PlaySound(SoundRef sound)
    {
        bool result = _Dll_PlaySound(sound.SoundID, (int)sound.SoundType);
        return result;
    }

    public static bool PauseSound(SoundRef sound)
    {
        bool result = _Dll_PauseSound((int)sound.SoundType);
        return result;
    }

    public static bool UnpauseSound(SoundRef sound)
    {
        bool result = _Dll_UnpauseSound((int)sound.SoundType);
        return result;
    }

    public static bool StopChannel(SoundType sound)
    {
        bool result = _Dll_StopSound((int)sound);
        return result;
    }

    public static bool StopSound(SoundRef sound)
    {
        bool result = _Dll_StopSound((int)sound.SoundType);
        return result;
    }

    public static bool FadeinSound(SoundRef sound, float fadeinDuration)
    {
        bool result = _Dll_FadeinSound((int)sound.SoundType, fadeinDuration);
        return result;
    }

    public static bool FadeoutSound(SoundRef sound, float fadeoutDuration)
    {
        bool result = _Dll_FadeoutSound((int)sound.SoundType, fadeoutDuration);
        return result;
    }


    //////////////////// Instance Functions /////////////////////////////////////
    NativeAudioHandler() : base()
    {
        // It's okay to call the 'initialise' function more than once. The DLL ignores it if requested a second time.
        bool result = _Dll_InitialiseSoundSystem();
    }

    protected void Update()
    {
        bool result = _Dll_UpdateSoundSystem(Time.deltaTime);
    }

    protected void OnApplicationQuit()
    {
        bool result = _Dll_TerminateSoundSystem();
    }
}