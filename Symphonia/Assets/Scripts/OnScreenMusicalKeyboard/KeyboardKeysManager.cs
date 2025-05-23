using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KeyboardInformation;

public class KeyboardKeysManager : MonoBehaviour
{
    public MusicRoadManager MusicRoadManager;

    protected KeyboardKey[,] m_mappedKeyboardKeys = null;

    protected void Awake()
    {
        m_mappedKeyboardKeys = new KeyboardKey[KeyboardOctaves.TotalOctaves, KeysInfo.KeysPerOctave];
        foreach (KeyboardKey onscreenKeyboardKey in GetComponentsInChildren<KeyboardKey>())
        {
            int index = GetKeyIDAsIndexValue(onscreenKeyboardKey.KeyID);
            m_mappedKeyboardKeys[(int)onscreenKeyboardKey.OctaveID, index] = onscreenKeyboardKey;
        }
    }

    protected void Update()
    {
        if (MusicRoadManager.KeyboardPlayMode == MusicRoadManager.PlaybackType.Automatic)
        {
            // Nothing to update
            return;
        }

        int keyId = MidiKeyboardInputHandler.GetNextTriggeredKey();
        while (keyId != -1)
        {
            KeyboardKey keyRef = GetKeyboardKeyFromKeyID(keyId);
            if (keyRef != null)
            {
                keyRef.OnKeyInputEvent(true);
            }
            keyId = MidiKeyboardInputHandler.GetNextTriggeredKey();
        }

        keyId = MidiKeyboardInputHandler.GetNextReleasedKey();
        while (keyId != -1)
        {
            KeyboardKey keyRef = GetKeyboardKeyFromKeyID(keyId);
            if (keyRef != null)
            {
                keyRef.OnKeyInputEvent(false);
            }

            keyId = MidiKeyboardInputHandler.GetNextReleasedKey();
        }
    }

    public KeyboardKey GetKeyboardKeyFromKeyID(int keyID)
    {
        if (keyID < KeyboardInformation.KeysInfo.LowestInputKeyID)
        {
            return null;
        }
        if (keyID > KeyboardInformation.KeysInfo.HighestInputKeyID)
        {
            return null;
        }

        int normalisedID = (keyID - KeyboardInformation.KeysInfo.LowestInputKeyID);
        int octave = normalisedID / KeyboardInformation.KeysInfo.KeysPerOctave;
        int key = (normalisedID % KeyboardInformation.KeysInfo.KeysPerOctave) + KeyboardInformation.KeyboardKeys.FirstKeyOffset;

        KeyboardKey keyboardKey = GetKeyboardKey((KeyboardOctaves.Octaves)octave, (KeyboardKeys.Keys)key);
        return keyboardKey;
    }

    public KeyboardKey GetKeyboardKeyFromNoteID(int noteId)
    {
        KeyboardInformation.NoteIDToKey noteIdToKey;
        if (KeyboardInformation.ConvertNoteIDToKeyboardData(noteId, out noteIdToKey) == false)
        {
            return null;
        }

        KeyboardKey keyboardKey = GetKeyboardKey(noteIdToKey.octave, noteIdToKey.key);
        return keyboardKey;
    }

    public KeyboardKey GetKeyboardKeyFromNoteID(KeyboardInformation.NoteIDToKey noteIdToKey)
    {
        KeyboardKey keyboardKey = GetKeyboardKey(noteIdToKey.octave, noteIdToKey.key);
        return keyboardKey;
    }

    public KeyboardKey GetKeyboardKey(KeyboardOctaves.Octaves octave, KeyboardKeys.Keys key)
    {
        if (m_mappedKeyboardKeys == null)
        {
            return null;
        }
        if ((int)octave >= m_mappedKeyboardKeys.GetLength(0))
        {
            return null;
        }

        int index = GetKeyIDAsIndexValue(key);
        if (index >= m_mappedKeyboardKeys.GetLength(1))
        {
            return null;
        }

        return m_mappedKeyboardKeys[(int)octave, index];
    }

    public void OnNoteReachedKeyboardTrigger(DropdownNote dropdownNote)
    {
        OnNoteAndKeyboardEvent(dropdownNote, true);
    }

    public void OnNoteExitKeyboardTrigger(DropdownNote dropdownNote)
    {
        OnNoteAndKeyboardEvent(dropdownNote, false);
    }

    public void OnNoteAndKeyboardEvent(DropdownNote dropdownNote, bool keyShouldBePressed)
    {
        KeyboardKey keyboardKey = GetKeyboardKey(dropdownNote.Octave, dropdownNote.Key);
        if (keyShouldBePressed)
        {
            keyboardKey.OnNoteReachedKey(dropdownNote, MusicRoadManager.KeyboardPlayMode);
        }
        else
        {
            keyboardKey.OnNoteExitedKey(dropdownNote, MusicRoadManager.KeyboardPlayMode);
        }
    }

    public void ResetKeyboard()
    {
        foreach (KeyboardKey key in m_mappedKeyboardKeys)
        {
            key.ResetKey();
        }
    }

    protected int GetKeyIDAsIndexValue(KeyboardKeys.Keys keyID)
    {
        int index = (int)keyID - KeyboardKeys.FirstKeyOffset;
        return index;
    }
}
