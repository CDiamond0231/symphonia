using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
    //// Editor Variables ////
    public KeyboardInformation.KeyboardKeys.Keys KeyID;
    public KeyboardInformation.KeyboardOctaves.Octaves OctaveID;

    public Color PressedColourWithNoActiveNote = new Color(0.33f, 0.29f, 0.29f, 1.0f);

    public Sprite UntouchedKeySprite;
    public Sprite TouchedKeySprite;

    public LineRenderer LeftBoundaryLine;
    public LineRenderer RightBoundaryLine;

    public KeyCode ComputerKeyboardKeyTrigger = KeyCode.Space;

    //// Non-Editor Variables ////
    public bool IsPressed { get; protected set; }

    public double TimeOfLastPress { get; protected set; }
    public int NotesCurrentlyAttached { get; protected set; }
    public int NotesMissedCount { get; protected set; }

    public delegate void OnKeyPressEvent(KeyboardKey keyboardKey);
    public OnKeyPressEvent OnKeyPressedEvent { get; set; }
    public OnKeyPressEvent OnKeyReleasedEvent { get; set; }

    protected LinkedList<DropdownNote> m_currentNotesAttached = new LinkedList<DropdownNote>(); // This one is a LinkedList because notes of varying length may take up different slots. So this will very likely be unordered with items aside from index 0 being removed at will.
    protected LinkedList<DropdownNote> m_attachedNotesSuccessfullyHit = new LinkedList<DropdownNote>();
    protected List<DropdownNote> m_attachedNotesUnsuccessfullyHit = new List<DropdownNote>(); // This one is a list because items will enter and exit in order (as soon as hit or miss occurs)

    protected SpriteRenderer m_spriteRenderer;

    public void OnNoteReachedKey(DropdownNote dropdownNote, MusicRoadManager.PlaybackType keyboardPlaybackType)
    {
        if (m_currentNotesAttached.Contains(dropdownNote))
        {
            // Already playing this note
            return;
        }

        bool successfullyHitNote = false;
        if (keyboardPlaybackType == MusicRoadManager.PlaybackType.Automatic)
        {
            if (NotesCurrentlyAttached == 0)
            {
                OnPressTriggered();
            }

            successfullyHitNote = true;
        }
        else
        {
            if (IsPressed)
            {
                KeyboardInformation.NoteHitTimings.HitResult hitResult = KeyboardInformation.NoteHitTimings.GetNoteHitResult(dropdownNote, TimeOfLastPress);
                successfullyHitNote = (hitResult != KeyboardInformation.NoteHitTimings.HitResult.Miss);
            }
        }

        if (successfullyHitNote)
        {
            m_spriteRenderer.color = dropdownNote.NoteBlendColour;
            dropdownNote.OnSuccessfullyHit();
            m_attachedNotesSuccessfullyHit.AddLast(dropdownNote);
        }
        else
        {
            m_attachedNotesUnsuccessfullyHit.Add(dropdownNote);
        }

        ++NotesCurrentlyAttached;
        m_currentNotesAttached.AddLast(dropdownNote);
    }

    public void OnNoteExitedKey(DropdownNote dropdownNote, MusicRoadManager.PlaybackType keyboardPlaybackType)
    {
        if (m_currentNotesAttached.Contains(dropdownNote) == false)
        {
            // Can't release this key because the note it is supposed to be playing was never attached here
            return;
        }

        --NotesCurrentlyAttached;

        m_currentNotesAttached.Remove(dropdownNote);
        m_attachedNotesSuccessfullyHit.Remove(dropdownNote);

        LinkedListNode<DropdownNote> lastAttachedHitNote = m_attachedNotesSuccessfullyHit.Last;
        if (lastAttachedHitNote != null && lastAttachedHitNote.Value != null)
        {
            m_spriteRenderer.color = lastAttachedHitNote.Value.NoteBlendColour;
        }
        else
        {
            // No notes currently active on this Key
            if (keyboardPlaybackType == MusicRoadManager.PlaybackType.Automatic)
            {
                OnPressReleased();
            }
            else if (IsPressed)
            {
                m_spriteRenderer.color = PressedColourWithNoActiveNote;
            }
        }
    }

    public void ResetKey()
    {
        IsPressed = false;
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.sprite = UntouchedKeySprite;
            m_spriteRenderer.color = Color.white;
        }

        m_currentNotesAttached.Clear();
        m_attachedNotesSuccessfullyHit.Clear();
        m_attachedNotesUnsuccessfullyHit.Clear();

        NotesCurrentlyAttached = 0;
        NotesMissedCount = 0;
        TimeOfLastPress = 0.0f;
    }

    public void OnKeyInputEvent(bool triggered)
    {
        if (triggered)
        {
            OnPressTriggered();
        }
        else
        {
            OnPressReleased();
        }
    }

    protected void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected void Update()
    {
        if (IsPressed)
        {
            TimeOfLastPress += Time.deltaTime;

            if (Input.GetKeyUp(ComputerKeyboardKeyTrigger))
            {
                OnPressReleased();
            }
        }
        else if (Input.GetKeyDown(ComputerKeyboardKeyTrigger))
        {
            OnPressTriggered();
        }

        while (m_attachedNotesUnsuccessfullyHit.Count > 0)
        {
            // This is an ordered array so the first note is the only one we really need to check. But do them all anyway
            DropdownNote unsuccessfulNote = m_attachedNotesUnsuccessfullyHit[0];
            bool canStillHitNote = KeyboardInformation.NoteHitTimings.IsStillPossibleToHitNote(unsuccessfulNote);
            if (canStillHitNote)
            {
                // If this one can still be hit. Then all other Notes in this ordered array can still be hit as well.
                break;
            }

            unsuccessfulNote.OnNoteMissed();
            m_attachedNotesUnsuccessfullyHit.RemoveAt(0);
        }
    }

    protected void OnPressTriggered()
    {
        if (IsPressed)
        {
            return;
        }

        IsPressed = true;
        m_spriteRenderer.sprite = TouchedKeySprite;
        m_spriteRenderer.color = PressedColourWithNoActiveNote;

        TimeOfLastPress = 0.0f;

        OnKeyPressedEvent?.Invoke(this);

        if (m_attachedNotesUnsuccessfullyHit.Count > 0)
        {
            // Since this note is still in the "Unsuccessful" list, it CAN in fact still be hit (See Update Function for removal conditions), so just award the hit.
            DropdownNote unsuccessfulNote = m_attachedNotesUnsuccessfullyHit[0];
            m_attachedNotesUnsuccessfullyHit.RemoveAt(0);

            unsuccessfulNote.OnSuccessfullyHit();

            m_attachedNotesSuccessfullyHit.AddLast(unsuccessfulNote);
        }
    }

    protected void OnPressReleased()
    {
        if (IsPressed == false)
        {
            return;
        }

        IsPressed = false;

        m_spriteRenderer.sprite = UntouchedKeySprite;
        m_spriteRenderer.color = Color.white;

        OnKeyReleasedEvent?.Invoke(this);
    }
}
