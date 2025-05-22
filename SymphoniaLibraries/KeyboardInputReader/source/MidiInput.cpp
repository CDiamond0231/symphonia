
#include <windows.h>
#include "MidiInput.h"
#include "MidiDevice.h"


MidiInput::MidiInput(unsigned int deviceID)
{
	m_currReadEventsPos = 0;
	m_currWriteEventsPos = 0;

	m_deviceID = deviceID;
	m_deviceOpened = m_midiDevice->Open(m_deviceID);
}

MidiInput::~MidiInput()
{
	m_midiDevice->Close();
	m_deviceOpened = false;
}

void MidiInput::DecodeEvents(unsigned short* buf, int size)
{
	int currIndex = 0;
	while (currIndex < size)
	{
		int length = (int)((buf[currIndex] & 0xff00) >> 8);

		if ((char)(buf[currIndex] & 0x00ff) != MIDI_EVENT_ID)
		{
			continue;
		}

		MidiEvent event;
		event.type = (int)((buf[currIndex + 1] & 0x00ff));
		event.channel = (int)((buf[currIndex + 1] & 0xff00) >> 8);
		event.value = (int)((buf[currIndex + 2] & 0x00ff));

		// Add this new event to the events list
		events[m_currWriteEventsPos] = event;
		
		++m_currWriteEventsPos;
		if (m_currWriteEventsPos == MAX_MIDI_EVENTS)
		{
			m_currWriteEventsPos = 0;
		}

		currIndex += length;
	}
}

void MidiInput::Update()
{
	unsigned short midibuf[MIDI_BUFFERSIZE];
	ZeroMemory(midibuf, MIDI_BUFFERSIZE);
	
	int size = m_midiDevice->Poll(midibuf);
	DecodeEvents(midibuf, size);
}

unsigned int MidiInput::PollEvents(MidiEvent* buf)
{
	int currPos = 0;
	for ( ; currPos < MAX_MIDI_EVENTS; ++currPos)
	{
		if (m_currReadEventsPos == m_currWriteEventsPos)
		{
			break;
		}
		
		buf[currPos] = events[m_currReadEventsPos];
		
		++m_currReadEventsPos;
		if (m_currReadEventsPos == MAX_MIDI_EVENTS)
		{
			// Data is entered sequentially. So any new data will be written from index 0 if we have reached the end of the Events Array. This is just to have allocated memory.
			m_currReadEventsPos = 0;
		}
	}

	return currPos;
}

void MidiInput::GetDeviceName(char* name, int bufSize)
{
	// Ensure the requested device number is within the range of known devices
	if (m_deviceID > midiInGetNumDevs()) 
	{
		return;
	}

	MIDIINCAPS deviceInfo;
	if (midiInGetDevCaps(m_deviceID, &deviceInfo, sizeof(MIDIINCAPS)) != MMSYSERR_NOERROR)
	{
		return;
	}

	strcpy_s(name, bufSize, (const char*)deviceInfo.szPname);
}

bool MidiInput::IsDeviceOpened()
{
	return m_deviceOpened;
}
