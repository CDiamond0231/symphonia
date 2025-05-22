///////////////////////////////////////////////////////////////////////////////////////////////////////////////
//																											///
// This information all came from: https://docs.microsoft.com/en-us/windows/win32/multimedia/midi-services  ///
//																											///
///////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MidiDevice.h"
#include <string.h>
#include <iostream>
#include <list>

////////// Declarations /////////////////////////
MidiDevice* g_audioManager = nullptr;
unsigned short g_midiBuffer[MIDI_BUFFERSIZE];

bool g_wasDeviceConnected = false;

std::list<int> g_keysTriggered;
std::list<int> g_pressedKeys;
std::list<int> g_keysReleased;

std::list<int>::iterator g_triggeredKeysIterator;
std::list<int>::iterator g_releasedKeysIterator;

extern "C"
{
	////////// Exported Functions /////////////////////////
	__declspec(dllexport) bool OpenMidiInput()
	{
		if (g_audioManager != nullptr)
		{
			return false;
		}

		g_audioManager = new MidiDevice();
        g_triggeredKeysIterator = g_keysTriggered.begin();
        g_releasedKeysIterator = g_keysReleased.begin();
		return g_audioManager->Open(0);
	}

	__declspec(dllexport) bool CloseMidiInput()
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		g_audioManager->Close();

		g_keysTriggered.clear();
        g_pressedKeys.clear();
		g_keysReleased.clear();

		delete g_audioManager;
		g_audioManager = nullptr;

		return true;
	}

    __declspec(dllexport) bool IsDeviceConnected()
    {
        int midiDevsConnected = midiInGetNumDevs();
        return midiDevsConnected > 0;
    }

	__declspec(dllexport) void GetDeviceName(char* buf, int bufSize)
	{
		if (g_audioManager == nullptr)
		{
			strcpy_s(buf, bufSize, "Device not found");
			return;
		}

		g_audioManager->GetDeviceName(buf, bufSize);
	}

	__declspec(dllexport) bool Update()
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		g_keysTriggered.clear();
		g_keysReleased.clear();

		// Go Through Keys List
		int newEventsCount = g_audioManager->Poll(g_midiBuffer);
        if (newEventsCount == -1)
        {
            // Device was just disconnected. So Ensure All Pressed keys are marked as Released.
            for (std::list<int>::iterator iter = g_pressedKeys.begin(); iter != g_pressedKeys.end(); ++iter)
            {
                int pressedKeyID = *iter;
                g_keysReleased.push_back(pressedKeyID);
            }

            g_pressedKeys.clear();
        }
        else
        {
            for (int i = 0; i < newEventsCount; i += MIDI_EVENT_DATA_LENGTH)
            {
                if ((unsigned char)(g_midiBuffer[i] & 0x00ff) != 63)
                {
                    continue;
                }

                int iType = (int)((g_midiBuffer[i + 1] & 0x00ff));
                int iChannel = (int)((g_midiBuffer[i + 1] & 0xff00) >> 8);

                if (iType == PRESSED_KEY_ID)
                {
                    g_keysTriggered.push_back(iChannel);
                    g_pressedKeys.push_back(iChannel);
                }
                else if (iType == RELEASED_KEY_ID)
                {
                    g_keysReleased.push_back(iChannel);
                    g_pressedKeys.remove(iChannel);
                }
            }
        }

		g_triggeredKeysIterator = g_keysTriggered.begin();
		g_releasedKeysIterator = g_keysReleased.begin();

		return true;
	}

	__declspec(dllexport) int GetNextTriggeredKey()
	{
		if (g_audioManager == nullptr)
		{
			return -1;
		}
		if (g_triggeredKeysIterator == g_keysTriggered.end())
		{
			return -1;
		}

		int keyID = *g_triggeredKeysIterator;
		++g_triggeredKeysIterator;

		return keyID;
	}

	__declspec(dllexport) int GetNextReleasedKey()
	{
		if (g_audioManager == nullptr)
		{
			return -1;
		}
		if (g_releasedKeysIterator == g_keysReleased.end())
		{
			return -1;
		}

		int keyID = *g_releasedKeysIterator;
		++g_releasedKeysIterator;

		return keyID;
	}
}


int main()
{
    while (true)
    {
        if (IsDeviceConnected())
        {
            if (OpenMidiInput())
            {
                char deviceName[32];
                GetDeviceName(deviceName, 32);

                std::cout << "Midi Device  " << deviceName << " is connected" << std::endl;

                while (IsDeviceConnected())
                {
                    Update();

                    int keyID = GetNextTriggeredKey();
                    while (keyID != -1)
                    {
                        std::cout << "Key  " << keyID << " has been pressed." << std::endl;
                        keyID = GetNextTriggeredKey();
                    }

                    keyID = GetNextReleasedKey();
                    while (keyID != -1)
                    {
                        std::cout << "Key  " << keyID << " has been released." << std::endl;
                        keyID = GetNextReleasedKey();
                    }
                }

                CloseMidiInput();
            }
            else
            {
                std::cout << "Error opening Midi Device tunnel" << std::endl;
            }
        }
        else
        {
            std::cout << "No Midi Device is connected" << std::endl;
        }
    }
    return 0;
}