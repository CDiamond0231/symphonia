
#include "MidiDevice.h"

MidiDevice::MidiDevice()
    : m_dataBuffer()
{
	m_device = nullptr;
	m_currBufReadPos = 0;
	m_currBufWritePos = 0;
}

MidiDevice::~MidiDevice()
{
	Close();
}

bool MidiDevice::Open(unsigned int deviceID)
{
    m_deviceID = deviceID;

	if (deviceID > midiInGetNumDevs()) 
	{
		// Make sure the device ID is valid.
		return false;
	}

	DWORD_PTR dwInstance = reinterpret_cast<DWORD_PTR>(this);
    DWORD_PTR callback = reinterpret_cast<DWORD_PTR>(OnMidiCallback);
	MMRESULT result = midiInOpen(&m_device, deviceID, callback, dwInstance, CALLBACK_FUNCTION);
	if (result != MMSYSERR_NOERROR) 
	{
		return false;
	}

	result = midiInStart(m_device);
	if (result != MMSYSERR_NOERROR) 
	{
		return false;
	}

	return true;
}

bool MidiDevice::IsOpen()
{
    return m_device != nullptr;
}

void MidiDevice::Close()
{
	MMRESULT result;

	if (m_device == nullptr) 
	{
		return;
	}

	result = midiInReset(m_device);
	if (result != MMSYSERR_NOERROR) 
	{
		return;
	}

	result = midiInClose(m_device);
	if (result != MMSYSERR_NOERROR) 
	{
		return;
	}

	m_device = nullptr;
}

void MidiDevice::GetDeviceName(char* name, int bufSize)
{
	// Ensure the requested device number is within the range of known devices
	if (m_deviceID >= midiInGetNumDevs())
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

int MidiDevice::Poll(unsigned short* buf)
{
    bool canBePolled = HandleDeviceConnectionStatusChange();
    if (canBePolled == false)
    {
        return -1;
    }

    union
    {
        unsigned long   asLong;      // unsigned long (DWORD)
        unsigned short  asShorts[2]; // unsigned short (jchar)
        unsigned char   asChars[2];  // unsigned char (MIDI)
    } u;

    int count = 0;
    for (; count < MIDI_BUFFERSIZE; count += MIDI_EVENT_DATA_LENGTH)
    {
        if (m_currBufReadPos == m_currBufWritePos)
        {
            // No more events exist at this time
            return count;
        }

        u.asChars[0] = MIDI_EVENT_ID;
        u.asChars[1] = MIDI_EVENT_DATA_LENGTH;

        // Combining the Event ID and Data Length, this is decoded later
        buf[count] = u.asShorts[0];

        // Get the next event queued up
        u.asLong = m_dataBuffer[m_currBufReadPos];
        ++m_currBufReadPos;

        if (m_currBufReadPos == MIDI_BUFFERSIZE)
        {
            m_currBufReadPos = 0;
        }

        // Assigning Read Data
        buf[count + 1] = u.asShorts[0];
        buf[count + 2] = u.asShorts[1];
    }

    return count;
}

void MidiDevice::AssignNewData(DWORD data)
{
	// Push this piece of data onto the end of the queue
	m_dataBuffer[m_currBufWritePos] = data;
	++m_currBufWritePos;

	if (m_currBufWritePos == MIDI_BUFFERSIZE)
	{
		// Reset Pos. We are writing in sequential order
		m_currBufWritePos = 0;
	}
}

bool MidiDevice::HandleDeviceConnectionStatusChange()
{
    bool isStillConnected = midiInGetNumDevs() > m_deviceID;
    if (m_device == nullptr)
    {
        // Would have just been connected
        if (isStillConnected)
        {
            if (Open(m_deviceID) == false)
            {
                return true;
            }
        }

        return false;
    }
    else if (isStillConnected == false)
    {
        // Was just disconnected
        Close();
        return false;
    }

    return isStillConnected;
}

void CALLBACK MidiDevice::OnMidiCallback(HMIDIIN device, UINT status, DWORD_PTR instancePtr, DWORD data, DWORD timestamp)
{
	if (status != MIM_DATA)
	{
        // Not Midi Device Input
		return;
	}
    if (device == nullptr)
    {
        return;
    }
    if (timestamp == (DWORD)(-1))
    {
        // Error case.
        return;
    }
	
	MidiDevice* midiDevice = reinterpret_cast<MidiDevice*>(instancePtr);
	if (midiDevice == nullptr)
	{
		return;
	}

	midiDevice->AssignNewData(data);
}