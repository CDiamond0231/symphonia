using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownNote : MonoBehaviour
{
    public struct SetupData
    {
        public KeyboardInformation.NoteIDToKey noteIdToKey;
        public MusicRoadManager musicRoadManager;

        public MidiFileReader.MidiEventInfo noteOnEvent;
        public MidiFileReader.MidiEventInfo noteOffEvent;

        public double musicRoadStartTime; // These can be different from the MidiEvent Time because the Music Road determines the Tempo of the Midi which can be different if the user specifies it.
        public double musicRoadEndTime;
        public float lineLength;

        public Color startColour;
        public Color endColour;
    }

    public MidiFileReader.MidiEventInfo NoteOnEvent { get; protected set; }
    public MidiFileReader.MidiEventInfo NoteOffEvent { get; protected set; }

    public double StartTime { get; protected set; }
    public double EndTime { get; protected set; }

    public bool HasReachedKeyboard { get; protected set; }

    public KeyboardInformation.KeyboardOctaves.Octaves Octave { get; protected set; }
    public KeyboardInformation.KeyboardKeys.Keys Key { get; protected set; }
    public int ChannelID { get; protected set; }

    public Vector3 StartPosition { get; protected set; }
    public Vector3 EndPosition { get; protected set; }

    public Color NoteBlendColour { get; protected set; }
    public Color NoteStartColour { get; protected set; }
    public Color NoteEndColour { get; protected set; }

    public KeyboardInformation.KeyboardKeys.KeyType KeyType
    {
        get
        {
            return KeyboardInformation.KeysInfo.GetKeyType(Key);
        }
    }

    public LineRenderer LineRenderer
    {
        get
        {
            return GetComponent<LineRenderer>();
        }
    }

    public Color HitStartColour
    {
        get
        {
            return Color.Lerp(NoteBlendColour, Color.white, 0.5f);
        }
    }

    public Color HitEndColour
    {
        get
        {
            return Color.Lerp(NoteBlendColour, Color.white, 0.5f);
        }
    }

    public Color MissedStartColour
    {
        get
        {
            return Color.Lerp(NoteBlendColour, Color.black, 0.5f);
        }
    }

    public Color MissedEndColour
    {
        get
        {
            return Color.Lerp(NoteBlendColour, Color.black, 0.5f);
        }
    }

    public MusicRoadManager MusicRoadManager { get; protected set; }

    protected KeyboardKeysManager KeyboardKeysManager
    {
        get
        {
            return MusicRoadManager.KeyboardKeysManager;
        }
    }

    public void ResetNote()
    {
        gameObject.SetActive(true);
        transform.localPosition = StartPosition;
        HasReachedKeyboard = false;

        LineRenderer.startColor = NoteStartColour;
        LineRenderer.endColor = NoteEndColour;
    }

    public void Setup(SetupData setupData)
    {
        MusicRoadManager = setupData.musicRoadManager;
        
        // Setting Line Length
        LineRenderer.SetPositions(new Vector3[]
        {
            Vector3.zero,
            new Vector3(0.0f, setupData.lineLength, 0.0f)
        });

        NoteOnEvent = setupData.noteOnEvent;
        NoteOffEvent = setupData.noteOffEvent;

        StartTime = setupData.musicRoadStartTime;
        EndTime = setupData.musicRoadEndTime;
        ChannelID = setupData.noteOnEvent.channelID;

        Octave = setupData.noteIdToKey.octave;
        Key = setupData.noteIdToKey.key;

        StartPosition = gameObject.transform.position;
        float endVerticalPos = setupData.musicRoadManager.VerticalContactPositionForNoteAndKey - LineRenderer.GetPosition(1).y;
        EndPosition = new Vector3(StartPosition.x, endVerticalPos, StartPosition.z);

        NoteStartColour = setupData.startColour;
        NoteEndColour = setupData.endColour;
        NoteBlendColour = Color.Lerp(setupData.startColour, setupData.endColour, 0.5f);

        ResetNote();
    }

    public void SetNotePosition(double gameTime)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (HasReachedKeyboard == false && gameTime >= StartTime)
        {
            HasReachedKeyboard = true;
            MusicRoadManager.OnNoteReachedKeyboardTrigger(this);
            KeyboardKeysManager.OnNoteReachedKeyboardTrigger(this);
        }

        float t = (float)(gameTime / EndTime);
        gameObject.transform.position = Vector3.Lerp(StartPosition, EndPosition, t);

        if (gameTime > EndTime)
        {
            MusicRoadManager.OnNoteExitKeyboardTrigger(this);
            KeyboardKeysManager.OnNoteExitKeyboardTrigger(this);
            gameObject.SetActive(false);
        }
    }

    public void OnSuccessfullyHit()
    {
        LineRenderer.startColor = HitStartColour;
        LineRenderer.endColor = HitStartColour;
    }

    public void OnNoteMissed()
    {
        LineRenderer.startColor = MissedStartColour;
        LineRenderer.endColor = MissedEndColour;

        MusicRoadManager.OnNoteMissedEvent(this);
    }
}
