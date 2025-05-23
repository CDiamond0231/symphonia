using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MusicRoadManager : MonoBehaviour
{
    public delegate void GenericCallback();

    protected struct ParseStatusTracker
    {
        public int currentChannelId;
        public int currentNoteId;
        public int currentEventIndex;
    }

    protected class ChannelNotesInfo
    {
        public bool IsActive { get; set; }
        public int NotesReachedKeyboard { get; set; } = 0;
        public int NotesHitSuccessfully { get; set; } = 0;
        public List<DropdownNote> DropdownNotesForChannel { get; protected set; } = new List<DropdownNote>();
        public LinkedList<DropdownNote> NotesCurrentlyPlaying { get; protected set; } = new LinkedList<DropdownNote>();

        // Just a wrapper to make code in loops easier to read/understand
        public int IndexOfNextNoteToTrigger
        {
            get => NotesReachedKeyboard;
            set => NotesReachedKeyboard = value;
        }

        public void Reset()
        {
            NotesReachedKeyboard = 0;
            NotesHitSuccessfully = 0;
            NotesCurrentlyPlaying.Clear();
        }
    }

    protected enum NotePlacementStatus
    {
        SUCCESS,
        INCOMPATIBLE,
        FAILED,
    }

    public enum PlaybackType
    {
        Automatic,
        PlayerControlled,
    }

    public PlaybackType KeyboardPlayMode
    {
        get
        {
            if (IsOnTitleScreen)
            {
                // Always automatic on title screen
                return PlaybackType.Automatic;
            }
            if (ShouldGamePlayAutomatically)
            {
                return PlaybackType.Automatic;
            }

            return PlaybackType.PlayerControlled;
        }
    }

    public bool IsOnTitleScreen
    {
        get
        {
            return IsPlayingGame == false;
        }
    }

    public int NotesReachedKeyboard
    {
        get
        {
            int reachedCount = 0;
            foreach (ChannelNotesInfo channelNotesInfo in m_channelNotes)
            {
                if (channelNotesInfo.IsActive)
                {
                    reachedCount += channelNotesInfo.NotesReachedKeyboard;
                }
            }
            return reachedCount;
        }
    }

    public int TotalNotesToPlay
    {
        get
        {
            int notesCount = 0;
            foreach (ChannelNotesInfo channelNotesInfo in m_channelNotes)
            {
                if (channelNotesInfo.IsActive)
                {
                    notesCount += channelNotesInfo.DropdownNotesForChannel.Count;
                }
            }
            return notesCount;
        }
    }

    public int TotalNotesHit
    {
        get
        {
            int notesHit = 0;
            foreach (ChannelNotesInfo channelNotesInfo in m_channelNotes)
            {
                if (channelNotesInfo.IsActive)
                {
                    notesHit += channelNotesInfo.NotesHitSuccessfully;
                }
            }
            return notesHit;
        }
    }

    public int TotalNotesMissed
    {
        get
        {
            int notesReached = NotesReachedKeyboard;
            int notesHit = TotalNotesHit;
            int notesMissed = notesReached - notesHit;
            return notesMissed;
        }
    }

    public float SecondsPerScreenLength
    {
        get
        {
            return m_secondsPerScreenLength;
        }
        set
        {
            m_secondsPerScreenLength = value;
            GenerateMusicRoad(m_currentMidiFilePath);

            MusicRoadAudioHandler.StopBGM();
            MusicRoadVideoHandler.Stop();
        }
    }

    public float GameStartDelayTime
    {
        get
        {
            return m_gameStartDelayTime;
        }
        set
        {
            m_gameStartDelayTime = value;
            GenerateMusicRoad(m_currentMidiFilePath);

            MusicRoadAudioHandler.StopBGM();
            MusicRoadVideoHandler.Stop();
        }
    }

    public float MusicStartDelayTime
    {
        get
        {
            return m_musicStartDelayTime;
        }
        set
        {
            m_musicStartDelayTime = value;
            GenerateMusicRoad(m_currentMidiFilePath);

            MusicRoadAudioHandler.StopBGM();
            MusicRoadVideoHandler.Stop();
        }
    }

    public float VideoStartDelayTime
    {
        get
        {
            return m_videoStartDelayTime;
        }
        set
        {
            m_videoStartDelayTime = value;

            if (CurrentGameTime >= m_videoStartDelayTime)
            {
                double playbackStart = CurrentGameTime - m_videoStartDelayTime; ;
                MusicRoadVideoHandler.Play(playbackStart);
            }
            else
            {
                MusicRoadVideoHandler.Stop();
            }
        }
    }

    public double DesiredMidiTempo
    {
        get
        {
            if (m_desiredMidiTempo > 0.0f)
            {
                return m_desiredMidiTempo;
            }
            if (m_midiFileReader != null)
            {
                return m_midiFileReader.GetAverageTempo();
            }
            return -1.0;
        }
        set
        {
            m_desiredMidiTempo = value;
            GenerateMusicRoad(m_currentMidiFilePath);

            MusicRoadAudioHandler.StopBGM();
            MusicRoadVideoHandler.Stop();
        }
    }

    ///////////////////////////// Editor Variables /////////////////////////////
    public MusicRoadAudioHandler MusicRoadAudioHandler;
    public MusicRoadVideoHandler MusicRoadVideoHandler;
    public InstrumentsManager InstrumentsManager;

    public SceneFadeEffect TitleScreenOpacityEffect;
    public FadeToGameEffect FadeToTitleScreenEffect;

    public float VerticalContactPositionForNoteAndKey = 0.6434f;
    public float EndOfScreenVerticalPosition = 3.871f;

    public MusicRoadAudioHandler.AudioPlaybackType AudioPlaybackMethod = MusicRoadAudioHandler.AudioPlaybackType.OverlayTrack;

    public KeyboardKeysManager KeyboardKeysManager;
    public GameObject DropdownNoteObj = null;
    public float DropdownNoteWidthMultiplier = 0.9f;

    public Color[] ChannelNotesColours = new Color[16];

    ///////////////////////////// Non-Editor Variables /////////////////////////////
    public GenericCallback OnStartOfMusicTriggeredCallback;
    public GenericCallback OnFirstNoteReachedKeyboardCallback;
    public GenericCallback OnEndOfMusicRoadReachedCallback;

    public bool IsPlayingGame { get; set; } = false;
    public double CurrentGameTime { get; protected set; } = 0.0f;
    public bool ShouldGamePlayAutomatically { get; set; } = false;

    public LinkedList<DropdownNote> NotesReachedKeyboardThisFrame { get; } = new LinkedList<DropdownNote>();
    public LinkedList<DropdownNote> NotesExitedKeyboardThisFrame { get; } = new LinkedList<DropdownNote>();

    protected MidiFileReader m_midiFileReader = new MidiFileReader();
    protected ParseStatusTracker m_parseStatusTracker = new ParseStatusTracker();
    protected GameObject[] m_channelNotesParents;

    protected double m_timeOfLastUpdate = 0;
    protected bool m_hasTriggeredMusicStart = false;

    protected List<DropdownNote> m_dropdownNotes = new List<DropdownNote>();
    protected ChannelNotesInfo[] m_channelNotes;

    protected string m_currentMidiFilePath = "";
    protected double m_desiredMidiTempo = -1.0;
    protected float m_secondsPerScreenLength = 1.25f;
    protected float m_gameStartDelayTime = 4.0f;
    protected float m_musicStartDelayTime = 4.0f;
    protected float m_videoStartDelayTime = 4.0f;

    // Start is called before the first frame update
    protected void Start()
    {
        if (m_channelNotesParents == null)
        {
            m_channelNotesParents = new GameObject[MidiFileConstants.NumberOfChannels];
            for (int i = 0; i < MidiFileConstants.NumberOfChannels; ++i)
            {
                m_channelNotesParents[i] = new GameObject($"Channel {((i + 1) < 10 ? "0" : "")}{(i + 1)}");
                m_channelNotesParents[i].transform.parent = gameObject.transform;
                m_channelNotesParents[i].transform.localScale = Vector3.one;
                m_channelNotesParents[i].transform.localPosition = Vector3.zero;
            }
        }

        m_channelNotes = new ChannelNotesInfo[MidiFileConstants.NumberOfChannels];
        for (int i = 0; i < MidiFileConstants.NumberOfChannels; ++i)
        {
            m_channelNotes[i] = new ChannelNotesInfo();
        }

        string firstMidiFileToPlay = HelperFunctions.GetRandomMidiFileFromDefaultLocation();
        MusicRoadAudioHandler.Setup(this, firstMidiFileToPlay);

        GenerateMusicRoad(firstMidiFileToPlay);

        m_timeOfLastUpdate = Time.realtimeSinceStartup;
    }

    protected void Update()
    {
        if (IsPlayingGame && Input.GetKeyDown(KeyCode.Escape))
        {
            // Exit Music Road
            OnEndOfMusicRoadReached();
        }

        if (MusicRoadAudioHandler.ReadyToPlay == false)
        {
            m_timeOfLastUpdate = Time.realtimeSinceStartup;
            return;
        }


        if (m_hasTriggeredMusicStart)
        {
            float soundPos = MusicRoadAudioHandler.GetSoundPosition();
            CurrentGameTime = soundPos + MusicStartDelayTime;
        }
        else
        {
            // Time of Last Update is only used for the initial Delay time.
            CurrentGameTime += (Time.realtimeSinceStartup - m_timeOfLastUpdate);
            m_timeOfLastUpdate = Time.realtimeSinceStartup;

            if (CurrentGameTime >= MusicStartDelayTime)
            {
                OnStartOfMusicTriggered();
            }
        }

        if (MusicRoadVideoHandler.IsPlaying == false && CurrentGameTime >= m_videoStartDelayTime)
        {
            MusicRoadVideoHandler.Play();
        }

        NotesReachedKeyboardThisFrame.Clear();
        NotesExitedKeyboardThisFrame.Clear();

        double visibleScreenTimeBoundary = CurrentGameTime + SecondsPerScreenLength;
        foreach (ChannelNotesInfo channelNotes in m_channelNotes)
        {
            foreach (DropdownNote dropdownNote in channelNotes.NotesCurrentlyPlaying)
            {
                dropdownNote.SetNotePosition(CurrentGameTime);
            }

            for (int i = channelNotes.NotesReachedKeyboard; i < channelNotes.DropdownNotesForChannel.Count; ++i)
            {
                DropdownNote dropdownNote = channelNotes.DropdownNotesForChannel[i];
                dropdownNote.SetNotePosition(CurrentGameTime);

                if (dropdownNote.StartTime > visibleScreenTimeBoundary)
                {
                    // Everything beyond here is not visible. No need to continue updating
                    break;
                }
            }
        }

        foreach (DropdownNote dropdownNote in NotesExitedKeyboardThisFrame)
        {
            // Removing Notes that have finished playing on the timeline from channels
            ChannelNotesInfo channelNotes = m_channelNotes[dropdownNote.ChannelID];
            channelNotes.NotesCurrentlyPlaying.Remove(dropdownNote);
        }

        if (MusicRoadAudioHandler.HasAudioStoppedEarly())
        {
            OnEndOfMusicRoadReached();
        }
    }

    public void OnNoteReachedKeyboardTrigger(DropdownNote dropdownNote)
    {
        if (NotesReachedKeyboard == 0)
        {
            OnFirstNoteReachedKeyboard();
        }

        ChannelNotesInfo channelNotesInfo = m_channelNotes[dropdownNote.ChannelID];
        ++channelNotesInfo.NotesReachedKeyboard;

        channelNotesInfo.NotesCurrentlyPlaying.AddLast(dropdownNote);
        NotesReachedKeyboardThisFrame.AddLast(dropdownNote);
    }

    public void OnNoteSuccessfullyHit(DropdownNote dropdownNote)
    {
        ChannelNotesInfo channelNotesInfo = m_channelNotes[dropdownNote.ChannelID];
        ++channelNotesInfo.NotesHitSuccessfully;
    }

    public void OnNoteMissedEvent(DropdownNote dropdownNote)
    {
        // Todo: Add some proper feedback here.
        // This is just a sloppy effect that will work for now
        MusicRoadAudioHandler.FadeinSound(0.05f);
    }

    public void OnNoteExitKeyboardTrigger(DropdownNote dropdownNote)
    {
        NotesExitedKeyboardThisFrame.AddLast(dropdownNote);

        if (NotesReachedKeyboard == TotalNotesToPlay)
        {
            OnEndOfMusicRoadReached();
        }
    }

    public void ResetMusicRoad()
    {
        m_timeOfLastUpdate = Time.realtimeSinceStartup;
        m_hasTriggeredMusicStart = false;
        CurrentGameTime = 0.0f;

        KeyboardKeysManager.ResetKeyboard();

        foreach (DropdownNote note in m_dropdownNotes)
        {
            note.ResetNote();
        }

        foreach (ChannelNotesInfo channelNotes in m_channelNotes)
        {
            // This one gets cleared whilst the other one does not because the m_dropdownNotes list is also acting as an object pool.
            channelNotes.Reset();
        }
    }

    public bool CheckIfChannelIsActive(int channelID)
    {
        if (channelID < 0 || channelID >= MidiFileConstants.NumberOfChannels)
        {
            return false;
        }

        ChannelNotesInfo channelNotesInfo = m_channelNotes[channelID];
        return channelNotesInfo.IsActive;
    }

    public void ToggleChannelNotes(int channelID, bool isActive)
    {
        if (channelID < 0 || channelID >= MidiFileConstants.NumberOfChannels)
        {
            return;
        }

        ChannelNotesInfo channelNotesInfo = m_channelNotes[channelID];
        channelNotesInfo.IsActive = isActive;
        if (isActive)
        {
            for (int i = channelNotesInfo.IndexOfNextNoteToTrigger; i < channelNotesInfo.DropdownNotesForChannel.Count; ++i)
            {
                DropdownNote dropdownNote = channelNotesInfo.DropdownNotesForChannel[i];

                bool wasPlayingWhenDisabled = channelNotesInfo.NotesCurrentlyPlaying.Contains(dropdownNote);
                if (dropdownNote.EndTime < CurrentGameTime)
                {
                    if (wasPlayingWhenDisabled == false)
                    {
                        // This note would have already played if the channel remained active. So just ignore it.
                        //      If this is not the case. It is better to just let the normal flow (updating active notes via 'NotesCurrentlyPlaying' list) because it will shut off the note instantaneously.
                        ++channelNotesInfo.IndexOfNextNoteToTrigger;
                    }
                    continue;
                }
                else if (wasPlayingWhenDisabled)
                {
                    // Inform the keyboard to keep showing the key being pressed for the note (since it is still playing)
                    KeyboardKeysManager.OnNoteAndKeyboardEvent(dropdownNote, true);
                }

                // Otherwise it should either currently be played on the keyboard or it should be upcoming. The 'SetNotePosition' function handles both cases.
                dropdownNote.gameObject.SetActive(true);
                dropdownNote.SetNotePosition(CurrentGameTime);
            }
        }
        else
        {
            foreach (DropdownNote dropdownNote in channelNotesInfo.DropdownNotesForChannel)
            {
                dropdownNote.gameObject.SetActive(false);
                KeyboardKeysManager.OnNoteAndKeyboardEvent(dropdownNote, false);
            }
        }
    }

    public bool GenerateMusicRoad(string midiFilePath)
    {
        if (DropdownNoteObj == null)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because {nameof(DropdownNoteObj)} is null");
            return false;
        }

        if (m_midiFileReader.ParseMidiFile(midiFilePath) == false)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because {nameof(m_midiFileReader)} failed to Parse {nameof(midiFilePath)} ({midiFilePath})");
            return false;
        }

        bool allowInstrumentsToBeReactivated = false;
        if (midiFilePath != m_currentMidiFilePath)
        {
            // Generating a new Music Road.
            m_currentMidiFilePath = midiFilePath;
            m_desiredMidiTempo = m_midiFileReader.GetAverageTempo();

            // Since it's a new Song, the old editable values the user generated are no longer valid. So sync everything up. The user ca re-edit afterwards if need be.
            float earliestStartTime = Math.Min(m_gameStartDelayTime, m_musicStartDelayTime);
            m_gameStartDelayTime = earliestStartTime;
            m_musicStartDelayTime = earliestStartTime;
            m_videoStartDelayTime = earliestStartTime;

            // SInce this is a new Music Road being generated (not just the same one with modified values) all instruments must be reactivated if they were toggle off for the user's beenfit.
            allowInstrumentsToBeReactivated = true;
        }

        Debug.Log($"Overlay Track Tempo is {MusicRoadAudioHandler.OverlayTrackTempo} and Midi Tempo is {m_midiFileReader.GetAverageTempo()}");

        ResetMusicRoad();

        foreach (ChannelNotesInfo channelNotes in m_channelNotes)
        {
            if (allowInstrumentsToBeReactivated)
            {
                channelNotes.IsActive = false;
            }
            channelNotes.DropdownNotesForChannel.Clear();
        }

        // We must have at least one note appear on the music road.
        bool successfulGeneration = false;
        int totalNotesToPlay = 0;

        for (int channelID = 0; channelID < MidiFileConstants.NumberOfChannels; ++channelID)
        {
            m_parseStatusTracker.currentChannelId = channelID;

            Dictionary<int, List<MidiFileReader.MidiEventInfo>> channelEvents = m_midiFileReader.GetNoteEventsForChannel(channelID);
            int totalChannelEvents = 0;

            // These notes have events assigned to them
            foreach (int noteId in channelEvents.Keys)
            {
                m_parseStatusTracker.currentNoteId = noteId;

                List<MidiFileReader.MidiEventInfo> midiEventsForNote = channelEvents[noteId];

                const int RequiredMidiEvents = 2;
                if (midiEventsForNote.Count % RequiredMidiEvents != 0)
                {
                    Debug.LogError($"{nameof(GenerateMusicRoad)} failed because Midi Channel ({channelID}) has an odd number of Midi Events for Note ({noteId}). We don't know the start and end points of some notes.");
                    return false;
                }

                for (int i = 0; i < midiEventsForNote.Count; i += RequiredMidiEvents)
                {
                    m_parseStatusTracker.currentEventIndex = i;

                    MidiFileReader.MidiEventInfo noteStartMidiEvent = midiEventsForNote[i];
                    MidiFileReader.MidiEventInfo noteEndMidiEvent = midiEventsForNote[i + 1];

                    // Syncing with Overlay Track (if applicable)
                    noteStartMidiEvent.timeOf = ConvertNoteTimingForNewTempo(noteStartMidiEvent.timeOf);
                    noteEndMidiEvent.timeOf = ConvertNoteTimingForNewTempo(noteEndMidiEvent.timeOf);

                    DropdownNote dropdownNote;
                    NotePlacementStatus placementResult = PlaceMidiEventOntoMusicRoad(noteStartMidiEvent, noteEndMidiEvent, totalNotesToPlay, out dropdownNote);

                    if (placementResult == NotePlacementStatus.SUCCESS)
                    {
                        // At least one note was generated for the timeline. So generation successful.
                        successfulGeneration = true;

                        // More notes than what is in the pool?
                        if (m_dropdownNotes.Count <= totalNotesToPlay)
                        {
                            m_dropdownNotes.Add(dropdownNote);
                        }

                        // Assigning to correct parent
                        dropdownNote.gameObject.transform.parent = m_channelNotesParents[channelID].transform;

                        ++totalChannelEvents;
                        ++totalNotesToPlay;
                    }
                    else if (placementResult == NotePlacementStatus.FAILED)
                    {
                        // Failed Placement: Error is output by the placement function.
                        return false;
                    }

                } // for (int i = 0; i < midiEventsForNote.Count; i += RequiredMidiEvents)
            } // foreach (int noteId in channelEvents.Keys)

            string channelName = $"Channel {((channelID + 1) < 10 ? "0" : "")}{(channelID + 1)}";
            m_channelNotesParents[channelID].name = channelName;

        } // for (int channelID = 0; channelID < m_midiFileReader.MidiTrackEvents.Count; ++channelID)

        if (successfulGeneration == false)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because not a single note was compatible with any of the on-screen Keyboard Keys");
            return false;
        }

        if (m_dropdownNotes.Count > totalNotesToPlay)
        {
            // These are excess notes that were used for the previous midi file. Remove them here so they don't interfere with the new one.
            int notesToRemove = m_dropdownNotes.Count - totalNotesToPlay;

            for (int i = totalNotesToPlay; i < m_dropdownNotes.Count; ++i)
            {
                // Need to remove unedded objects
                UnityEngine.Object.Destroy(m_dropdownNotes[i].gameObject);
            }
            m_dropdownNotes.RemoveRange(totalNotesToPlay, notesToRemove);
        }

        // Sort Dropdown notes to ensure we are looping from first to last.
        m_dropdownNotes.Sort((DropdownNote a, DropdownNote b) =>
        {
            if (a.StartTime < b.StartTime)
            {
                return -1;
            }
            else if (a.StartTime == b.StartTime)
            {
                return 0;
            }
            return 1;
        });

        // Now that we have every note in order from first to last. Ensure that they all draw correctly (later notes appear over the top of already playing notes)
        // This is to ensure any notes from other instruments that play for a short burst (0.1 seconds) are not made invisible by a string instrument playing for 3 seconds straight, etc.
        for (int i = 1; i < m_dropdownNotes.Count; ++i)
        {
            DropdownNote dropdownNote = m_dropdownNotes[i];
            LineRenderer lineRenderer = dropdownNote.LineRenderer;
            int newPriority = lineRenderer.sortingOrder + i;
            lineRenderer.sortingOrder = newPriority;

            // Also adding to lists separated by CHannel so we can activate/deactivate channels on the fly.
            ChannelNotesInfo channelNotesInfo = m_channelNotes[dropdownNote.ChannelID];
            channelNotesInfo.DropdownNotesForChannel.Add(dropdownNote);

            if (allowInstrumentsToBeReactivated)
            {
                channelNotesInfo.IsActive = true;
            }
            else
            {
                // Regenerating the same midi file. If a channel was already disabled by the user. Keep it disabled
                if (channelNotesInfo.IsActive == false)
                {
                    dropdownNote.gameObject.SetActive(false);
                }
            }
        }

        InstrumentsManager.OnMusicRoadGenerated(m_midiFileReader);

        return true;
    }

    /// <summary>
    /// Places Midi Event Onto Music Road
    /// </summary>
    /// <param name="noteStartEvent">Start Event of Midi Note</param>
    /// <param name="noteEndEvent">End Event of Midi Note</param>
    /// <param name="poolSlotId">When loading a Midi file after another one has previously been loaded. We will reuse the instantiated notes from the first midi. This represents the index we are currently located in.</param>
    /// <param name="returnedDropdownNote">Returns the Dropdown Note generated from this function</param>
    /// <returns></returns>
    protected NotePlacementStatus PlaceMidiEventOntoMusicRoad(MidiFileReader.MidiEventInfo noteStartEvent, MidiFileReader.MidiEventInfo noteEndEvent, int poolSlotId, out DropdownNote returnedDropdownNote)
    {
        returnedDropdownNote = null;

        if (noteStartEvent.noteID != noteEndEvent.noteID)
        {
            // Considering that the dictionary is separated by Note IDs, this should never happen. But definitely good to check in case of error.
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because Midi Channel ({m_parseStatusTracker.currentChannelId}) contains a mismatch of Note IDs for paired indexes "
                + $"({m_parseStatusTracker.currentEventIndex}) & ({m_parseStatusTracker.currentEventIndex + 1}) for supposed note ({m_parseStatusTracker.currentNoteId})");
            return NotePlacementStatus.FAILED;
        }

        if (noteStartEvent.isNoteActive == false || noteEndEvent.isNoteActive == true)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because Midi Channel ({m_parseStatusTracker.currentChannelId}) does not have a note on/note off "
                + $"pairing at indexes ({m_parseStatusTracker.currentEventIndex}) & ({m_parseStatusTracker.currentEventIndex + 1}) for note ({m_parseStatusTracker.currentNoteId})");
            return NotePlacementStatus.FAILED;
        }

        KeyboardInformation.NoteIDToKey noteIDToKey;
        if (KeyboardInformation.ConvertNoteIDToKeyboardData(noteStartEvent.noteID, out noteIDToKey) == false)
        {
            // Not an error. This just means that this note won't be able to appear on our on-screen keyboard because it's either
            // an octave higher or lower than our visible min/max. Just ignore this Note. If only we could fit every single key onto our limited space ;_;
            return NotePlacementStatus.INCOMPATIBLE;
        }

        float leftHorizontalPosition;
        float rightHorizontalPosition;
        if (GetLineBoundaries(noteIDToKey, out leftHorizontalPosition, out rightHorizontalPosition) == false)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because horizontal position for Key ({noteIDToKey.key}) at Octave ({noteIDToKey.octave}) could not be found.");
            return NotePlacementStatus.FAILED;
        }

        if (DropdownNoteObj == null)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because the {nameof(DropdownNoteObj)} is null");
            return NotePlacementStatus.FAILED;
        }

        float screenLengthInUnits = EndOfScreenVerticalPosition - VerticalContactPositionForNoteAndKey;

        double startTime = noteStartEvent.timeOf;
        startTime += GameStartDelayTime;

        float horizontalPosition = Mathf.Lerp(leftHorizontalPosition, rightHorizontalPosition, 0.5f);

        float verticalPosition = VerticalContactPositionForNoteAndKey;
        verticalPosition += ((float)startTime / SecondsPerScreenLength) * screenLengthInUnits;

        Vector3 notePosition = new Vector3(horizontalPosition, verticalPosition);

        if (m_dropdownNotes.Count <= poolSlotId)
        {
            GameObject playableNote = GameObject.Instantiate(DropdownNoteObj, notePosition, Quaternion.identity);
            returnedDropdownNote = playableNote.GetComponent<DropdownNote>();
        }
        else
        {
            returnedDropdownNote = m_dropdownNotes[poolSlotId];
            returnedDropdownNote.gameObject.transform.localPosition = notePosition;
        }

        if (returnedDropdownNote == null)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because the {nameof(DropdownNote)} componenet could not be retrieved from the {nameof(DropdownNoteObj)}");
            return NotePlacementStatus.FAILED;
        }

        LineRenderer lineRenderer = returnedDropdownNote.LineRenderer;
        if (lineRenderer == null)
        {
            Debug.LogError($"{nameof(GenerateMusicRoad)} failed because the {nameof(LineRenderer)} componenet could not be retrieved from the {nameof(DropdownNoteObj)}");
            return NotePlacementStatus.FAILED;
        }

        double noteEndTime = noteEndEvent.timeOf;
        noteEndTime += GameStartDelayTime;

        double noteDuration = noteEndTime - startTime;
        float noteLineLength = ((float)noteDuration / SecondsPerScreenLength) * screenLengthInUnits;

        // Setting Line Colour
        Color noteStartColour;
        Color noteEndColour;
        if (m_parseStatusTracker.currentChannelId < ChannelNotesColours.Length)
        {
            noteStartColour = ChannelNotesColours[m_parseStatusTracker.currentChannelId];
            noteEndColour = ChannelNotesColours[m_parseStatusTracker.currentChannelId];
        }
        else
        {
            noteStartColour = Color.white;
            noteEndColour = Color.white;
        }

        float lineWidth = (rightHorizontalPosition - leftHorizontalPosition) * DropdownNoteWidthMultiplier;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;


        DropdownNote.SetupData setupData = new DropdownNote.SetupData()
        {
            noteIdToKey = noteIDToKey,
            musicRoadManager = this,

            noteOnEvent = noteStartEvent,
            noteOffEvent = noteEndEvent,

            musicRoadStartTime = startTime,
            musicRoadEndTime = noteEndTime,
            lineLength = noteLineLength,

            startColour = noteStartColour,
            endColour = noteEndColour
        };

        returnedDropdownNote.Setup(setupData);

        return NotePlacementStatus.SUCCESS;
    }

    protected bool GetLineBoundaries(KeyboardInformation.NoteIDToKey keyInfo, out float leftHorizontalPosition, out float rightHorizontalPosition)
    {
        if (KeyboardKeysManager == null)
        {
            leftHorizontalPosition = default(float);
            rightHorizontalPosition = default(float);
            return false;
        }

        KeyboardKey keyObj = KeyboardKeysManager.GetKeyboardKeyFromNoteID(keyInfo);
        if (keyObj == null
            || keyObj.LeftBoundaryLine == null
            || keyObj.RightBoundaryLine == null)
        {
            leftHorizontalPosition = default(float);
            rightHorizontalPosition = default(float);
            return false;
        }

        leftHorizontalPosition = keyObj.LeftBoundaryLine.transform.position.x;
        rightHorizontalPosition = keyObj.RightBoundaryLine.transform.position.x;

        return true;
    }

    protected void OnStartOfMusicTriggered()
    {
        m_hasTriggeredMusicStart = true;
        OnStartOfMusicTriggeredCallback?.Invoke();
    }

    protected void OnFirstNoteReachedKeyboard()
    {
        OnFirstNoteReachedKeyboardCallback?.Invoke();
    }

    protected void OnEndOfMusicRoadReached()
    {
        OnEndOfMusicRoadReachedCallback?.Invoke();

        if (IsPlayingGame)
        {
            MusicRoadAudioHandler.FadeoutSound();
            MusicRoadVideoHandler.Stop();
            ResetMusicRoad();

            // Go back to Title Screen
            if (FadeToTitleScreenEffect != null)
            {

                FadeToTitleScreenEffect.PerformFadeEffect(() =>
                {
                    IsPlayingGame = false;

                    if (TitleScreenOpacityEffect != null)
                    {
                        TitleScreenOpacityEffect.InitiateFadeIn();
                    }
                }, null);
            }
        }
        else
        {
            // Still on title screen
            MusicRoadAudioHandler.StopBGM();
            MusicRoadVideoHandler.Stop();
            ResetMusicRoad();
        }
    }

    protected double ConvertNoteTimingForNewTempo(double eventTime)
    {
        // Do we need to adjust out Midi Notes to account for different Tempo between the Midi File and the Overlay Track (This keeps everything in sync)
        if (DesiredMidiTempo <= 0.0f)
        {
            return eventTime;
        }

        double midiTempo = m_midiFileReader.GetAverageTempo();
        if (midiTempo == DesiredMidiTempo)
        {
            // No need to convert
            return eventTime;
        }

        //return eventTime;

        const double SecondsPerMinute = 60.0;
        double midiBPMIterations = midiTempo * (m_midiFileReader.TotalMidiPlayTime / SecondsPerMinute);
        double overlayBPMIterations = DesiredMidiTempo * (m_midiFileReader.TotalMidiPlayTime / SecondsPerMinute);

        double overlayTrackDuration = (midiTempo / DesiredMidiTempo) * m_midiFileReader.TotalMidiPlayTime;
        double positionYouAreIn = (eventTime / m_midiFileReader.TotalMidiPlayTime);
        double newTempoTime = overlayTrackDuration * positionYouAreIn;

        return newTempoTime;
    }
}
