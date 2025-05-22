#ifndef _MIDIDATAHANDLER_H_
#define _MIDIDATAHANDLER_H_

#define MIDI_CHANNELS_COUNT 16


class MidiDataHandler
{
public:

	MidiDataHandler();
	~MidiDataHandler();

	bool Parse(const char* midiFilePath);

	double GetMidiDuration() const;
	int GetActiveChannelsCount() const;
	int GetChannelsCount() const;
	class MidiChannelInfo* GetMidiChannel(int channelID) const;

private:

	class MidiChannelInfo* m_midiChannels;
	double m_midiDuration;
};


#endif // _MIDIDATAHANDLER_H_