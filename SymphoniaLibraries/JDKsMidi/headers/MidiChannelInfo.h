#ifndef _MIDICHANNELINFO_H_
#define _MIDICHANNELINFO_H_


#include <vector>
#include <string>


struct MidiEvent
{
	int noteID;
	bool isNoteActive;
	double activationTime;	// (RealTime in seconds) 
	double tempo;			// BPM
};

class MidiChannelInfo
{
public:
	MidiChannelInfo();
	~MidiChannelInfo();

	void AddMidiEvent(MidiEvent* midiEvent);
	void SetChannelName(std::string name);

	size_t GetMidiEventsCount() const;
	const char* GetChannelName() const;
	MidiEvent* GetMidiEvent(unsigned int eventID) const;

protected:
	std::vector<MidiEvent*> m_midiEvents;
	std::string m_channelName; // Defined in Midi File
};


#endif // _MIDICHANNELINFO_H_