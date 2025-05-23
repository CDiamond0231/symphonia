using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInformation
{
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	** Note Hit Timings
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public struct NoteHitTimings
    {
        public enum HitResult
        {
            Perfect,
            Good,
            Acceptable,
            Miss
        }

        public static readonly Dictionary<HitResult, float> HitTimings = new Dictionary<HitResult, float>()
        {
                               // Tempo Range (note must be hit within this tempo time period)
            { HitResult.Perfect,    8.0f },
            { HitResult.Good,       12.0f },
            { HitResult.Acceptable, 16.0f },
        };

        public static bool IsStillPossibleToHitNote(DropdownNote dropdownNote)
        {
            double currentGameTime = dropdownNote.MusicRoadManager.CurrentGameTime;
            double noteStartTime = dropdownNote.StartTime;

            if (currentGameTime < noteStartTime)
            {
                return true;
            }

            double songTempo = dropdownNote.MusicRoadManager.DesiredMidiTempo;
            double timingOffset = HitTimings[HitResult.Acceptable] / songTempo;
            double timingRange = noteStartTime + timingOffset;

            if (currentGameTime > timingRange)
            {
                return false;
            }

            return true;
        }

        public static HitResult GetNoteHitResult(DropdownNote dropdownNote, double timeOfKeyPress)
        {
            double currentGameTime = dropdownNote.MusicRoadManager.CurrentGameTime;
            double timeKeyWasPressed = currentGameTime - timeOfKeyPress;

            double noteStartTime = dropdownNote.StartTime;
            double songTempo = dropdownNote.MusicRoadManager.DesiredMidiTempo;

            foreach (var hitTiming in HitTimings)
            {
                float bpmWindow = hitTiming.Value;
                double timingOffset = bpmWindow / songTempo;

                // Checking earliest time Key can be hit (this is before the Note actually reaches the keyboard)
                double timingRange = noteStartTime - timingOffset;
                if (timeKeyWasPressed < timingRange)
                {
                    // Pressed too early
                    continue;
                }

                // Checking latest (this is after the note has reached the keyboard)
                timingRange = noteStartTime + timingOffset;
                if (timeKeyWasPressed > timingRange)
                {
                    continue;
                }

                HitResult result = hitTiming.Key;
                return result;
            }

            return HitResult.Miss;
        }
    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	** Keyboard Keys
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public struct NoteIDToKey
    {
        public KeyboardOctaves.Octaves octave;
        public KeyboardKeys.Keys key;
    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	** Keyboard Keys
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public struct KeyboardKeys
    {
        public const int FirstKeyOffset = (int)Keys.C;
        public const Keys InvalidKey = (Keys)(-1);

        public enum Keys : int
        {
            C       = 9,      /*Cb = 8,	CFlat = 8,		CS = 10,*/
            DFlat   = 10,
            D       = 11,     /*Db = 10,	DFlat = 10,		DS = 12,*/
            EFlat   = 12,
            E       = 13,     /*Eb = 12,	EFlat = 12,		ES = 14,*/
            F       = 14,     /*Fb = 13,	FFlat = 13,		FS = 15,*/
            FSharp  = 15,
            G       = 16,     /*Gb = 15,	GFlat = 15,		GS = 17,*/
            AFlat   = 17,
            A       = 18,     /*Ab = 17,	AFlat = 17,		AS = 19,*/
            BFlat   = 19,
            B       = 20,     /*Bb = 19,	BFlat = 19,		BS = 9,	*/
        }

        public enum KeyType : int
        {
            LEFT,
            MIDDLE,
            RIGHT,
            BLACK,
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	** Keys Info
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public struct KeysInfo
    {
        public const int LowestInputKeyID = 36;   // Pressing the lowest key on the keyboard returns this Input ID Value
        public const int HighestInputKeyID = 96;

        public const int KeysPerOctave = 12;
        public const int WhiteKeysPerOctave = 7;
        public const int BlackKeysPerOctave = 5;

        public const int TotalWhiteKeys = 36;
        public const int TotalBlackKeys = 25;
        public const int TotalOctaves = 5;

        public const int BeginningNoteID = (2 * KeysPerOctave); // Edited To Suit My Current Keyboard Setup
        public const int HighestKeyID = BeginningNoteID + 60; // Edited To Suit My Current Keyboard Setup
        public const int KeysArraySize = HighestKeyID + 1;      // For Arrays.


        public static KeyboardKeys.KeyType GetKeyType(KeyboardKeys.Keys currentKey)
        {
            switch (currentKey)
            {
                case KeyboardKeys.Keys.DFlat:
                case KeyboardKeys.Keys.EFlat:
                case KeyboardKeys.Keys.FSharp:
                case KeyboardKeys.Keys.AFlat:
                case KeyboardKeys.Keys.BFlat:
                    {
                        return KeyboardKeys.KeyType.BLACK;
                    }

                case KeyboardKeys.Keys.C:
                case KeyboardKeys.Keys.F:
                    {
                        return KeyboardKeys.KeyType.LEFT;
                    }

                case KeyboardKeys.Keys.D:
                case KeyboardKeys.Keys.G:
                case KeyboardKeys.Keys.A:
                    {
                        return KeyboardKeys.KeyType.MIDDLE;
                    }

                case KeyboardKeys.Keys.E:
                case KeyboardKeys.Keys.B:
                    {
                        return KeyboardKeys.KeyType.RIGHT;
                    }
            }

            return KeyboardKeys.KeyType.RIGHT;
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	** Keyboard Octaves
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public struct KeyboardOctaves
    {
        public const Octaves MaxSupportedOctave = Octaves.OctaveSeven;
        public const int TotalOctaves = (int)MaxSupportedOctave + 1;

        public enum Octaves
        {
            OctaveOne = 0,
            OctaveTwo = 1,
            OctaveThree = 2,
            OctaveFour = 3,
            OctaveFive = 4,
            OctaveSix = 5,
            OctaveSeven = 6,
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	*** Data Conversion
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static bool ConvertNoteIDToKeyboardData(int noteID, out NoteIDToKey noteIdToKey)
    {
        noteIdToKey = new NoteIDToKey();

        if (noteID < KeysInfo.BeginningNoteID)
        {
            // Notes that are less than the beginning Note ID would appear to the far left of our first visible key. No need to process them.
            return false;
        }

        int fromFirstKey = noteID - KeysInfo.BeginningNoteID;
        KeyboardOctaves.Octaves octaveId = (KeyboardOctaves.Octaves)(fromFirstKey / KeysInfo.KeysPerOctave);

        if (octaveId > KeyboardOctaves.MaxSupportedOctave)
        {
            // This note would be further right than our rightmost visible key. No need to process them.
            return false;
        }

        KeyboardKeys.Keys keyId = (KeyboardKeys.Keys)((fromFirstKey % KeysInfo.KeysPerOctave) + KeyboardKeys.FirstKeyOffset);

        noteIdToKey.octave = octaveId;
        noteIdToKey.key = keyId;
        return true;
    }
}
