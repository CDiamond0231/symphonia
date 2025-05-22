// JDKsMidi.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>
#include "MidiDataHandler.h"
#include "MidiChannelInfo.h"

////////// Declarations /////////////////////////
MidiDataHandler* g_midiDataHandler = nullptr;

extern "C"
{
	////////// Exported Functions /////////////////////////
    __declspec(dllexport) bool ParseMidiFile(const wchar_t*  filePath)
    {
        if (g_midiDataHandler == nullptr)
        {
            g_midiDataHandler = new MidiDataHandler();
        }

        // Converting Unicode characters
        size_t filePathLength = wcslen(filePath);
        size_t outputSize = filePathLength + 1;
        char* convertedChars = new char[outputSize];
        size_t charsConverted = 0;
        wcstombs_s(&charsConverted, convertedChars, outputSize, filePath, filePathLength);

        bool success = g_midiDataHandler->Parse(convertedChars);

        delete[] convertedChars;
        return success;
    }

    __declspec(dllexport) double GetMidiDuration()
    {
        if (g_midiDataHandler == nullptr)
        {
            return -1;
        }

        return g_midiDataHandler->GetMidiDuration();
    }

    __declspec(dllexport) int GetActiveMidiChannelsCount()
    {
        if (g_midiDataHandler == nullptr)
        {
            return -1;
        }

        return g_midiDataHandler->GetActiveChannelsCount();
    }

    __declspec(dllexport) void GetMidiChannelName(int channelID, char* buf, int bufSize)
    {
        if (g_midiDataHandler == nullptr)
        {
            strcpy_s(buf, bufSize, "Stream doesn't exist, nothing has been imported");
            return;
        }

        MidiChannelInfo* channelData = g_midiDataHandler->GetMidiChannel(channelID);
        if (channelData == nullptr)
        {
            strcpy_s(buf, bufSize, "Channel Could not be found");
            return;
        }

        strcpy_s(buf, bufSize, channelData->GetChannelName());
    }

    __declspec(dllexport) int GetMidiChannelsCount()
    {
        if (g_midiDataHandler == nullptr)
        {
            return -1;
        }

        return g_midiDataHandler->GetChannelsCount();
    }

    __declspec(dllexport) int GetEventsForChannel(int channelId)
    {
        if (g_midiDataHandler == nullptr)
        {
            return -1;
        }

        MidiChannelInfo* channelInfo = g_midiDataHandler->GetMidiChannel(channelId);
        if (channelInfo == nullptr)
        {
            return -1;
        }

        return (int)channelInfo->GetMidiEventsCount();
    }

    __declspec(dllexport) MidiEvent* GetEvent(int channelId, int eventId)
    {
        if (g_midiDataHandler == nullptr)
        {
            return nullptr;
        }

        MidiChannelInfo* channelInfo = g_midiDataHandler->GetMidiChannel(channelId);
        if (channelInfo == nullptr)
        {
            return nullptr;
        }

        return channelInfo->GetMidiEvent(eventId);
    }

    void __declspec(dllexport) ClearMidiData()
    {
        if (g_midiDataHandler == nullptr)
        {
            return;
        }

        delete g_midiDataHandler;
        g_midiDataHandler = nullptr;
    }
}

int main()
{
    ParseMidiFile(L"E:\\Music\\Midi\\PMD2\\Brine Cave.mid");
    {
        int channelCount = GetMidiChannelsCount();
        for (int channelID = 0; channelID < channelCount; ++channelID)
        {
            int eventsCount = GetEventsForChannel(channelID);
            for (int eventID = 0; eventID < eventsCount; ++eventID)
            {
                MidiEvent* midiEvent = GetEvent(channelID, eventID);
                std::cout << "Channel: " << channelID
                          << "   Note: " << midiEvent->noteID
                          << "   Time: " << midiEvent->activationTime
                          << "   IsActive: " << midiEvent->isNoteActive
                          << std::endl;
            }
        }
    }
    ClearMidiData();

    return 0;
}
