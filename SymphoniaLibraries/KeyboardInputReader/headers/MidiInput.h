#ifndef __MIDIINPUT_H__
#define __MIDIINPUT_H__

#define MIDI_POLL_INTERVAL  10      // in Milliseconds
#define MAX_MIDI_EVENTS     512

struct MidiEvent 
{
	int type;
	int channel;
	int value;
};

class MidiInput 
{
public:
	MidiInput(unsigned int deviceID);
	~MidiInput();
	
	void Update();
	void GetDeviceName(char* name, int bufSize);
	bool IsDeviceOpened();

private:
	bool m_deviceOpened;
	unsigned int m_deviceID;

	class MidiDevice* m_midiDevice;
	MidiEvent events[MAX_MIDI_EVENTS];
	unsigned int m_currReadEventsPos;
	unsigned int m_currWriteEventsPos;

	unsigned int PollEvents(MidiEvent* buf);
	void DecodeEvents(unsigned short* buf, int size);
};


#endif // __MIDI_H__