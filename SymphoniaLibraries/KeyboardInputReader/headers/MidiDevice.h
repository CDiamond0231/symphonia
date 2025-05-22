#ifndef __MIDI_DEVICE_H__
#define __MIDI_DEVICE_H__

#include <windows.h>
#include <mmsystem.h>

#define MIDI_EVENT_ID 63
#define MIDI_EVENT_DATA_LENGTH 3

#define MIDI_BUFFERSIZE 128

#define PRESSED_KEY_ID 144
#define RELEASED_KEY_ID 128

class MidiDevice
{
public:
	MidiDevice();
	~MidiDevice();

	bool Open(unsigned int deviceID);
    bool IsOpen();
	void Close();

	void GetDeviceName(char* name, int bufSize);

	int Poll(unsigned short* buf);

private:
	unsigned int m_deviceID;
	HMIDIIN m_device;

	unsigned long m_dataBuffer[MIDI_BUFFERSIZE];
	unsigned int m_currBufReadPos;
	unsigned int m_currBufWritePos;

	void AssignNewData(DWORD data);
    bool HandleDeviceConnectionStatusChange();

	// This is used as a function pointer for the inbuilt Windows Libraries
	static void CALLBACK OnMidiCallback(HMIDIIN device, UINT status, DWORD_PTR instancePtr, DWORD data, DWORD timestamp);
};

#endif // __MIDI_DEVICE_H__
