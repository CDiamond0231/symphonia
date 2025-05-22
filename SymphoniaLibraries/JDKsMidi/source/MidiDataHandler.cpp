#include "MidiDataHandler.h"
#include "MidiChannelInfo.h"
#include "jdksmidi/world.h"
#include "jdksmidi/filereadmultitrack.h"
#include "jdksmidi/manager.h"
#include "jdksmidi/driverdump.h"
#include "jdksmidi/driver.h"


MidiDataHandler::MidiDataHandler()
{
	m_midiChannels = new MidiChannelInfo[MIDI_CHANNELS_COUNT];
	for (int i = 0; i < MIDI_CHANNELS_COUNT; ++i)
	{
		std::string name = "Track ";
		if ((i + 1) < 10)
		{
			name = name + "0";
		}
		name = name + std::to_string(i);

		m_midiChannels[i].SetChannelName(name);
	}
}

MidiDataHandler::~MidiDataHandler()
{
	delete[] m_midiChannels;
	m_midiChannels = nullptr;
}

bool MidiDataHandler::Parse(const char* midiFilePath)
{
	jdksmidi::MIDIFileReadStreamFile midiFileReadStream(midiFilePath);
	jdksmidi::MIDIMultiTrack tracks(64);
	jdksmidi::MIDIFileReadMultiTrack track_loader(&tracks);
	jdksmidi::MIDIFileRead reader(&midiFileReadStream, &track_loader);
	reader.Parse();

	// Create JDKsMidi Sequencer Which will read through the tracks
	jdksmidi::MIDISequencer seq(&tracks);
	seq.GoToZero();

	// Get Total Midi Time
	m_midiDuration = seq.GetMisicDurationInSeconds();
	seq.GoToZero();

	// In the next Function, these will both be changed
	int trackID = 0;
	jdksmidi::MIDITimedBigMessage midiEventMessage;

	// Get All Events
	while (seq.GetNextEvent(&trackID, &midiEventMessage))
	{
		if (midiEventMessage.IsBeatMarker())
		{
			continue;
		}

		// Is Note Message? (We don't really care about any other messages)
		int eventType = midiEventMessage.status & 0xf0;
		if (eventType == jdksmidi::NOTE_ON || eventType == jdksmidi::NOTE_OFF)
		{
			int noteID = (int)midiEventMessage.byte1;
			unsigned int channelID = midiEventMessage.GetChannel();

			MidiEvent* midiEvent = new MidiEvent();
			{
				midiEvent->noteID = noteID;
				midiEvent->activationTime = (float)(seq.GetCurrentTimeInMs() * 0.001);
				midiEvent->tempo = seq.GetCurrentTempo();

				if ((midiEventMessage.status & 0xf0) == jdksmidi::NOTE_OFF)
				{
					midiEvent->isNoteActive = false;
				}
				else if (midiEventMessage.IsNoteOnV0())
				{
					// The Midi file says this is a 'Note_On' event. But the velocity of the note is zero. Which means the note won't play anything.
					// Some Midi files forego the 'Note_Off' event and only change the note velocity to zero. So we need to identify if this is the case.
					midiEvent->isNoteActive = false;
				}
				else
				{
					midiEvent->isNoteActive = true;
				}
			}

			if (m_midiChannels[channelID].GetMidiEventsCount() == 0)
			{
				// First Time Setup
				char* channelName = seq.GetTrackState(trackID)->track_name;
				m_midiChannels[channelID].SetChannelName(channelName);
			}

			m_midiChannels[channelID].AddMidiEvent(midiEvent);
		}
	}

	return true;
}

double MidiDataHandler::GetMidiDuration() const
{
	return m_midiDuration;
}

int MidiDataHandler::GetActiveChannelsCount() const
{
	int count = 0;
	for (int i = 0; i < MIDI_CHANNELS_COUNT; ++i)
	{
		if (m_midiChannels[i].GetMidiEventsCount() > 0)
		{
			++count;
		}
	}

	return count;
}

int MidiDataHandler::GetChannelsCount() const
{
	return MIDI_CHANNELS_COUNT;
}

MidiChannelInfo* MidiDataHandler::GetMidiChannel(int channelID) const
{
	if (channelID < 0)
	{
		return nullptr;
	}
	if (channelID >= MIDI_CHANNELS_COUNT)
	{
		return nullptr;
	}

	return &m_midiChannels[channelID];
}
