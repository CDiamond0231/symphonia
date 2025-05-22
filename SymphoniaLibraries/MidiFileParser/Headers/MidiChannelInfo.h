#ifndef _MIDICHANNELINFO_H_
#define _MIDICHANNELINFO_H_

#include <list>

enum MidiEventType : int
{
	// These are the events that can be received from the Midi File.
	NOTE_ON = 0x90,				///< Note on message with velocity or Note off if velocity is 0
	NOTE_OFF = 0x80,			///< Note off message with velocity

	// These will also be received. But I have no need of them for Symphonia... For all intents and purposes. These will be actively ignored.
	POLY_PRESSURE = 0xA0,		///< Polyphonic key pressure/aftertouch with note and pressure
	CONTROL_CHANGE = 0xB0,		///< Control change message with controller number and 7 bit value
	PROGRAM_CHANGE = 0xC0,		///< Program change message with 7 bit program number
	CHANNEL_PRESSURE = 0xD0,	///< Channel pressure/aftertouch with pressure
	PITCH_BEND = 0xE0,			///< Channel bender with 14 bit bender value
	SYSEX_START_N = 0xF0,		///< Start of a MIDI Normal System-Exclusive message
	MTC = 0xF1,					///< Start of a two byte MIDI Time Code message
	SONG_POSITION = 0xF2,		///< Start of a three byte MIDI Song Position message
	SONG_SELECT = 0xF3,			///< Start of a two byte MIDI Song Select message
	TUNE_REQUEST = 0xF6,		///< Single byte tune request message
	SYSEX_END = 0xF7,			///< End of a MIDI Normal System-Exclusive message
	SYSEX_START_A = 0xF7,		///< Start of a MIDI Authorization System-Exclusive message
	RESET = 0xFF,				///< Reset byte on the serial line.
	META_EVENT = 0xFF			///< Meta event in our internal processing.
};

struct MidiEvent
{
	unsigned long timeOf;
	bool isNoteActive;
	int noteId;
	int value;
};

class MidiChannelInfo
{
public:
	MidiChannelInfo();
	~MidiChannelInfo();

	void Clear();
	MidiEvent* GetEvent(int index);
	size_t GetEventsCount() const;
	bool AddEvent(unsigned long timeOf, int eventType, int noteId, int eventValue);

private:
	std::string m_channelName;
	std::list<MidiEvent*> m_midiEvents;
};

#endif