using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class MidiFileConstants
{
    public const int NumberOfChannels = 16;
    public const double TicksPerMinute = 60000.0; // According to Midi Files, at least.
    public const double MillisecondsPerSecond = 1000.0;
}

public class MidiFileReader
{
    //// DLL Import /////////////////////////////////////////////////
    [DllImport("JDKsMidi", EntryPoint = "ParseMidiFile", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_ParseMidiFile(IntPtr filePath);

    [DllImport("JDKsMidi", EntryPoint = "GetMidiDuration", CallingConvention = CallingConvention.StdCall)]
    private static extern double _Dll_GetMidiDuration();

    [DllImport("JDKsMidi", EntryPoint = "GetActiveMidiChannelsCount", CallingConvention = CallingConvention.StdCall)]
    private static extern int _Dll_GetActiveMidiChannelsCount();

    [DllImport("JDKsMidi", EntryPoint = "GetMidiChannelsCount", CallingConvention = CallingConvention.StdCall)]
    private static extern int _Dll_GetMidiChannelsCount();

    [DllImport("JDKsMidi", EntryPoint = "GetMidiChannelName", CallingConvention = CallingConvention.StdCall)]
    private static extern void _Dll_GetMidiChannelName(int channelID, byte[] buf, int bufSize);

    [DllImport("JDKsMidi", EntryPoint = "GetEventsForChannel", CallingConvention = CallingConvention.StdCall)]
    private static extern int _Dll_GetEventsCountForChannel(int channelID);

    [DllImport("JDKsMidi", EntryPoint = "GetEvent", CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr _Dll_GetEvent(int channelID, int eventID);

    [DllImport("JDKsMidi", EntryPoint = "ClearMidiData", CallingConvention = CallingConvention.StdCall)]
    private static extern void _Dll_ClearMidiData();
    /////////////////////////////////////////////////////////////////
    public struct MidiEventInfo
    {
        // This is the C# version of the Midi Event with some extra data
        public int noteID;
        public int channelID;
        public bool isNoteActive;
        public double timeOf;	
        public double tempo;    
    }

    [StructLayout(LayoutKind.Sequential)]
    protected unsafe struct UnmanagedMidiEventData
    {
        // This is the same layout as the struct on the C++ side. So it can easily be deserialised using this exact layout. Thus, it is protected
        public int noteID;
        public bool isNoteActive;
        public double timeOf;	// (RealTime in seconds) 
        public double tempo;    // BPM (this CAN change between notes, so it needs to be a known variable (at least on the C++ side))
    }

    protected class ChannelInfo
    {
        public string channelName;
        public double averageTempo;
        public Dictionary<int, List<MidiEventInfo>> midiEvents;
    }
    /////////////////////////////////////////////////////////////////
    
    public double TotalMidiPlayTime { get; protected set; }
    public double FirstEventStartTime { get; protected set; }

    protected ChannelInfo[] m_channels = new ChannelInfo[MidiFileConstants.NumberOfChannels];

    /////////////////////////////////////////////////////////////////
    public bool ParseMidiFile(string sourceFilePath)
    {
        string actualFilePath = sourceFilePath.Replace("file:///", "");
        IntPtr asCStyleString = IntPtr.Zero;
        asCStyleString = Marshal.StringToHGlobalUni(actualFilePath);

        bool successfulParse = _Dll_ParseMidiFile(asCStyleString);
        Marshal.FreeHGlobal(asCStyleString);

        if (successfulParse == false)
        {
            _Dll_ClearMidiData();
            return false;
        }

        TotalMidiPlayTime = _Dll_GetMidiDuration();

        int channelsCount = _Dll_GetMidiChannelsCount();
        for (int channelID = 0; channelID < channelsCount; ++channelID)
        {
            ParseChannelEvents(channelID);
        }

        // Clear Unmanaged Code : Bye-Bye potential memory leak :)
        _Dll_ClearMidiData();

        return true;
    }

    public double GetAverageTempo()
    {
        double totalTempo = 0;
        int activeChannels = 0;

        for (int i = 0; i < MidiFileConstants.NumberOfChannels; ++i)
        {
            if (m_channels[i].averageTempo != 0.0)
            {
                totalTempo += m_channels[i].averageTempo;
                ++activeChannels;
            }
        }

        double averageTempo = 0.0;
        if (activeChannels > 0)
        {
            averageTempo = totalTempo / activeChannels;
        }

        return averageTempo;
    }

    public double GetAverageChannelTempo(int channelID)
    {
        if (channelID < 0)
        {
            return 0;
        }
        if (channelID >= MidiFileConstants.NumberOfChannels)
        {
            return 0;
        }

        return m_channels[channelID].averageTempo;
    }

    public string GetChannelName(int channelID)
    {
        if (channelID < 0)
        {
            return "Unknown";
        }
        if (channelID >= MidiFileConstants.NumberOfChannels)
        {
            return "Unknown";
        }

        return m_channels[channelID].channelName;
    }

    public bool IsActiveChannel(int channelID)
    {
        if (channelID < 0)
        {
            return false;
        }
        if (channelID >= MidiFileConstants.NumberOfChannels)
        {
            return false;
        }

        bool result = m_channels[channelID].midiEvents.Values.Count > 0;
        return result;
    }

    public Dictionary<int, List<MidiEventInfo>> GetNoteEventsForChannel(int channelID)
    {
        if (channelID < 0)
        {
            return new Dictionary<int, List<MidiEventInfo>>();
        }
        if (channelID >= MidiFileConstants.NumberOfChannels)
        {
            return new Dictionary<int, List<MidiEventInfo>>();
        }

        Dictionary<int, List<MidiEventInfo>> original = m_channels[channelID].midiEvents;
        Dictionary<int, List<MidiEventInfo>> copy = new Dictionary<int, List<MidiEventInfo>>();
        foreach (int noteID in original.Keys)
        {
            List<MidiEventInfo> copiedMidiEvents = new List<MidiEventInfo>(original[noteID]);
            copy.Add(noteID, copiedMidiEvents);
        }

        return copy;
    }

    protected void ParseChannelEvents(int channelID)
    {
        if (m_channels[channelID] == null)
        {
            m_channels[channelID] = new ChannelInfo();
        }

        ChannelInfo channelInfo = m_channels[channelID];
        if (channelInfo.midiEvents == null)
        {
            channelInfo.midiEvents = new Dictionary<int, List<MidiEventInfo>>();
        }
        else
        {
            channelInfo.midiEvents.Clear();
        }

        // Get the name of the Channel
        const int bufSize = 256;
        byte[] buf = new byte[bufSize];
        _Dll_GetMidiChannelName(channelID, buf, bufSize);
        channelInfo.channelName = System.Text.Encoding.ASCII.GetString(buf);

        // Some midi file software generators put a space for the first character if the user does not give the Instruemnt/Channel a name. 
        // Therefore, I'm checking both the first and second character for 'end-of-string' terminator to account for these.
        if (channelInfo.channelName[0] == '\0' || channelInfo.channelName[1] == '\0')
        {
            // This Channel does not have any name associated with it. So give it a default one
            channelInfo.channelName = $"Channel {((channelID + 1) < 10 ? "0" : "")}{channelID}";
        }

        int eventsCount = _Dll_GetEventsCountForChannel(channelID);
        if (eventsCount == 0)
        {
            return;
        }

        double totalTempo = 0.0;

        for (int eventID = 0; eventID < eventsCount; ++eventID)
        {
            IntPtr eventPtr = _Dll_GetEvent(channelID, eventID);
            UnmanagedMidiEventData unmanagedMidiEvent = (UnmanagedMidiEventData)Marshal.PtrToStructure(eventPtr, typeof(UnmanagedMidiEventData));

            MidiEventInfo managedMidiEvent = new MidiEventInfo();
            managedMidiEvent.noteID = unmanagedMidiEvent.noteID;
            managedMidiEvent.channelID = channelID;
            managedMidiEvent.isNoteActive = unmanagedMidiEvent.isNoteActive;
            managedMidiEvent.timeOf = unmanagedMidiEvent.timeOf;
            managedMidiEvent.tempo = unmanagedMidiEvent.tempo;

            totalTempo += managedMidiEvent.tempo;

            List<MidiEventInfo> keyEvents;
            if (channelInfo.midiEvents.TryGetValue(managedMidiEvent.noteID, out keyEvents) == false)
            {
                keyEvents = new List<MidiEventInfo>();
                channelInfo.midiEvents.Add(managedMidiEvent.noteID, keyEvents);
            }

            keyEvents.Add(managedMidiEvent);
        }

        if (eventsCount > 0)
        {
            double averageTempo = totalTempo / eventsCount;
            m_channels[channelID].averageTempo = averageTempo;
        }

        SortAndValidateNoteEventsForChannel(channelID);
    }

    protected void SortAndValidateNoteEventsForChannel(int channelID)
    {
        ChannelInfo channelInfo = m_channels[channelID];
        if (channelInfo.midiEvents == null)
        {
            return;
        }

        foreach (List<MidiEventInfo> noteEvents in channelInfo.midiEvents.Values)
        {
            // Ensuring that all events are timed correctly. NAudio seems to get incorrect 'NoteOff' events sometimes. 
            // This sorts out that issue. Which is relevant for bulding the music road.
            noteEvents.Sort((MidiEventInfo a, MidiEventInfo b) =>
            {
                if (a.timeOf < b.timeOf)
                {
                    return -1;
                }
                if (a.timeOf == b.timeOf)
                {
                    if (a.isNoteActive)
                    {
                        return 1;
                    }
                    if (b.isNoteActive)
                    {
                        return -1;
                    }
                    return 0;
                }
                return 1;
            });

            // Going through the events and removing any duplicate entries. Now that everything is sorted by time this is much easier.
            int index = 0;
            if (noteEvents[index].isNoteActive == false)
            {
                // The first note is invalid... Some creators add a'Note-Off' at the beginning off their midi files for some reason...
                noteEvents.RemoveAt(0);
            }

            int finalIndex = noteEvents.Count - 1;
            while (index < finalIndex)
            {
                int nextIndex = index + 1;
                MidiEventInfo currentMidiEvent = noteEvents[index];
                MidiEventInfo nextMidiEvent = noteEvents[nextIndex];

                if (currentMidiEvent.isNoteActive == nextMidiEvent.isNoteActive)
                {
                    // The next event is a duplicate of the current one. Remove it
                    noteEvents.RemoveAt(nextIndex);
                    finalIndex = noteEvents.Count - 1;
                    continue;
                }

                if (currentMidiEvent.timeOf == nextMidiEvent.timeOf)
                {
                    if (currentMidiEvent.isNoteActive)
                    {
                        // The current 'NoteOn' midi event is immediately switched off by the following 'NoteOff' event. There is
                        // no point in having either of these events in the list as they only take up processing time. Remove both.
                        noteEvents.RemoveRange(index, 2);
                        finalIndex = noteEvents.Count - 1;
                        continue;
                    }

                    // Or this new note is activated as soon as the other one is finished. Which is valid... Might do something here later...
                    else
                    {
                    }
                }
                index = nextIndex;
            }

            if (noteEvents.Count % 2 != 0)
            {
                // We have a useless event somewhere in this array
                // The useless event is the final one. Get rid of it.
                noteEvents.RemoveAt(noteEvents.Count - 1);
            }

            // Setting the first Event Start time. This is easy since we know the first element in the array will be the first note (sorted list)
            if (noteEvents.Count > 0)
            {
                double firstNoteStartTime = noteEvents[0].timeOf;
                if (firstNoteStartTime < FirstEventStartTime)
                {
                    FirstEventStartTime = firstNoteStartTime;
                }
            }
        }
    }

    //private double GetRealtime(NAudio.Midi.MidiEvent midiEvent, List<TempoEventInfo> tempoEvents)
    //{
    //    double BPM = 120.0;   // 120BPM is assumed unless a TempoEvent exists that says otherwise
    //    double reldelta = midiEvent.AbsoluteTime;   //The number of delta ticks between the delta time being converted and the tempo change immediately at or before it
    //    double time = 0.0;   //The real time position of the tempo change immediately at or before the delta time being converted
    //    for (int i = tempoEvents.Count - 1; i >= 0; --i)
    //    {
    //        TempoEventInfo tempoEvent = tempoEvents[i];
    //        if (tempoEvent.AbsoluteTime > midiEvent.AbsoluteTime)
    //        {
    //            continue;
    //        }

    //        BPM = tempoEvent.BPM;
    //        time = tempoEvent.realtime;
    //        reldelta = midiEvent.AbsoluteTime - tempoEvent.AbsoluteTime;
    //        break;
    //    }
    //    time += (reldelta / TicksPerQuarter) * (TicksPerMinute / BPM);
    //    double realTime = time / MillisecondsPerSecond;// Math.Round(time / MillisecondsPerSecond, 5);
    //    return realTime;
    //}
}
