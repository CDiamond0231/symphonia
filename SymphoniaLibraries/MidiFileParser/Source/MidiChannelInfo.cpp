#include "MidiChannelInfo.h"

MidiChannelInfo::MidiChannelInfo()
	: m_channelName("Untitled")
	, m_midiEvents()
{
}

MidiChannelInfo::~MidiChannelInfo()
{
	Clear();
}

void MidiChannelInfo::Clear()
{
	for (auto iter = m_midiEvents.begin(); iter != m_midiEvents.end(); ++iter)
	{
		MidiEvent* eventInfo = (*iter);
		if (eventInfo != nullptr)
		{
			delete eventInfo;
		}
	}

	m_midiEvents.clear();
}

MidiEvent* MidiChannelInfo::GetEvent(int index)
{
	if ((unsigned int)index >= m_midiEvents.size())
	{
		return nullptr;
	}

	std::list<MidiEvent*>::iterator iter = m_midiEvents.begin();
	if (index > 0)
	{
		std::advance(iter, index);
	}
	return (*iter);
}

size_t MidiChannelInfo::GetEventsCount() const
{
	return m_midiEvents.size();
}

bool MidiChannelInfo::AddEvent(unsigned long timeOf, int eventTypeId, int noteId, int eventValue)
{
	MidiEventType eventType = (MidiEventType)eventTypeId;
	switch (eventType)
	{
		case MidiEventType::NOTE_ON:
		case MidiEventType::NOTE_OFF:
		{
			break;
		}
		default:
		{
			// We don't care about any other Midi Event
			return false;
		}
	}

	MidiEvent* midiEvent = new MidiEvent();
	midiEvent->timeOf = timeOf;
	midiEvent->noteId = noteId;
	midiEvent->value = eventValue;

	midiEvent->isNoteActive = true;
	if (eventType == MidiEventType::NOTE_OFF)
	{
		midiEvent->isNoteActive = false;
	}
	else if (eventValue == 0)
	{
		// This is how hard the key is pressed. If it has a 'Velocity' of Zero. It basically means that the note has finished.
		// Some midi file have an explicit 'Note_Off' event and some others don't have a 'Note_Off' event and must be identified via the velocity value ('eventValue')
		midiEvent->isNoteActive = false;
	}

	m_midiEvents.push_back(midiEvent);
	return true;
}