#ifndef _MIDIFILESTREAM_H_
#define _MIDIFILESTREAM_H_

#include <stdio.h>
#include <corecrt_wstdio.h>
#include <iostream>

#include "MidiChannelInfo.h"

class MidiFileStream
{
public:
	MidiFileStream();
	MidiFileStream(const char* relFilePath);
	~MidiFileStream();

	bool ParseMidiFile(const char* filePath);
	bool IsValid() const;
	std::string GetParseError() const;
	int GetMidiChannelsCount() const;
	unsigned long GetTempo() const;
	MidiChannelInfo* GetChannelInfo(int channelId) const;

private:
	std::string m_parseError;
	FILE* m_midiFile;
	MidiChannelInfo* m_midiChannels;

	int m_remainingBytesForDataChunk;
	int m_numberOfMidiChannels;
	unsigned long m_midiTempo;

	void Reset();
	int ReadChar();
	unsigned long ReadVariableNum();
	int ReadNext2Bytes();
	int ReadNext4Bytes();
	bool PerformParse();
	bool ReadHeaderInfo();
	bool ReadChannelInfo(int channelId);
};

#endif