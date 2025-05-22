// dllmain.cpp : Defines the entry point for the DLL application.
#include "MidiFileStream.h"
#include <iostream>

////////// Declarations /////////////////////////
MidiFileStream* g_midiDataHandler = nullptr;


extern "C"
{
	////////// Exported Functions /////////////////////////
	__declspec(dllexport) bool ParseMidiFile(const char* filePath)
	{
		if (g_midiDataHandler == nullptr)
		{
			g_midiDataHandler = new MidiFileStream();
		}

		bool success = g_midiDataHandler->ParseMidiFile(filePath);
		return success;
	}
	
	__declspec(dllexport) void GetParseError(char* buf, int bufSize)
	{
		if (g_midiDataHandler == nullptr)
		{
			strcpy_s(buf, bufSize, "Stream doesn't exist, nothing has been imported");
			return;
		}

		strcpy_s(buf, bufSize, g_midiDataHandler->GetParseError().c_str());
	}

	__declspec(dllexport) int GetMidiChannelsCount()
	{
		if (g_midiDataHandler == nullptr)
		{
			return -1;
		}

		return g_midiDataHandler->GetMidiChannelsCount();
	}

	__declspec(dllexport) unsigned long GetMidiTempo()
	{
		if (g_midiDataHandler == nullptr)
		{
			return 120;
		}

		return g_midiDataHandler->GetTempo();
	}

	__declspec(dllexport) size_t GetEventsForChannel(int channelId)
	{
		if (g_midiDataHandler == nullptr)
		{
			return 0;
		}

		MidiChannelInfo* channelInfo = g_midiDataHandler->GetChannelInfo(channelId);
		if (channelInfo == nullptr)
		{
			return 0;
		}

		return channelInfo->GetEventsCount();
	}

	__declspec(dllexport) MidiEvent* GetEvent(int channelId, int eventId)
	{
		if (g_midiDataHandler == nullptr)
		{
			return nullptr;
		}

		MidiChannelInfo* channelInfo = g_midiDataHandler->GetChannelInfo(channelId);
		if (channelInfo == nullptr)
		{
			return nullptr;
		}

		return channelInfo->GetEvent(eventId);
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
	const char* midiFile = "C:/Users/christopher.diamond/placeholder.mid";

	bool pretendDll = true;
	if (pretendDll)
	{
		ParseMidiFile(midiFile);
		int midiChannelsCount = GetMidiChannelsCount();
		for (int i = 0; i < midiChannelsCount; ++i)
		{
			size_t totalEvents = GetEventsForChannel(i);
			for (unsigned int j = 0; j < totalEvents; ++j)
			{
				MidiEvent* midiEvent = GetEvent(i, j);
				std::cout << "NoteId: " << midiEvent->noteId
					<< ",    Active: " << (midiEvent->isNoteActive ? "True" : "False")
					<< ",    Value: " << midiEvent->value
					<< ",    Time: " << midiEvent->timeOf
					<< std::endl;
			}
		}
	}
	else
	{
		MidiFileStream stream(midiFile);
		int midiChannelsCount = stream.GetMidiChannelsCount();
		for (int i = 0; i < midiChannelsCount; ++i)
		{
			MidiChannelInfo* channelInfo = stream.GetChannelInfo(i);
			size_t totalEvents = channelInfo->GetEventsCount();
			for (unsigned int j = 0; j < totalEvents; ++j)
			{
				MidiEvent* midiEvent = channelInfo->GetEvent(j);
				std::cout << "NoteId: " << midiEvent->noteId 
							<< ",    Active: " << (midiEvent->isNoteActive ? "True" : "False")
							<< ",    Value: " << midiEvent->value 
							<< ",    Time: " << midiEvent->timeOf 
							<< std::endl;
			}
		}
	}
	return 0;
}

