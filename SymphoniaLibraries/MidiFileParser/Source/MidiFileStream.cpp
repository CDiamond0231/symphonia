#include "MidiFileStream.h"
#include "MidiChannelInfo.h"

MidiFileStream::MidiFileStream()
	: m_midiFile(nullptr)
	, m_midiChannels(nullptr)
	, m_numberOfMidiChannels(0)
	, m_remainingBytesForDataChunk(0)
	, m_parseError("")
{
}

MidiFileStream::MidiFileStream(const char* filePath)
	: m_midiTempo(120)
{
	ParseMidiFile(filePath);
}

MidiFileStream::~MidiFileStream()
{
	Reset();

	if (m_midiFile == nullptr)
	{
		return;
	}

	fclose(m_midiFile);
	m_midiFile = nullptr;
}

bool MidiFileStream::ParseMidiFile(const char * filePath)
{
	if (m_midiFile != nullptr)
	{
		// This shouldn't be triggred. But just in case; if we somehow have an open midi file while parsing a new one. Close old one.
		fclose(m_midiFile);
		m_midiFile = nullptr;
	}

	// "rb" => Because Midi file must be read as binary
	fopen_s(&m_midiFile, filePath, "rb");

	if (m_midiFile != nullptr)
	{
		bool parseSuccess = PerformParse();
		
		// Close Midi file. We've already read all content.
		fclose(m_midiFile);
		m_midiFile = nullptr;

		return parseSuccess;
	}
	else
	{
		m_parseError = "Could not find specified file; ";
		return false;
	}
}

bool MidiFileStream::IsValid() const
{
	return m_midiFile != nullptr;
}

std::string MidiFileStream::GetParseError() const
{
	return m_parseError;
}

int MidiFileStream::GetMidiChannelsCount() const
{
	return m_numberOfMidiChannels;
}

unsigned long MidiFileStream::GetTempo() const
{
	return m_midiTempo;
}

MidiChannelInfo* MidiFileStream::GetChannelInfo(int channelId) const
{
	if (m_midiChannels == nullptr)
	{
		return nullptr;
	}
	if (channelId >= m_numberOfMidiChannels)
	{
		return nullptr;
	}

	return &m_midiChannels[channelId];
}

void MidiFileStream::Reset()
{
	m_numberOfMidiChannels = 0;
	m_midiTempo = 120;

	if (m_midiChannels != nullptr)
	{
		delete[] m_midiChannels;
		m_midiChannels = nullptr;
	}
}

int MidiFileStream::ReadChar()
{
	if (m_midiFile == nullptr)
	{
		return -1;
	}
	if (feof(m_midiFile))
	{
		return -1;
	}
	if (ferror(m_midiFile))
	{
		return -1;
	}

	int character = fgetc(m_midiFile);

	--m_remainingBytesForDataChunk;
	return character;
}

unsigned long MidiFileStream::ReadVariableNum()
{
	int c = ReadChar();
	if (c == -1)
	{
		return 0;
	}

	unsigned long value = c;
	if (c & 0x80)
	{
		value &= 0x7f;

		while (c & 0x80)
		{
			c = ReadChar();
			value = (value << 7) + (c & 0x7f);
		}
	}

	return value;
}

int MidiFileStream::ReadNext2Bytes()
{
	const int count = 2;
	unsigned char characters[count];
	for (int i = 0; i < count; ++i)
	{
		int result = ReadChar();

		if (result == -1)
		{
			printf("Error: Expecting %d Bytes, but only found %d valid ones", count, i + 1);
			return -1;
		}

		characters[i] = (unsigned char)result;
	}

	int as16Bit =	(static_cast<unsigned long>(characters[0]) << 8) 
				|	(static_cast<unsigned long>(characters[1]));

	return as16Bit;
}

int MidiFileStream::ReadNext4Bytes()
{
	const int count = 4;
	unsigned char characters[count];
	for (int i = 0; i < count; ++i)
	{
		int result = ReadChar();

		if (result == -1)
		{
			printf("Error: Expecting %d Bytes, but only found %d valid ones", count, i + 1);
			return -1;
		}

		characters[i] = (unsigned char)result;
	}

	int as32Bit =	(static_cast<unsigned long>(characters[0]) << 24) 
				|	(static_cast<unsigned long>(characters[1]) << 16) 
				|	(static_cast<unsigned long>(characters[2]) << 8) 
				|	(static_cast<unsigned long>(characters[3]));

	return as32Bit;
}

bool MidiFileStream::PerformParse()
{
	// The following parse uses knowledge gathered from here: https://www.csie.ntu.edu.tw/~r92092/ref/midi/
	// Which provides very helpful information regarding where content is located and how many bits/bytes are assigned to each.
	m_parseError = "";
	m_remainingBytesForDataChunk = 0;

	Reset();

	bool readSuccess = ReadHeaderInfo();
	if (readSuccess == false)
	{
		return false;
	}

	m_midiChannels = new MidiChannelInfo[m_numberOfMidiChannels];

	for (int channelId = 0; channelId < m_numberOfMidiChannels; ++channelId)
	{
		readSuccess = ReadChannelInfo(channelId);
		if (readSuccess == false)
		{
			return false;
		}
	}

	return true;
}

bool MidiFileStream::ReadHeaderInfo()
{
	// The first four bytes references the chunk data type. But we already know it. So just read past them to get to where we need to be.
	ReadNext4Bytes();

	m_remainingBytesForDataChunk = ReadNext4Bytes();

	ReadNext2Bytes(); // This returns the MIdi Format, but I don't need to know which format it is, so I'm not referencing it.

	m_numberOfMidiChannels = ReadNext2Bytes();

	ReadNext2Bytes(); // This returns MidiDivision; I believe this is supposed to be tempo...? It's a bit confusing.

	if (m_remainingBytesForDataChunk < 0)
	{
		m_parseError = m_parseError + "Header Info returned Invalid number of Bytes; ";
		return false;
	}
	if (m_numberOfMidiChannels == -1)
	{
		m_parseError = m_parseError + "Error when Parsing Number of Midi Channels; ";
		return false;
	}
	if (m_numberOfMidiChannels == 0)
	{
		m_parseError = m_parseError + "Size of Midi Channels is 0. Is this a valid Midi File?; ";
		return false;
	}

	// Everything else besides the Number of Midi Channels is junk header data that we don't care about. Read past all of it so we can reach the content we want.
	while (m_remainingBytesForDataChunk > 0)
	{
		ReadChar();
	}

	return true;
}

bool MidiFileStream::ReadChannelInfo(int channelId)
{
	// This array is indexed by the high half of a status byte.
	// Its value is either the number of bytes needed (1 or 2) for a channel message,
	// or 0 (meaning it's not a channel message).
	const char chantype[] =
	{
		0, 0, 0, 0, 0, 0, 0, 0,  // 0x00 through 0x70
		2, 2, 2, 2, 1, 1, 2, 0   // 0x80 through 0xF0
	};

	// The first four bytes represent the chunk type. This doesn't affect parsing. So read past it.
	ReadNext4Bytes();
	   
	int midiEventType = 0;
	unsigned long eventTime = 0;
	m_remainingBytesForDataChunk = ReadNext4Bytes();

	while (m_remainingBytesForDataChunk > 0)
	{
		unsigned long deltat = ReadVariableNum();
		eventTime += deltat;
		int byte = ReadChar();

		if (byte == -1)
		{
			break;
		}

		int channelMessageType;
		if ((byte & 0x80) == 0)
		{
			channelMessageType = chantype[(midiEventType >> 4) & 0x0F];
		}
		else
		{
			midiEventType = byte;
			channelMessageType = chantype[(byte >> 4) & 0x0F];

			if (channelMessageType != 0)
			{
				byte = ReadChar();
			}
		}

		// If 0, this is not a message.
		if (channelMessageType != 0)
		{
			if (midiEventType < 0x80 || midiEventType >= 0xf0)
			{
				m_parseError = m_parseError + "Event Status out of range; ";
				return false;
			}

			int eventValue = 0;
			if (channelMessageType > 1)
			{
				eventValue = ReadChar();
			}

			int eventType = midiEventType & 0xf0;
			int noteId = byte;
			bool eventAddSuccess = m_midiChannels[channelId].AddEvent(eventTime, eventType, noteId, eventValue);
			if (eventAddSuccess)
			{
				//std::cout << "Channel: " << channelId << "   NoteId: " << noteId << "   Time: " << eventTime << '\n';
			}
			continue;
		}

		// Otherwise, System Exclusive Event or Meta Event.
		// We only care about the TEMPO META EVENT and don't care about anything else. However, we DO need to read past them to get back to the content we want.
		int messageType;
		int messageLength;
		const char TEMPO_EVENT = 0x51;
		switch (midiEventType)
		{
			case 0xFF: // META_EVENT
			{
				messageType = ReadChar();
				if (messageType == TEMPO_EVENT)
				{
					messageLength = ReadVariableNum();
				}
				else
				{
					messageLength = ReadVariableNum();
				}
				break;
			}
			case 0xF0: // SYSEX_START
			case 0xF7: // SYSEX_START_A
			{
				messageType = midiEventType;
				messageLength = ReadVariableNum();
				break;
			}
			default:
			{
				m_parseError = m_parseError + "Unexpected Midi Event Type; ";
				return false;
			}
		}

		if (messageLength > m_remainingBytesForDataChunk)
		{
			m_parseError = m_parseError + "Variable length is longer than remaining Bytes for Data Chunk; ";
			return false;
		}

		if (messageType == TEMPO_EVENT)
		{
			int byte2 = ReadChar();
			int byte3 = ReadChar();
			int byte4 = ReadChar();
			int tempoInMicroSeconds =  (static_cast<unsigned long>(byte2) << 16)
				| (static_cast<unsigned long>(byte3) << 8)
				| (static_cast<unsigned long>(byte4));

			const double beats_per_second = 1e6;	// 1 million microseconds per second
			const double beats_per_minute = beats_per_second * 60.0;

			m_midiTempo = (unsigned long)((0.5 + beats_per_minute) / (double)tempoInMicroSeconds);
			m_parseError = m_parseError + "Unexpected Midi Event Type; ";
		}
		else
		{
			for (int i = 0; i < messageLength; ++i)
			{
				// Reading past the contents of this Meta/System Event
				ReadChar();
			}
		}
	}

	return true;
}